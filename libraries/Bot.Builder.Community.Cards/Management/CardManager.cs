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
    public class CardManager
    {
        public CardManager(BotState botState)
            : this(botState?.CreateProperty<CardManagerState>(nameof(CardManagerState)) ?? throw new ArgumentNullException(nameof(botState)))
        {
        }

        private CardManager(IStatePropertyAccessor<CardManagerState> stateAccessor)
        {
            StateAccessor = stateAccessor;
        }

        /// <summary>
        /// Gets the card manager state accessor.
        /// </summary>
        /// <value>
        /// Because there is only a getter, this is guaranteed to not be null.
        /// </value>
        public IStatePropertyAccessor<CardManagerState> StateAccessor { get; }

        public static CardManager Create(IStatePropertyAccessor<CardManagerState> stateAccessor)
        {
            return new CardManager(stateAccessor ?? throw new ArgumentNullException(nameof(stateAccessor)));
        }

        // --------------------
        // NON-UPDATING METHODS
        // --------------------

        public async Task EnableIdAsync(ITurnContext turnContext, PayloadItem payloadId, bool trackEnabledIds = true, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (payloadId is null)
            {
                throw new ArgumentNullException(nameof(payloadId));
            }

            await (trackEnabledIds ? TrackIdAsync(turnContext, payloadId, cancellationToken) : ForgetIdAsync(turnContext, payloadId, cancellationToken)).ConfigureAwait(false);
        }

        public async Task DisableIdAsync(ITurnContext turnContext, PayloadItem payloadId, bool trackEnabledIds = true, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (payloadId is null)
            {
                throw new ArgumentNullException(nameof(payloadId));
            }

            await (trackEnabledIds ? ForgetIdAsync(turnContext, payloadId, cancellationToken) : TrackIdAsync(turnContext, payloadId, cancellationToken)).ConfigureAwait(false);
        }

        public async Task TrackIdAsync(ITurnContext turnContext, PayloadItem payloadId, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (payloadId is null)
            {
                throw new ArgumentNullException(nameof(payloadId));
            }

            var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);

            state.PayloadIdsByType.InitializeKey(payloadId.Key, new HashSet<string>()).Add(payloadId.Value);
        }

        public async Task ForgetIdAsync(ITurnContext turnContext, PayloadItem payloadId, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (payloadId is null)
            {
                throw new ArgumentNullException(nameof(payloadId));
            }

            var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);

            if (state.PayloadIdsByType.TryGetValue(payloadId.Key, out var ids))
            {
                ids?.Remove(payloadId.Value);
            }
        }

        public async Task ClearTrackedIdsAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);

            state.PayloadIdsByType.Clear();
        }

        // ----------------
        // UPDATING METHODS
        // ----------------

        public async Task SaveActivitiesAsync(ITurnContext turnContext, IEnumerable<IMessageActivity> activities, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (activities is null)
            {
                throw new ArgumentNullException(nameof(activities));
            }

            var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);

            state.SavedActivities.UnionWith(activities);

            await CleanSavedActivitiesAsync(turnContext, cancellationToken).ConfigureAwait(false);
        }

        public async Task PreserveValuesAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (turnContext.GetIncomingPayload() is JObject payload)
            {
                var matchResult = await GetPayloadMatchAsync(turnContext, cancellationToken).ConfigureAwait(false);

                if (matchResult.SavedActivity != null
                    && matchResult.SavedAttachment?.ContentType.EqualsCI(CardConstants.AdaptiveCardContentType) == true)
                {
                    var changed = false;

                    // The content must be non-null or else the attachment couldn't have been a match
                    matchResult.SavedAttachment.Content = matchResult.SavedAttachment.Content.ToJObjectAndBack(
                        card =>
                        {
                            // Iterate through all inputs in the card
                            foreach (var input in AdaptiveCardUtil.GetAdaptiveInputs(card))
                            {
                                var id = AdaptiveCardUtil.GetAdaptiveInputId(input);
                                var inputValue = payload.GetValue(id);

                                input.SetValue(CardConstants.KeyValue, inputValue);

                                changed = true;
                            }
                        });

                    if (changed)
                    {
                        try
                        {
                            // The changes to the attachment will already be reflected in the activity
                            await turnContext.UpdateActivityAsync(matchResult.SavedActivity).ConfigureAwait(false);
                        }
                        catch
                        {
                            // TODO: Find out what exceptions I need to handle
                            throw;
                        }
                    }
                }
            }
        }

        public async Task DeleteAsync(ITurnContext turnContext, string payloadIdType, CancellationToken cancellationToken = default)
        {
            // TODO: Provide a way to delete elements by specifying an ID that's not in the incoming payload

            BotAssert.ContextNotNull(turnContext);

            if (string.IsNullOrEmpty(payloadIdType))
            {
                throw new ArgumentNullException(nameof(payloadIdType));
            }

            var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);

            if (payloadIdType == PayloadIdTypes.Batch)
            {
                if (turnContext.GetIncomingPayload().ToJObject().GetIdFromPayload(PayloadIdTypes.Batch) is string batchId)
                {
                    var toDelete = new PayloadItem(PayloadIdTypes.Batch, batchId);

                    // Iterate over a copy of the set so the original can be modified
                    foreach (var activity in state.SavedActivities.ToList())
                    {
                        // Delete any activity that contains the specified batch ID (payload items are compared by value)
                        if (CardTree.GetIds(activity).Any(toDelete.Equals))
                        {
                            await DeleteActivityAsync(turnContext, activity, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
            }
            else
            {
                var matchResult = await GetPayloadMatchAsync(turnContext, cancellationToken).ConfigureAwait(false);
                var matchedActivity = matchResult.SavedActivity;
                var matchedAttachment = matchResult.SavedAttachment;
                var matchedAction = matchResult.SavedAction;
                var shouldUpdateActivity = false;

                // TODO: Provide options for how to determine emptiness when cascading deletion
                // (e.g. when a card has no more actions rather than only when the card is completely empty)

                if (payloadIdType == PayloadIdTypes.Action && matchedActivity != null && matchedAttachment != null && matchedAction != null)
                {
                    if (matchedAttachment.ContentType.EqualsCI(CardConstants.AdaptiveCardContentType))
                    {
                        // For Adaptive Cards
                        if (matchedAction is JObject savedSubmitAction)
                        {
                            var adaptiveCard = (JObject)savedSubmitAction.Root;

                            // Remove the submit action from the Adaptive Card.
                            // SafeRemove will work whether the action is in an array
                            // or is the value of a select action property.
                            savedSubmitAction.SafeRemove();

                            matchedAttachment.Content = matchedAttachment.Content.FromJObject(adaptiveCard);
                            shouldUpdateActivity = true;

                            // Check if the Adaptive Card is now empty
                            if (adaptiveCard.GetValue(CardConstants.KeyBody).IsNullishOrEmpty()
                                && adaptiveCard.GetValue(CardConstants.KeyActions).IsNullishOrEmpty())
                            {
                                // If the card is now empty, execute the next if block to delete it
                                payloadIdType = PayloadIdTypes.Card;
                            }
                        }
                    }
                    else
                    {
                        // For Bot Framework rich cards
                        if (matchedAction is CardAction cardAction)
                        {
                            // Remove the card action from the card
                            CardTree.Recurse(
                                matchedAttachment,
                                (IList<CardAction> actions) =>
                                {
                                    actions.Remove(cardAction);
                                });

                            shouldUpdateActivity = true;

                            // Check if the card is now empty.
                            // We are assuming that if a developer wants to make a rich card
                            // with only postBack/messageBack buttons then they will use a hero card
                            // and any other card would have more content than just postBack/messageBack buttons
                            // so only a hero card should potentially be empty at this point.
                            // We aren't checking if Buttons is null because it can't be at this point.
                            if (matchedAttachment.Content is HeroCard heroCard
                                && !heroCard.Buttons.Any()
                                && heroCard.Images?.Any() != true
                                && string.IsNullOrWhiteSpace(heroCard.Title)
                                && string.IsNullOrWhiteSpace(heroCard.Subtitle)
                                && string.IsNullOrWhiteSpace(heroCard.Text))
                            {
                                // If the card is now empty, execute the next if block to delete it
                                payloadIdType = PayloadIdTypes.Card;
                            }
                        }
                    }
                }

                if (payloadIdType == PayloadIdTypes.Card && matchedActivity != null && matchedAttachment != null)
                {
                    matchedActivity.Attachments.Remove(matchedAttachment);

                    shouldUpdateActivity = true;

                    // Check if the activity is now empty
                    if (string.IsNullOrWhiteSpace(matchedActivity.Text) && !matchedActivity.Attachments.Any())
                    {
                        // If the activity is now empty, execute the next if block to delete it
                        payloadIdType = PayloadIdTypes.Carousel;
                    }
                }

                if (payloadIdType == PayloadIdTypes.Carousel && matchedActivity != null)
                {
                    await DeleteActivityAsync(turnContext, matchedActivity, cancellationToken).ConfigureAwait(false);
                }
                else if (shouldUpdateActivity)
                {
                    await turnContext.UpdateActivityAsync(matchedActivity, cancellationToken).ConfigureAwait(false);
                }
            }

            await CleanSavedActivitiesAsync(turnContext, cancellationToken).ConfigureAwait(false);
        }

        private async Task CleanSavedActivitiesAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);
            var toRemove = new List<IMessageActivity>();

            foreach (var savedActivity in state.SavedActivities)
            {
                if (!CardTree.GetIds(savedActivity).Any())
                {
                    toRemove.Add(savedActivity);
                }
            }

            state.SavedActivities.ExceptWith(toRemove);
        }

        private async Task DeleteActivityAsync(ITurnContext turnContext, IMessageActivity activity, CancellationToken cancellationToken)
        {
            await turnContext.DeleteActivityAsync(activity.Id, cancellationToken).ConfigureAwait(false);

            var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);

            state.SavedActivities.Remove(activity);
        }

        private async Task<PayloadMatchResult> GetPayloadMatchAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken = default)
        {
            var result = new PayloadMatchResult();

            if (!(turnContext.GetIncomingPayload() is JObject incomingPayload))
            {
                return result;
            }

            var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);
            var couldBeFromAdaptiveCard = turnContext.Activity.Value.ToJObject() is JObject;

            // Iterate over all saved activities that contain any of the payload ID's from the incoming payload
            foreach (var savedActivity in state.SavedActivities
                .Where(activity => activity?.Attachments?.Any() == true))
            {
                foreach (var savedAttachment in savedActivity.Attachments.WhereNotNull())
                {
                    if (savedAttachment.ContentType.EqualsCI(CardConstants.AdaptiveCardContentType))
                    {
                        if (couldBeFromAdaptiveCard && savedAttachment.Content.ToJObject() is JObject savedAdaptiveCard)
                        {
                            // For Adaptive Cards we need to check the inputs
                            var inputsMatch = true;
                            var payloadWithoutInputs = incomingPayload.DeepClone() as JObject;

                            // First, determine what matching submit action data is expected to look like
                            // by removing the input ID's in the Adaptive Card from the payload
                            // that would have been added to the data to form the payload
                            foreach (var inputId in AdaptiveCardUtil.GetAdaptiveInputs(savedAdaptiveCard)
                                .Select(AdaptiveCardUtil.GetAdaptiveInputId))
                            {
                                // If the Adaptive Card is poorly designed,
                                // the same input ID might show up multiple times.
                                // Therefore we're checking if the original payload
                                // contained the key, because the inputs might still
                                // match even if this input was already removed.
                                if (incomingPayload.ContainsKey(inputId))
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

                            // Second, if all the input ID's found in the card were present in the payload
                            // then check each submit action in the card to see if the data matches the payload
                            if (inputsMatch)
                            {
                                CardTree.Recurse(
                                    savedAdaptiveCard,
                                    (JObject savedSubmitAction) =>
                                    {
                                        var submitActionData = savedSubmitAction.GetValue(
                                            CardConstants.KeyData) ?? new JObject();

                                        if (JToken.DeepEquals(submitActionData, payloadWithoutInputs))
                                        {
                                            result.Add(savedActivity, savedAttachment, savedSubmitAction);
                                        }
                                    },
                                    TreeNodeType.AdaptiveCard,
                                    TreeNodeType.SubmitAction,
                                    true);
                            } 
                        }
                    }
                    else
                    {
                        // For Bot Framework cards that are not Adaptive Cards
                        CardTree.Recurse(savedAttachment, (CardAction savedCardAction) =>
                        {
                            var savedPayload = savedCardAction.Value.ToJObject(true);

                            // This will not throw an exception if the saved payload is null
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

        private async Task<CardManagerState> GetStateAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            return await StateAccessor.GetNotNullAsync(turnContext, () => new CardManagerState(), cancellationToken).ConfigureAwait(false);
        }
    }
}
