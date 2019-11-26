using System;
using System.Threading;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Alexa.Middleware
{
    public class AlexaRequestToMessageEventActivitiesMiddleware : IMiddleware
    {
        private readonly string _slotName;

        public AlexaRequestToMessageEventActivitiesMiddleware(string defaultIntentSlotName = "phrase")
        {
            _slotName = defaultIntentSlotName;
        }

        public async Task OnTurnAsync(
            ITurnContext context, 
            NextDelegate next,
            CancellationToken cancellationToken = default)
        {
            if (context.Activity.ChannelId == "alexa")
            {
                var skillRequest = (SkillRequest)context.Activity.ChannelData;

                if (context.Activity.Type == "IntentRequest" 
                    && skillRequest.Request is IntentRequest intentRequest
                    && intentRequest.Intent.Slots != null
                    && intentRequest.Intent.Slots.ContainsKey(_slotName))
                {
                    context.Activity.Type = ActivityTypes.Message;
                    context.Activity.Text = intentRequest.Intent.Slots[_slotName].Value;
                    context.Activity.Value = intentRequest;
                }
                else
                {
                    context.Activity.Type = ActivityTypes.Event;
                    context.Activity.Name = skillRequest.Request.Type;

                    switch (skillRequest.Request.Type)
                    {
                        case "AccountLinkSkillEventRequest":
                            context.Activity.Value = skillRequest.Request as AccountLinkSkillEventRequest;
                            break;
                        case "AudioPlayerRequest":
                            context.Activity.Value = skillRequest.Request as AudioPlayerRequest;
                            break;
                        case "DisplayElementSelectedRequest":
                            context.Activity.Value = skillRequest.Request as DisplayElementSelectedRequest;
                            break;
                        case "LaunchRequest":
                            context.Activity.Value = skillRequest.Request as LaunchRequest;
                            break;
                        case "PermissionSkillEventRequest":
                            context.Activity.Value = skillRequest.Request as PermissionSkillEventRequest;
                            break;
                        case "PlaybackControllerRequest":
                            context.Activity.Value = skillRequest.Request as PlaybackControllerRequest;
                            break;
                        case "SessionEndedRequest":
                            context.Activity.Value = skillRequest.Request as SessionEndedRequest;
                            break;
                        case "SkillEventRequest":
                            context.Activity.Value = skillRequest.Request as SkillEventRequest;
                            break;
                        case "SystemExceptionRequest":
                            context.Activity.Value = skillRequest.Request as SystemExceptionRequest;
                            break;
                    }
                }
            }

            await next(cancellationToken).ConfigureAwait(false);
        }
    }
}
