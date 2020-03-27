using System;
using Microsoft.Bot.Builder;

namespace Bot.Builder.Community.Cards.Management
{
    public class CardManager<T> : CardManager
        where T : BotState
    {
        public CardManager(T botState)
            : base(botState)
        {
        }
    }
}
