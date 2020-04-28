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
            AutoClearEnabledOnSend = false,
            AutoConvertAdaptiveCards = true,
            AutoDeleteOnAction = true,
            AutoDisableOnAction = false,
            AutoEnableOnSend = false,
            AutoSaveActivitiesOnSend = true,
            AutoSeparateAttachmentsOnSend = true,
            IdTrackingStyle = TrackingStyle.None,
            IdOptions = new DataIdOptions(DataIdTypes.Action),
        };

        public static CardManagerMiddlewareOptions DefaultNonUpdatingOptions => new CardManagerMiddlewareOptions
        {
            AutoAdaptOutgoingCardActions = true,
            AutoApplyIds = true,
            AutoClearEnabledOnSend = true,
            AutoConvertAdaptiveCards = true,
            AutoDeleteOnAction = false,
            AutoDisableOnAction = true,
            AutoEnableOnSend = true,
            AutoSaveActivitiesOnSend = false,
            AutoSeparateAttachmentsOnSend = false,
            IdTrackingStyle = TrackingStyle.TrackEnabled,
            IdOptions = new DataIdOptions(DataIdTypes.Action),
        };

        public CardManagerMiddlewareOptions UpdatingOptions { get; } = DefaultUpdatingOptions;

        public CardManagerMiddlewareOptions NonUpdatingOptions { get; } = DefaultNonUpdatingOptions;

        public CardManager Manager { get; }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (next is null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            var options = GetOptionsForChannel(turnContext.Activity.ChannelId);
            var idTypes = options.IdOptions is null ? new List<string> { DataIdTypes.Action } : options.IdOptions.GetIdTypes();
            var isTracking = options.IdTrackingStyle != TrackingStyle.None;
            var shouldDelete = options.AutoDeleteOnAction && idTypes.Any();
            var shouldProceed = true;

            // Is this activity from a button?
            if ((isTracking || shouldDelete) && turnContext.GetIncomingActionData() is JObject data)
            {
                if (isTracking)
                {
                    // Whether we should proceed by default depends on the ID-tracking style
                    shouldProceed = options.IdTrackingStyle == TrackingStyle.TrackDisabled;

                    var state = await Manager.GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);

                    foreach (var type in idTypes)
                    {
                        if (data.GetIdFromActionData(type) is string id)
                        {
                            state.DataIdsByType.TryGetValue(type, out var trackedSet);

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
                                    new DataId(type, id),
                                    options.IdTrackingStyle,
                                    cancellationToken).ConfigureAwait(false);
                            }
                        }
                    }
                }

                if (shouldDelete)
                {
                    // If there are multiple ID types in use,
                    // just delete the one that represents the largest scope
                    var type = DataId.Types.ElementAtOrDefault(idTypes.Max(idType => DataId.Types.IndexOf(idType)));

                    await Manager.DeleteAsync(turnContext, type, cancellationToken).ConfigureAwait(false);
                }
            }

            turnContext.OnSendActivities(OnSendActivities);
            turnContext.OnUpdateActivity(OnUpdateActivity);
            turnContext.OnDeleteActivity(OnDeleteActivity);

            if (shouldProceed)
            {
                // If this is not called, the middleware chain is effectively "short-circuited"
                await next(cancellationToken).ConfigureAwait(false);
            }
        }

        // This will be called by the Bot Builder SDK and all three of these parameters are guaranteed to not be null
        private async Task<ResourceResponse[]> OnSendActivities(ITurnContext turnContext, List<Activity> activities, Func<Task<ResourceResponse[]>> next)
        {
            if (activities.Any(activity => activity.Attachments?.Any() != true))
            {
                return await next().ConfigureAwait(false);
            }

            var options = GetOptionsForChannel(turnContext.Activity.ChannelId);

            if (options.AutoClearEnabledOnSend && options.IdTrackingStyle == TrackingStyle.TrackEnabled)
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
            // so this return value is only passed along as the outer return value
            // and is not used for tracking/management.
            // The needed activity ID's can be extracted from the activities directly.
            var resourceResponses = await next().ConfigureAwait(false);

            if (options.AutoEnableOnSend && options.IdTrackingStyle == TrackingStyle.TrackEnabled)
            {
                foreach (var dataId in activities.GetIdsFromBatch())
                {
                    await Manager.EnableIdAsync(turnContext, dataId, options.IdTrackingStyle).ConfigureAwait(false);
                }
            }

            if (options.AutoSaveActivitiesOnSend)
            {
                await Manager.SaveActivitiesAsync(turnContext, activities).ConfigureAwait(false);
            }

            return resourceResponses;
        }

        // This will be called by the Bot Builder SDK and all three of these parameters are guaranteed to not be null
        private async Task<ResourceResponse> OnUpdateActivity(ITurnContext turnContext, Activity activity, Func<Task<ResourceResponse>> next)
        {
            var options = GetOptionsForChannel(turnContext.Activity.ChannelId);
            var activities = new[] { activity };

            // Some functionality that is in OnSendActivities is intentionally omitted here.
            // We don't clear enabled ID's because a new activity isn't being sent.
            // We don't separate attachments because it's impossible to update an activity with multiple activities.

            if (options.AutoConvertAdaptiveCards)
            {
                activities.ConvertAdaptiveCards();
            }

            if (options.AutoAdaptOutgoingCardActions)
            {
                activities.AdaptOutgoingCardActions(turnContext.Activity.ChannelId);
            }

            if (options.AutoApplyIds)
            {
                activities.ApplyIdsToBatch(options.IdOptions);
            }

            // The resource response ID will already be the ID of the activity
            // so this return value is only passed along as the outer return value
            // and is not used for tracking/management.
            // The needed activity ID can be extracted from the activity directly.
            var resourceResponse = await next().ConfigureAwait(false);

            if (options.AutoEnableOnSend && options.IdTrackingStyle == TrackingStyle.TrackEnabled)
            {
                foreach (var dataId in activities.GetIdsFromBatch())
                {
                    await Manager.EnableIdAsync(turnContext, dataId, options.IdTrackingStyle).ConfigureAwait(false);
                }
            }

            var state = await Manager.GetStateAsync(turnContext).ConfigureAwait(false);
            var savedActivity = state.SavedActivities.FirstOrDefault(a => a.Id == activity.Id);

            if (options.AutoSaveActivitiesOnSend || savedActivity != null)
            {
                await Manager.SaveActivitiesAsync(turnContext, activities).ConfigureAwait(false);
            }

            return resourceResponse;
        }

        // This will be called by the Bot Builder SDK and all three of these parameters are guaranteed to not be null
        private async Task OnDeleteActivity(ITurnContext turnContext, ConversationReference reference, Func<Task> next)
        {
            await Manager.UnsaveActivityAsync(turnContext, reference.ActivityId).ConfigureAwait(false); 

            await next().ConfigureAwait(false);
        }

        private CardManagerMiddlewareOptions GetOptionsForChannel(string channelId)
        {
            return ChannelsWithMessageUpdates.Contains(channelId) ? UpdatingOptions : NonUpdatingOptions;
        }
    }
}
