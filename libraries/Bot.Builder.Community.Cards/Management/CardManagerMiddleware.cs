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
            AutoApplyId = true,
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
            AutoApplyId = true,
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

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken)
        {
            BotAssert.ContextNotNull(turnContext);

            var options = GetOptionsForChannel(turnContext.Activity.ChannelId);

            // Whether we should proceed by default depends on the ID-tracking style
            var shouldProceed = !options.TrackEnabledIds;

            if (options.IdOptions != null
                && turnContext.Activity?.Type == ActivityTypes.Message
                && turnContext.Activity.Value is JObject value)
            {
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
                            await Manager.DisableIdAsync(turnContext, id, type, options.TrackEnabledIds, cancellationToken).ConfigureAwait(false);
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

        private async Task<ResourceResponse[]> OnSendActivities(ITurnContext turnContext, List<Activity> activities, Func<Task<ResourceResponse[]>> next)
        {
            var options = GetOptionsForChannel(turnContext.Activity.ChannelId);

            if (options.AutoSeparateAttachmentsOnSend)
            {
                // We need to iterate backwards because we're changing the length of the list
                for (int i = activities.Count() - 1; i > -1; i--)
                {
                    var activity = activities[i];
                    var attachmentCount = activity.Attachments?.Count();
                    var hasText = activity.Text != null;

                    if (activity.AttachmentLayout == AttachmentLayoutTypes.List
                        && ((attachmentCount > 0 && hasText) || attachmentCount > 1))
                    {
                        var separateActivities = new List<Activity>();
                        var js = new JsonSerializerSettings();
                        var json = JsonConvert.SerializeObject(activity, js);

                        if (hasText)
                        {
                            var textActivity = JsonConvert.DeserializeObject<Activity>(json, js);

                            textActivity.Attachments = null;
                            separateActivities.Add(textActivity);
                        }

                        foreach (var attachment in activity.Attachments)
                        {
                            var attachmentActivity = JsonConvert.DeserializeObject<Activity>(json, js);

                            attachmentActivity.Text = null;
                            attachmentActivity.Attachments = new List<Attachment> { attachment };
                            separateActivities.Add(attachmentActivity);
                        }

                        activities.RemoveAt(i);
                        activities.InsertRange(i, separateActivities);
                    }
                }
            }

            if (options.AutoClearTrackedOnSend && options.TrackEnabledIds && activities.Any(activity => activity.Type == ActivityTypes.Message))
            {
                await Manager.ClearTrackedIdsAsync(turnContext).ConfigureAwait(false);
            }

            if (options.AutoApplyId)
            {
                activities.ApplyIdsToBatch(options.IdOptions);
            }

            // The resource response ID's will be automatically applied to the activities
            // so this return value is only passed along as the next return value.
            // The activity ID's can be extracted from the activities directly.
            var resourceResponses = await next().ConfigureAwait(false);

            if (options.AutoEnableSentIds && options.TrackEnabledIds)
            {
                await activities.GetIdsFromBatch().RecurseOverIdsAsync(async (id, type) =>
                {
                    if (id != null)
                    {
                        await Manager.EnableIdAsync(turnContext, id, type, options.TrackEnabledIds).ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);
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
