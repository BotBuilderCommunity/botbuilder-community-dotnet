using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Cards.Management.Tree;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards.Management
{
    public class CardManager<TState>
        where TState : BotState
    {
        public CardManager(TState botState)
            : this(botState?.CreateProperty<CardManagerState>(nameof(CardManagerState)) ?? throw new ArgumentNullException(nameof(botState)))
        {
        }

        private CardManager(IStatePropertyAccessor<CardManagerState> stateAccessor)
        {
            StateAccessor = stateAccessor ?? throw new ArgumentNullException(nameof(stateAccessor));
        }

        public IStatePropertyAccessor<CardManagerState> StateAccessor { get; }

        public static CardManager<TState> Create(IStatePropertyAccessor<CardManagerState> stateAccessor)
        {
            return new CardManager<TState>(stateAccessor ?? throw new ArgumentNullException(nameof(stateAccessor)));
        }

        // --------------------
        // NON-UPDATING METHODS
        // --------------------

        public async Task DisableIdAsync(ITurnContext turnContext, PayloadId payloadId, bool trackEnabledIds = true, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (payloadId is null)
            {
                throw new ArgumentNullException(nameof(payloadId));
            }

            await (trackEnabledIds ? ForgetIdAsync(turnContext, payloadId.Id, cancellationToken) : TrackIdAsync(turnContext, payloadId, cancellationToken)).ConfigureAwait(false);
        }

        public async Task EnableIdAsync(ITurnContext turnContext, PayloadId payloadId, bool trackEnabledIds = true, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (payloadId is null)
            {
                throw new ArgumentNullException(nameof(payloadId));
            }

            await (trackEnabledIds ? TrackIdAsync(turnContext, payloadId, cancellationToken) : ForgetIdAsync(turnContext, payloadId.Id, cancellationToken)).ConfigureAwait(false);
        }

        public async Task TrackIdAsync(ITurnContext turnContext, PayloadId payloadId, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (payloadId is null)
            {
                throw new ArgumentNullException(nameof(payloadId));
            }

            var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);
            var payloadIdsByType = state.PayloadIdsByType;

            payloadIdsByType.InitializeKey(payloadId.Type, new HashSet<string>()).Add(payloadId.Id);
            state.PayloadIdsByType = payloadIdsByType;
        }

        public async Task ForgetIdAsync(ITurnContext turnContext, string id, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (id is null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);

            state.PayloadIdsByType?.Values.WhereNotNull().ToList().ForEach(list => list.Remove(id));
        }

        public async Task ClearTrackedIdsAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);
            var payloadIdsByType = state.PayloadIdsByType;

            payloadIdsByType.Clear();
            state.PayloadIdsByType = payloadIdsByType;
        }

        // ----------------
        // UPDATING METHODS
        // ----------------

        public async Task SaveActivitiesAsync(ITurnContext turnContext, IEnumerable<Activity> activities, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);
            BotAssert.ActivityListNotNull(activities);

            var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);
            var activityIdsByPayloadId = state.ActivityIdsByPayloadId;
            var activitiesById = state.ActivityById;

            foreach (var activity in activities)
            {
                if (activity != null)
                {
                    var activityId = activity.Id;

                    CardTree.RecurseAsync(activity, (PayloadId payloadId) =>
                    {
                        activityIdsByPayloadId.InitializeKey(payloadId.Id, new HashSet<string>()).Add(activityId);

                        return Task.CompletedTask;
                    }).Wait();

                    activitiesById[activityId] = activity;
                }
            }
        }

        public async Task DeleteAsync(ITurnContext turnContext, PayloadId payloadId, CancellationToken cancellationToken = default)
        {
            if (turnContext.GetIncomingButtonPayload() is JObject payload)
            {
                var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);
                var activityIdsByPayloadId = state.ActivityIdsByPayloadId;

                if (payloadId.Type == PayloadIdType.Batch)
                {
                    if (activityIdsByPayloadId.TryGetValue(payloadId.Id, out var activityIds))
                    {
                        foreach (var activityId in activityIds)
                        {
                            await DeleteActivityAsync(turnContext, activityId, cancellationToken).ConfigureAwait(false);
                        }

                        activityIdsByPayloadId.Remove(payloadId.Id);
                    }
                }
                else
                {
                    var matchResult = await GetPayloadMatchAsync(turnContext, cancellationToken).ConfigureAwait(false);
                    var savedAction = matchResult.FoundAction();
                    var savedAttachment = matchResult.FoundAttachment();
                    var savedActivity = matchResult.FoundActivity();

                    switch (payloadId.Type)
                    {
                        case PayloadIdType.Action:

                            break;
                        case PayloadIdType.Card:

                            if (savedAttachment != null)
                            {


                            }

                            break;

                        case PayloadIdType.Carousel:

                            if (savedActivity != null)
                            {
                                await DeleteActivityAsync(turnContext, savedActivity.Id, cancellationToken).ConfigureAwait(false);
                            }

                            break;
                    }
                }
            }
        }

        private async Task DeleteActivityAsync(ITurnContext turnContext, string activityId, CancellationToken cancellationToken)
        {
            var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);

            await turnContext.DeleteActivityAsync(activityId, cancellationToken).ConfigureAwait(false);

            state.ActivityById.Remove(activityId);

            foreach (var activityIdSet in state.ActivityIdsByPayloadId.Values)
            {
                activityIdSet.Remove(activityId);
            }
        }

        public async Task PreserveValuesAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (turnContext.GetIncomingButtonPayload() is JObject payload)
            {
                var matchResult = await GetPayloadMatchAsync(turnContext, cancellationToken).ConfigureAwait(false);

                if (matchResult.FoundAttachment() is Attachment savedAttachment
                    && matchResult.GetAttachmentParent(savedAttachment) is IMessageActivity savedActivity
                    && savedAttachment.ContentType.EqualsCI(CardConstants.AdaptiveCardContentType))
                {
                    savedAttachment.Content = savedAttachment.Content?.ToJObjectAndBackAsync(
                        card =>
                        {
                            // Iterate through all inputs in the card
                            foreach (var input in card.GetAdaptiveInputs())
                            {
                                var id = input.GetAdaptiveInputId();
                                var inputValue = payload.GetValueCI(id);

                                input.SetValueCI(CardConstants.KeyValue, inputValue);
                            }

                            return Task.CompletedTask;
                        }).Result;

                    try
                    {
                        // The changes to the attachment will already be reflected in the activity
                        await turnContext.UpdateActivityAsync(savedActivity).ConfigureAwait(false);
                    }
                    catch
                    {
                        // TODO: Find out what exceptions I need to handle
                        throw;
                    }
                }
            }
        }

        private async Task<PayloadMatchResult> GetPayloadMatchAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken = default)
        {
            var result = new PayloadMatchResult();
            var incomingPayload = turnContext.GetIncomingButtonPayload();

            if (incomingPayload is null)
            {
                return result;
            }

            var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);

            // Iterate over all saved activities
            foreach (var savedActivity in Helper.GetEnumValues<PayloadIdType>()
                .SelectMany(type => incomingPayload.GetIdFromPayload(type) is string payloadId
                    && state.ActivityIdsByPayloadId.TryGetValue(payloadId, out var activityIds)
                        ? activityIds
                        : new HashSet<string>())
                .Distinct()
                .Select(activityId => activityId != null
                    && state.ActivityById.TryGetValue(activityId, out var activity)
                        ? activity
                        : null)
                .Where(activity => activity?.Attachments?.Any() == true))
            {
                foreach (var savedAttachment in savedActivity.Attachments.WhereNotNull())
                {
                    if (savedAttachment.ContentType.EqualsCI(CardConstants.AdaptiveCardContentType)
                        && savedAttachment.Content.ToJObject() is JObject savedAdaptiveCard)
                    {
                        // For Adaptive Cards we need to check the inputs
                        var inputsMatch = true;
                        var payloadWithoutInputs = incomingPayload.DeepClone() as JObject;

                        foreach (var inputId in savedAdaptiveCard.GetAdaptiveInputs()
                            .Select(CardExtensions.GetAdaptiveInputId))
                        {
                            // If the Adaptive Card is poorly designed,
                            // the same input ID might show up multiple times.
                            // Therefore we're checking if the original payload
                            // contained the key, because it might still be valid
                            // even if the input was already removed.
                            if (incomingPayload.TryGetValue(inputId, StringComparison.OrdinalIgnoreCase, out _))
                            {
                                // Removing a property that doesn't exist
                                // will not throw an exception.
                                payloadWithoutInputs.Remove(inputId);
                            }
                            else
                            {
                                inputsMatch = false;

                                break;
                            }
                        }

                        if (inputsMatch)
                        {
                            CardTree.RecurseAsync(
                                savedAdaptiveCard,
                                (JObject savedSubmitAction) =>
                                {
                                    var submitActionData = savedSubmitAction.GetValueCI(
                                        CardConstants.KeyData)?.DeepClone();

                                    if (JToken.DeepEquals(submitActionData, payloadWithoutInputs))
                                    {
                                        result.Add(savedActivity, savedAttachment, savedSubmitAction);
                                    }

                                    return Task.CompletedTask;
                                },
                                TreeNodeType.AdaptiveCard,
                                TreeNodeType.SubmitAction).Wait();
                        }
                    }
                    else
                    {
                        // For Bot Framework cards that are not Adaptive Cards
                        CardTree.RecurseAsync(savedAttachment, (CardAction savedCardAction) =>
                        {
                            var savedPayload = savedCardAction?.Value.ToJObject(true);

                            if (JToken.DeepEquals(savedPayload, incomingPayload))
                            {
                                result.Add(savedActivity, savedAttachment, savedCardAction);
                            }

                            return Task.CompletedTask;
                        }).Wait();
                    }
                }
            }

            return result;
        }

        private async Task<CardManagerState> GetStateAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            return await StateAccessor.GetNotNullAsync(turnContext, () => new CardManagerState(), cancellationToken).ConfigureAwait(false);
        }
    }
}
