using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapter.ActionsSDK.Core.Model.ContentItems
{
    public class CollectionContentItem : ContentItem
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public ImageFill ImageFill { get; set; }
        public List<CollectionItem> Items { get; set; }
    }
}
