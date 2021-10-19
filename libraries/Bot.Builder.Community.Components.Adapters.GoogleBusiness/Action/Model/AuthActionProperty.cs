using System.Collections.Generic;
using AdaptiveExpressions.Properties;

namespace Bot.Builder.Community.Components.Adapters.GoogleBusiness.Action.Model
{
    public class AuthActionProperty
    {
        public StringExpression ClientId { get; set; }
        public StringExpression CodeChallenge { get; set; }
        public List<StringExpression> Scopes { get; set; }
        public StringExpression CodeChallengeMethod { get; set; }
    }
}
