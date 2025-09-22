using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Infobip.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Logging;

namespace Infobip_Messages_Adapter_Sample.Controllers
{
    /// <summary>
    /// ASP Controller handling requests from Infobip Messages API.
    /// This controller uses the actual Bot.Builder.Community.Adapters.Infobip.Messages library
    /// to process webhooks and send real messages to Infobip.
    /// </summary>
    [Route("api/infobip")]
    [ApiController]
    public class InfobipMessagesController : ControllerBase
    {
        private readonly InfobipMessagesAdapter _adapter;
        private readonly IBot _bot;
        private readonly ILogger<InfobipMessagesController> _logger;

        public InfobipMessagesController(InfobipMessagesAdapter adapter, IBot bot, ILogger<InfobipMessagesController> logger)
        {
            _adapter = adapter;
            _bot = bot;
            _logger = logger;
        }

        /// <summary>
        /// Main endpoint for all Infobip Messages API webhook callbacks.
        /// This handles messages from all supported channels (WhatsApp, SMS, Viber, etc.)
        /// URL: POST /api/infobip
        /// </summary>
        [HttpPost]
        public async Task PostAsync()
        {
            _logger.LogInformation("[WEBHOOK] Received message via Infobip Messages API (main endpoint)");
            
            // Delegate processing to the actual Infobip Messages Adapter
            // This will parse the webhook, convert to Bot Framework Activity, and invoke the bot
            await _adapter.ProcessAsync(Request, Response, _bot, default(CancellationToken));
        }

        /// <summary>
        /// Channel-specific endpoint for WhatsApp messages.
        /// URL: POST /api/infobip/whatsapp
        /// </summary>
        [HttpPost("whatsapp")]
        public async Task PostWhatsAppAsync()
        {
            _logger.LogInformation("[WHATSAPP] Received WhatsApp message via Infobip Messages API");
            await _adapter.ProcessAsync(Request, Response, _bot, default(CancellationToken));
        }

        /// <summary>
        /// Channel-specific endpoint for SMS messages.
        /// URL: POST /api/infobip/sms
        /// </summary>
        [HttpPost("sms")]
        public async Task PostSmsAsync()
        {
            _logger.LogInformation("[SMS] Received SMS message via Infobip Messages API");
            await _adapter.ProcessAsync(Request, Response, _bot, default(CancellationToken));
        }

        /// <summary>
        /// Channel-specific endpoint for Viber messages.
        /// URL: POST /api/infobip/viber
        /// </summary>
        [HttpPost("viber")]
        public async Task PostViberAsync()
        {
            _logger.LogInformation("[VIBER] Received Viber message via Infobip Messages API");
            await _adapter.ProcessAsync(Request, Response, _bot, default(CancellationToken));
        }

        /// <summary>
        /// Channel-specific endpoint for RCS messages.
        /// URL: POST /api/infobip/rcs
        /// </summary>
        [HttpPost("rcs")]
        public async Task PostRcsAsync()
        {
            _logger.LogInformation("[RCS] Received RCS message via Infobip Messages API");
            await _adapter.ProcessAsync(Request, Response, _bot, default(CancellationToken));
        }

        /// <summary>
        /// Generic channel endpoint that accepts a channel parameter.
        /// URL: POST /api/infobip/channel/{channelType}
        /// Example: POST /api/infobip/channel/whatsapp
        /// </summary>
        [HttpPost("channel/{channelType}")]
        public async Task PostChannelAsync(string channelType)
        {
            _logger.LogInformation($"[{channelType.ToUpper()}] Received {channelType.ToUpper()} message via Infobip Messages API (generic endpoint)");
            
            // Store channel type in context for potential use in bot logic
            HttpContext.Items["ChannelType"] = channelType;
            
            await _adapter.ProcessAsync(Request, Response, _bot, default(CancellationToken));
        }

        /// <summary>
        /// Health check endpoint to verify the webhook is working.
        /// URL: GET /api/infobip
        /// </summary>
        [HttpGet]
        public ActionResult Get()
        {
            _logger.LogInformation("[HEALTH] Health check requested for Infobip Messages API endpoint");
            
            return Ok(new 
            { 
                status = "[READY] Ready",
                message = "Infobip Messages API endpoint is active and ready to receive webhooks",
                endpoints = new 
                {
                    main = "/api/infobip",
                    whatsapp = "/api/infobip/whatsapp",
                    sms = "/api/infobip/sms",
                    viber = "/api/infobip/viber",
                    rcs = "/api/infobip/rcs",
                    generic = "/api/infobip/channel/{channelType}"
                },
                library = "Bot.Builder.Community.Adapters.Infobip.Messages",
                timestamp = System.DateTime.UtcNow
            });
        }

        /// <summary>
        /// Channel-specific health check endpoints.
        /// </summary>
        [HttpGet("whatsapp")]
        public ActionResult GetWhatsApp()
        {
            return Ok(new { status = "[READY] Ready", channel = "WhatsApp", endpoint = "/api/infobip/whatsapp" });
        }

        [HttpGet("sms")]
        public ActionResult GetSms()
        {
            return Ok(new { status = "[READY] Ready", channel = "SMS", endpoint = "/api/infobip/sms" });
        }

        [HttpGet("viber")]
        public ActionResult GetViber()
        {
            return Ok(new { status = "[READY] Ready", channel = "Viber", endpoint = "/api/infobip/viber" });
        }

        [HttpGet("rcs")]
        public ActionResult GetRcs()
        {
            return Ok(new { status = "[READY] Ready", channel = "RCS", endpoint = "/api/infobip/rcs" });
        }
    }
}