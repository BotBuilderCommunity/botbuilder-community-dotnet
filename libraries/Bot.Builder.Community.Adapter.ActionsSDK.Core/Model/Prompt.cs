using System.Collections.Generic;
using Bot.Builder.Community.Adapters.ActionsSDK.Core.Model.ContentItems;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapters.ActionsSDK.Core.Model
{
    public class Prompt
    {
        public bool Override { get; set; }

        public ContentItem Content { get; set; }

        public Simple FirstSimple { get; set; }

        public Simple LastSimple { get; set; }

        public List<Suggestion> Suggestions { get; set; }

        public Link Link { get; set; }

        public Canvas Canvas { get; set; }

        public JObject OrderUpdate { get; set; }
    }
}