using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.ActionsSDK;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;

namespace Bot.Builder.Community.Samples.ActionsSdk.Controllers
{
    // This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
    // implementation at runtime. Multiple different IBot implementations running at different endpoints can be
    // achieved by specifying a more specific type for the bot constructor argument.
    [Route("api/google")]
    [ApiController]
    public class GoogleController : ControllerBase
    {
        private readonly ActionsSdkAdapter Adapter;
        private readonly IBot Bot;

        public GoogleController(ActionsSdkAdapter adapter, IBot bot)
        { 
            Adapter = adapter;
            Bot = bot;
        }

        [HttpPost]
        public async Task PostAsync()
        {
            // Delegate the processing of the HTTP POST to the adapter.
            // The adapter will invoke the bot.
            await Adapter.ProcessAsync(Request, Response, Bot);
        }
    }
}
