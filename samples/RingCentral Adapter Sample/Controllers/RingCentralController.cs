using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.RingCentral;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;

namespace RingCentral_Adapter_Sample.Controllers
{
    // This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
    // implementation at runtime. Multiple different IBot implementations running at different endpoints can be
    // achieved by specifying a more specific type for the bot constructor argument.
    [ApiController]
    public class RingCentralController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly RingCentralAdapter _adapter;
        private readonly IBot _bot;

        public RingCentralController(IConfiguration configuration, RingCentralAdapter adapter, IBot bot)
        {
            _configuration = configuration;
            _bot = bot;
            _adapter = adapter;
        }

        [HttpGet]
        [HttpPost]
        [Route("api/ringcentral/actions")]
        public async Task Actions()
        {
            await _adapter.ProcessAsync(Request, Response, _bot, default(CancellationToken));
        }

        [HttpGet]
        [HttpPost]
        [Route("api/ringcentral/webhooks")]
        public async Task Webhooks()
        {
            await _adapter.ProcessAsync(Request, Response, _bot, default(CancellationToken));
        }
    }
}
