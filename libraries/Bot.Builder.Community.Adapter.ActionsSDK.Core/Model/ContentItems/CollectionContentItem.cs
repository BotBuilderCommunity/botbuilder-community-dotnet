using System.Collections.Generic;

namespace Bot.Builder.Community.Adapters.ActionsSDK.Core.Model.ContentItems
{
    public class CollectionContentItem : ContentItem
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public ImageFill ImageFill { get; set; }
        public List<CollectionItem> Items { get; set; }
    }
}
