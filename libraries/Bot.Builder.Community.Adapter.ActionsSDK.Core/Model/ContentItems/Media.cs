using System.Collections.Generic;

namespace Bot.Builder.Community.Adapters.ActionsSDK.Core.Model.ContentItems
{
    public class Media
    { 
        public MediaType MediaType { get; set; }
        public string StartOffset { get; set; }
        public List<MediaObject> MediaObjects { get; set; }
        public List<OptionalMediaControl> OptionsMediaControls { get; set; }
    }
}
