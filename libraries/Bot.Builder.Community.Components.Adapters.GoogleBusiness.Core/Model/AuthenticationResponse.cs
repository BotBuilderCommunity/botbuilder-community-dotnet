using Newtonsoft.Json;

namespace Bot.Builder.Community.Components.Adapters.GoogleBusiness.Core.Model
{
    public class AuthenticationResponse
    {
        public string Code { get; set; }
        [JsonProperty(PropertyName = "redirect_uri")]
        public string RedirectUri { get; set; }
        public ErrorDetails ErrorDetails { get; set; }
    }
}