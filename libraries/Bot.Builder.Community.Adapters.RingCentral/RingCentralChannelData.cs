using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Builder.Community.Adapters.RingCentral
{
    /// <summary>
    /// Channel data that are specific to RingCentral messages.
    /// </summary>
    public class RingCentralChannelData
    {
        /// <summary>
        /// Gets or sets id of the RingCentral source.
        /// </summary>
        /// <value>
        /// The Id of the RingCentral source.
        /// </value>
        public string SourceId { get; set; }

        /// <summary>
        /// Gets or sets id of the RingCentral thread.
        /// </summary>
        /// <value>
        /// The Id of the RingCentral thread.
        /// </value>
        public string ThreadId { get; set; }
    }
}
