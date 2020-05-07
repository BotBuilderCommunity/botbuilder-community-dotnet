using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Google.Core.Model.SystemIntents
{
    public class CarouselIntent : SystemIntent
    {
        public CarouselIntent()
        {
            Intent = "actions.intent.OPTION";
        }

        public CarouselOptionIntentInputValueData InputValueData { get; set; }
    }

    public class ListIntent : SystemIntent
    {
        public ListIntent()
        {
            Intent = "actions.intent.OPTION";
        }

        public ListOptionIntentInputValueData InputValueData { get; set; }
    }

    public class ListOptionIntentInputValueData : IntentInputValueData
    {
        public ListOptionIntentInputValueData()
        {
            Type = "type.googleapis.com/google.actions.v2.OptionValueSpec";
        }

        public OptionIntentSelect ListSelect { get; set; }
    }

    public class CarouselOptionIntentInputValueData : IntentInputValueData
    {
        [JsonProperty(PropertyName = "@type")]
        public string Type { get; set; }

        public CarouselOptionIntentInputValueData()
        {
            Type = "type.googleapis.com/google.actions.v2.OptionValueSpec";
        }

        public OptionIntentSelect CarouselSelect { get; set; }
    }

    public class OptionIntentSelect
    {
        public string Title { get; set; }

        public List<OptionItem> Items { get; set; }
    }

    public class OptionItem
    {
        public OptionItemInfo OptionInfo { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public OptionItemImage Image { get; set; }
    }

    public class OptionItemImage
    {
        public string Url { get; set; }
        public string AccessibilityText { get; set; }
    }

    public class OptionItemInfo
    {
        public string Key { get; set; }
        public List<string> Synonyms { get; set; }
    }
}