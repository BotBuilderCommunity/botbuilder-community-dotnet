using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Cards.Nodes;
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

            var state = await StateAccessor.GetNotNullAsync(turnContext, () => new CardManagerState(), cancellationToken).ConfigureAwait(false);

            state.PayloadIdsByType.InitializeKey(payloadId.Type, new HashSet<string>()).Add(payloadId.Id);
        }

        public async Task ForgetIdAsync(ITurnContext turnContext, string id, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (id is null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var state = await StateAccessor.GetAsync(turnContext, () => new CardManagerState(), cancellationToken).ConfigureAwait(false);

            state?.PayloadIdsByType?.Values.WhereNotNull().ToList().ForEach(list => list.Remove(id));
        }

        public async Task ClearTrackedIdsAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            var state = await StateAccessor.GetNotNullAsync(turnContext, () => new CardManagerState(), cancellationToken).ConfigureAwait(false);

            state.PayloadIdsByType.Clear();
        }

        // ----------------
        // UPDATING METHODS
        // ----------------

        public async Task SaveActivities(ITurnContext turnContext, IEnumerable<Activity> activities)
        {
            var state = await StateAccessor.GetNotNullAsync(turnContext, () => new CardManagerState()).ConfigureAwait(false);
            var activityIdsByPayloadId = state.ActivityIdsByPayloadId;
            var activitiesById = state.ActivitiesById;

            foreach (var activity in activities)
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

        public async Task DeleteAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await turnContext.SendActivityAsync("Not implemented", cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public async Task PreserveValuesAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            var incomingActivity = turnContext?.Activity;

            if (incomingActivity?.Value is object value)
            {
                await value.ToJObjectAndBackAsync(
                    async payload =>
                    {
                        var matchResult = await GetPayloadMatchAsync(turnContext, cancellationToken).ConfigureAwait(false);

                        if (matchResult.FoundActivity() is IMessageActivity savedActivity
                            && matchResult.FoundAttachment() is Attachment savedAttachment
                            && savedAttachment.ContentType.Equals(CardConstants.AdaptiveCardContentType, StringComparison.OrdinalIgnoreCase))
                        {
                            savedAttachment.Content = savedAttachment.Content?.ToJObjectAndBackAsync(
                                card =>
                                {
                                    // Iterate through all inputs in the card
                                    foreach (var input in card.GetAdaptiveInputs())
                                    {
                                        var id = input.GetAdaptiveInputId();
                                        var inputValue = payload.GetValue(id, StringComparison.OrdinalIgnoreCase);

                                        input.SetValue(CardConstants.KeyValue, inputValue);
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
                    }, true).ConfigureAwait(false);
            }
        }

        private async Task<PayloadMatchResult> GetPayloadMatchAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            var incomingActivity = turnContext?.Activity;
            var result = new PayloadMatchResult();

            if (incomingActivity?.Value is object incomingValue)
            {
                await incomingValue.ToJObjectAndBackAsync(
                    async incomingPayload =>
                    {
                        var state = await StateAccessor.GetNotNullAsync(turnContext, () => new CardManagerState(), cancellationToken).ConfigureAwait(false);

                        // Iterate over all saved activities
                        foreach (var savedActivity in Helper.GetEnumValues<PayloadIdType>()
                            .SelectMany(type => incomingPayload.GetIdFromPayload(type) is string payloadId
                                && state.ActivityIdsByPayloadId.TryGetValue(payloadId, out var activityIds)
                                    ? activityIds
                                    : new HashSet<string>())
                            .Distinct()
                            .Select(activityId => activityId != null
                                && state.ActivitiesById.TryGetValue(activityId, out var activity)
                                    ? activity
                                    : null)
                            .Where(activity => activity?.Attachments?.Any() == true))
                        {
                            foreach (var savedAttachment in savedActivity.Attachments.WhereNotNull())
                            {
                                if (savedAttachment.ContentType.Equals(CardConstants.AdaptiveCardContentType, StringComparison.OrdinalIgnoreCase))
                                {
                                    savedAttachment.Content.ToJObjectAndBackAsync(
                                        adaptiveCard =>
                                        {
                                            // For Adaptive Cards we need to check the inputs
                                            var inputsMatch = true;
                                            var payloadWithoutInputs = incomingPayload.DeepClone() as JObject;

                                            foreach (var inputId in adaptiveCard.GetAdaptiveInputs()
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
                                                    adaptiveCard,
                                                    (JObject submitAction) =>
                                                    {
                                                        var submitActionData = submitAction?.GetValue(
                                                            CardConstants.KeyData,
                                                            StringComparison.OrdinalIgnoreCase)?.DeepClone();

                                                        if (JToken.DeepEquals(submitActionData, payloadWithoutInputs))
                                                        {
                                                            result.Add(savedActivity, savedAttachment, submitAction);
                                                        }

                                                        return Task.CompletedTask;
                                                    },
                                                    NodeType.AdaptiveCard,
                                                    NodeType.SubmitAction).Wait();
                                            }

                                            return Task.CompletedTask;
                                        }).Wait();
                                }
                                else
                                {
                                    // For Bot Framework cards that are not Adaptive Cards
                                    CardTree.RecurseAsync(savedAttachment, (CardAction savedAction) =>
                                    {
                                        savedAction?.Value.ToJObjectAndBackAsync(
                                            savedPayload =>
                                            {
                                                if (JToken.DeepEquals(savedPayload, incomingPayload))
                                                {
                                                    result.Add(savedActivity, savedAttachment, savedAction);
                                                }

                                                return Task.CompletedTask;
                                            }, true).Wait();

                                        return Task.CompletedTask;
                                    }).Wait();
                                }
                            }
                        }
                    }, true).ConfigureAwait(false);
            }

            return result;
        }
    }
}
