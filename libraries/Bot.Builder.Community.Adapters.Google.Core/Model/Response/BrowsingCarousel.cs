using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Google.Core.Model.Response
{
    public class BrowsingCarousel : ResponseItem
    {
        [JsonProperty(PropertyName = "carouselBrowse")]
        public BrowsingCarouselContent Content { get; set; }
    }

    public class BrowsingCarouselContent
    {
        public List<BrowsingCarouselItem> Items { get; set; } = new List<BrowsingCarouselItem>();
    }

    public class BrowsingCarouselItem
    {
        public string Title { get; set; }

        public Image Image { get; set; }

        public OpenUrlAction OpenUrlAction { get; set; }

        public string Description { get; set; }

        public string Footer { get; set; }
    }
}
