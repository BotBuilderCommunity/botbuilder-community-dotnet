using System.Collections.Generic;

namespace Bot.Builder.Community.Cards.Management
{
    public class CardManagerMiddlewareOptions
    {
        public bool AutoAdaptCardActions { get; set; }

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

        public bool AutoEnableSentIds { get; set; }

        public bool AutoSaveActivitiesOnSend { get; set; }

        public bool AutoSeparateAttachmentsOnSend { get; set; }

        public bool TrackEnabledIds { get; set; }

        public PayloadIdOptions IdOptions { get; set; }
    }
}