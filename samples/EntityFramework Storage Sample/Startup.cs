using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using EntityFrameworkTranscriptStoreExample.Bots;
using Bot.Builder.Community.Storage.EntityFramework;
using Microsoft.Extensions.Hosting;

namespace EntityFrameworkTranscriptStoreExample
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

            var loggerConnectionString = Configuration["StoreConnectionString"];
            var logger = new EntityFrameworkTranscriptStore(loggerConnectionString);

            services.AddSingleton<ITranscriptStore>(logger);

            //// Conversation State Storage
            IStorage conversationDataStore = new EntityFrameworkStorage(Configuration["StoreConnectionString"]);
            services.AddSingleton<ConversationState>(new ConversationState(conversationDataStore));

            //// User State Storage
            //IStorage userDataStore = new EntityFrameworkStorage(connectionString);
            //var userState = new UserState(userDataStore);

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
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
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }
    }
}
