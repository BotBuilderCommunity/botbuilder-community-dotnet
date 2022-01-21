using System;
using Bot.Builder.Community.Components.Middleware.TextRecognizer.BaseSettings;
using Microsoft.Extensions.Configuration;

namespace Bot.Builder.Community.Components.Middleware.TextRecognizer.Settings
{
    public class SocialMediaMiddlewareSettings : BaseMiddlewareSettings, ISocialMediaMiddlewareSettings
    {
        public SocialMediaMiddlewareSettings(IConfiguration configuration) : base(configuration, "turn.MediaTypeEntities")
        {
            IsSocialMediaEnable = Convert.ToBoolean(configuration[nameof(IsSocialMediaEnable)]);

            if (IsSocialMediaEnable && !string.IsNullOrEmpty(configuration[nameof(MediaType)]))
            {
                Enum.TryParse(configuration[nameof(MediaType)], true,out SocialMediaType tempMediaTypeType);
                MediaType = tempMediaTypeType;
            }
            else
            {
                MediaType = SocialMediaType.Mention;
            }
        }

        public bool IsSocialMediaEnable { get; }
        public SocialMediaType MediaType { get; }
    }
}