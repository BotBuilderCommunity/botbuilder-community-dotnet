using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Cards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards.Management
{
    public class CardManagerMiddleware<TState> : IMiddleware
        where TState : BotState
    {
        public CardManagerMiddleware(TState botState, CardManagerOptions options = null)
            : this(new CardManager<TState>(botState ?? throw new ArgumentNullException(nameof(botState))), options)
        {
        }

        public CardManagerMiddleware(CardManager<TState> manager, CardManagerOptions options = null)
        {
            Manager = manager ?? throw new ArgumentNullException(nameof(manager));
            Options = options ?? new CardManagerOptions();
        }

        public CardManager<TState> Manager { get; }

        public CardManagerOptions Options { get; }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken)
        {
            BotAssert.ContextNotNull(turnContext);

            // Whether we should proceed by default depends on the ID-tracking style
            var shouldProceed = !Manager.TrackEnabledIds;

            if (turnContext.Activity?.Type == ActivityTypes.Message && turnContext.Activity.Value is JObject value)
            {
                var state = await Manager.StateAccessor.GetNotNullAsync(turnContext, () => new CardManagerState(), cancellationToken);

                foreach (var type in Options.IdOptions.GetIdTypes())
                {
                    if (value.GetIdFromPayload(type) is string id)
                    {
                        state.TrackedIdsByType.TryGetValue(type, out var trackedList);

                        var listHasId = trackedList?.Contains(id) == true;

                        if (listHasId)
                        {
                            // Proceed if the presence of the ID indicates that the ID is enabled (opt-in logic),
                            // short-circuit if the presence of the ID indicates that the ID is disabled (opt-out logic)
                            shouldProceed = Manager.TrackEnabledIds;
                        }

                        // Whether we should disable the ID depends on both the ID-tracking style
                        // and whether the ID is already tracked
                        if (Options.AutoDisableOnAction && listHasId == Manager.TrackEnabledIds)
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
            if (Options.AutoClearListOnSend && Manager.TrackEnabledIds && activities.Any(activity => activity.Type == ActivityTypes.Message))
            {
                await Manager.ClearTrackedIdsAsync(turnContext);
            }

            if (Options.AutoApplyId)
            {
                var batchAndAttachmentsIds = activities.ApplyIdToBatch(Options.IdOptions);

                if (Options.AutoEnableSentId && Manager.TrackEnabledIds && batchAndAttachmentsIds != null)
                {
                    if (batchAndAttachmentsIds.BatchId is string batchId)
                    {
                       await Manager.EnableIdAsync(turnContext, batchId, IdType.Batch);
                    }

                    foreach (var attachmentsAndCardIds in batchAndAttachmentsIds.AttachmentsIds)
                    {
                        if (attachmentsAndCardIds.AttachmentsId is string attachmentsId)
                        {
                            await Manager.EnableIdAsync(turnContext, attachmentsId, IdType.Attachments);
                        }

                        foreach (var cardAndActionIds in attachmentsAndCardIds.CardIds)
                        {
                            if (cardAndActionIds.CardId is string cardId)
                            {
                                await Manager.EnableIdAsync(turnContext, cardId, IdType.Card);
                            }

                            foreach (var actionId in cardAndActionIds.ActionIds)
                            {
                                await Manager.EnableIdAsync(turnContext, actionId, IdType.Action);
                            }
                        }
                    }
                }
            }

            return await next();
        }
    }
}
