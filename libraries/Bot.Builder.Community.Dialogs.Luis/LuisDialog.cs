using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Dialogs.Luis.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Dialogs.Luis
{
    /// <summary>
    /// The handler for a LUIS intent.
    /// </summary>
    /// <param name="context">The dialog context.</param>
    /// <param name="luisResult">The LUIS result.</param>
    /// <returns>A task representing the completion of the intent processing.</returns>
    public delegate Task<DialogTurnResult> IntentHandler(DialogContext context, LuisResult luisResult);

    /// <summary>
    /// The handler for a LUIS intent.
    /// </summary>
    /// <param name="context">The dialog context.</param>
    /// <param name="message">The dialog message.</param>
    /// <param name="luisResult">The LUIS result.</param>
    /// <returns>A task representing the completion of the intent processing.</returns>
    public delegate Task<DialogTurnResult> IntentActivityHandler(DialogContext context, IMessageActivity message, LuisResult luisResult);

    /// <summary>
    /// A dialog specialized to handle intents and entities from LUIS.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    [Serializable]
#pragma warning disable SA1402 // File may only contain a single class
    public class LuisDialog<TResult> : ComponentDialog
#pragma warning restore SA1402 // File may only contain a single class
    {
        public const string LuisTraceType = "https://www.luis.ai/schemas/trace";
        public const string LuisTraceLabel = "Luis Trace";
        public const string LuisTraceName = "LuisDialog";
        public const string Obfuscated = "****";

#pragma warning disable SA1401 // Fields must be private
        protected readonly IReadOnlyList<ILuisService> services;
#pragma warning restore SA1401 // Fields must be private

        /// <summary>   Mapping from intent string to the appropriate handler. </summary>
        [NonSerialized]
#pragma warning disable SA1401 // Fields must be private
        protected Dictionary<string, IntentActivityHandler> handlerByIntent;
#pragma warning restore SA1401 // Fields must be private

        /// <summary>
        /// Construct the LUIS dialog.
        /// </summary>
        /// <param name="services">The LUIS service.</param>
        public LuisDialog(string dialogId, params ILuisService[] services)
            : base(dialogId)
        {
            if (services.Length == 0)
            {
                services = MakeServicesFromAttributes();
            }

            SetField.NotNull(out this.services, nameof(services), services);
        }

        public static ILuisModel RemoveSensitiveData(ILuisModel luisModel)
        {
            if (luisModel == null)
            {
                return null;
            }

            return new LuisModelAttribute(luisModel.ModelID, Obfuscated, luisModel.ApiVersion, luisModel.UriBase.Host, luisModel.Threshold);
        }

        public ILuisService[] MakeServicesFromAttributes()
        {
            var type = this.GetType();
            var luisModels = type.GetCustomAttributes<LuisModelAttribute>(inherit: true);
            return luisModels.Select(m => new LuisService(m)).Cast<ILuisService>().ToArray();
        }

        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await MessageReceived(innerDc, innerDc.Context.Activity, cancellationToken);
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var childResult = await innerDc.ContinueDialogAsync(cancellationToken);
            if(childResult.Status == DialogTurnStatus.Empty)
            {
                return await MessageReceived(innerDc, innerDc.Context.Activity, cancellationToken);
            }
            return childResult;
        }

        /// <summary>
        /// Calculates the best scored <see cref="IntentRecommendation" /> from a <see cref="LuisResult" />.
        /// </summary>
        /// <param name="result">A result of a LUIS service call.</param>
        /// <returns>The best scored <see cref="IntentRecommendation" />, or null if <paramref name="result" /> doesn't contain any intents.</returns>
        protected virtual IntentRecommendation BestIntentFrom(LuisResult result)
        {
            return result.TopScoringIntent ?? result.Intents?.MaxBy(i => i.Score ?? 0d);
        }

        /// <summary>
        /// Calculates the best scored <see cref="LuisServiceResult" /> across multiple <see cref="LuisServiceResult" /> returned by
        /// different <see cref="ILuisService"/>.
        /// </summary>
        /// <param name="results">Results of multiple LUIS services calls.</param>
        /// <returns>A <see cref="LuisServiceResult" /> with the best scored <see cref="IntentRecommendation" /> and related <see cref="LuisResult" />,
        /// or null if no one of <paramref name="results" /> contains any intents.</returns>
        protected virtual LuisServiceResult BestResultFrom(IEnumerable<LuisServiceResult> results)
        {
            return results.MaxBy(i => i.BestIntent.Score ?? 0d);
        }

        /// <summary>
        /// Modify LUIS request before it is sent.
        /// </summary>
        /// <param name="request">Request so far.</param>
        /// <returns>Modified request.</returns>
        protected virtual LuisRequest ModifyLuisRequest(LuisRequest request)
        {
            return request;
        }

        protected virtual async Task<DialogTurnResult> MessageReceived(DialogContext context, IMessageActivity item, CancellationToken cancellationToken = default(CancellationToken))
        {
            var message = item;
            var messageText = await GetLuisQueryTextAsync(context, message);

            if (messageText != null)
            {
                // Modify request by the service to add attributes and then by the dialog to reflect the particular query
                var tasks = this.services.Select(async s =>
                {
                    var request = ModifyLuisRequest(s.ModifyRequest(new LuisRequest(messageText)));
                    var result = await s.QueryAsync(request, cancellationToken);

                    return Tuple.Create(request, result);
                }).ToArray();

                var results = await Task.WhenAll(tasks);

                var winners = from result in results.Select((value, index) => new { value = value.Item2, request = value.Item1, index })
                              let resultWinner = BestIntentFrom(result.value)
                              where resultWinner != null
                              select new LuisServiceResult(result.value, resultWinner, this.services[result.index], result.request);

                var winner = this.BestResultFrom(winners);

                if (winner == null)
                {
                    throw new InvalidOperationException("No winning intent selected from Luis results.");
                }

                await EmitTraceInfo(context, winner.Result, winner.LuisRequest, winner.LuisService.LuisModel);

                return await DispatchToIntentHandler(context, item, winner.BestIntent, winner.Result);
            }
            else
            {
                var intent = new IntentRecommendation() { Intent = string.Empty, Score = 1.0 };
                var result = new LuisResult() { TopScoringIntent = intent };
                return await DispatchToIntentHandler(context, item, intent, result);
            }
        }

        protected virtual async Task<DialogTurnResult> DispatchToIntentHandler(
                                                            DialogContext context,
                                                            IMessageActivity item,
                                                            IntentRecommendation bestIntent,
                                                            LuisResult result)
        {
            if (this.handlerByIntent == null)
            {
                this.handlerByIntent = new Dictionary<string, IntentActivityHandler>(GetHandlersByIntent());
            }

            IntentActivityHandler handler = null;
            if (result == null || !this.handlerByIntent.TryGetValue(bestIntent.Intent, out handler))
            {
                handler = this.handlerByIntent[string.Empty];
            }

            if (handler != null)
            {
                return await handler(context, item, result);
            }
            else
            {
                var text = $"No default intent handler found.";
                throw new Exception(text);
            }
        }

        protected virtual Task<string> GetLuisQueryTextAsync(DialogContext context, IMessageActivity message)
        {
            return Task.FromResult(message.Text);
        }

        protected virtual IDictionary<string, IntentActivityHandler> GetHandlersByIntent()
        {
            return LuisDialog.EnumerateHandlers(this).ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        private static async Task EmitTraceInfo(DialogContext context, LuisResult luisResult, ILuisOptions luisOptions, ILuisModel luisModel)
        {
            var luisTraceInfo = new LuisTraceInfo
            {
                LuisResult = luisResult,
                LuisOptions = luisOptions,
                LuisModel = RemoveSensitiveData(luisModel)
            };
            var trace = context.Context.Activity.CreateTrace(LuisTraceName, luisTraceInfo, LuisTraceType, LuisTraceLabel);
            await context.Context.SendActivityAsync(trace);
        }
    }

    internal static class LuisDialog
    {
        /// <summary>
        /// Enumerate the handlers based on the attributes on the dialog instance.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <returns>An enumeration of handlers.</returns>
        public static IEnumerable<KeyValuePair<string, IntentActivityHandler>> EnumerateHandlers(object dialog)
        {
            var type = dialog.GetType();
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var method in methods)
            {
                var intents = method.GetCustomAttributes<LuisIntentAttribute>(inherit: true).ToArray();
                IntentActivityHandler intentHandler = null;

                try
                {
                    intentHandler = (IntentActivityHandler)Delegate.CreateDelegate(typeof(IntentActivityHandler), dialog, method, throwOnBindFailure: false);
                }
                catch (ArgumentException)
                {
                    // "Cannot bind to the target method because its signature or security transparency is not compatible with that of the delegate type."
                    // https://github.com/Microsoft/BotBuilder/issues/634
                    // https://github.com/Microsoft/BotBuilder/issues/435
                }

                // fall back for compatibility
                if (intentHandler == null)
                {
                    try
                    {
                        var handler = (IntentHandler)Delegate.CreateDelegate(typeof(IntentHandler), dialog, method, throwOnBindFailure: false);

                        if (handler != null)
                        {
                            // thunk from new to old delegate type
                            intentHandler = (context, message, result) => handler(context, result);
                        }
                    }
                    catch (ArgumentException)
                    {
                        // "Cannot bind to the target method because its signature or security transparency is not compatible with that of the delegate type."
                        // https://github.com/Microsoft/BotBuilder/issues/634
                        // https://github.com/Microsoft/BotBuilder/issues/435
                    }
                }

                if (intentHandler != null)
                {
                    var intentNames = intents.Select(i => i.IntentName).DefaultIfEmpty(method.Name);

                    foreach (var intentName in intentNames)
                    {
                        yield return new KeyValuePair<string, IntentActivityHandler>(intentName?.Trim() ?? string.Empty, intentHandler);
                    }
                }
                else
                {
                    if (intents.Length > 0)
                    {
                        throw new InvalidIntentHandlerException(string.Join(";", intents.Select(i => i.IntentName)), method);
                    }
                }
            }
        }
    }
}