using System;

namespace Bot.Builder.Community.Adapters.RingCentral
{
    /// <summary>
    /// Ids of channels supported by the RingCentral adapter.
    /// </summary>
    public static class RingCentralChannels
    {
        /// <summary>
        /// RingCentral WhatsApp channel.
        /// </summary>
        public const string WhatsApp = "whatsapp";

        /// <summary>
        /// RingCentral unspecific channel.
        /// </summary>
        public const string Unspecific = "unspecific";

        /// <summary>
        /// Gets the adapter's channel id based on the RingCentral message/resource type.
        /// </summary>
        /// <param name="resourceType">RingCentrla resource type.</param>
        /// <returns>String name of the channel.</returns>
        public static string GetFromResourceType(string resourceType)
        {
            string channel;
            
            switch (resourceType)
            {
                case string rt when rt.Equals("whats_app/message", StringComparison.InvariantCultureIgnoreCase):
                    channel = WhatsApp;
                    break;
                default:
                    channel = Unspecific;
                    break;
            }

            return channel;
        }
    }
}
