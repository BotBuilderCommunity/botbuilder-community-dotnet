using System.Collections.Generic;
using AdaptiveExpressions.Converters;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Components.CallDialogs
{
    /// <summary>
    /// <see cref="BotComponent"/> implementation for the <see cref="CallDialogs"/> component.
    /// </summary>
    public class CallDialogsBotComponent : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<DeclarativeType>(new DeclarativeType<AddDialogCall>(AddDialogCall.Kind));
            services.AddSingleton<DeclarativeType>(new DeclarativeType<CallDialogs>(CallDialogs.Kind));

            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<ObjectExpressionConverter<object>>>();
        }
    }
}
