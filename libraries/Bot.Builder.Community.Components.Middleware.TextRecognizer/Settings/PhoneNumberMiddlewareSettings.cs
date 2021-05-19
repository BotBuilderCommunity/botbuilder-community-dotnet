using System;
using Bot.Builder.Community.Components.Middleware.TextRecognizer.BaseSettings;
using Microsoft.Extensions.Configuration;

namespace Bot.Builder.Community.Components.Middleware.TextRecognizer.Settings
{
    public class PhoneNumberMiddlewareSettings : BaseMiddlewareSettings, IPhoneNumberMiddlewareSettings
    {
        public PhoneNumberMiddlewareSettings(IConfiguration configuration) : base(configuration, "turn.PhoneNumberEntities")
        {
            IsPhoneNumberEnable = Convert.ToBoolean(configuration[nameof(IsPhoneNumberEnable)]);
        }

        public bool IsPhoneNumberEnable { get; }
    }
}