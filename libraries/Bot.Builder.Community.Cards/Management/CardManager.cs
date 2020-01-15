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

        public async Task DisableIdAsync(ITurnContext turnContext, TypedId typedId, bool trackEnabledIds = true, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (typedId is null)
            {
                throw new ArgumentNullException(nameof(typedId));
            }

            await (trackEnabledIds ? ForgetIdAsync(turnContext, typedId.Id, cancellationToken) : TrackIdAsync(turnContext, typedId, cancellationToken)).ConfigureAwait(false);
        }

        public async Task EnableIdAsync(ITurnContext turnContext, TypedId typedId, bool trackEnabledIds = true, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (typedId is null)
            {
                throw new ArgumentNullException(nameof(typedId));
            }

            await (trackEnabledIds ? TrackIdAsync(turnContext, typedId, cancellationToken) : ForgetIdAsync(turnContext, typedId.Id, cancellationToken)).ConfigureAwait(false);
        }

        public async Task TrackIdAsync(ITurnContext turnContext, TypedId typedId, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (typedId is null)
            {
                throw new ArgumentNullException(nameof(typedId));
            }

            var state = await StateAccessor.GetNotNullAsync(turnContext, () => new CardManagerState(), cancellationToken).ConfigureAwait(false);

            state.TrackedIdsByType.InitializeKey(typedId.Type, new HashSet<string>()).Add(typedId.Id);
        }

        public async Task ForgetIdAsync(ITurnContext turnContext, string id, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (id is null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var state = await StateAccessor.GetAsync(turnContext, () => new CardManagerState(), cancellationToken).ConfigureAwait(false);

            state?.TrackedIdsByType?.Values.Where(list => list != null).ToList().ForEach(list => list.Remove(id));
        }

        public async Task ClearTrackedIdsAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            var state = await StateAccessor.GetNotNullAsync(turnContext, () => new CardManagerState(), cancellationToken).ConfigureAwait(false);

            state.TrackedIdsByType.Clear();
        }

        // ----------------
        // UPDATING METHODS
        // ----------------

        public async Task SaveActivities(ITurnContext turnContext, IEnumerable<Activity> activities)
        {
            var state = await StateAccessor.GetNotNullAsync(turnContext, () => new CardManagerState()).ConfigureAwait(false);
            var activitiesById = state.ActivitiesById;

            foreach (var activity in activities)
            {
                var storedActivity = activity.ToStoredActivity();

                CardTree.RecurseAsync(activity, (TypedId typedId) =>
                {
                    activitiesById.InitializeKey(typedId.Id, new HashSet<Activity>()).Add(storedActivity);

                    return Task.CompletedTask;
                }).Wait();
            }
        }

        public async Task DeleteAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await turnContext.SendActivityAsync("Not implemented", cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public async Task PreserveValuesAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            var inputTypes = new[] { "Input.Text", "Input.Number", "Input.Date", "Input.Time", "Input.Toggle", "Input.ChoiceSet" };
            var incomingActivity = turnContext?.Activity;

            if (incomingActivity?.Value is object value)
            {
                await value.ToJObjectAndBackAsync(
                    async payload =>
                    {
                        var state = await StateAccessor.GetNotNullAsync(turnContext, () => new CardManagerState(), cancellationToken).ConfigureAwait(false);

                        foreach (var savedActivities in Helper.GetEnumValues<IdType>()
                            .Select(type => payload.GetIdFromPayload(type))
                            .Where(id => id != null && state.ActivitiesById.ContainsKey(id))
                            .Select(id => state.ActivitiesById[id])
                            .Where(savedActivities => savedActivities != null))
                        {
                            foreach (var savedActivity in savedActivities)
                            {
                                if (savedActivity is Activity activity && activity.Attachments?.Any() == true)
                                {
                                    foreach (var attachment in activity.Attachments)
                                    {
                                        if (attachment.ContentType == CardConstants.AdaptiveCardContentType && attachment.Content != null)
                                        {
                                            attachment.Content = attachment.Content?.ToJObjectAndBackAsync(
                                                card =>
                                                {
                                                    // Iterate through all inputs in the card
                                                    foreach (var input in card.NonDataDescendants()
                                                            .Select(token => token as JObject)
                                                            .Where(element => inputTypes.Contains((element?[CardConstants.KeyType] as JProperty)?.Value?.ToString())
                                                                && element[CardConstants.KeyId] != null))
                                                    {
                                                        var id = input[CardConstants.KeyId].ToString();

                                                        if (payload.ContainsKey(id))
                                                        {
                                                            input[CardConstants.KeyValue] = payload[id]; 
                                                        }
                                                    }

                                                    return Task.CompletedTask;
                                                }).Result;

                                            var update = MessageFactory.Attachment(attachment);

                                            update.Conversation = incomingActivity.Conversation;
                                            update.Id = savedActivity.Id;

                                            try
                                            {
                                                await turnContext.UpdateActivityAsync(update).ConfigureAwait(false);
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

                            break;
                        }
                    }, true).ConfigureAwait(false);
            }
        }
    }
}
