using System.Collections.Generic;

namespace Bot.Builder.Community.Adapters.ActionsSDK.Core.Model.ContentItems
{
    public class ListContentItem : ContentItem
    {
        public string Title { get; set; }
        
        public string Subtitle { get; set; }

        public List<ListItem> Items { get; set; }
    }
}
