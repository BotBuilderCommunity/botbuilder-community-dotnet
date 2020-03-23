using System;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.RingCentral.Handoff
{
    /// <summary>
    /// Analze (incoming) activity to detect based on the activtiy text, whether 
    /// the user wants do handoff the conversation to a human or bot.
    /// </summary>
    public class StaticHandoffRequestRecognizer : IHandoffRequestRecognizer
    {
        /// <summary>
        /// Analyzes an activity to determine, if the conversation should be handed off or not.
        /// </summary>
        /// <param name="activity">Activity to be analyzed.</param>
        /// <returns>Target, where to handoff the conversation.</returns>
        public async Task<HandoffTarget> RecognizeHandoffRequestAsync(Activity activity)
        {
            _ = activity ?? throw new ArgumentNullException(nameof(activity));

            var activityText = activity.Text ?? "";

            var botRequested = activityText.Contains("bot", StringComparison.InvariantCultureIgnoreCase);
            var agentRequested = activityText.Contains("human", StringComparison.InvariantCultureIgnoreCase);

            if (botRequested)
            {
                return await Task.FromResult(HandoffTarget.Bot);
            }

            if (agentRequested)
            {
                return await Task.FromResult(HandoffTarget.Agent);
            }

            return await Task.FromResult(HandoffTarget.None);
        }
    }
}
