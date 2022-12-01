using Bot.Builder.Community.Recognizers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Builder.Community.Recognizers
{
    public class CustomQuestionAnsweringRecognizerBotComponent : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<DeclarativeType>((sp) => new DeclarativeType<CustomQuestionAnsweringRecognizer>(CustomQuestionAnsweringRecognizer.Kind));
        }
    }
}
