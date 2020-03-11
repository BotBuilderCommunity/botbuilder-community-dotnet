using Microsoft.Extensions.Logging;

namespace Bot.Builder.Community.Adapters.Alexa.Core
{
    public class AlexaRequestMapperOptions
    {
        public string BotId { get; internal set; }
        public string ChannelId { get; internal set; } = "alexa";
        public string DefaultIntentSlotName { get; set; } = "phrase";
        public bool ShouldEndSessionByDefault { get; set; } = true;
    }
}
