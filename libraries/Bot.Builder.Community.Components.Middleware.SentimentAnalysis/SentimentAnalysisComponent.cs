using Bot.Builder.Community.Components.Middleware.SentimentAnalysis.Models;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Builder.Community.Components.Middleware.SentimentAnalysis
{
    public class SentimentAnalysisComponent : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IMiddleware, SentimentMiddleware>();
            services.AddSingleton<ISentimentAnalysisCredentialsProvider>(sp => new SentimentAnalysisCredentialsProvider(configuration));
        }
    }
}
