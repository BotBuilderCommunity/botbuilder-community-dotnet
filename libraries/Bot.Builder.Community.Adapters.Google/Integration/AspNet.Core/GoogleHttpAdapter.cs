using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Google.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Bot.Builder.Community.Adapters.Google.Integration.AspNet.Core
{
    public class GoogleHttpAdapter : GoogleAdapter, IGoogleHttpAdapter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GoogleHttpAdapter(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public static readonly JsonSerializer GoogleBotMessageSerializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        });

        public async Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, CancellationToken cancellationToken = default(CancellationToken))
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

            var projectId = AuthenticationHelpers.GetProjectIdFromRequest(httpRequest);
            ActionProjectId = projectId;

            GoogleRequestBody actionRequest;
            Payload actionPayload;

            var memoryStream = new MemoryStream();
            httpRequest.Body.CopyTo(memoryStream);
            memoryStream.Position = 0;

            using (var bodyReader = new StreamReader(memoryStream, Encoding.UTF8))
            {
                var skillRequestContent = bodyReader.ReadToEnd();
                try
                {
                    actionRequest = JsonConvert.DeserializeObject<GoogleRequestBody>(skillRequestContent);
                    actionPayload = actionRequest.OriginalDetectIntentRequest.Payload;
                }
                catch
                {
                    actionPayload = JsonConvert.DeserializeObject<Payload>(skillRequestContent);
                }
            }

            var uniqueRequestId = _httpContextAccessor.HttpContext.Items["GoogleUniqueRequestId"]?.ToString();

            var googleResponse = await ProcessActivity(
                actionPayload,
                bot.OnTurnAsync,
                uniqueRequestId);

            if (googleResponse == null)
            {
                throw new ArgumentNullException(nameof(googleResponse));
            }

            httpResponse.ContentType = "application/json;charset=utf-8";
            httpResponse.StatusCode = (int)HttpStatusCode.OK;

            using (var writer = new StreamWriter(httpResponse.Body))
            {
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    GoogleBotMessageSerializer.Serialize(jsonWriter, googleResponse);
                }
            }
        }
    }
}
