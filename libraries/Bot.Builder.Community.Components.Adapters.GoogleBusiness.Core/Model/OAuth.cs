using System.Collections.Generic;

namespace Bot.Builder.Community.Components.Adapters.GoogleBusiness.Core.Model
{
    public class OAuth
    {
        public string ClientId { get; set; }
        public string CodeChallenge { get; set; }
        public List<string> Scopes { get; set; }
        public string CodeChallengeMethod { get; set; }
    }
}