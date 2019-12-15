using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace Bot.Builder.Community.Cards.Management
{
    public class CardFocuser<T>
        where T : BotState
    {
        public CardFocuser(T botState)
            : this(botState?.CreateProperty<CardFocuserState>(nameof(CardFocuserState)) ?? throw new ArgumentNullException(nameof(botState)))
        {
        }

        public CardFocuser(IStatePropertyAccessor<CardFocuserState> stateAccessor)
        {
            StateAccessor = stateAccessor ?? throw new ArgumentNullException(nameof(stateAccessor));
        }

        public IStatePropertyAccessor<CardFocuserState> StateAccessor { get; set; }

        public async Task FocusIdAsync(ITurnContext turnContext, string id, CancellationToken cancellationToken = default)
        {
            if (turnContext is null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (id is null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var state = await StateAccessor.GetAsync(turnContext, () => new CardFocuserState(), cancellationToken);

            if (state is null)
            {
                await StateAccessor.SetAsync(turnContext, state = new CardFocuserState(), cancellationToken);
            }

            state.FocusedId = id;
        }
    }
}