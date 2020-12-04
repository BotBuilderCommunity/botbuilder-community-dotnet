using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.MessageBird;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;

namespace MessageBird_Adapter_Sample.Controllers
{
    [Route("api/messagebird")]
    [ApiController]
    public class MessageBirdController : ControllerBase
    {
        private readonly MessageBirdAdapter Adapter;
        private readonly IBot Bot;

        public MessageBirdController(MessageBirdAdapter adapter, IBot bot)
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
