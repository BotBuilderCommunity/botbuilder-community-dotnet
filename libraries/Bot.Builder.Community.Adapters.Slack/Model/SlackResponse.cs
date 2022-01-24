using Bot.Builder.Community.Adapters.Slack.Model.Events;

namespace Bot.Builder.Community.Adapters.Slack.Model
{
    /// <summary>
    /// SlackResponse class.
    /// </summary>
    public class SlackResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the Ok status is true or false.
        /// </summary>
        /// <value>The ok status of the response.</value>
        public bool Ok { get; set; }

        /// <summary>
        /// Gets or sets the Channel property.
        /// </summary>
        /// <value>The channel.</value>
        public string Channel { get; set; }

        /// <summary>
        /// Gets or sets the Ts property.
        /// </summary>
        /// <value>The timestamp.</value>
        public string Ts { get; set; }

        /// <summary>
        /// Gets or sets the Message property.
        /// </summary>
        /// <value>The message.</value>
        public MessageEvent Message { get; set; }
    }
}
