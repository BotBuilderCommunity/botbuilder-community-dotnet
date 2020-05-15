using Bot.Builder.Community.Adapters.Infobip.Models;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Rest;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Infobip
{
    public class ToInfobipConverter
    {
        /// <summary>
        /// Converts a Bot Framework activity to a Infobip OMNI failover message ready for the OMNI API.
        /// </summary>
        /// <param name="activity">The activity to be converted to Infobip OMNI failover message.</param>
        /// <param name="scenarioKey">The scenario key of scenario that will be used for sending messages.</param>
        /// <returns>The resulting messages.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="activity"/> is null.</exception>
        /// <exception cref="ValidationException"><paramref name="activity"/> is null.</exception>
        public static List<InfobipOmniFailoverMessage> Convert(Activity activity, string scenarioKey)
        {
            if (activity == null) throw new ArgumentNullException(nameof(activity));

            var messages = new List<InfobipOmniFailoverMessage>();

            var callbackData = activity.Entities?
                .SingleOrDefault(x =>
                    x.Type == InfobipConstants.InfobipCallbackDataEntityType)?
                .Properties.ToInfobipCallbackDataJson();

            if (activity.Recipient?.Id == null)
                throw new ValidationException("Activity must have a Recipient Id");

            var destinations = new[] { new InfobipDestination { To = new InfobipTo { PhoneNumber = activity.Recipient.Id } } };

            if (activity.Attachments != null && activity.Attachments.Any())
                HandleAttachments(activity, scenarioKey, destinations, messages, callbackData);

            if (!string.IsNullOrWhiteSpace(activity.Text))
                HandleText(activity, scenarioKey, destinations, messages, callbackData);

            if (activity.Entities != null && activity.Entities.Any())
                HandleEntities(activity, scenarioKey, destinations, messages, callbackData);

            return messages;
        }

        private static void HandleEntities(IActivity activity, string scenarioKey, InfobipDestination[] destinations,
            ICollection<InfobipOmniFailoverMessage> messages, string callbackData)
        {
            foreach (var entity in activity.Entities)
            {
                if (entity.Type == InfobipConstants.InfobipCallbackDataEntityType)
                    continue;

                var geoCoordinates = entity.GetAs<GeoCoordinates>();
                if (geoCoordinates.Type == nameof(GeoCoordinates))
                {
                    var message = CreateMessage(destinations, scenarioKey, callbackData);
                    message.WhatsApp.SetLocation(geoCoordinates);
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

                    var newMessage = CreateMessage(destinations, scenarioKey, callbackData);
                    newMessage.WhatsApp.Address = place.Address?.ToString();
                    newMessage.WhatsApp.SetLocation(geoEntity);
                    messages.Add(newMessage);
                    continue;
                }

                throw new Exception("Supported entities are GeoCoordinate and Place.");
            }
        }

        private static void HandleAttachments(IMessageActivity activity, string scenarioKey, InfobipDestination[] destinations,
            ICollection<InfobipOmniFailoverMessage> messages, string callbackData)
        {
            foreach (var attachment in activity.Attachments)
            {
                ValidateContentType(attachment);
                var contentType = attachment.ContentType.ToLower();
                var message = CreateMessage(destinations, scenarioKey, callbackData);

                if (contentType == InfobipConstants.WhatsAppMessageTemplateContentType && attachment.Content is InfobipWhatsAppTemplateMessage templateMessage)
                {
                    message.WhatsApp.SetTemplate(templateMessage);
                }
                else
                {
                    ValidateContentUrl(attachment);
                    SetMedaUrl(contentType, message, attachment);
                }

                messages.Add(message);
            }
        }

        private static void HandleText(IMessageActivity activity, string scenarioKey, InfobipDestination[] destinations,
            IList<InfobipOmniFailoverMessage> messages, string callbackData)
        {
            var mediaMessage = messages.FirstOrDefault();
            if (mediaMessage != null && IsMediaWithCaption(mediaMessage.WhatsApp))
            {
                mediaMessage.WhatsApp.Text = activity.Text;
                return;
            }

            var message = CreateMessage(destinations, scenarioKey, callbackData);
            message.WhatsApp.Text = activity.Text;
            messages.Insert(0, message);
        }

        private static InfobipOmniFailoverMessage CreateMessage(InfobipDestination[] destinations, string scenarioKey, string callbackData)
        {
            return new InfobipOmniFailoverMessage
            {
                Destinations = destinations,
                ScenarioKey = scenarioKey,
                WhatsApp = new InfobipOmniWhatsAppMessage(),
                CallbackData = callbackData
            };
        }

        private static void SetMedaUrl(string contentType, InfobipOmniFailoverMessage message, Attachment attachment)
        {
            if (contentType.Contains("audio"))
                message.WhatsApp.AudioUrl = attachment.ContentUrl;
            else if (contentType.Contains("image"))
                message.WhatsApp.ImageUrl = attachment.ContentUrl;
            else if (contentType.Contains("video"))
                message.WhatsApp.VideoUrl = attachment.ContentUrl;
            else
                message.WhatsApp.FileUrl = attachment.ContentUrl;
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
    }
}
