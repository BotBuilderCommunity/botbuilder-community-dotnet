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
        /// Only used when <see cref="CardManager{TState}.TrackEnabledIds">TrackEnabledIds</see> is <see langword="true"/>.
        /// </summary>
        /// <value>
        /// If true, the middleware will clear the enabled ID list every time a message activity is sent from the bot.
        /// </value>
        public bool AutoClearTrackedOnSend { get; set; }

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

        /// <summary>
        /// Gets or sets a value indicating whether enabled or disabled ID's are tracked.
        /// This effectively toggles the ID-tracking "style."
        /// Setting this to false will not prevent ID's from being tracked, it will only change
        /// the way ID's are tracked. If you don't want to track ID's then you should modify
        /// <see cref="AutoApplyIds"/>, <see cref="AutoDisableOnAction"/>, <see cref="AutoEnableOnSend"/>,
        /// and/or <see cref="IdOptions"/>.
        /// </summary>
        /// <value>
        /// If true, enabled ID's will be tracked. If false, disabled ID's will be tracked.
        /// </value>
        public bool TrackEnabledIds { get; set; }

        public PayloadIdOptions IdOptions { get; set; }
    }
}