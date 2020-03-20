using System.Threading.Tasks;
using Alexa.NET.Request;
using Bot.Builder.Community.Adapters.Alexa.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bot.Builder.Community.Adapters.Alexa
{
    internal class ValidationHelper
    {
        public static async Task<bool> ValidateRequest(HttpRequest request, SkillRequest skillRequest, string body, string alexaSkillId, ILogger logger)
        {
            request.Headers.TryGetValue(AlexaAuthorizationHandler.SignatureCertChainUrlHeader, out var signatureChainUrls);
            request.Headers.TryGetValue(AlexaAuthorizationHandler.SignatureHeader, out var signatureHeaders);

            var validator = new AlexaAuthorizationHandler(logger);

            if (!await validator.ValidateSkillRequest(skillRequest, body, signatureChainUrls, signatureHeaders).ConfigureAwait(false))
                return false;

            // Alexa recommends you verify the Skill Id. Some bot developers use the same bot to service multiple skills. In this case they do their own validation
            // and set this value to null.
            if (alexaSkillId == null)
                return true;

            return validator.ValidateSkillId(skillRequest, alexaSkillId);
        }
    }
}