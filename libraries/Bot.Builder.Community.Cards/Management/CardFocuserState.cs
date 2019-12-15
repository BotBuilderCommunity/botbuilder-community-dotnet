using Newtonsoft.Json;

namespace Bot.Builder.Community.Cards.Management
{
    public class CardFocuserState
    {
        [JsonProperty("focusedId")]
        public string FocusedId { get; set; }
    }
}