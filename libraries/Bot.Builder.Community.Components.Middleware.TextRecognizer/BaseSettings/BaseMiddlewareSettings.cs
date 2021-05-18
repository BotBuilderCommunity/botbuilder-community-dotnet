using Microsoft.Extensions.Configuration;

namespace Bot.Builder.Community.Components.Middleware.TextRecognizer.BaseSettings
{
    public abstract class BaseMiddlewareSettings
    {
        protected BaseMiddlewareSettings(IConfiguration configuration,string propertyName)
        {
            Locale = configuration[nameof(Locale)];
            PropertyName = propertyName;
        }

        public string Locale { get; }
        
        public string PropertyName { get; }
    }
}
