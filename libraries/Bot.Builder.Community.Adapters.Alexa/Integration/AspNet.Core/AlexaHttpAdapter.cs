using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Alexa.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Bot.Builder.Community.Adapters.Alexa.Integration.AspNet.Core
{
    public class AlexaHttpAdapter : AlexaAdapter
    {
        public bool ValidateRequests { get; set; }

        public AlexaHttpAdapter(bool validateRequests, bool shouldEndSessionByDefault, bool translateCardAttachments)
            : base(shouldEndSessionByDefault, translateCardAttachments)
        {
            ValidateRequests = validateRequests;
        }

        public static readonly JsonSerializer AlexaBotMessageSerializer = JsonSerializer.Create(new JsonSerializerSettings
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

            AlexaRequestBody skillRequest;

            var memoryStream = new MemoryStream();
            httpResponse.Body.CopyTo(memoryStream);
            var requestBytes = memoryStream.ToArray();
            memoryStream.Position = 0;

            using (var bodyReader = new JsonTextReader(new StreamReader(memoryStream, Encoding.UTF8)))
            {
                skillRequest = AlexaBotMessageSerializer.Deserialize<AlexaRequestBody>(bodyReader);
            }

            if (skillRequest.Version != "1.0")
                throw new Exception($"Unexpected version of '{skillRequest.Version}' received.");

            if (ValidateRequests)
            {
                httpRequest.Headers.TryGetValue("SignatureCertChainUrl", out var certUrls);
                httpRequest.Headers.TryGetValue("Signature", out var signatures);
                var certChainUrl = certUrls.FirstOrDefault();
                var signature = signatures.FirstOrDefault();
                await AlexaValidateRequestSecurityHelper.Validate(skillRequest, requestBytes, certChainUrl, signature);
            }

            var alexaResponse = await ProcessActivity(
                skillRequest,
                bot.OnTurnAsync);

            if (alexaResponse == null)
            {
                throw new ArgumentNullException(nameof(alexaResponse));
            }

            httpResponse.ContentType = "application/json";
            httpResponse.StatusCode = (int)HttpStatusCode.OK;

            using (var writer = new StreamWriter(httpResponse.Body))
            {
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    AlexaBotMessageSerializer.Serialize(jsonWriter, alexaResponse.Response);
                }
            }
        }
    }
}
