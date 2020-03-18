using System;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Microsoft.Extensions.Logging;

namespace Bot.Builder.Community.Adapters.Alexa.Core
{
    /// <summary>
    /// Alexa Authorization Handler.
    /// </summary>
    public class AlexaAuthorizationHandler
    {
        /// <summary>
        /// SignatureCertChainUrl header name.
        /// </summary>
        public const string SignatureCertChainUrlHeader = @"SignatureCertChainUrl";

        /// <summary>
        /// Signature header name.
        /// </summary>
        public const string SignatureHeader = @"Signature";

        private readonly ILogger _logger;

        public AlexaAuthorizationHandler(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Authorize the SkillRequest is coming from Alexa.
        /// </summary>
        /// <param name="skillRequest">Incoming SkillRequest from Alexa.</param>
        /// <param name="requestBody">Full request body from Alexa.</param>
        /// <param name="signatureChainUrl">Signature Chain Url. This is the SignatureCertChainUrl header value.</param>
        /// <param name="signature">Signature. This is the Signature header value.</param>
        /// <returns>True if this is a valid SkillRequest otherwise false.</returns>
        public async Task<bool> ValidateSkillRequest(SkillRequest skillRequest, string requestBody, string signatureChainUrl, string signature)
        {
            if (string.IsNullOrWhiteSpace(signatureChainUrl))
            {
                _logger.LogError("Validation failed due to empty SignatureCertChainUrl header");
                return false;
            }

            Uri certUrl;
            try
            {
                certUrl = new Uri(signatureChainUrl);
            }
            catch
            {
                _logger.LogError($"Validation failed. SignatureChainUrl not valid: {signatureChainUrl}");
                return false;
            }

            if (string.IsNullOrWhiteSpace(signature))
            {
                _logger.LogError("Validation failed - Empty Signature header");
                return false;
            }

            var isTimestampValid = RequestVerification.RequestTimestampWithinTolerance(skillRequest);
            var valid = await RequestVerification.Verify(signature, certUrl, requestBody);

            if (!valid || !isTimestampValid)
            {
                _logger.LogError("Validation failed - RequestVerification failed");
                return false;
            }

            return true;
        }
    }
}
