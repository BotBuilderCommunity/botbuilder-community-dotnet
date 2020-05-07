using System.Collections.Generic;

namespace Bot.Builder.Community.Adapters.Google.Core.Model.Request
{
    public class GoogleOptionItem
    {
        public string Key { get; set; }
        public List<string> Synonyms { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string ImageAccessibilityText { get; set; }
        public string Title { get; set; }
    }
}
