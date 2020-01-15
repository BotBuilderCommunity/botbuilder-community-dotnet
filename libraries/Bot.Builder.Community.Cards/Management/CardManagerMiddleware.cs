using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Cards.Nodes;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards.Management
{
    public class CardManagerMiddleware<TState> : IMiddleware
        where TState : BotState
    {
        public static readonly IList<string> ChannelsWithMessageUpdates = new List<string> { Channels.Msteams, Channels.Skype, Channels.Slack, Channels.Telegram };

        public CardManagerMiddleware(TState botState)
            : this(new CardManager<TState>(botState ?? throw new ArgumentNullException(nameof(botState))))
        {
        }

        public CardManagerMiddleware(CardManager<TState> manager)
        {
            Manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        public static CardManagerMiddlewareOptions DefaultUpdatingOptions => new CardManagerMiddlewareOptions
        {
            AutoAdaptCardActions = true,
            AutoApplyIds = true,
            AutoClearTrackedOnSend = false,
            AutoDisableOnAction = false,
            AutoEnableSentIds = false,
            AutoSaveActivitiesOnSend = true,
            AutoSeparateAttachmentsOnSend = true,
            TrackEnabledIds = false,
            IdOptions = new IdOptions(IdType.Carousel),
        };

        public static CardManagerMiddlewareOptions DefaultNonUpdatingOptions => new CardManagerMiddlewareOptions
        {
            AutoAdaptCardActions = true,
            AutoApplyIds = true,
            AutoClearTrackedOnSend = true,
            AutoDisableOnAction = true,
            AutoEnableSentIds = true,
            AutoSaveActivitiesOnSend = false,
            AutoSeparateAttachmentsOnSend = false,
            TrackEnabledIds = true,
            IdOptions = new IdOptions(IdType.Batch),
        };

        public CardManagerMiddlewareOptions UpdatingOptions { get; } = DefaultUpdatingOptions;

        public CardManagerMiddlewareOptions NonUpdatingOptions { get; } = DefaultNonUpdatingOptions;

        public CardManager<TState> Manager { get; }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            var options = GetOptionsForChannel(turnContext.Activity.ChannelId);
            var shouldProceed = true;

            if (options.IdOptions != null
                && turnContext.Activity?.Type == ActivityTypes.Message
                && turnContext.Activity.Value is JObject value)
            {
                // Whether we should proceed by default depends on the ID-tracking style
                shouldProceed = !options.TrackEnabledIds;

                var state = await Manager.StateAccessor.GetNotNullAsync(turnContext, () => new CardManagerState(), cancellationToken).ConfigureAwait(false);

                foreach (var type in options.IdOptions.GetIdTypes())
                {
                    if (value.GetIdFromPayload(type) is string id)
                    {
                        state.TrackedIdsByType.TryGetValue(type, out var trackedSet);

                        var setContainsId = trackedSet?.Contains(id) == true;

                        if (setContainsId)
                        {
                            // Proceed if the presence of the ID indicates that the ID is enabled (opt-in logic),
                            // short-circuit if the presence of the ID indicates that the ID is disabled (opt-out logic)
                            shouldProceed = options.TrackEnabledIds;
                        }

                        // Whether we should disable the ID depends on both the ID-tracking style (TrackEnabledIds)
                        // and whether the ID is already tracked (listHasId)
                        if (options.AutoDisableOnAction && setContainsId == options.TrackEnabledIds)
                        {
                            await Manager.DisableIdAsync(
                                turnContext,
                                new TypedId(type, id),
                                options.TrackEnabledIds,
                                cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
            }

            turnContext.OnSendActivities(OnSendActivities);

            if (shouldProceed && next != null)
            {
                // If this is not called, the middleware chain is effectively "short-circuited"
                await next(cancellationToken).ConfigureAwait(false);
            }
        }

        // This will be called by the Bot Builder SDK and all three of these parameters are guaranteed to not be null
        private async Task<ResourceResponse[]> OnSendActivities(ITurnContext turnContext, List<Activity> activities, Func<Task<ResourceResponse[]>> next)
        {
            var options = GetOptionsForChannel(turnContext.Activity.ChannelId);

            if (options.AutoClearTrackedOnSend && options.TrackEnabledIds && activities.Any(activity => activity.Type == ActivityTypes.Message))
            {
                await Manager.ClearTrackedIdsAsync(turnContext).ConfigureAwait(false);
            }

            if (options.AutoSeparateAttachmentsOnSend)
            {
                activities.SeparateAttachments();
            }

            if (options.AutoApplyIds)
            {
                activities.ApplyIdsToBatch(options.IdOptions);
            }

            if (options.AutoAdaptCardActions)
            {
                activities.AdaptCardActions(turnContext.Activity.ChannelId);
            }

            // The resource response ID's will be automatically applied to the activities
            // so this return value is only passed along as the next return value
            // and is not used for tracking/management.
            // The needed activity ID's can be extracted from the activities directly.
            var resourceResponses = await next().ConfigureAwait(false);

            if (options.AutoEnableSentIds && options.TrackEnabledIds)
            {
                foreach (var kvp in activities.GetIdsFromBatch())
                {
                    var type = kvp.Key;

                    foreach (var id in kvp.Value)
                    {
                        await Manager.EnableIdAsync(turnContext, new TypedId(type, id), options.TrackEnabledIds).ConfigureAwait(false);
                    }
                }
            }

            if (options.AutoSaveActivitiesOnSend)
            {
                await Manager.SaveActivities(turnContext, activities).ConfigureAwait(false);
            }

            return resourceResponses;
        }

        private CardManagerMiddlewareOptions GetOptionsForChannel(string channelId)
        {
            return ChannelsWithMessageUpdates.Contains(channelId) ? UpdatingOptions : NonUpdatingOptions;
        }
    }
}
