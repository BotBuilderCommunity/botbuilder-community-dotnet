using AdaptiveExpressions.Properties;

namespace Bot.Builder.Community.Components.Adapters.GoogleBusiness.Action.Model
{
    public class OpenUrlActionProperty
    {
        public StringExpression Text { get; set; }
        public StringExpression PostBackData { get; set; }
        public StringExpression Url { get; set; }
    }
}
