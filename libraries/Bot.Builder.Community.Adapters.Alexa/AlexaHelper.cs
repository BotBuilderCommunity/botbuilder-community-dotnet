using System;
using System.IO;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bot.Builder.Community.Adapters.Alexa
{
    internal class AlexaHelper
    {
        public static async Task<bool> ValidateRequest(HttpRequest request, ILogger logger, SkillRequest skillRequest)
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

            request.Body.Position = 0;
            string body;
            
            using (var sr = new StreamReader(request.Body))
            {
                body = await sr.ReadToEndAsync();
            }

            request.Body.Position = 0;

            if (string.IsNullOrWhiteSpace(body))
            {
                logger.LogError("Validation failed - the JSON is empty");
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
    }
}