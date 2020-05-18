using System.Collections.Generic;

namespace Bot.Builder.Community.Cards.Management
{
    public class CardManagerMiddlewareOptions
    {
        public bool AutoAdaptOutgoingCardActions { get; set; }

        public bool AutoApplyIds { get; set; }

        public bool AutoDisableOnAction { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to disable all ID's every time a message activity is sent from the bot.
        /// Only used when <see cref="IdTrackingStyle">IdTrackingStyle</see> is
        /// <see cref="TrackingStyle.TrackEnabled">TrackEnabled</see>.
        /// </summary>
        /// <value>
        /// If true, the middleware will clear the enabled ID list every time a message activity is sent from the bot.
        /// </value>
        public bool AutoClearEnabledOnSend { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to automatically convert outgoing Adaptive Cards to JObject instances.
        /// </summary>
        /// <value>
        /// If true, outgoing Adaptive Cards will automatically be converted to JObject instances to work around this issue:
        /// https://github.com/microsoft/AdaptiveCards/issues/2148.
        /// </value>
        public bool AutoConvertAdaptiveCards { get; set; }

        public bool AutoDeleteOnAction { get; set; }

        public bool AutoEnableOnSend { get; set; }

        public bool AutoSaveActivitiesOnSend { get; set; }

        public bool AutoSeparateAttachmentsOnSend { get; set; }

        public TrackingStyle IdTrackingStyle { get; set; }

        public DataIdOptions IdOptions { get; set; }
    }
}