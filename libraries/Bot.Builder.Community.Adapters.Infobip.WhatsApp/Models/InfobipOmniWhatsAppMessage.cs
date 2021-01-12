using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Infobip.WhatsApp.Models
{
    public class InfobipOmniWhatsAppMessage : InfobipWhatsAppTemplateMessage
    {
        [JsonProperty("text")] public string Text { get; set; }
        [JsonProperty("latitude")] public double? Latitude { get; set; }
        [JsonProperty("longitude")] public double? Longitude { get; set; }
        [JsonProperty("videoUrl")] public string VideoUrl { get; set; }
        [JsonProperty("imageUrl")] public string ImageUrl { get; set; }
        [JsonProperty("locationName")] public string LocationName { get; set; }
        [JsonProperty("address")] public string Address { get; set; }
        [JsonProperty("fileUrl")] public string FileUrl { get; set; }
        [JsonProperty("audioUrl")] public string AudioUrl { get; set; }

        public void SetTemplate(InfobipWhatsAppTemplateMessage templateMessage)
        {
            MediaTemplateData = templateMessage.MediaTemplateData;
            TemplateData = templateMessage.TemplateData;
            TemplateNamespace = templateMessage.TemplateNamespace;
            TemplateName = templateMessage.TemplateName;
            Language = templateMessage.Language;
        }

        public void SetLocation(GeoCoordinates geoCoordinates)
        {
            Longitude = geoCoordinates.Longitude;
            Latitude = geoCoordinates.Latitude;
            LocationName = geoCoordinates.Name;
        }
    }

    // https://www.infobip.com/docs/whatsapp/message-templates-guidelines
    // https://dev.infobip.com/#programmable-communications/omni-failover/send-omni-failover-message
    public class InfobipWhatsAppTemplateMessage
    {
        [JsonProperty("templateData")] public string[] TemplateData { get; set; }
        [JsonProperty("templateNamespace")] public string TemplateNamespace { get; set; }
        [JsonProperty("templateName")] public string TemplateName { get; set; }
        [JsonProperty("language")] public string Language { get; set; }
        [JsonProperty("mediaTemplateData")] public InfobipWhatsAppMediaTemplateData MediaTemplateData { get; set; }
    }

    public class InfobipWhatsAppMediaTemplateData
    {
        [JsonProperty("header")] public InfobipWhatsAppMediaTemplateHeader MediaTemplateHeader { get; set; }
        [JsonProperty("body")] public InfobipWhatsAppMediaTemplateBody MediaTemplateBody { get; set; }
    }

    public class InfobipWhatsAppMediaTemplateHeader
    {
        [JsonProperty("textPlaceholder")] public string TextPlaceholder { get; set; }
        [JsonProperty("videoUrl")] public string VideoUrl { get; set; }
        [JsonProperty("imageUrl")] public string ImageUrl { get; set; }
        [JsonProperty("documentUrl")] public string DocumentUrl { get; set; }
        [JsonProperty("latitude")] public double? Latitude { get; set; }
        [JsonProperty("longitude")] public double? Longitude { get; set; }
        [JsonProperty("documentFilename")] public string DocumentFilename { get; set; }
    }

    public class InfobipWhatsAppMediaTemplateBody
    {
        [JsonProperty("placeholders")] public string[] Placeholders { get; set; }
    }
}
