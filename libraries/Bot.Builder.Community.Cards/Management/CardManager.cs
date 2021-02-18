﻿using System;
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

        public async Task<CardManagerState> GetStateAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            return await StateAccessor.GetNotNullAsync(turnContext, () => new CardManagerState(), cancellationToken).ConfigureAwait(false);
        }

        // --------------------
        // NON-UPDATING METHODS
        // --------------------

        public async Task EnableIdAsync(
            ITurnContext turnContext,
            DataId dataId,
            TrackingStyle style = TrackingStyle.TrackEnabled,
            CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (dataId is null)
            {
                throw new ArgumentNullException(nameof(dataId));
            }

            switch (style)
            {
                case TrackingStyle.TrackEnabled:
                    await TrackIdAsync(turnContext, dataId, cancellationToken).ConfigureAwait(false);
                    break;
                case TrackingStyle.TrackDisabled:
                    await ForgetIdAsync(turnContext, dataId, cancellationToken).ConfigureAwait(false);
                    break;
            }
        }

        public async Task DisableIdAsync(
            ITurnContext turnContext,
            DataId dataId,
            TrackingStyle style = TrackingStyle.TrackEnabled,
            CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (dataId is null)
            {
                throw new ArgumentNullException(nameof(dataId));
            }

            switch (style)
            {
                case TrackingStyle.TrackEnabled:
                    await ForgetIdAsync(turnContext, dataId, cancellationToken).ConfigureAwait(false);
                    break;
                case TrackingStyle.TrackDisabled:
                    await TrackIdAsync(turnContext, dataId, cancellationToken).ConfigureAwait(false);
                    break;
            }
        }

        public async Task TrackIdAsync(ITurnContext turnContext, DataId dataId, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (dataId is null)
            {
                throw new ArgumentNullException(nameof(dataId));
            }

            var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);

            state.DataIdsByScope.InitializeKey(dataId.Key, new HashSet<string>()).Add(dataId.Value);

            await StateAccessor.SetAsync(turnContext, state, cancellationToken).ConfigureAwait(false);
        }

        public async Task ForgetIdAsync(ITurnContext turnContext, DataId dataId, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (dataId is null)
            {
                throw new ArgumentNullException(nameof(dataId));
            }

            var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);

            if (state.DataIdsByScope.TryGetValue(dataId.Key, out var ids))
            {
                ids?.Remove(dataId.Value);
            }

            await StateAccessor.SetAsync(turnContext, state, cancellationToken).ConfigureAwait(false);
        }

        public async Task ClearTrackedIdsAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);

            state.DataIdsByScope.Clear();

            await StateAccessor.SetAsync(turnContext, state, cancellationToken).ConfigureAwait(false);
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

            foreach (var activity in activities)
            {
                if (activity.Id != null)
                {
                    // Remove any activities with a matching ID so that a duplicate isn't saved when updating
                    await UnsaveActivityAsync(turnContext, activity.Id, cancellationToken).ConfigureAwait(false);
                }

                if (CardTree.GetIds(activity, TreeNodeType.Activity).Any())
                {
                    state.SavedActivities.Add(activity);
                }
            }

            await StateAccessor.SetAsync(turnContext, state, cancellationToken).ConfigureAwait(false);
        }

        public async Task UnsaveActivityAsync(ITurnContext turnContext, string activityId, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (activityId is null)
            {
                throw new ArgumentNullException(nameof(activityId));
            }

            var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);
            var activitiesWithMatchingId = state.SavedActivities.Where(activity => activity.Id == activityId).ToArray();

            // We used ToArray because the WhereEnumerableIterator must be copied
            // or else it would throw an exception here
            state.SavedActivities.ExceptWith(activitiesWithMatchingId);

            await StateAccessor.SetAsync(turnContext, state, cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteActionSourceAsync(ITurnContext turnContext, string dataIdScope, CancellationToken cancellationToken = default)
        {
            // TODO: Provide a way to delete elements by specifying an ID that's not in the incoming action data

            BotAssert.ContextNotNull(turnContext);

            if (string.IsNullOrEmpty(dataIdScope))
            {
                throw new ArgumentNullException(nameof(dataIdScope));
            }

            var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);

            if (dataIdScope == DataIdScopes.Batch)
            {
                if (turnContext.GetIncomingActionData().ToJObject().GetIdFromActionData(DataIdScopes.Batch) is string batchId)
                {
                    var toDelete = new DataId(DataIdScopes.Batch, batchId);

                    // Iterate over a copy of the set so the original can be modified
                    foreach (var activity in state.SavedActivities.ToList())
                    {
                        // Delete any activity that contains the specified batch ID (data items are compared by value)
                        if (CardTree.GetIds(activity, TreeNodeType.Activity).Any(toDelete.Equals))
                        {
                            await DeleteActivityAsync(turnContext, activity, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
            }
            else
            {
                var matchResult = await GetDataMatchAsync(turnContext, cancellationToken).ConfigureAwait(false);
                var matchedActivity = matchResult.SavedActivity;
                var matchedAttachment = matchResult.SavedAttachment;
                var matchedAction = matchResult.SavedAction;
                var shouldUpdateActivity = false;

                // TODO: Provide options for how to determine emptiness when cascading deletion
                // (e.g. when a card has no more actions rather than only when the card is completely empty)

                if (dataIdScope == DataIdScopes.Action && matchedActivity != null && matchedAttachment != null && matchedAction != null)
                {
                    if (matchedAttachment.ContentType.EqualsCI(ContentTypes.AdaptiveCard))
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
                            if (adaptiveCard.GetValue(AdaptiveProperties.Body).IsNullishOrEmpty()
                                && adaptiveCard.GetValue(AdaptiveProperties.Actions).IsNullishOrEmpty())
                            {
                                // If the card is now empty, execute the next if block to delete it
                                dataIdScope = DataIdScopes.Card;
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
                                },
                                TreeNodeType.Attachment,
                                TreeNodeType.CardActionList);

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
                                dataIdScope = DataIdScopes.Card;
                            }
                        }
                    }
                }

                if (dataIdScope == DataIdScopes.Card && matchedActivity != null && matchedAttachment != null)
                {
                    matchedActivity.Attachments.Remove(matchedAttachment);

                    shouldUpdateActivity = true;

                    // Check if the activity is now empty
                    if (string.IsNullOrWhiteSpace(matchedActivity.Text) && !matchedActivity.Attachments.Any())
                    {
                        // If the activity is now empty, execute the next if block to delete it
                        dataIdScope = DataIdScopes.Carousel;
                    }
                }

                if (dataIdScope == DataIdScopes.Carousel && matchedActivity != null)
                {
                    await DeleteActivityAsync(turnContext, matchedActivity, cancellationToken).ConfigureAwait(false);
                }
                else if (shouldUpdateActivity)
                {
                    await UpdateActivityAsync(turnContext, matchedActivity, cancellationToken).ConfigureAwait(false);
                }
            }

            await StateAccessor.SetAsync(turnContext, state, cancellationToken).ConfigureAwait(false);
        }

        private async Task UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            var ignoreUpdate = turnContext.TurnState.Get<CardManagerTurnState>()?.MiddlewareIgnoreUpdate;

            ignoreUpdate?.Add(activity);

            try
            {
                await turnContext.UpdateActivityAsync(activity, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // TODO: Find out what exceptions I need to handle
                throw;
            }
            finally
            {
                ignoreUpdate?.Remove(activity);
            }

            if (!CardTree.GetIds(activity, TreeNodeType.Activity).Any())
            {
                await RemoveActivityAsync(turnContext, activity, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task DeleteActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            var ignoreDelete = turnContext.TurnState.Get<CardManagerTurnState>()?.MiddlewareIgnoreDelete;

            ignoreDelete?.Add(activity.Id);

            try
            {
                await turnContext.DeleteActivityAsync(activity.Id, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // TODO: Find out what exceptions I need to handle
                throw;
            }
            finally
            {
                ignoreDelete?.Remove(activity.Id);
            }

            await RemoveActivityAsync(turnContext, activity, cancellationToken).ConfigureAwait(false);
        }

        private async Task RemoveActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);

            state.SavedActivities.Remove(activity);

            await StateAccessor.SetAsync(turnContext, state, cancellationToken).ConfigureAwait(false);
        }

        private async Task<DataMatchResult> GetDataMatchAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken = default)
        {
            var result = new DataMatchResult();

            if (!(turnContext.GetIncomingActionData() is JObject incomingData))
            {
                return result;
            }

            var state = await GetStateAsync(turnContext, cancellationToken).ConfigureAwait(false);
            var couldBeFromAdaptiveCard = turnContext.Activity.Value.ToJObject() is JObject;

            // Iterate over all saved activities that contain any of the action data ID's from the incoming action data
            foreach (var savedActivity in state.SavedActivities
                .Where(activity => activity?.Attachments?.Any() == true))
            {
                foreach (var savedAttachment in savedActivity.Attachments.WhereNotNull())
                {
                    if (savedAttachment.ContentType.EqualsCI(ContentTypes.AdaptiveCard))
                    {
                        if (couldBeFromAdaptiveCard && savedAttachment.Content.ToJObject() is JObject savedAdaptiveCard)
                        {
                            // For Adaptive Cards we need to check the inputs
                            var inputsMatch = true;
                            var dataWithoutInputValues = incomingData.DeepClone() as JObject;

                            // First, determine what matching submit action data is expected to look like
                            // by taking the incoming data and removing the values associated with
                            // the inputs found in the Adaptive Card
                            foreach (var inputId in AdaptiveCardUtil.GetAdaptiveInputs(savedAdaptiveCard)
                                .Select(AdaptiveCardUtil.GetAdaptiveInputId))
                            {
                                // If the Adaptive Card is poorly designed,
                                // the same input ID might show up multiple times.
                                // Therefore we're checking if the original incoming data
                                // contained the ID, because the inputs might still
                                // match even if this input was already removed.
                                if (incomingData.ContainsKey(inputId))
                                {
                                    // Removing a property that doesn't exist
                                    // will not throw an exception
                                    dataWithoutInputValues.Remove(inputId);
                                }
                                else
                                {
                                    inputsMatch = false;

                                    break;
                                }
                            }

                            // Second, if all the input ID's found in the card were present in the incoming data
                            // then check each submit action in the card to see if its data matches the incoming data
                            if (inputsMatch)
                            {
                                CardTree.Recurse(
                                    savedAdaptiveCard,
                                    (JObject savedSubmitAction) =>
                                    {
                                        var submitActionData = savedSubmitAction.GetValue(
                                            AdaptiveProperties.Data) ?? new JObject();

                                        if (JToken.DeepEquals(submitActionData, dataWithoutInputValues))
                                        {
                                            result.Add(savedActivity, savedAttachment, savedSubmitAction);
                                        }
                                    },
                                    TreeNodeType.AdaptiveCard,
                                    TreeNodeType.SubmitAction);
                            }
                        }
                    }
                    else
                    {
                        // For Bot Framework cards that are not Adaptive Cards
                        CardTree.Recurse(
                            savedAttachment,
                            (CardAction savedCardAction) =>
                            {
                                var savedData = savedCardAction.Value.ToJObject(true);

                                // This will not throw an exception if the saved action data is null
                                if (JToken.DeepEquals(savedData, incomingData))
                                {
                                    result.Add(savedActivity, savedAttachment, savedCardAction);
                                }
                            },
                            TreeNodeType.Attachment,
                            TreeNodeType.CardAction);
                    }
                }
            }

            return result;
        }
    }
}
