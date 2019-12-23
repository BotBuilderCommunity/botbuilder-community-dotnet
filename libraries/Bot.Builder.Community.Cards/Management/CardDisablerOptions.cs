using System.Collections.Generic;

namespace Bot.Builder.Community.Cards.Management
{
    public class CardDisablerOptions
    {
        public bool AutoApplyId { get; set; } = true;

        public bool AutoDisable { get; set; } = true;

        public bool TrackEnabledIds { get; set; } = true;

        public IdOptions IdOptions { get; set; } = new IdOptions(IdType.Card);
    }
}