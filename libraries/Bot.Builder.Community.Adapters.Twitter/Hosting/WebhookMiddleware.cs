using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Models;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IMiddleware = Microsoft.AspNetCore.Http.IMiddleware;

namespace Bot.Builder.Community.Adapters.Twitter.Hosting
{
    public class WebhookMiddleware : IMiddleware
    {
        private readonly ILogger<WebhookMiddleware> _logger;
        private readonly IBot _bot;
        private readonly TwitterAdapter _adapter;
        private readonly TwitterOptions _options;
        private readonly WebhookInterceptor _interceptor;

        public WebhookMiddleware(IOptions<TwitterOptions> options, ILogger<WebhookMiddleware> logger, IBot bot,
            TwitterAdapter adapter)
        {
            _logger = logger;
            _bot = bot;
            _adapter = adapter;
            _options = options.Value;
            _interceptor = new WebhookInterceptor(_options.ConsumerSecret);
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var result = await _interceptor.InterceptIncomingRequest(context.Request, OnDirectMessageReceived);
            if (result.IsHandled)
            {
                context.Response.StatusCode = (int) HttpStatusCode.OK;
                await context.Response.WriteAsync(result.Response);
            }
            else
            {
                _logger.LogError($"Failed to intercept message.");
                await next(context);
            }
        }

        private async void OnDirectMessageReceived(DirectMessageEvent obj)
        {
            _logger.LogInformation(
                $"Direct Message from {obj.Sender.Name} (@{obj.Sender.ScreenName}): {obj.MessageText}");

            // Important: Ignore messages originating from the bot else recursion happens
            if (obj.Sender.ScreenName == _options.BotUsername) return;

            if (_options.AllowedUsernamesConfigured()
                && _options.AllowedUsernames.Contains(obj.Sender.ScreenName)
                || !_options.AllowedUsernamesConfigured())
            {
                // Only respond if sender is different than the bot
                await _bot.OnTurnAsync(new TurnContext(_adapter, new Activity
                {
                    Text = obj.MessageText,
                    Type = "message",
                    From = new ChannelAccount(obj.Sender.Id, obj.Sender.ScreenName),
                    Recipient = new ChannelAccount(obj.Recipient.Id, obj.Recipient.ScreenName),
                    Conversation = new ConversationAccount {Id = obj.Sender.Id}
                }));
            }
        }
    }
}