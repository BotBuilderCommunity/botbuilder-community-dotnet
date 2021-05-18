using System;
using Bot.Builder.Community.Components.Middleware.TextRecognizer.BaseSettings;
using Microsoft.Extensions.Configuration;

namespace Bot.Builder.Community.Components.Middleware.TextRecognizer.Settings
{
    public class InternetProtocolMiddlewareSettings : BaseMiddlewareSettings, IInternetProtocolMiddlewareSettings
    {
        public InternetProtocolMiddlewareSettings(IConfiguration configuration) : base(configuration, "InternetTypeEntities")
        {
            IsInternetProtocolEnable = Convert.ToBoolean(configuration[nameof(IsInternetProtocolEnable)]);

            if (IsInternetProtocolEnable && !string.IsNullOrEmpty(configuration[nameof(InternetProtocolType)]))
            {
                Enum.TryParse(configuration[nameof(InternetProtocolType)],true, out InternetProtocolType tempMediaTypeType);
                InternetProtocolType = tempMediaTypeType;
            }
            else
            {
                InternetProtocolType = InternetProtocolType.Url;
            }
        }

        public bool IsInternetProtocolEnable { get; }

        public InternetProtocolType InternetProtocolType { get; }
    }
}
