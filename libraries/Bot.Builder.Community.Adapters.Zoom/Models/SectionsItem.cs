using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Zoom.Models
{
    public class SectionsItem : BodyItem
    {
        public string Type => "section";

        [JsonProperty(PropertyName = "sidebar_color")]
        public Uri SidebarColor { get; set; }

        public List<BodyItem> Sections { get; set; } = new List<BodyItem>();

        public string Footer { get; set; }

        [JsonProperty(PropertyName = "footer_icon")]
        public Uri FooterIcon { get; set; }

        public string Ts { get; set; }
    }
}