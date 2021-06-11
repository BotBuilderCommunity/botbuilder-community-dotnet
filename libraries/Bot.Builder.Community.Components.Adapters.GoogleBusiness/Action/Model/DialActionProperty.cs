using AdaptiveExpressions.Properties;

namespace Bot.Builder.Community.Components.Adapters.GoogleBusiness.Action.Model
{
    public class DialActionProperty
    {
        public StringExpression Text { get; set; }
        public StringExpression PostBackData { get; set; }
        public StringExpression PhoneNumber { get; set; }
    }
}
