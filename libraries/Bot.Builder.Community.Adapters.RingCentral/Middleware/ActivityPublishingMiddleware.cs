using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.RingCentral.Handoff;
using Bot.Builder.Community.Adapters.RingCentral.Helpers;
using Bot.Builder.Community.Adapters.RingCentral.Schema;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bot.Builder.Community.Adapters.RingCentral.Middleware
{
    /// <summary>
    /// This middleware's primary purpose is to forward all the bot messages to RingCentral platform.
    /// It can be used when RingCentral is used to track all end user activities with the bot, including Microsoft bot adapter channels.
    /// </summary>
    public class ActivityPublishingMiddleware : IMiddleware
    {
        private readonly ILogger _logger;
        private readonly RingCentralClientWrapper _ringCentralClient;
        private readonly IHandoffRequestRecognizer _handoffRequestRecognizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityPublishingMiddleware"/> class.
        /// Creates a new <see cref="ActivityPublishingMiddleware"/>.
        /// </summary>
        /// <param name="ringCentralClient">Client that is used to publish the activities to the RingCentral platform.</param>
        /// <param name="handoffRequestRecognizer">Analyzer to detect a handoff request.</param>
        /// <param name="logger">Optional logger.</param>
        public ActivityPublishingMiddleware(RingCentralClientWrapper ringCentralClient, IHandoffRequestRecognizer handoffRequestRecognizer, ILogger logger = null)
        {
            _ringCentralClient = ringCentralClient ?? throw new ArgumentNullException(nameof(ringCentralClient));
            _handoffRequestRecognizer = handoffRequestRecognizer ?? throw new ArgumentNullException(nameof(handoffRequestRecognizer));
            _logger = logger ?? NullLogger.Instance;
        }

        /// <summary>
        /// Process incoming activity. This could be a message coming in from a channel (e.g. WebChat) or an
        /// agent reply coming RingCentral.
        /// </summary>
        /// <param name="turnContext"> The context object for this turn.</param>
        /// <param name="next">The delegate to call to continue the bot middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
        /// Middleware calls the next delegate to pass control to the next middleware in
        /// the pipeline. If middleware doesn’t call the next delegate, the adapter does
        /// not call any of the subsequent middleware’s request handlers or the bot’s receive
        /// handler, and the pipeline short circuits.
        /// The <paramref name="turnContext"/> provides information about the incoming activity, and other data
        /// needed to process the activity.
        /// </remarks>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            bool isFromRingCentral = MessageFromRingCentralOperator(turnContext.Activity);

            if (turnContext.Activity.Type == ActivityTypes.Message && !isFromRingCentral)
            {
                await _ringCentralClient.SendActivityToRingCentralAsync(turnContext.Activity).ConfigureAwait(false);

                var handoffRequestState = await _handoffRequestRecognizer.RecognizeHandoffRequestAsync(turnContext.Activity).ConfigureAwait(false);

                if (handoffRequestState != HandoffTarget.None)
                {
                    string foreignThreadId = RingCentralSdkHelper.BuildForeignThreadIdFromActivity(turnContext.Activity);
                    var thread = await _ringCentralClient.GetThreadByForeignThreadIdAsync(foreignThreadId).ConfigureAwait(false);

                    if (thread != null)
                    {
                        await _ringCentralClient.HandoffConversationControlToAsync(handoffRequestState, thread).ConfigureAwait(false);
                        await turnContext.SendActivityAsync($"Transfer to {handoffRequestState.ToString()} has been initiated.").ConfigureAwait(false);

                        if (handoffRequestState == HandoffTarget.Bot)
                        {
                            return;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Could not handoff the conversation, thread with foreign id \"{ForeignThreadId}\"could not be found.", foreignThreadId);
                    }
                }
            }

            // Hook on messages that are sent from the bot to the user
            turnContext.OnSendActivities(SendActivitiesToUserHook);

            await next(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets called, when the bot is sending activities back to the user.
        /// </summary>
        /// <param name="turnContext">The context object for the turn.</param>
        /// <param name="activities">The activities to send.</param>
        /// <param name="next">The delegate to call to continue event processing.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task<ResourceResponse[]> SendActivitiesToUserHook(ITurnContext turnContext, List<Activity> activities, Func<Task<ResourceResponse[]>> next)
        {
            // Run full pipeline
            var responses = await next().ConfigureAwait(false);

            foreach (var activity in activities)
            {
                // Send out messages from the bot to RingCentral
                if (activity.Type == ActivityTypes.Message)
                {
                    await _ringCentralClient.SendActivityToRingCentralAsync(activity).ConfigureAwait(false);
                }
            }
            return responses;
        }

        private bool MessageFromRingCentralOperator(Activity activity)
        {
            bool retVal = false;

            if (activity.Entities == null)
            {
                return retVal;
            }

            foreach (var a in activity?.Entities)
            {
                try
                {
                    if (a.Type == nameof(RingCentralMetadata))
                    {
                        var meta = a.GetAs<RingCentralMetadata>();
                        retVal = meta.IsHumanResponse;
                    }
                }
                catch 
                { 
                    // eat
                }
            }

            return retVal;
        }
    }
}
