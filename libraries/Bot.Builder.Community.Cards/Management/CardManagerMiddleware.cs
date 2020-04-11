using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards.Management
{
    public class CardManagerMiddleware : IMiddleware
    {
        public static readonly IList<string> ChannelsWithMessageUpdates = new List<string> { Channels.Msteams, Channels.Skype, Channels.Slack, Channels.Telegram };

        public CardManagerMiddleware(CardManager manager)
        {
            Manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        public static CardManagerMiddlewareOptions DefaultUpdatingOptions => new CardManagerMiddlewareOptions
        {
            AutoAdaptOutgoingCardActions = true,
            AutoApplyIds = true,
            AutoClearTrackedOnSend = false,
            AutoConvertAdaptiveCards = true,
            AutoDeleteOnAction = true,
            AutoDisableOnAction = false,
            AutoEnableOnSend = false,
            AutoSaveActivitiesOnSend = true,
            AutoSeparateAttachmentsOnSend = true,
            IdTrackingStyle = TrackingStyle.None,
            IdOptions = new PayloadIdOptions(PayloadIdTypes.Action),
        };

        public static CardManagerMiddlewareOptions DefaultNonUpdatingOptions => new CardManagerMiddlewareOptions
        {
            AutoAdaptOutgoingCardActions = true,
            AutoApplyIds = true,
            AutoClearTrackedOnSend = true,
            AutoConvertAdaptiveCards = true,
            AutoDeleteOnAction = false,
            AutoDisableOnAction = true,
            AutoEnableOnSend = true,
            AutoSaveActivitiesOnSend = false,
            AutoSeparateAttachmentsOnSend = false,
            IdTrackingStyle = TrackingStyle.TrackEnabled,
            IdOptions = new PayloadIdOptions(PayloadIdTypes.Action),
        };

        public CardManagerMiddlewareOptions UpdatingOptions { get; } = DefaultUpdatingOptions;

        public CardManagerMiddlewareOptions NonUpdatingOptions { get; } = DefaultNonUpdatingOptions;

        public CardManager Manager { get; }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            var options = GetOptionsForChannel(turnContext.Activity.ChannelId);
            var shouldProceed = true;

            // Is this activity from a button?
            if (turnContext.GetIncomingPayload() is JObject payload)
            {
                var idTypes = options.IdOptions is null ? new List<string> { PayloadIdTypes.Action } : options.IdOptions.GetIdTypes();

                if (options.IdTrackingStyle != TrackingStyle.None)
                {
                    // Whether we should proceed by default depends on the ID-tracking style
                    shouldProceed = options.IdTrackingStyle == TrackingStyle.TrackDisabled;

                    var state = await Manager.StateAccessor.GetNotNullAsync(turnContext, () => new CardManagerState(), cancellationToken).ConfigureAwait(false);

                    foreach (var type in idTypes)
                    {
                        if (payload.GetIdFromPayload(type) is string id)
                        {
                            state.PayloadIdsByType.TryGetValue(type, out var trackedSet);

                            var setContainsId = trackedSet?.Contains(id) == true;

                            if (setContainsId)
                            {
                                // Proceed if the presence of the ID indicates that the ID is enabled (opt-in logic),
                                // short-circuit if the presence of the ID indicates that the ID is disabled (opt-out logic)
                                shouldProceed = options.IdTrackingStyle == TrackingStyle.TrackEnabled;
                            }

                            if (options.AutoDisableOnAction)
                            {
                                // This might disable an already-disabled ID but that's okay
                                await Manager.DisableIdAsync(
                                    turnContext,
                                    new PayloadItem(type, id),
                                    options.IdTrackingStyle,
                                    cancellationToken).ConfigureAwait(false);
                            }
                        }
                    }
                }

                if (options.AutoDeleteOnAction && idTypes.Any())
                {
                    // If there are multiple ID types in use,
                    // just delete the one that represents the largest scope
                    var type = PayloadIdTypes.Collection.ElementAtOrDefault(idTypes.Max(idType => PayloadIdTypes.GetIndex(idType)));

                    await Manager.DeleteAsync(turnContext, type, cancellationToken).ConfigureAwait(false);
                }
            }

            // TODO: Add update and delete handlers
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
            if (activities.Any(activity => activity.Type == ActivityTypes.Message))
            {
                var options = GetOptionsForChannel(turnContext.Activity.ChannelId);

                if (options.AutoClearTrackedOnSend && options.IdTrackingStyle == TrackingStyle.TrackEnabled)
                {
                    await Manager.ClearTrackedIdsAsync(turnContext).ConfigureAwait(false);
                }

                if (options.AutoConvertAdaptiveCards)
                {
                    activities.ConvertAdaptiveCards();
                }

                if (options.AutoSeparateAttachmentsOnSend)
                {
                    activities.SeparateAttachments();
                }

                if (options.AutoAdaptOutgoingCardActions)
                {
                    activities.AdaptOutgoingCardActions(turnContext.Activity.ChannelId);
                }

                if (options.AutoApplyIds)
                {
                    activities.ApplyIdsToBatch(options.IdOptions);
                }

                // The resource response ID's will be automatically applied to the activities
                // so this return value is only passed along as the next return value
                // and is not used for tracking/management.
                // The needed activity ID's can be extracted from the activities directly.
                var resourceResponses = await next().ConfigureAwait(false);

                if (options.AutoEnableOnSend && options.IdTrackingStyle == TrackingStyle.TrackEnabled)
                {
                    foreach (var payloadId in activities.GetIdsFromBatch())
                    {
                        await Manager.EnableIdAsync(turnContext, payloadId, options.IdTrackingStyle).ConfigureAwait(false);
                    }
                }

                if (options.AutoSaveActivitiesOnSend)
                {
                    await Manager.SaveActivitiesAsync(turnContext, activities).ConfigureAwait(false);
                }

                return resourceResponses; 
            }

            return await next().ConfigureAwait(false);
        }

        private CardManagerMiddlewareOptions GetOptionsForChannel(string channelId)
        {
            return ChannelsWithMessageUpdates.Contains(channelId) ? UpdatingOptions : NonUpdatingOptions;
        }
    }
}
