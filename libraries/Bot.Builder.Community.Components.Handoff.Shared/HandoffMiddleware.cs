using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Components.Handoff.Shared
{
    public abstract class HandoffMiddleware : IMiddleware
    {
        private readonly ConversationHandoffRecordMap _conversationHandoffRecordMap;

        public HandoffMiddleware(ConversationHandoffRecordMap conversationHandoffRecordMap)
        {
            _conversationHandoffRecordMap = conversationHandoffRecordMap;
        }

        public virtual async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            // Get the conversation escalation record - if null then the conversation has not been handed off
            var handoffRecord = await GetConversationHandoffRecordIfExists(turnContext);

            // If an escalation record exists then we should route the message through to LivePerson
            if (turnContext.Activity.Type == ActivityTypes.Message && handoffRecord != null)
            {
                await RouteActivityToExistingHandoff(turnContext, handoffRecord);
                return;
            }

            // Hook into the onSendActivities event.  Check for outgoing HandoffEvent initiate events. If we find one
            // this means we should start a new connection with LivePerson and create an escalation record so that future
            // messages are routed correctly.
            turnContext.OnSendActivities(async (sendTurnContext, activities, nextSend) =>
            {
                var responses = new ResourceResponse[0];

                // Handle any escalation events, and let them propagate through the pipeline
                // This is useful for debugging with the Emulator
                var handoffEvents = activities.Where(activity =>
                    activity.Type == ActivityTypes.Event 
                    && (activity.Name == HandoffEventNames.InitiateHandoff || activity.Name == HandoffEventNames.HandoffStatus));

                var eventActivity = handoffEvents.ToList().FirstOrDefault();

                if (eventActivity != null)
                {
                    switch (eventActivity.Name)
                    {
                        case HandoffEventNames.HandoffStatus:
                            try
                            {
                                var state = (eventActivity.Value as JObject)?.Value<string>("state");
                                if (state == "completed")
                                {
                                    await HandleHandoffStatusCompletedEvent(turnContext, handoffRecord);
                                }
                                else
                                {
                                    await HandleHandoffStatusEvent(turnContext, handoffRecord);
                                }
                            }
                            catch { }
                            break;
                        case HandoffEventNames.InitiateHandoff:
                            handoffRecord = await Escalate(sendTurnContext, eventActivity).ConfigureAwait(false);
                            await _conversationHandoffRecordMap.Add(eventActivity.Conversation.Id, handoffRecord);
                            break;
                        default:
                            // run full pipeline
                            responses = await nextSend().ConfigureAwait(false);
                            break;
                    }
                }
                else
                {
                    // run full pipeline
                    responses = await nextSend().ConfigureAwait(false);
                }

                return responses;
            });

            await next(cancellationToken).ConfigureAwait(false);
        }

        public virtual Task HandleHandoffStatusEvent(ITurnContext turnContext, HandoffRecord handoffRecord)
        {
            return Task.CompletedTask;
        }

        public virtual async Task<HandoffRecord> GetConversationHandoffRecordIfExists(ITurnContext turnContext)
        {
            return await _conversationHandoffRecordMap.GetByConversationId(turnContext.Activity.Conversation.Id);
        }

        public virtual async Task HandleHandoffStatusCompletedEvent(ITurnContext turnContext, HandoffRecord handoffRecord)
        {
            await _conversationHandoffRecordMap.Remove(turnContext.Activity.Conversation.Id);
        }

        public abstract Task RouteActivityToExistingHandoff(ITurnContext turnContext, HandoffRecord handoffRecord);

        public abstract Task<HandoffRecord> Escalate(ITurnContext turnContext, IEventActivity handoffEvent);
    }
}
