using System;
using System.Collections.Generic;
using System.Linq;
using Bot.Builder.Community.Adapters.Infobip.Core;
using Bot.Builder.Community.Adapters.Infobip.WhatsApp.Models;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Infobip.WhatsApp.ToInfobip
{
    public class InfobipOmniWhatsAppMessageFactory
    {
        public static IList<InfobipOmniWhatsAppMessage> Create(Activity activity)
        {
            var messages = new List<InfobipOmniWhatsAppMessage>();
            if (activity.Attachments != null && activity.Attachments.Any())
                HandleAttachments(activity, messages);

            if (!string.IsNullOrWhiteSpace(activity.Text))
                HandleText(activity, messages);

            if (activity.Entities != null && activity.Entities.Any())
                HandleEntities(activity, messages);

            return messages;
        }

        private static void HandleAttachments(IMessageActivity activity, ICollection<InfobipOmniWhatsAppMessage> messages)
        {
            foreach (var attachment in activity.Attachments)
            {
                ValidateContentType(attachment);
                var contentType = attachment.ContentType.ToLower();
                var message = new InfobipOmniWhatsAppMessage();

                if (contentType == InfobipWhatsAppAttachmentContentTypes.WhatsAppMessageTemplate && attachment.Content is InfobipWhatsAppTemplateMessage templateMessage)
                {
                    message.SetTemplate(templateMessage);
                }
                else
                {
                    ValidateContentUrl(attachment);
                    SetMediaUrl(contentType, message, attachment);
                }

                messages.Add(message);
            }
        }

        private static void HandleText(IMessageActivity activity, IList<InfobipOmniWhatsAppMessage> messages)
        {
            var mediaMessage = messages.FirstOrDefault();
            if (mediaMessage != null && IsMediaWithCaption(mediaMessage))
            {
                mediaMessage.Text = activity.Text;
                return;
            }

            var message = new InfobipOmniWhatsAppMessage { Text = activity.Text };
            messages.Insert(0, message);
        }

        private static void HandleEntities(IActivity activity, ICollection<InfobipOmniWhatsAppMessage> messages)
        {
            foreach (var entity in activity.Entities)
            {
                if (entity.Type == InfobipEntityType.CallbackData)
                    continue;

                var geoCoordinates = entity.GetAs<GeoCoordinates>();
                if (geoCoordinates.Type == nameof(GeoCoordinates))
                {
                    var message = new InfobipOmniWhatsAppMessage();
                    message.SetLocation(geoCoordinates);
                    messages.Add(message);
                    continue;
                }

                var place = entity.GetAs<Place>();
                if (place.Type == nameof(Place))
                {
                    if (place.Address != null && place.Address.GetType() != typeof(string))
                        throw new Exception("For Place object Address can only be string");

                    var geoEntity = JsonConvert.DeserializeObject<GeoCoordinates>(JsonConvert.SerializeObject(place.Geo));
                    if (geoEntity == null || geoEntity.Type != nameof(GeoCoordinates))
                        throw new Exception("For Place object required param is Geo and must be type of GeoCoordinates");

                    var newMessage = new InfobipOmniWhatsAppMessage();
                    newMessage.Address = place.Address?.ToString();
                    newMessage.SetLocation(geoEntity);
                    messages.Add(newMessage);
                    continue;
                }

                throw new Exception("Supported entities are GeoCoordinate and Place.");
            }
        }

        private static void ValidateContentType(Attachment attachment)
        {
            if (string.IsNullOrWhiteSpace(attachment.ContentType))
                throw new ValidationException(
                    $"Content type is required property for attachment. Attachment with name: {attachment.Name} will not be processed");
        }

        private static void ValidateContentUrl(Attachment attachment)
        {
            if (string.IsNullOrWhiteSpace(attachment.ContentUrl))
                throw new ValidationException(
                    $"Content url is required property for media attachment. Attachment with name: {attachment.Name} will not be processed");
        }

        private static bool IsMediaWithCaption(InfobipOmniWhatsAppMessage message)
        {
            var mediaWithCaptionUrls = new List<string> { message.FileUrl, message.ImageUrl, message.VideoUrl };
            return mediaWithCaptionUrls.Any(x => !string.IsNullOrWhiteSpace(x));
        }

        private static void SetMediaUrl(string contentType, InfobipOmniWhatsAppMessage message, Attachment attachment)
        {
            if (contentType.Contains("audio"))
                message.AudioUrl = attachment.ContentUrl;
            else if (contentType.Contains("image"))
                message.ImageUrl = attachment.ContentUrl;
            else if (contentType.Contains("video"))
                message.VideoUrl = attachment.ContentUrl;
            else
                message.FileUrl = attachment.ContentUrl;
        }
    }
}
