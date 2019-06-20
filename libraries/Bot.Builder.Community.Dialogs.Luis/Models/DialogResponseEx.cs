namespace Bot.Builder.Community.Dialogs.Luis.Models
{
    public partial class DialogResponse
    {
        /// <summary>
        /// Possible values for <see cref="DialogResponse.Status"/>
        /// </summary>
        public static class DialogStatus
        {
            /// <summary>
            /// Send the prompt in <see cref="DialogResponse.Prompt"/>
            /// </summary>
            public const string Question = "Question";

            /// <summary>
            /// Dialog is finished.
            /// </summary>
            public const string Finished = "Finished";
        }
    }
}
