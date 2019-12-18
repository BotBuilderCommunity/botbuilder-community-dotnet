using System;
using System.Collections.Generic;
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

                if (skillRequest != null)
                {
                    if (skillRequest.Request is IntentRequest intentRequest
                        && intentRequest.Intent.Slots != null
                        && intentRequest.Intent.Slots.ContainsKey(_slotName))
                    {
                        context.Activity.Type = ActivityTypes.Message;
                        context.Activity.Text = intentRequest.Intent.Slots[_slotName].Value;
                        context.Activity.Value = intentRequest;
                    }
                    else if (skillRequest.Request is LaunchRequest launchRequest)
                    {
                        context.Activity.Type = ActivityTypes.ConversationUpdate;
                        context.Activity.MembersAdded = new List<ChannelAccount>() { new ChannelAccount() { Id = skillRequest.Session.User.UserId } };
                        context.Activity.Value = launchRequest;
                    }
                    else
                    {
                        context.Activity.Type = ActivityTypes.Event;
                        context.Activity.Name = skillRequest.Request.Type;

                        switch (skillRequest.Request)
                        {
                            case IntentRequest skillIntentRequest:
                                context.Activity.Value = skillIntentRequest;
                                break;
                            case AccountLinkSkillEventRequest accountLinkSkillEventRequest:
                                context.Activity.Value = accountLinkSkillEventRequest;
                                break;
                            case AudioPlayerRequest audioPlayerRequest:
                                context.Activity.Value = audioPlayerRequest;
                                break;
                            case DisplayElementSelectedRequest displayElementSelectedRequest:
                                context.Activity.Value = displayElementSelectedRequest;
                                break;
                            case PermissionSkillEventRequest permissionSkillEventRequest:
                                context.Activity.Value = permissionSkillEventRequest;
                                break;
                            case PlaybackControllerRequest playbackControllerRequest:
                                context.Activity.Value = playbackControllerRequest;
                                break;
                            case SessionEndedRequest sessionEndedRequest:
                                context.Activity.Value = sessionEndedRequest;
                                break;
                            case SkillEventRequest skillEventRequest:
                                context.Activity.Value = skillEventRequest;
                                break;
                            case SystemExceptionRequest systemExceptionRequest:
                                context.Activity.Value = systemExceptionRequest;
                                break;
                        }
                    }
                }
            }

            await next(cancellationToken).ConfigureAwait(false);
        }
    }
}
