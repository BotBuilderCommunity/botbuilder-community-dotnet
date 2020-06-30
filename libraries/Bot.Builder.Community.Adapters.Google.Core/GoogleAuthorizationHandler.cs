using System;
using JWT.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapters.Google.Core
{
    /// <summary>
    /// Google Authorization Handler.
    /// </summary>
    public class GoogleAuthorizationHandler
    {
        private readonly ILogger _logger;

        public GoogleAuthorizationHandler(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public static bool ValidateActionProjectId(string authorizationHeader, string authorizationValue)
        {
            if (authorizationHeader == authorizationValue)
            {
                return true;
            }

            var payload = new JwtBuilder().Decode(authorizationHeader);
            var payloadJObj = JObject.Parse(payload);
            var aud = (string)payloadJObj["aud"];
            return aud.ToLowerInvariant() == authorizationValue.ToLowerInvariant();
        }
    }
}
