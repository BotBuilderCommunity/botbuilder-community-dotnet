using System.Collections.Generic;

namespace Bot.Builder.Community.Cards.Management
{
    public class CardManagerOptions
    {
        public bool AutoApplyId { get; set; } = true;

        public bool AutoDisableOnAction { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to disable all ID's every time a message activity is sent from the bot.
        /// Only used when <see cref="CardManager{TState}.TrackEnabledIds">TrackEnabledIds</see> is <see langword="true"/>.
        /// </summary>
        /// <value>
        /// If true, the middleware will clear the enabled ID list every time a message activity is sent from the bot.
        /// </value>
        public bool AutoClearListOnSend { get; set; } = true;

        public bool AutoEnableSentId { get; set; } = true;

        public IdOptions IdOptions { get; set; } = new IdOptions(IdType.Batch);
    }
}