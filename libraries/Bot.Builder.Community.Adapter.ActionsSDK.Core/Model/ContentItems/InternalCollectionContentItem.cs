using System.Collections.Generic;

namespace Bot.Builder.Community.Adapters.ActionsSDK.Core.Model.ContentItems
{
    internal class InternalCollectionContentItem : ContentItem
    {
        public Collection Collection { get; set; }
    }

    internal class Collection
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public ImageFill ImageFill { get; set; }
        public List<InternalCollectionItem> Items { get; set; }
    }

    internal class InternalCollectionItem
    {
        public string Key { get; set; }
    }
}
