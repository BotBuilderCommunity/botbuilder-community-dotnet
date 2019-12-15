using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards.Management
{
    public class CardDisablerMiddleware<T> : IMiddleware
        where T : BotState
    {
        public CardDisablerMiddleware(T botState, CardDisablerOptions options = null)
            : this(new CardDisabler<T>(botState ?? throw new ArgumentNullException(nameof(botState))), options)
        {
        }

        public CardDisablerMiddleware(CardDisabler<T> manager, CardDisablerOptions options = null)
        {
            Manager = manager ?? throw new ArgumentNullException(nameof(manager));
            Options = options ?? new CardDisablerOptions();
        }

        public CardDisabler<T> Manager { get; }

        public CardDisablerOptions Options { get; }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken)
        {
            BotAssert.ContextNotNull(turnContext);

            var shouldProceed = true;

            if (turnContext.Activity?.Type == ActivityTypes.Message && turnContext.Activity.Value is JObject value)
            {
                var state = await Manager.StateAccessor.GetNonNullAsync(turnContext, () => new CardDisablerState(), cancellationToken);

                foreach (var type in Enum.GetValues(typeof(IdType)).Cast<IdType>())
                {
                    var id = value.GetIdFromPayload(type);

                    if (Options.IdOptions.HasIdType(type) && id != null)
                    {
                        state.DisabledIdsByType.TryGetValue(type, out var disabledList);

                        if (disabledList?.Contains(id) == true)
                        {
                            shouldProceed = false;
                        }
                        else if (Options.AutoDisable)
                        {
                            await Manager.DisableIdAsync(turnContext, id, type, cancellationToken);
                        }
                    }
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
                activities.ApplyIdToBatch(Options.IdOptions);
            }

            return await next();
        }
    }
}
