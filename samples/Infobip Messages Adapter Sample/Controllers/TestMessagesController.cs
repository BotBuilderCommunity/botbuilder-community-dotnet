using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Infobip.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Infobip_Messages_Adapter_Sample.Controllers
{
    // Controller for testing outgoing WhatsApp messages
    [Route("api/test")]
    [ApiController]
    public class TestMessagesController : ControllerBase
    {
        private readonly InfobipMessagesAdapter _adapter;
        private readonly InfobipMessagesAdapterOptions _options;
        private readonly IInfobipMessagesClient _client;
        private readonly ILogger<TestMessagesController> _logger;

        public TestMessagesController(
            InfobipMessagesAdapter adapter, 
            InfobipMessagesAdapterOptions options,
            IInfobipMessagesClient client,
            ILogger<TestMessagesController> logger)
        {
            _adapter = adapter;
            _options = options;
            _client = client;
            _logger = logger;
        }

        /// <summary>
        /// Test endpoint to send a simple WhatsApp text message using the actual library
        /// POST /api/test/whatsapp/text?to=447860099123&message=Hello World
        /// </summary>
        [HttpPost("whatsapp/text")]
        public async Task<IActionResult> SendTextMessageAsync([FromQuery] string to, [FromQuery] string message)
        {
            if (string.IsNullOrEmpty(to) || string.IsNullOrEmpty(message))
            {
                return BadRequest("Both 'to' and 'message' parameters are required");
            }

            try
            {
                // Create a simple text message for WhatsApp with correct structure
                var infobipMessage = new
                {
                    messages = new[]
                    {
                        new
                        {
                            sender = _options.DefaultSender,  // Changed from "from" to "sender"
                            channel = "WHATSAPP",              // Added required channel field
                            destinations = new[]
                            {
                                new { to = to }
                            },
                            content = new                      // Fixed content structure
                            {
                                body = new
                                {
                                    type = "TEXT",
                                    text = message
                                }
                            }
                        }
                    }
                };

                _logger.LogInformation("Sending WhatsApp text message to {To}: {Message}", to, message);
                _logger.LogInformation("Using sender: {Sender}", _options.DefaultSender);
                
                // Use the correct method signature from the actual library
                var result = await _client.SendAsync<object>(infobipMessage);
                
                return Ok(new { 
                    success = true, 
                    result = result, 
                    to = to, 
                    message = message,
                    channel = "WHATSAPP",
                    sender = _options.DefaultSender
                });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to send WhatsApp message");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Test endpoint to send a WhatsApp message with buttons using the actual library
        /// POST /api/test/whatsapp/buttons?to=447860099123
        /// </summary>
        [HttpPost("whatsapp/buttons")]
        public async Task<IActionResult> SendButtonMessageAsync([FromQuery] string to)
        {
            if (string.IsNullOrEmpty(to))
            {
                return BadRequest("'to' parameter is required");
            }

            try
            {
                // Create a WhatsApp interactive button message with correct structure
                var infobipMessage = new
                {
                    messages = new[]
                    {
                        new
                        {
                            sender = _options.DefaultSender,   // Changed from "from" to "sender"
                            channel = "WHATSAPP",               // Added required channel field
                            destinations = new[]
                            {
                                new { to = to }
                            },
                            content = new
                            {
                                body = new                      // Added proper body structure
                                {
                                    type = "TEXT",
                                    text = "Please choose an option:"
                                },
                                buttons = new[]                 // Simplified button structure for Messages API
                                {
                                    new
                                    {
                                        type = "QUICK_REPLY",
                                        text = "Option 1",
                                        postbackData = "option_1"
                                    },
                                    new
                                    {
                                        type = "QUICK_REPLY",
                                        text = "Option 2", 
                                        postbackData = "option_2"
                                    },
                                    new
                                    {
                                        type = "QUICK_REPLY",
                                        text = "Help",
                                        postbackData = "help"
                                    }
                                }
                            }
                        }
                    }
                };

                _logger.LogInformation("Sending WhatsApp button message to {To}", to);
                _logger.LogInformation("Using sender: {Sender}", _options.DefaultSender);
                
                // Use the correct method signature from the actual library
                var result = await _client.SendAsync<object>(infobipMessage);
                
                return Ok(new { 
                    success = true, 
                    result = result, 
                    to = to, 
                    messageType = "buttons",
                    channel = "WHATSAPP",
                    sender = _options.DefaultSender
                });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to send WhatsApp button message");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Get current configuration for testing
        /// GET /api/test/config
        /// </summary>
        [HttpGet("config")]
        public IActionResult GetConfigurationAsync()
        {
            return Ok(new
            {
                defaultSender = _options.DefaultSender,
                defaultChannel = _options.DefaultChannel,
                apiBaseUrl = _options.InfobipMessagesApiBaseUrl,
                hasApiKey = !string.IsNullOrEmpty(_options.InfobipApiKey),
                adaptationMode = _options.AdaptationMode,
                enableInteractiveMessaging = _options.EnableInteractiveMessaging,
                enableWhatsAppTemplates = _options.EnableWhatsAppTemplates,
                senderConfigured = !string.IsNullOrEmpty(_options.DefaultSender),
                note = "Using actual Bot.Builder.Community.Adapters.Infobip.Messages library",
                payloadFormatFixed = "Corrected sender, channel, and content.body structure"
            });
        }
    }
}