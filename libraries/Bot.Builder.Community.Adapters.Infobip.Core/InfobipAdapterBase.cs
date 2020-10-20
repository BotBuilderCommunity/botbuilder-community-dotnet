using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Adapters.Infobip.Core
{
    public abstract class InfobipAdapterBase: BotAdapter, IBotFrameworkHttpAdapter
    {
        /// <summary>
        /// Accepts an incoming webhook request, creates a turn context,
        /// and runs the middleware pipeline for an incoming TRUSTED activity.
        /// </summary>
        /// <param name="httpRequest">Represents the incoming side of an HTTP request.</param>
        /// <param name="httpResponse">Represents the outgoing side of an HTTP request.</param>
        /// <param name="bot">The code to run at the end of the adapter's middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public abstract Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot,
            CancellationToken cancellationToken = new CancellationToken());
        
        /// <summary>
        /// Throws a <see cref="NotImplementedException"/> exception in all cases.
        /// </summary>
        /// <param name="turnContext">The context object for the turn.</param>
        /// <param name="activity">New replacement activity.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity,
            CancellationToken cancellationToken)
        {
            return Task.FromException<ResourceResponse>(
                new NotImplementedException("Infobip adapter does not support updateActivity."));
        }

        /// <summary>
        /// Throws a <see cref="NotImplementedException"/> exception in all cases.
        /// </summary>
        /// <param name="turnContext">The context object for the turn.</param>
        /// <param name="reference">Conversation reference for the activity to delete.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference,
            CancellationToken cancellationToken)
        {
            return Task.FromException<ResourceResponse>(
                new NotImplementedException("Infobip adapter does not support updateActivity."));
        }
    }
}
