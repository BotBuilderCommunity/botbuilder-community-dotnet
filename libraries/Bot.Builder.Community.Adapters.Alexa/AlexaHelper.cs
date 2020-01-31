using System;
using System.Linq;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bot.Builder.Community.Adapters.Alexa
{
    internal class AlexaHelper
    {
        public static async Task<bool> ValidateRequest(HttpRequest request, SkillRequest skillRequest, string body, ILogger logger)
        { 
            request.Headers.TryGetValue("SignatureCertChainUrl", out var signatureChainUrl);
            if (string.IsNullOrWhiteSpace(signatureChainUrl))
            {
                logger.LogError("Validation failed due to empty SignatureCertChainUrl header");
                return false;
            }

            Uri certUrl;
            try
            {
                certUrl = new Uri(signatureChainUrl);
            }
            catch
            {
                logger.LogError($"Validation failed. SignatureChainUrl not valid: {signatureChainUrl}");
                return false;
            }

            request.Headers.TryGetValue("Signature", out var signature);
            if (string.IsNullOrWhiteSpace(signature))
            {
                logger.LogError("Validation failed - Empty Signature header");
                return false;
            }

            var isTimestampValid = RequestVerification.RequestTimestampWithinTolerance(skillRequest);
            var valid = await RequestVerification.Verify(signature, certUrl, body);

            if (!valid || !isTimestampValid)
            {
                logger.LogError("Validation failed - RequestVerification failed");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Under certain circumstances, such as the inclusion of certain types of directives
        /// on a response, should force the 'ShouldEndSession' property not be included on
        /// an outgoing response. This method determines if this property is allowed to have
        /// a value assigned.
        /// </summary>
        /// <param name="response">Boolean indicating if the 'ShouldEndSession' property can be populated on the response.'</param>
        /// <returns>bool</returns>
        internal static bool ShouldSetEndSession(SkillResponse response)
        {
            if (response.Response.Directives.Any(d => d is IEndSessionDirective))
            {
                return false;
            }

            return true;
        }
    }
}