using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Bot.Builder.Community.Adapters.Infobip.Messages;
using Microsoft.Extensions.Logging;

namespace Infobip_Messages_Adapter_Sample.Controllers
{
    // This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
    // implementation at runtime. Multiple different IBot implementations running at different endpoints can be
    // achieved by specifying a more specific type for the bot constructor argument.
    [Route("api/messages")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly InfobipMessagesAdapter _infobipAdapter;
        private readonly IBot _bot;
        private readonly ILogger<BotController> _logger;

        public BotController(IBotFrameworkHttpAdapter adapter, InfobipMessagesAdapter infobipAdapter, IBot bot, ILogger<BotController> logger)
        {
            _adapter = adapter;
            _infobipAdapter = infobipAdapter;
            _bot = bot;
            _logger = logger;
        }

        [HttpPost, HttpGet]
        public async Task PostAsync()
        {
            // Check if this is a request that should use Infobip adapter
            // For Bot Framework Emulator testing, we'll route messages through the standard adapter
            // The Infobip adapter expects properly signed webhook requests from Infobip

            var userAgent = Request.Headers["User-Agent"].ToString();
            var isFromEmulator = userAgent.Contains("Microsoft-BotFramework") || userAgent.Contains("Bot-Framework");
            var hasInfobipSignature = Request.Headers.ContainsKey("X-Infobip-Signature") || 
                                    Request.Headers.ContainsKey("Infobip-Signature");

            if (isFromEmulator)
            {
                _logger.LogInformation("[BOT] Processing request from Bot Framework Emulator via standard adapter");

                // Use the standard Bot Framework adapter for emulator testing
                await _adapter.ProcessAsync(Request, Response, _bot);
            }
            else if (hasInfobipSignature)
            {
                _logger.LogInformation("[WEBHOOK] Processing request via Infobip Messages adapter (webhook with signature)");

                // This is a properly signed webhook from Infobip, use the Infobip adapter
                await _infobipAdapter.ProcessAsync(Request, Response, _bot);
            }
            else
            {
                _logger.LogWarning("[UNKNOWN] Request received without Bot Framework or Infobip signature - treating as Bot Framework request");

                // Default to Bot Framework adapter for other requests
                await _adapter.ProcessAsync(Request, Response, _bot);
            }
        }
    }
}