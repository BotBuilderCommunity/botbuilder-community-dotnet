using System;
using System.Collections.Generic;
using System.Text;
using JWT.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapters.Google.Helpers
{
    public static class AuthenticationHelpers
    {
        public static string GetProjectIdFromRequest(HttpRequest httpRequest)
        {
            string payload = new JwtBuilder().Decode(httpRequest.Headers["Authorization"]);
            JObject payloadJObj = JObject.Parse(payload);
            return (string)payloadJObj["aud"];
        }
    }
}
