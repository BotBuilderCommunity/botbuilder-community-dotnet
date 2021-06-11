using AdaptiveExpressions.Properties;

namespace Bot.Builder.Community.Components.Adapters.GoogleBusiness.Action.Model
{
    public class RichCardDetailsProperty
    {
        public StringExpression Title { get; set; }
        public StringExpression Description { get; set; }
        public StringExpression MediaHeight { get; set; }
        public StringExpression MediaFileUrl { get; set; }
        public StringExpression MediaAltText { get; set; }
        public BoolExpression ForceRefresh { get; set; }
    }
}
