using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards.Management
{
    internal class CardManagerCache
    {
        public JObject IncomingButtonPayload { get; internal set; }

        public bool HasIncomingButtonPayload { get; internal set; }
    }
}