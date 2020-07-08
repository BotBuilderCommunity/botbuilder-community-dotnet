using System.Collections.Generic;

namespace Bot.Builder.Community.Adapter.ActionsSDK.Core.Model
{
    public class Entry
    {
        public string Name { get; set; }

        public List<string> Synonyms { get; set; }

        public EntryDisplay Display { get; set; }
    }
}