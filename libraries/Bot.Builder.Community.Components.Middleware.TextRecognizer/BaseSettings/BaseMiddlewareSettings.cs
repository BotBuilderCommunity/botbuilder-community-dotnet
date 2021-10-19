using Microsoft.Extensions.Configuration;

namespace Bot.Builder.Community.Components.Middleware.TextRecognizer.BaseSettings
{
    public abstract class BaseMiddlewareSettings
    {
        protected BaseMiddlewareSettings(IConfiguration configuration,string property)
        {
            Locale = configuration[nameof(Locale)];
            Property = property;
        }

        public string Locale { get; }
        
        public string Property { get; }
    }
}
