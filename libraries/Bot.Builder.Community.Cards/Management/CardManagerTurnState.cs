using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards.Management
{
    internal class CardManagerTurnState
    {
        public JObject IncomingButtonPayload { get; internal set; }

        public bool CheckedForIncomingButtonPayload { get; internal set; }
    }
}