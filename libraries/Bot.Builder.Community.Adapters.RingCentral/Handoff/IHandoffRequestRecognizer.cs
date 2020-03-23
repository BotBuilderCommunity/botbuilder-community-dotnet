using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.RingCentral.Handoff
{
    /// <summary>
    /// Analze (incoming) activity to detect, whether 
    /// the user wants do handoff the conversation to a human or bot.
    /// </summary>
    public interface IHandoffRequestRecognizer
    {
        /// <summary>
        /// Analyzes an activity to determine, if the conversation should be handed off or not.
        /// </summary>
        /// <param name="activity">Activity to be analyzed.</param>
        /// <returns>Target, where to handoff the conversation.</returns>
        Task<HandoffTarget> RecognizeHandoffRequestAsync(Activity activity);
    }
}
