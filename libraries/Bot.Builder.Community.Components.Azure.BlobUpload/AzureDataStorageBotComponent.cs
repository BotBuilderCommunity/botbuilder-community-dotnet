using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Builder.Community.Components.Azure.BlobUpload
{
    public class AzureDataStorageBotComponent : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<DeclarativeType>(sp =>
                new DeclarativeType<AzureBlobUpload>(AzureBlobUpload.Kind));
        }
    }
}
