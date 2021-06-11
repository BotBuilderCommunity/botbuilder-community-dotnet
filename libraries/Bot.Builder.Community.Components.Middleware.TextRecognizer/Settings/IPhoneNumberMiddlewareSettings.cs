using Bot.Builder.Community.Components.Middleware.TextRecognizer.BaseSettings;

namespace Bot.Builder.Community.Components.Middleware.TextRecognizer.Settings
{
    public interface IPhoneNumberMiddlewareSettings : IBaseMiddlewareSettings
    {
        bool IsPhoneNumberEnable { get; }
    }
}