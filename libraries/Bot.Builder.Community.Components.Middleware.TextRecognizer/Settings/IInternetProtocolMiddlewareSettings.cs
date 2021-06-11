using Bot.Builder.Community.Components.Middleware.TextRecognizer.BaseSettings;

namespace Bot.Builder.Community.Components.Middleware.TextRecognizer.Settings
{
    public interface IInternetProtocolMiddlewareSettings : IBaseMiddlewareSettings
    {
        bool IsInternetProtocolEnable { get; }

        InternetProtocolType InternetProtocolType { get; }
    }
}
