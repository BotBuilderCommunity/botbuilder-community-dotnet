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

        /// <summary>
        /// Gets the card manager state accessor.
        /// </summary>
        /// <value>
        /// Because there is only a getter, this is guaranteed to not be null.
        /// </value>
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

            await (trackEnabledIds ? ForgetIdAsync(turnContext, payloadId, cancellationToken) : TrackIdAsync(turnContext, payloadId, cancellationToken)).ConfigureAwait(false);
        }

        public async Task EnableIdAsync(ITurnContext turnContext, PayloadId payloadId, bool trackEnabledIds = true, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (payloadId is null)
            {
                throw new ArgumentNullException(nameof(payloadId));
            }

            await (trackEnabledIds ? TrackIdAsync(turnContext, payloadId, cancellationToken) : ForgetIdAsync(turnContext, payloadId, cancellationToken)).ConfigureAwait(false);
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

        public async Task ForgetIdAsync(ITurnContext turnContext, PayloadId payloadId, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (payloadId is null)
            {
                throw new ArgumentNullException(nameof(payloadId));
            }

            var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);

            if (state.PayloadIdsByType != null && state.PayloadIdsByType.TryGetValue(payloadId.Type, out var ids))
            {
                // Even though the dictionary will be a copy,
                // the set will be the same as the one in the original dictionary
                // and so calling Remove will modify the original set
                ids.Remove(payloadId.Id);
            }
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

                    CardTree.Recurse(activity, (PayloadId payloadId) =>
                    {
                        activityIdsByPayloadId.InitializeKey(payloadId.Id, new HashSet<string>()).Add(activityId);
                    });

                    activitiesById[activityId] = activity;
                }
            }
        }

        public async Task PreserveValuesAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            if (turnContext?.GetIncomingPayload() is JObject payload)
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

        public async Task DeleteAsync(ITurnContext turnContext, PayloadId toDelete, CancellationToken cancellationToken = default)
        {
            if (turnContext is null || toDelete is null)
            {
                return;
            }

            var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);
            var type = toDelete.Type;

            if (type == PayloadIdType.Batch)
            {
                if (state.ActivityIdsByPayloadId.TryGetValue(toDelete.Id, out var activityIds))
                {
                    // Iterate over a copy of the set so the original can be modified
                    foreach (var activityId in activityIds.ToList())
                    {
                        //await DeleteActivityAsync(turnContext, activityId, cancellationToken).ConfigureAwait(false);
                        await turnContext.DeleteActivityAsync(activityId, cancellationToken).ConfigureAwait(false);

                        if (state.ActivityById.TryGetValue(activityId, out var activity))
                        {
                            // Delete all payload ID's found in the batch from the card manager state
                            await CardTree.RecurseAsync(activity, async (PayloadId payloadId) =>
                            {
                                await ForgetIdAsync(turnContext, payloadId, cancellationToken);

                                state.ActivityIdsByPayloadId.Remove(payloadId.Id);

                            }).ConfigureAwait(false);

                            // Delete the activity from the card manager state
                            state.ActivityById.Remove(activityId);
                        }

                        // Delete the activity ID from the card manager state.
                        // One activity ID may be associated with many payload ID's.
                        foreach (var activityIdSet in state.ActivityIdsByPayloadId.Values)
                        {
                            activityIdSet.Remove(activityId);
                        }
                    }

                    // Delete the batch ID from the card manager state
                    state.ActivityIdsByPayloadId.Remove(toDelete.Id);
                }
            }
            else
            {
                var matchResult = await GetPayloadMatchAsync(turnContext, cancellationToken).ConfigureAwait(false);
                var savedAction = matchResult.FoundAction();
                var savedAttachment = matchResult.FoundAttachment();
                var savedActivity = matchResult.FoundActivity();

                if (type == PayloadIdType.Action && savedActivity != null && savedAttachment != null && savedAction != null)
                {
                    if (savedAttachment.ContentType.EqualsCI(CardConstants.AdaptiveCardContentType))
                    {
                        if (savedAction is JObject savedSubmitAction)
                        {
                            // Remove the submit action from the Adaptive Card
                            CardTree.Recurse(
                                savedAttachment,
                                (JObject submitAction) =>
                                {
                                    // This should only be true for one submit action in the attachment
                                    // because if there was more than one match then the found action would be null
                                    if (JToken.DeepEquals(submitAction, savedSubmitAction))
                                    {
                                        submitAction.Remove();
                                    }
                                },
                                TreeNodeType.Attachment,
                                TreeNodeType.SubmitAction);

                            // Check all ID's from the submit action to see if they need to be deleted from state
                            foreach (var payloadId in CardTree.GetIds(savedSubmitAction, TreeNodeType.SubmitAction))
                            {
                                switch (payloadId.Type)
                                {
                                    case PayloadIdType.Action:

                                        // Delete the action ID from the tracked ID set
                                        await ForgetIdAsync(turnContext, payloadId, cancellationToken).ConfigureAwait(false);

                                        // Delete associations between the action ID and activity ID's
                                        // (there shouldn't be more than one activity ID associated with an action ID)
                                        state.ActivityIdsByPayloadId.Remove(payloadId.Id);

                                        break;

                                    case PayloadIdType.Card:

                                        var idHasBeenOrphaned = true;

                                        // The submit action will already have been removed from the Adaptive Card,
                                        // so we're checking to see if the card has any other submit actions with this card ID
                                        foreach (var otherId in CardTree.GetIds(savedAttachment))
                                        {
                                            if (PayloadIdComparer.Instance.Equals(payloadId, otherId))
                                            {
                                                idHasBeenOrphaned = false;

                                                break;
                                            }
                                        }

                                        if (idHasBeenOrphaned)
                                        {
                                            // Delete the card ID from the tracked ID set
                                            await ForgetIdAsync(turnContext, payloadId, cancellationToken).ConfigureAwait(false);

                                            // Delete associations between the card ID and activity ID's
                                            // (there shouldn't be more than one activity ID associated with a card ID)
                                            state.ActivityIdsByPayloadId.Remove(payloadId.Id);
                                        }

                                        break;

                                    case PayloadIdType.Carousel:


                                        break;
                                    case PayloadIdType.Batch:


                                        break;
                                }
                            }

                            // Check if the Adaptive Card is now empty

                        }
                    }
                    else
                    {

                    }
                }

                if (type == PayloadIdType.Card && savedActivity != null && savedAttachment != null)
                {
                    savedActivity.Attachments.Remove(savedAttachment);

                    if (savedActivity.Attachments.Any() || !string.IsNullOrWhiteSpace(savedActivity.Text))
                    {
                        await turnContext.UpdateActivityAsync(savedActivity, cancellationToken).ConfigureAwait(false);

                        CardTree.Recurse(savedAttachment, (PayloadId payloadId) =>
                        {
                            if (payloadId.Type <= PayloadIdType.Card)
                            {
                                state.ActivityIdsByPayloadId.Remove(payloadId.Id);
                            }
                        });

                    }
                    else
                    {
                        // If the activity is now empty, execute the next if block as well
                        type = PayloadIdType.Carousel;
                    }
                }

                if (type == PayloadIdType.Carousel && savedActivity != null)
                {
                    await DeleteActivityAsync(turnContext, savedActivity.Id, cancellationToken).ConfigureAwait(false);

                }
            }

            // Check for orphaned payload ID's that are no longer contained in any activities


            var activityIdsAssociatedWithPayloads = state.ActivityIdsByPayloadId.Values.SelectMany(set => set).Distinct();

            // Check for orphaned activities that are no longer associated with any payload ID's.
            // The key set is copied so that the original key set can be modified while iterating.
            foreach (var activityId in state.ActivityById.Keys.ToList())
            {
                // If the activity ID isn't associated with any payload ID's
                // then the activity should be deleted from state
                if (!activityIdsAssociatedWithPayloads.Contains(activityId))
                {
                    state.ActivityById.Remove(activityId);
                }
            }
        }

        private async Task<PayloadMatchResult> GetPayloadMatchAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken = default)
        {
            var result = new PayloadMatchResult();
            var incomingPayload = turnContext.GetIncomingPayload();

            if (incomingPayload is null)
            {
                return result;
            }

            var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);

            // Iterate over all saved activities that contain any of the payload ID's from the incoming payload
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
                            if (incomingPayload.ContainsKeyCI(inputId))
                            {
                                // Removing a property that doesn't exist
                                // will not throw an exception.
                                payloadWithoutInputs.RemoveCI(inputId);
                            }
                            else
                            {
                                inputsMatch = false;

                                break;
                            }
                        }

                        if (inputsMatch)
                        {
                            CardTree.Recurse(
                                savedAdaptiveCard,
                                (JObject savedSubmitAction) =>
                                {
                                    var submitActionData = savedSubmitAction.GetValueCI(
                                        CardConstants.KeyData)?.DeepClone();

                                    if (JToken.DeepEquals(submitActionData, payloadWithoutInputs))
                                    {
                                        result.Add(savedActivity, savedAttachment, savedSubmitAction);
                                    }
                                },
                                TreeNodeType.AdaptiveCard,
                                TreeNodeType.SubmitAction);
                        }
                    }
                    else
                    {
                        // For Bot Framework cards that are not Adaptive Cards
                        CardTree.Recurse(savedAttachment, (CardAction savedCardAction) =>
                        {
                            var savedPayload = savedCardAction?.Value.ToJObject(true);

                            if (JToken.DeepEquals(savedPayload, incomingPayload))
                            {
                                result.Add(savedActivity, savedAttachment, savedCardAction);
                            }
                        });
                    }
                }
            }

            return result;
        }

        private async Task DeleteActivityAsync(ITurnContext turnContext, string activityId, CancellationToken cancellationToken)
        {
            var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);

            await turnContext.DeleteActivityAsync(activityId, cancellationToken).ConfigureAwait(false);

            if (state.ActivityById.TryGetValue(activityId, out var activity))
            {


                state.ActivityById.Remove(activityId);
            }


            // One activity ID may be associated with many payload ID's
            foreach (var activityIdSet in state.ActivityIdsByPayloadId.Values)
            {
                activityIdSet.Remove(activityId);
            }
        }

        private async Task<CardManagerState> GetStateAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            return await StateAccessor.GetNotNullAsync(turnContext, () => new CardManagerState(), cancellationToken).ConfigureAwait(false);
        }
    }
}
