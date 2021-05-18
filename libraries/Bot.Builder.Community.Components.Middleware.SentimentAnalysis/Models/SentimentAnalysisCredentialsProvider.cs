using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Builder.Community.Components.Middleware.SentimentAnalysis.Models
{
    public class SentimentAnalysisCredentialsProvider : ISentimentAnalysisCredentialsProvider
    {
        public SentimentAnalysisCredentialsProvider(IConfiguration configuration)
        {
            IsEnabled = !(String.IsNullOrEmpty(configuration["IsEnabled"])) ? Convert.ToBoolean(configuration["IsEnabled"]) : false;
            APIKey = configuration["APIKey"];
            EndpointUrl = configuration["EndpointUrl"];
        }

        public bool IsEnabled { get; }

        public string APIKey { get; }

        public string EndpointUrl { get; }
    }
}
