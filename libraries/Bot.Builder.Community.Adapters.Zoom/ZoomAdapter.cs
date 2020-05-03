using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Zoom.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using HttpResponse = Microsoft.AspNetCore.Http.HttpResponse;

namespace Bot.Builder.Community.Adapters.Zoom
{
    public class ZoomAdapter : BotAdapter, IBotFrameworkHttpAdapter
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        private readonly ZoomAdapterOptions _options;
        private readonly ILogger _logger;
        private readonly ZoomRequestMapper _requestMapper;
        private readonly ZoomClient _zoomClient;

        public ZoomAdapter(ZoomAdapterOptions options = null, ILogger logger = null)
        {
            _options = options ?? new ZoomAdapterOptions();
            _logger = logger ?? NullLogger.Instance;

            _zoomClient = new ZoomClient(_options.ClientId, _options.ClientSecret);

            _requestMapper = new ZoomRequestMapper(new ZoomRequestMapperOptions()
            {
                RobotJid = _options.BotJid
            }, null);
        }

        public async Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, CancellationToken cancellationToken = default)
        {
            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            if (httpResponse == null)
            {
                throw new ArgumentNullException(nameof(httpResponse));
            }

            if (bot == null)
            {
                throw new ArgumentNullException(nameof(bot));
            }

            string body;
            using (var sr = new StreamReader(httpRequest.Body))
            {
                body = await sr.ReadToEndAsync();
            }

            var zoomRequest = JsonConvert.DeserializeObject<ZoomRequest>(body, JsonSerializerSettings);

            if (_options.ValidateIncomingZoomRequests
                && !ValidationHelper.ValidateRequest(httpRequest, zoomRequest, body, _logger))
            {
                throw new AuthenticationException("Failed to validate incoming request.");
            }

            var activity = RequestToActivity(zoomRequest);

            using (var context = new TurnContext(this, activity))
            {
                await RunPipelineAsync(context, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);

                var statusCode = Convert.ToInt32(context.TurnState.Get<string>("httpStatus"), CultureInfo.InvariantCulture);
                var text = context.TurnState.Get<object>("httpBody") != null ? context.TurnState.Get<object>("httpBody").ToString() : string.Empty;

                await WriteAsync(httpResponse, statusCode, text, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
            }
        }

        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            return Task.FromException<ResourceResponse>(new NotImplementedException("Zoom adapter does not support updateActivity."));
        }

        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            return Task.FromException(new NotImplementedException("Zoom adapter does not support deleteActivity."));
        }

        public virtual Activity RequestToActivity(ZoomRequest request)
        {
            return _requestMapper.RequestToActivity(request);
        }

        public override async Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            var responses = new List<ResourceResponse>();

            foreach (var activity in activities)
            {
                if (activity.Type != ActivityTypes.Message)
                {
                    _logger.LogTrace($"Unsupported Activity Type: '{activity.Type}'. Only Activities of type 'Message' are supported.");
                }
                else
                {
                    var message = _requestMapper.ActivityToZoom(activity);
                    var clientResponse = await _zoomClient.SendMessageAsync(message, cancellationToken).ConfigureAwait(false);

                    if (clientResponse.IsSuccessful)
                    {
                        responses.Add(new ResourceResponse() {Id = JObject.Parse(clientResponse.Content)["message_id"].ToString()});
                    }
                    else
                    {
                        _logger.LogError(clientResponse.ErrorException, $"Error sending message to Zoom. {clientResponse.ErrorMessage}");
                    }
                }
            }

            return responses.ToArray();
        }

        public static async Task WriteAsync(HttpResponse response, int code, string text, Encoding encoding, CancellationToken cancellationToken)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            response.ContentType = "text/plain";
            response.StatusCode = code;

            var data = encoding.GetBytes(text);

            await response.Body.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
        }
    }
}
