using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Builder.Community.Components.Middleware.SentimentAnalysis.Models
{
    public interface ISentimentAnalysisCredentialsProvider
    {
        bool IsEnabled { get; }
        string APIKey { get; }

        string EndpointUrl { get; }
    }
}
