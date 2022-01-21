using System;
using Bot.Builder.Community.Components.Middleware.TextRecognizer.Middleware;
using Bot.Builder.Community.Components.Middleware.TextRecognizer.Settings;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Builder.Community.Components.Middleware.TextRecognizer
{
    public class TextRecognizerComponent : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {

            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            services.AddSingleton<IMiddleware, EmailRecognizerMiddleware>();
            services.AddSingleton<IEmailMiddlewareSettings>(sp => new EmailMiddlewareSettings(configuration));

            services.AddSingleton<IMiddleware, PhoneNumberRecognizerMiddleware>();
            services.AddSingleton<IPhoneNumberMiddlewareSettings>(sp => new PhoneNumberMiddlewareSettings(configuration));

            services.AddSingleton<IMiddleware, SocialMediaRecognizerMiddleware>();
            services.AddSingleton<ISocialMediaMiddlewareSettings>(sp => new SocialMediaMiddlewareSettings(configuration));

            services.AddSingleton<IMiddleware, InternetProtocolRecognizerMiddleware>();
            services.AddSingleton<IInternetProtocolMiddlewareSettings>(sp =>
                new InternetProtocolMiddlewareSettings(configuration));
        }
    }
}
