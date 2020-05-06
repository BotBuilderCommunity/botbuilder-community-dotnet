using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Infobip.Models
{
    public class InfobipOmniFailoverMessage
    {
        [JsonProperty("scenarioKey")] public string ScenarioKey { get; set; }

        [JsonProperty("destinations")] public InfobipDestination[] Destinations { get; set; }

        [JsonProperty("whatsApp")] public InfobipOmniWhatsAppMessage WhatsApp { get; set; }

        [JsonProperty("callbackData")] public string CallbackData { get; set; }
    }

    public class InfobipDestination
    {
        [JsonProperty("to")] public InfobipTo To { get; set; }
    }

    public class InfobipTo
    {
        [JsonProperty("phoneNumber")] public string PhoneNumber { get; set; }
    }

    public class InfobipOmniWhatsAppMessage: InfobipWhatsAppTemplateMessage
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
