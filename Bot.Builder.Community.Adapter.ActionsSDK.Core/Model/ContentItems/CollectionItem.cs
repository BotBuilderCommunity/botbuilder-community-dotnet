using System.Collections.Generic;

namespace Bot.Builder.Community.Adapter.ActionsSDK.Core.Model.ContentItems
{
    public class CollectionItem
    {
        public string Key { get; set; }

        public List<string> Synonyms { get; set; }

        public EntryDisplay Item { get; set; }
    }
}