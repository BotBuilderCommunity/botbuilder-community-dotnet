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
            StateAccessor = stateAccessor ?? throw new ArgumentNullException(nameof(stateAccessor));
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

        public async Task DisableIdAsync(ITurnContext turnContext, PayloadItem payloadId, bool trackEnabledIds = true, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (payloadId is null)
            {
                throw new ArgumentNullException(nameof(payloadId));
            }

            await (trackEnabledIds ? ForgetIdAsync(turnContext, payloadId, cancellationToken) : TrackIdAsync(turnContext, payloadId, cancellationToken)).ConfigureAwait(false);
        }

        public async Task EnableIdAsync(ITurnContext turnContext, PayloadItem payloadId, bool trackEnabledIds = true, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (payloadId is null)
            {
                throw new ArgumentNullException(nameof(payloadId));
            }

            await (trackEnabledIds ? TrackIdAsync(turnContext, payloadId, cancellationToken) : ForgetIdAsync(turnContext, payloadId, cancellationToken)).ConfigureAwait(false);
        }

        public async Task TrackIdAsync(ITurnContext turnContext, PayloadItem payloadId, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (payloadId is null)
            {
                throw new ArgumentNullException(nameof(payloadId));
            }

            var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);

            state.PayloadIdsByType.InitializeKey(payloadId.Path, new HashSet<string>()).Add(payloadId.Value);
        }

        public async Task ForgetIdAsync(ITurnContext turnContext, PayloadItem payloadId, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (payloadId is null)
            {
                throw new ArgumentNullException(nameof(payloadId));
            }

            var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);

            if (state.PayloadIdsByType.TryGetValue(payloadId.Path, out var ids))
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

        public async Task SaveActivitiesAsync(ITurnContext turnContext, IEnumerable<Activity> activities, CancellationToken cancellationToken = default)
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
            if (turnContext?.GetIncomingPayload() is JObject payload)
            {
                var matchResult = await GetPayloadMatchAsync(turnContext, cancellationToken).ConfigureAwait(false);

                if (matchResult.SavedActivity != null
                    && matchResult.SavedAttachment?.ContentType.EqualsCI(CardConstants.AdaptiveCardContentType) == true)
                {
                    matchResult.SavedAttachment.Content = matchResult.SavedAttachment.Content?.ToJObjectAndBackAsync(
                        card =>
                        {
                            // Iterate through all inputs in the card
                            foreach (var input in AdaptiveCardUtil.GetAdaptiveInputs(card))
                            {
                                var id = AdaptiveCardUtil.GetAdaptiveInputId(input);
                                var inputValue = payload.GetValue(id);

                                input.SetValue(CardConstants.KeyValue, inputValue);
                            }

                            return Task.CompletedTask;
                        }).Result;

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

        public async Task DeleteAsync(ITurnContext turnContext, PayloadItem toDelete, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (toDelete is null)
            {
                throw new ArgumentNullException(nameof(toDelete));
            }

            var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);
            var type = toDelete.Path;

            if (type == PayloadIdTypes.Batch)
            {
                // Iterate over a copy of the set so the original can be modified
                foreach (var activity in state.SavedActivities.ToList())
                {
                    var hasBatchId = false;

                    CardTree.Recurse(activity, (PayloadItem payloadId) =>
                    {
                        // Payload ID's are compared by value
                        if (payloadId == toDelete)
                        {
                            hasBatchId = true;
                        }
                    });

                    // Delete any activity that contains the specified batch ID
                    if (hasBatchId)
                    {
                        await DeleteActivityAsync(turnContext, activity, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                var matchResult = await GetPayloadMatchAsync(turnContext, cancellationToken).ConfigureAwait(false);
                var savedActivity = matchResult.SavedActivity;
                var savedAttachment = matchResult.SavedAttachment;
                var savedAction = matchResult.SavedAction;

                if (type == PayloadIdTypes.Action && savedActivity != null && savedAttachment != null && savedAction != null)
                {
                    if (savedAttachment.ContentType.EqualsCI(CardConstants.AdaptiveCardContentType))
                    {
                        // For Adaptive Cards
                        if (savedAction is JObject savedSubmitAction)
                        {
                            // Remove the submit action from the Adaptive Card.
                            // Safe remove will work whether the action is in an array
                            // or is the value of a select action property.
                            savedSubmitAction.SafeRemove();

                            // Check if the Adaptive Card is now empty
                            if (savedAttachment.Content.ToJObject() is JObject adaptiveCard
                                && adaptiveCard.GetValue(CardConstants.KeyBody).IsNullishOrEmpty()
                                && adaptiveCard.GetValue(CardConstants.KeyActions).IsNullishOrEmpty())
                            {
                                // If the card is now empty, execute the next if block to delete it
                                type = PayloadIdTypes.Card;
                            }
                            else
                            {
                                await turnContext.UpdateActivityAsync(savedActivity, cancellationToken).ConfigureAwait(false);
                            }
                        }
                    }
                    else
                    {
                        // For Bot Framework rich cards
                        if (savedAction is CardAction cardAction)
                        {
                            // Remove the card action from the card
                            CardTree.Recurse(
                                savedAttachment,
                                (IList<CardAction> actions) =>
                                {
                                    actions.Remove(cardAction);
                                });

                            // Check if the card is now empty.
                            // We are assuming that if a developer wants to make a rich card
                            // with only postBack/messageBack buttons then they will use a hero card
                            // and any other card would have more content than just postBack/messageBack buttons
                            // so only a hero card should potentially be empty at this point.
                            if (savedAttachment.Content is HeroCard heroCard
                                && !heroCard.Buttons.Any()
                                && heroCard.Images?.Any() != true
                                && string.IsNullOrEmpty(heroCard.Title)
                                && string.IsNullOrEmpty(heroCard.Subtitle)
                                && string.IsNullOrEmpty(heroCard.Text))
                            {
                                // If the card is now empty, execute the next if block to delete it
                                type = PayloadIdTypes.Card;
                            }
                            else
                            {
                                await turnContext.UpdateActivityAsync(savedActivity, cancellationToken).ConfigureAwait(false);
                            }
                        }
                    }
                }

                if (type == PayloadIdTypes.Card && savedActivity != null && savedAttachment != null)
                {
                    savedActivity.Attachments.Remove(savedAttachment);

                    // Check if the activity is now empty
                    if (string.IsNullOrWhiteSpace(savedActivity.Text) && !savedActivity.Attachments.Any())
                    {
                        // If the activity is now empty, execute the next if block to delete it
                        type = PayloadIdTypes.Carousel;
                    }
                    else
                    {
                        await turnContext.UpdateActivityAsync(savedActivity, cancellationToken).ConfigureAwait(false);
                    }
                }

                if (type == PayloadIdTypes.Carousel && savedActivity != null)
                {
                    await DeleteActivityAsync(turnContext, savedActivity, cancellationToken).ConfigureAwait(false);
                }
            }

            await CleanSavedActivitiesAsync(turnContext, cancellationToken).ConfigureAwait(false);
        }

        private async Task CleanSavedActivitiesAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);

            foreach (var savedActivity in state.SavedActivities)
            {
                var hasNoPayloadIds = true;

                CardTree.Recurse(savedActivity, (PayloadItem payloadId) =>
                {
                    hasNoPayloadIds = false;
                });

                if (hasNoPayloadIds)
                {
                    state.SavedActivities.Remove(savedActivity);
                }
            }
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

            // Iterate over all saved activities that contain any of the payload ID's from the incoming payload
            foreach (var savedActivity in state.SavedActivities
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
