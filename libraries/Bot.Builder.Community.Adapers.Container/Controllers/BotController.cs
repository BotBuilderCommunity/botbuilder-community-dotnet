// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Alexa;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;

namespace AdapterBot.Controllers
{
    [Route("api/messages")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly AlexaAdapter _adapter;
        private readonly IBot _bot;

        public BotController(AlexaAdapter adapter, IBot bot)
        {
            _adapter = adapter;
            _bot = bot;
        }

        [HttpPost]
        public async Task PostAsync()
        {
            await _adapter.ProcessAsync(Request, Response, _bot);
        }
    }
}
