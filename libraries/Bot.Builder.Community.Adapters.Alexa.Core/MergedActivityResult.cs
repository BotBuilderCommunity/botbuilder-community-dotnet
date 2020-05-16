using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Alexa.Core
{
    /// <summary>
    /// The result of merging a collection of Activities.
    /// </summary>
    public class MergedActivityResult
    {
        /// <summary>
        /// The merged activity
        /// </summary>
        public Activity MergedActivity;

        /// <summary>
        /// Is end of conversation flagged (outside of the properties in the merged activity itself)
        /// </summary>
        public bool EndOfConversationFlagged;
    }
}
