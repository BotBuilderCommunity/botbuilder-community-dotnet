using Bot.Builder.Community.Cards.Management;
using Bot.Builder.Community.Cards.Translation;
using Cards_Library_Sample.Bots;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cards_Library_Sample
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            // Create the storage we'll be using for Conversation state. (Memory is great for testing purposes.)
            services.AddSingleton<IStorage, MemoryStorage>();

            // The Inspection Middleware needs some scratch state to keep track of the (logical) listening connections.
            services.AddSingleton<InspectionState>();

            // Create the conversation state.
            services.AddSingleton<ConversationState>();

            // Create the card manager. The card manager middleware depends on this.
            services.AddSingleton<CardManager, CardManager<ConversationState>>();

            // Create the Adaptive Card translator
            services.AddSingleton<AdaptiveCardTranslator>();

            // Create the Bot Framework Adapter with middleware.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithMiddleware>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, CardsLibraryBot>();
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
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }
    }
}
