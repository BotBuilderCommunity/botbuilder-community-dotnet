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
        /// Id of the RingCentral source.
        /// </summary>
        public string SourceId { get; set; }

        /// <summary>
        /// Id of the RingCentral thread.
        /// </summary>
        public string ThreadId { get; set; }
    }
}
