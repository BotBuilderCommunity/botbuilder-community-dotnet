using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace Bot.Builder.Community.Middleware.HandleActivityType
{
    public class HandleActivityTypeMiddleware : IMiddleware
    {
        /// <summary>
        /// The type of Activity that the middleware should check for. e.g. ConversationUpdated or Message
        /// </summary>
        private readonly string _activityType;

        /// <summary>
        /// Handler to call when a matching Activity type is received
        /// </summary>
        private readonly Func<ITurnContext, MiddlewareSet.NextDelegate, Task> _handler;

        /// <summary>
        /// Allows you to respond to particular Activity types within the middleware pipeline. 
        /// e.g. sending the user a greeting or deleting user data
        /// </summary>
        /// <param name="activityType">The type of Activity the middleware should handle. Activity types can be found in the ActivityTypes enum.</param>
        /// <param name="handler">Handler to execute when the incoming type of activity is a match.</param>
        public HandleActivityTypeMiddleware(string activityType, Func<ITurnContext, MiddlewareSet.NextDelegate, Task> handler)
        {
            if (string.IsNullOrEmpty(activityType))
                throw new ArgumentNullException(nameof(activityType));

            _activityType = activityType;
            _handler = handler;
        }

        public delegate Task ActivityTypeHandler(ITurnContext context, MiddlewareSet.NextDelegate next);

        public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            if (string.Equals(context.Activity.Type, _activityType, StringComparison.InvariantCultureIgnoreCase))
            {
                // if the incoming Activity type matches the type of activity we are checking for then
                // invoke our handler
                await _handler.Invoke(context, next).ConfigureAwait(false);
            }
            else
            {
                // If the incoming Activity is not a match then continue routing
                await next().ConfigureAwait(false);
            }
        }
    }
}
