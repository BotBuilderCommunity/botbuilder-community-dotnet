using Bot.Builder.Community.Adapters.Infobip.Messages;
using Infobip_Messages_Adapter_Sample.Bots;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infobip_Messages_Adapter_Sample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Configure Infobip Messages Adapter Options
            services.AddSingleton<InfobipMessagesAdapterOptions>(serviceProvider =>
            {
                return new InfobipMessagesAdapterOptions(Configuration);
            });

            // Add Infobip Messages Client (using the actual library implementation)
            services.AddSingleton<IInfobipMessagesClient, InfobipMessagesClient>();

            // Add Infobip Messages Adapter (using the actual library implementation)
            services.AddSingleton<InfobipMessagesAdapter>(serviceProvider =>
            {
                var options = serviceProvider.GetRequiredService<InfobipMessagesAdapterOptions>();
                var client = serviceProvider.GetRequiredService<IInfobipMessagesClient>();
                var logger = serviceProvider.GetRequiredService<ILogger<InfobipMessagesAdapter>>();
                
                var adapter = new InfobipMessagesAdapter(options, client, logger);
                
                // Configure error handler for better debugging
                adapter.OnTurnError = async (turnContext, exception) =>
                {
                    logger.LogError(exception, "[OnTurnError] unhandled error: {ErrorMessage}", exception.Message);
                    
                    // Send user-friendly error message
                    await turnContext.SendActivityAsync("?? Sorry, something went wrong. Please try again.");
                    
                    // In development, show more details
                    if (options.InfobipMessagesApiBaseUrl?.Contains("localhost") == true)
                    {
                        await turnContext.SendActivityAsync($"?? Debug info: {exception.Message}");
                    }
                };
                
                return adapter;
            });

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            // The EchoBot now requires Infobip client injection for direct API calls
            services.AddTransient<IBot, EchoBot>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
        }
    }
}