using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Bot.Builder.Community.Adapters.Zoom.Models
{
    public class AttachmentBodyItem : BodyItem
    {
        public string Type => "attachments";

        [JsonProperty(PropertyName = "resource_url")]
        public Uri ResourceUrl { get; set; }

        [JsonProperty(PropertyName = "img_url")]
        public Uri ImageUrl { get; set; }

        public ZoomAttachmentInfo Information { get; set; }

        public FileExtensions Ext { get; set; }

        public int Size { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum FileExtensions
    {
        pdf,
        txt,
        doc,
        xlsx,
        zip,
        jpeg,
        png
    }
}