using Bot.Builder.Community.Components.Middleware.TextRecognizer.BaseSettings;

namespace Bot.Builder.Community.Components.Middleware.TextRecognizer.Settings
{
    public interface ISocialMediaMiddlewareSettings : IBaseMiddlewareSettings
    {
        bool IsSocialMediaEnable { get; }

        SocialMediaType MediaType { get; }
    }
}