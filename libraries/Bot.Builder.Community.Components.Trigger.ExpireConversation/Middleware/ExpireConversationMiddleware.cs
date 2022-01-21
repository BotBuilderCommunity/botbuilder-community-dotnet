using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Components.Trigger.ExpireConversation.Middleware
{
    public class ExpireConversationMiddleware : IMiddleware
    {
        protected IStatePropertyAccessor<DateTime> LastAccssedTimeProperty;

        private readonly ConversationState _conversationState;
        public static double ExpireAfterSeconds { get; set; }
        
        public ExpireConversationMiddleware(ConversationState conversationState)
        {
            _conversationState = conversationState;
            LastAccssedTimeProperty = conversationState.CreateProperty<DateTime>(nameof(LastAccssedTimeProperty));
        }
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next,
            CancellationToken cancellationToken = new CancellationToken())
        {

            var lastChatTime = await LastAccssedTimeProperty
                .GetAsync(turnContext, () => DateTime.UtcNow, cancellationToken).ConfigureAwait(false);

            if (ExpireAfterSeconds > 0)
            {
                var timeInterval = DateTime.UtcNow - lastChatTime;

                if (timeInterval >= TimeSpan.FromSeconds(ExpireAfterSeconds))
                {
                    // Clear state.
                    await _conversationState.ClearStateAsync(turnContext, cancellationToken).ConfigureAwait(false);

                    var conversationExpire = new
                    {
                        TimeDelay = timeInterval,
                        ExceptedWaitingTime = ExpireAfterSeconds,
                        ReceivedType = turnContext.Activity.Type
                    };


                    turnContext.Activity.Type = ConversationActivity.ActivityName;


                    var expireDetails = JsonConvert.SerializeObject(conversationExpire);
                    ObjectPath.SetPathValue(turnContext.TurnState, "turn.expire", JObject.Parse(expireDetails));
                    

                }
            }

            
            // Set LastAccessedTime to the current time.
            await LastAccssedTimeProperty.SetAsync(turnContext, DateTime.UtcNow, cancellationToken)
                    .ConfigureAwait(false);

            // Save any state changes that might have occurred during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken)
                .ConfigureAwait(false);
            
            await next(cancellationToken);

        }
    }
}
