using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alexa.NET.CustomerProfile;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Bot.Builder.Community.Adapters.Alexa.Attachments;
using Microsoft.Bot.Builder;

namespace Bot.Builder.Community.Adapters.Alexa
{
    public static class AlexaContextExtensions
    {
        public static async Task AlexaSendProgressiveResponse(this ITurnContext context, string content)
        {
            var progressiveResponse = new ProgressiveResponse(context.GetAlexaRequestBody());
            await progressiveResponse.SendSpeech(content);
        }

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

        public static async Task AlexaSendPermissionConsentRequestActivity(this ITurnContext context, string message, List<string> permissions)
        {
            var activity = MessageFactory.Attachment(new PermissionConsentRequestAttachment(permissions), message);
            await context.SendActivityAsync(activity).ConfigureAwait(false);
        }

        public static CustomerProfileClient AlexaGetCustomerProfileClient(this ITurnContext context)
        {
            return new CustomerProfileClient(context.GetAlexaRequestBody());
        }

        public static Dictionary<string, object> AlexaSessionAttributes(this ITurnContext context)
        {
            return context.GetAlexaRequestBody().Session.Attributes;
        }

        [Obsolete("The AlexaGetUserAddress extension method is deprecated. Please use AlexaGetCustomerProfileClient instead.")]
        public static async Task<FullAddress> AlexaGetUserAddress(this ITurnContext context)
        {
            var profileClient = new CustomerProfileClient(context.GetAlexaRequestBody());
            return await profileClient.FullAddress();
        }

        [Obsolete("The AlexaGetCustomerProfile extension method is deprecated. Please use AlexaGetCustomerProfileClient instead.", true)]
        public static Task<string> AlexaGetCustomerProfile(this ITurnContext context, string item)
        {
            throw new NotImplementedException();
        }

        [Obsolete("The AlexaResponseDirectives extension method is deprecated. To send directives, please add DirectiveAttachments to the outgoing activity instead.", true)]
        public static List<object> AlexaResponseDirectives(this ITurnContext context)
        {
            throw new NotImplementedException();
        }

        public static void AlexaSetRepromptSpeech(this ITurnContext context, string repromptSpeech)
        {
            context.TurnState.Add("AlexaReprompt", repromptSpeech);
        }

        [Obsolete("The AlexaSetCard extension method is deprecated. To send a card, please add a CardAttachment to the outgoing activity instead.", true)]
        public static void AlexaSetCard(this ITurnContext context, ICard card)
        {
            throw new NotImplementedException();
        }
    }
}
