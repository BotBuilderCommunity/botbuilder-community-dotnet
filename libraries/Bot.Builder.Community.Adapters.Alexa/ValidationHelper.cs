using System;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Bot.Builder.Community.Adapters.Alexa.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bot.Builder.Community.Adapters.Alexa
{
    internal class ValidationHelper
    {
        public static async Task<bool> ValidateRequest(HttpRequest request, SkillRequest skillRequest, string body, ILogger logger)
        {
            request.Headers.TryGetValue(AlexaMessageAuthorizationHandler.SignatureCertChainUrlHeader, out var signatureChainUrls);
            request.Headers.TryGetValue(AlexaMessageAuthorizationHandler.SignatureHeader, out var signatureHeaders);

            return await new AlexaMessageAuthorizationHandler(logger).ValidateSkillRequest(skillRequest, body, signatureChainUrls, signatureHeaders).ConfigureAwait(false);
        }
    }
}