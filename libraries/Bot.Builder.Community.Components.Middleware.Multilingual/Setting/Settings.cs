using System;
using Bot.Builder.Community.Components.Middleware.Multilingual.AzureTranslateService;
using Microsoft.Extensions.Configuration;

namespace Bot.Builder.Community.Components.Middleware.Multilingual.Setting
{
    internal class Settings : ISetting
    {
        public string Key { get; set; }
        public string Endpoint { get; set; }
        public string Location { get; set; }
        public string DefaultLanguageCode { get; set; }
        public double ScoreThreshold { get; set; }
        public bool IsMultilingualEnabled { get; set; }

        public Settings(IConfiguration configuration)
        {

            IsMultilingualEnabled = string.IsNullOrEmpty(configuration[nameof(IsMultilingualEnabled)]) 
                                    || Convert.ToBoolean(configuration[nameof(IsMultilingualEnabled)]);

            if (!IsMultilingualEnabled)
                return;

            IsMultilingualEnabled = true;

            Key = configuration[nameof(Key)];

            if (string.IsNullOrEmpty(Key))
            {
                throw new Exception("Multilingual Key is not set");
            }

            Endpoint = configuration[nameof(Endpoint)];

            if (string.IsNullOrEmpty(Endpoint))
            {
                throw new Exception("Multilingual Endpoint is not set");
            }

            Location = configuration[nameof(Location)];

            if (string.IsNullOrEmpty(Location))
            {
                throw new Exception("Multilingual Location is not set");
            }

            DefaultLanguageCode = configuration[nameof(DefaultLanguageCode)];

            if (string.IsNullOrEmpty(DefaultLanguageCode))
            {
                DefaultLanguageCode = "en";
            }

            ScoreThreshold = Convert.ToDouble(configuration[nameof(ScoreThreshold)]);

            if (ScoreThreshold > 0.0 && ScoreThreshold <= 1.0)
                return;
            
            ScoreThreshold = 0.5;
           
        }
    }
}
