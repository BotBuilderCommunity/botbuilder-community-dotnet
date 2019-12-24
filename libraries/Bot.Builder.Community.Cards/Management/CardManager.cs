using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace Bot.Builder.Community.Cards.Management
{
    public class CardManager<TState>
        where TState : BotState
    {
        public CardManager(TState botState, bool trackEnabledIds = true)
            : this(botState?.CreateProperty<CardManagerState>(nameof(CardManagerState)) ?? throw new ArgumentNullException(nameof(botState)), trackEnabledIds)
        {
        }

        public CardManager(IStatePropertyAccessor<CardManagerState> stateAccessor, bool trackEnabledIds = true)
        {
            StateAccessor = stateAccessor ?? throw new ArgumentNullException(nameof(stateAccessor));
            TrackEnabledIds = trackEnabledIds;
        }

        public IStatePropertyAccessor<CardManagerState> StateAccessor { get; }

        public bool TrackEnabledIds { get; set; } = true;

        public async Task DisableIdAsync(ITurnContext turnContext, string id, IdType type = IdType.Card, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (id is null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            await (TrackEnabledIds ? ForgetIdAsync(turnContext, id, cancellationToken) : TrackIdAsync(turnContext, id, type, cancellationToken));
        }

        public async Task EnableIdAsync(ITurnContext turnContext, string id, IdType type = IdType.Card, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (id is null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            await (TrackEnabledIds ? TrackIdAsync(turnContext, id, type, cancellationToken) : ForgetIdAsync(turnContext, id, cancellationToken));
        }

        public async Task TrackIdAsync(ITurnContext turnContext, string id, IdType type = IdType.Card, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (id is null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var state = await StateAccessor.GetNotNullAsync(turnContext, () => new CardManagerState(), cancellationToken);

            state.TrackedIdsByType.TryGetValue(type, out var trackedList);

            if (trackedList is null)
            {
                state.TrackedIdsByType[type] = trackedList = new List<string>();
            }

            trackedList.Add(id);
        }

        public async Task ForgetIdAsync(ITurnContext turnContext, string id, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (id is null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var state = await StateAccessor.GetAsync(turnContext, () => new CardManagerState(), cancellationToken);

            state?.TrackedIdsByType?.Values.Where(list => list != null).ToList().ForEach(list => list.Remove(id));
        }

        public async Task ClearTrackedIdsAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            var state = await StateAccessor.GetNotNullAsync(turnContext, () => new CardManagerState(), cancellationToken);

            state.TrackedIdsByType.Clear();
        }
    }
}
