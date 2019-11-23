using System.Collections.Generic;
using Alexa.NET.Request;
using Microsoft.Bot.Builder;

namespace Bot.Builder.Community.Adapters.Alexa
{
    public static class AlexaContextExtensions
    {
        public static Dictionary<string, object> AlexaSessionAttributes(this ITurnContext context)
        {
            return context.TurnState.Get<Dictionary<string, object>>("AlexaSessionAttributes");
        }

        //public static List<IAlexaDirective> AlexaResponseDirectives(this ITurnContext context)
        //{
        //    return context.TurnState.Get<List<IAlexaDirective>>("AlexaResponseDirectives");
        //}

        //public static void AlexaSetRepromptSpeech(this ITurnContext context, string repromptSpeech)
        //{
        //    context.TurnState.Add("AlexaReprompt", repromptSpeech);
        //}

        //public static void AlexaSetCard(this ITurnContext context, AlexaCard card)
        //{
        //    context.TurnState.Add("AlexaCard", card);
        //}

        //public static async Task<HttpResponseMessage> AlexaSendProgressiveResponse(this ITurnContext context, string content)
        //{
        //    var originalAlexaRequest = (AlexaRequestBody)context.Activity.ChannelData;

        //    var directive = new AlexaDirectiveRequest()
        //    {
        //        Header = new AlexaDirectiveRequest.DirectiveHeader()
        //        {
        //            RequestId = originalAlexaRequest.Request.RequestId
        //        },
        //        Directive = new AlexaDirectiveRequest.DirectiveContent()
        //        {
        //            Type = "VoicePlayer.Speak",
        //            Speech = content
        //        }
        //    };

        //    var client = new HttpClient();

        //    var jsonRequest = JsonConvert.SerializeObject(directive,
        //        new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

        //    var directiveContent = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");
        //    var directiveEndpoint = $"{originalAlexaRequest.Context.System.ApiEndpoint}/v1/directives";

        //    client.DefaultRequestHeaders.Authorization =
        //        new AuthenticationHeaderValue("Bearer", originalAlexaRequest.Context.System.ApiAccessToken);

        //    return await client.PostAsync(directiveEndpoint, directiveContent);
        //}

        public static SkillRequest GetAlexaRequestBody(this ITurnContext context)
        {
            try
            {
                return (SkillRequest)context.Activity.ChannelData;
            }
            catch
            {
                return null;
            }
        }

        public static bool AlexaDeviceHasDisplay(this ITurnContext context)
        {
            var alexaRequest = (SkillRequest)context.Activity.ChannelData;
            var hasDisplay =
                alexaRequest?.Context?.System?.Device?.SupportedInterfaces?.ContainsKey("Display");
            return hasDisplay.HasValue && hasDisplay.Value;
        }

        public static bool AlexaDeviceHasAudioPlayer(this ITurnContext context)
        {
            var alexaRequest = (SkillRequest)context.Activity.ChannelData;
            var hasDisplay =
                alexaRequest?.Context?.System?.Device?.SupportedInterfaces?.ContainsKey("AudioPlayer");
            return hasDisplay.HasValue && hasDisplay.Value;
        }

        //public static async Task<Address> AlexaGetUserAddress(this ITurnContext context)
        //{
        //    var originalAlexaRequest = (SkillRequest)context.Activity.ChannelData;

        //    var deviceId = originalAlexaRequest.Context.System.Device.DeviceID;

        //    var client = new HttpClient();

        //    var directiveEndpoint = $"{originalAlexaRequest.Context.System.ApiEndpoint}/v1/devices/{deviceId}/settings/address";

        //    client.DefaultRequestHeaders.Authorization =
        //        new AuthenticationHeaderValue("Bearer", originalAlexaRequest.Context.System.ApiAccessToken);

        //    var response = await client.GetAsync(directiveEndpoint);

        //    if (response.StatusCode == System.Net.HttpStatusCode.OK)
        //    {
        //        var responseContent = await response.Content.ReadAsStringAsync();
        //        var address = JsonConvert.DeserializeObject<AlexaAddress>(responseContent);
        //        return address;
        //    }
        //    else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        //    {
        //        throw new UnauthorizedAccessException($"Alexa API returned status " +
        //            $"code {response.StatusCode} with message {response.ReasonPhrase}.  " +
        //            $"This potentially means that the user has not granted your skill " +
        //            $"permission to access their address.");
        //    }

        //    throw new Exception($"Alexa API returned status code " +
        //        $"{response.StatusCode} with message {response.ReasonPhrase}");
        //}

        //public static async Task<string> AlexaGetCustomerProfile(this ITurnContext context, string item)
        //{
        //    if ((item != AlexaCustomerItem.Name) & (item != AlexaCustomerItem.GivenName) & (item != AlexaCustomerItem.Email) & (item != AlexaCustomerItem.MobileNumber))
        //        throw new ArgumentException($"Invalid AlexaGetCustomerProfile item: {item}");

        //    var originalAlexaRequest = (AlexaRequestBody)context.Activity.ChannelData;

        //    var client = new HttpClient();

        //    var directiveEndpoint = $"{originalAlexaRequest.Context.System.ApiEndpoint}/v2/accounts/~current/settings/Profile.{item}";

        //    client.DefaultRequestHeaders.Authorization =
        //        new AuthenticationHeaderValue("Bearer", originalAlexaRequest.Context.System.ApiAccessToken);

        //    var response = await client.GetAsync(directiveEndpoint);

        //    if (response.StatusCode == System.Net.HttpStatusCode.OK)
        //    {
        //        var responseContent = await response.Content.ReadAsStringAsync();
        //        var data = JsonConvert.DeserializeObject<string>(responseContent);
        //        return data;
        //    }
        //    else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        //    {
        //        throw new UnauthorizedAccessException($"Alexa API returned status " +
        //            $"code {response.StatusCode} with message {response.ReasonPhrase}.  " +
        //            $"This potentially means that the user has not granted your skill " +
        //            $"permission to access their profile item {item}.");
        //    }

        //    throw new Exception($"Alexa API returned status code " +
        //        $"{response.StatusCode} with message {response.ReasonPhrase}");
        //}
    }
}
