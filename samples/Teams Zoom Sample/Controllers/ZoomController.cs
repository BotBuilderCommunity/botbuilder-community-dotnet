using Bot.Builder.Community.Adapters.Zoom;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teams_Zoom_Sample.Controllers
{
    [Route("api/zoom")]
    [ApiController]
    public class ZoomController : ControllerBase
    {
        private readonly ZoomAdapter _adapter;
        private readonly IBot _bot;

        public ZoomController(ZoomAdapter adapter, IBot bot)
        {
            _adapter = adapter;
            _bot = bot;
        }

        [HttpPost]
        public async Task PostAsync()
        {
            // Delegate the processing of the HTTP POST to the adapter.
            // The adapter will invoke the bot.
            await _adapter.ProcessAsync(Request, Response, _bot);
        }
    }
}
