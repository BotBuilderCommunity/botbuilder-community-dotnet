using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards.Management
{
    public class CardFocuserMiddleware<T> : IMiddleware
        where T : BotState
    {
        public CardFocuserMiddleware(T botState, CardFocuserOptions options = null)
            : this(new CardFocuser<T>(botState ?? throw new ArgumentNullException(nameof(botState))), options)
        {
        }

        public CardFocuserMiddleware(CardFocuser<T> manager, CardFocuserOptions options = null)
        {
            Manager = manager ?? throw new ArgumentNullException(nameof(manager));
            Options = options ?? new CardFocuserOptions();
        }

        public CardFocuser<T> Manager { get; }

        public CardFocuserOptions Options { get; }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken)
        {
            BotAssert.ContextNotNull(turnContext);

            var shouldProceed = false;

            if (turnContext.Activity?.Type == ActivityTypes.Message && turnContext.Activity.Value is JObject value)
            {
                var id = value.GetIdFromPayload(Options.IdType);
                var state = await Manager.StateAccessor.GetAsync(turnContext, () => new CardFocuserState(), cancellationToken);

                if (id != null && id == state?.FocusedId)
                {
                    if (Options.AutoUnfocus)
                    {
                        state.FocusedId = null;
                    }

                    shouldProceed = true;
                }
            }

            turnContext.OnSendActivities(OnSendActivities);

            if (shouldProceed)
            {
                await next?.Invoke(cancellationToken);
            }
        }

        private async Task<ResourceResponse[]> OnSendActivities(ITurnContext turnContext, List<Activity> activities, Func<Task<ResourceResponse[]>> next)
        {
            if (Options.AutoApplyId)
            {
                var options = new IdOptions(Options.IdType);
                var id = activities.ApplyIdToBatch(options);

                if (Options.AutoFocus)
                {
                    var state = await Manager.StateAccessor.GetNonNullAsync(turnContext, () => new CardFocuserState());

                    state.FocusedId = id;
                }
            }

            return await next();
        }
    }
}
