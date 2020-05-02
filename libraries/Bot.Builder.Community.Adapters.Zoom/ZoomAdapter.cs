using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Zoom.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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

        public ZoomAdapter(ZoomAdapterOptions options = null, ILogger logger = null)
        {
            _options = options ?? new ZoomAdapterOptions();
            _logger = logger ?? NullLogger.Instance;

            _requestMapper = new ZoomRequestMapper();
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
                && !await ValidationHelper.ValidateRequest(httpRequest, zoomRequest, body, "", _logger))
            {
                throw new AuthenticationException("Failed to validate incoming request.");
            }

            var zoomResponse = await ProcessZoomRequestAsync(zoomRequest, bot.OnTurnAsync);

            if (zoomResponse == null)
            {
                throw new ArgumentNullException(nameof(zoomResponse));
            }
            
            httpResponse.ContentType = "application/json";
            httpResponse.StatusCode = (int)HttpStatusCode.OK;

            var responseJson = JsonConvert.SerializeObject(zoomResponse, JsonSerializerSettings);
            var responseData = Encoding.UTF8.GetBytes(responseJson);
            await httpResponse.Body.WriteAsync(responseData, 0, responseData.Length, cancellationToken).ConfigureAwait(false);
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

        public override Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            return Task.FromResult(new ResourceResponse[0]);
        }
    }
}
