using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Builder.Community.Adapter.ActionsSDK.Core.Model.ContentItems
{
    public class ListContentItem : ContentItem
    {
        public string Title { get; set; }
        
        public string Subtitle { get; set; }

        public List<ListItem> Items { get; set; }
    }
}
