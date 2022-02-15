namespace Bot.Builder.Community.Adapters.Slack
{
    /// <summary>
    /// Class for defining implementation of the SlackAdapter Options.
    /// </summary>
    public class SlackAdapterOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the signatures of incoming requests should be verified.
        /// </summary>
        /// <value>
        /// A value indicating whether the signatures of incoming requests should be verified.
        /// </value>
        public bool VerifyIncomingRequests { get; set; } = true;
    }
}
