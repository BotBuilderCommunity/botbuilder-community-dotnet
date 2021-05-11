using AdaptiveExpressions.Converters;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Builder.Community.Components.Storage
{
    /// <summary>
    /// <see cref="BotComponent"/> implementation for the storage actions.
    /// </summary>
    public class StorageBotComponent : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<DeclarativeType>(new DeclarativeType<DeleteStorageItem>(DeleteStorageItem.Kind));
            services.AddSingleton<DeclarativeType>(new DeclarativeType<ReadStorageItem>(ReadStorageItem.Kind));
            services.AddSingleton<DeclarativeType>(new DeclarativeType<WriteStorageItem>(WriteStorageItem.Kind));
        }
    }
}
