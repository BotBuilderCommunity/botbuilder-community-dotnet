using System;
using Bot.Builder.Community.Components.Middleware.TextRecognizer.BaseSettings;
using Microsoft.Extensions.Configuration;

namespace Bot.Builder.Community.Components.Middleware.TextRecognizer.Settings
{
    public class EmailMiddlewareSettings : BaseMiddlewareSettings , IEmailMiddlewareSettings
    {
        public EmailMiddlewareSettings(IConfiguration configuration) : base(configuration, "EmailEntities")
        {
            IsEmailEnable = Convert.ToBoolean(configuration[nameof(IsEmailEnable)]);
        }

        public bool IsEmailEnable { get; }
    }
}