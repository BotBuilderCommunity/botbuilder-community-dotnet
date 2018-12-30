using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Builder.Community.Adapters.Google.Model
{ 
    public class OptionSystemIntent : ISystemIntent
    {
        public OptionSystemIntent()
        {
            Intent = "actions.intent.OPTION";
        }

        public OptionSystemIntentData Data { get; set; }
    }

    public class SystemIntentData
    {
        [JsonProperty(PropertyName = "@type")]
        public string Type { get; set; }
    }

    public class OptionSystemIntentData : SystemIntentData
    {
        public OptionSystemIntentData()
        {
            Type = "type.googleapis.com/google.actions.v2.OptionValueSpec";
        }

        public OptionIntentListSelect ListSelect { get; set; }
    }

    public class OptionIntentListSelect
    {
        public string Title { get; set; }

        public List<OptionIntentSelectListItem> Items { get; set; }
    }

    public class OptionIntentSelectListItem
    {
        public SelectListItemOptionInfo OptionInfo { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public SelectListItemOptionImage Image { get; set; }
    }

    public class SelectListItemOptionImage
    {
        public string Url { get; set; }
        public string AccessibilityText { get; set; }
    }

    public class SelectListItemOptionInfo
    {
        public string Key { get; set; }
        public List<string> Synonyms { get; set; }
    }
}
