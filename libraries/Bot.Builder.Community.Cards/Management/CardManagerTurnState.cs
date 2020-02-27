using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards.Management
{
    internal class CardManagerTurnState
    {
        public JObject IncomingPayload { get; internal set; }

        public bool CheckedForIncomingPayload { get; internal set; }
    }
}