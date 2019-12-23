using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace Bot.Builder.Community.Cards.Management
{
    public class CardDisabler<T>
        where T : BotState
    {
        public CardDisabler(T botState)
            : this(botState?.CreateProperty<CardDisablerState>(nameof(CardDisablerState)) ?? throw new ArgumentNullException(nameof(botState)))
        {
        }

        public CardDisabler(IStatePropertyAccessor<CardDisablerState> stateAccessor)
        {
            StateAccessor = stateAccessor ?? throw new ArgumentNullException(nameof(stateAccessor));
        }

        public IStatePropertyAccessor<CardDisablerState> StateAccessor { get; }

        public async Task DisableIdAsync(ITurnContext turnContext, string id, IdType type = IdType.Card, CancellationToken cancellationToken = default)
        {
            if (turnContext is null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (id is null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var state = await StateAccessor.GetNotNullAsync(turnContext, () => new CardDisablerState(), cancellationToken);

            state.TrackedIdsByType.TryGetValue(type, out var disabledList);

            if (disabledList is null)
            {
                state.TrackedIdsByType[type] = disabledList = new List<string>();
            }

            disabledList.Add(id);
        }
    }
}
