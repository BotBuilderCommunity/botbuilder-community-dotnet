using Bot.Builder.Community.Adapters.Infobip;
using Bot.Builder.Community.Adapters.Infobip.Models;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bot.Builder.Community.Adapter.Infobip.Tests
{
    public class ToInfobipConverterTest
    {
        private Activity _activity;
        private const string SCENARIO_KEY = TestOptions.ScenarioKey;

        public ToInfobipConverterTest()
        {
            _activity = new Activity
            {
                Type = ActivityTypes.Message,
                Id = "message id",
                Timestamp = DateTimeOffset.Parse("2020-02-26T10:15:48.734+0000"),
                ChannelId = InfobipConstants.ChannelName,
                Conversation = new ConversationAccount { Id = "subscriber-number" },
                From = new ChannelAccount { Id = "whatsapp-number" },
                Recipient = new ChannelAccount { Id = "subscriber-number" }
            };
        }

        [Fact]
        public void ConvertTextActivityToOmniFailoverMessage()
        {
            _activity.Text = "Test text";

            var omniFailoverMessages = ToInfobipConverter.Convert(_activity, SCENARIO_KEY);
            Assert.NotNull(omniFailoverMessages);
            Assert.Single(omniFailoverMessages);

            var omniFailoverMessage = omniFailoverMessages.First();
            Assert.Equal(omniFailoverMessage.ScenarioKey, SCENARIO_KEY);
            CheckDestinations(omniFailoverMessage.Destinations);

            var whatsAppMessage = omniFailoverMessage.WhatsApp;
            Assert.NotNull(whatsAppMessage);
            Assert.Equal(whatsAppMessage.Text, _activity.Text);
            Assert.Null(whatsAppMessage.FileUrl);
            Assert.Null(whatsAppMessage.LocationName);
            Assert.Null(whatsAppMessage.AudioUrl);
            Assert.Null(whatsAppMessage.Address);
            Assert.Null(whatsAppMessage.Latitude);
            Assert.Null(whatsAppMessage.Longitude);
            Assert.Null(whatsAppMessage.ImageUrl);
            Assert.Null(whatsAppMessage.VideoUrl);
            Assert.Null(whatsAppMessage.TemplateNamespace);
            Assert.Null(whatsAppMessage.TemplateData);
            Assert.Null(whatsAppMessage.TemplateName);
            Assert.Null(whatsAppMessage.Language);
            Assert.Null(whatsAppMessage.MediaTemplateData);
        }

        [Fact]
        public void ConvertActivityWithSingleGeoCoordinateEntityToOmniFailoverMessage()
        {
            _activity.Entities = new List<Entity> { new GeoCoordinates { Latitude = 12.3456789, Longitude = 23.456789, Name = "Test" } };

            var omniFailoverMessages = ToInfobipConverter.Convert(_activity, SCENARIO_KEY);
            Assert.NotNull(omniFailoverMessages);
            Assert.Single(omniFailoverMessages);

            var omniFailoverMessage = omniFailoverMessages.First();
            CheckLocationMessage(omniFailoverMessage, _activity.Entities.First().GetAs<GeoCoordinates>());
        }

        [Fact]
        public void ConvertActivityWithMultipleGeoCoordinateEntityToOmniFailoverMessage()
        {
            _activity.Entities = new List<Entity>
            {
                new GeoCoordinates {Latitude = 12.3456789, Longitude = 23.456789, Name = "Test"},
                new GeoCoordinates {Latitude = 45.56789, Longitude = 87.12345, Name = "Test2"}
            };

            var omniFailoverMessages = ToInfobipConverter.Convert(_activity, SCENARIO_KEY);
            Assert.NotNull(omniFailoverMessages);
            Assert.Equal(2, omniFailoverMessages.Count);

            for (var i = 0; i < omniFailoverMessages.Count; i++)
                CheckLocationMessage(omniFailoverMessages.ElementAt(i), _activity.Entities.ElementAt(i).GetAs<GeoCoordinates>());
        }

        [Fact]
        public void ConvertActivityWithSinglePlaceEntityToOmniFailoverMessage()
        {
            _activity.Entities = new List<Entity>
            {
                new Place
                {
                    Address = "Address",
                    Geo = new GeoCoordinates {Latitude = 12.3456789, Longitude = 23.456789, Name = "Test"}
                }
            };

            var omniFailoverMessages = ToInfobipConverter.Convert(_activity, SCENARIO_KEY);
            Assert.NotNull(omniFailoverMessages);
            Assert.Single(omniFailoverMessages);

            var omniFailoverMessage = omniFailoverMessages.First();
            CheckLocationMessage(omniFailoverMessage, _activity.Entities.First().GetAs<Place>());
        }

        [Fact]
        public void ConvertActivityWithSinglePlaceEntityWithoutGeoToOmniFailoverMessage()
        {
            _activity.Entities = new List<Entity>
            {
                new Place
                {
                    Address = "Address"
                }
            };

            Assert.Throws<Exception>(() => ToInfobipConverter.Convert(_activity, SCENARIO_KEY));
        }

        [Fact]
        public void ConvertActivityWithSinglePlaceEntityWithNonStringAddressFailoverMessage()
        {
            _activity.Entities = new List<Entity>
            {
                new Place
                {
                    Address = new { Key = "value" }
                }
            };

            Assert.Throws<Exception>(() => ToInfobipConverter.Convert(_activity, SCENARIO_KEY));
        }

        [Fact]
        public void ConvertActivityWithImageAttachmentToOmniFailoverMessage()
        {
            var attachment = new Attachment { ContentUrl = "http://dummy-image.com", ContentType = "image/jpeg" };
            _activity.Attachments = new List<Attachment> { attachment };

            var omniFailoverMessages = ToInfobipConverter.Convert(_activity, SCENARIO_KEY);
            Assert.NotNull(omniFailoverMessages);
            Assert.Single(omniFailoverMessages);

            var omniFailoverMessage = omniFailoverMessages.First();
            Assert.Equal(omniFailoverMessage.ScenarioKey, SCENARIO_KEY);
            CheckDestinations(omniFailoverMessage.Destinations);

            var whatsAppMessage = omniFailoverMessage.WhatsApp;
            Assert.NotNull(whatsAppMessage);
            Assert.Null(whatsAppMessage.Text);
            Assert.Null(whatsAppMessage.FileUrl);
            Assert.Null(whatsAppMessage.LocationName);
            Assert.Null(whatsAppMessage.AudioUrl);
            Assert.Null(whatsAppMessage.Address);
            Assert.Null(whatsAppMessage.Latitude);
            Assert.Null(whatsAppMessage.Longitude);
            Assert.Equal(whatsAppMessage.ImageUrl, attachment.ContentUrl);
            Assert.Null(whatsAppMessage.VideoUrl);
            Assert.Null(whatsAppMessage.TemplateNamespace);
            Assert.Null(whatsAppMessage.TemplateData);
            Assert.Null(whatsAppMessage.TemplateName);
            Assert.Null(whatsAppMessage.Language);
            Assert.Null(whatsAppMessage.MediaTemplateData);
        }

        [Fact]
        public void ConvertActivityWithImageAttachmentAndTextToOmniFailoverMessage()
        {
            var attachment = new Attachment { ContentUrl = "http://dummy-image.com", ContentType = "image/jpeg" };
            _activity.Attachments = new List<Attachment> { attachment };
            _activity.Text = "Test text";

            var omniFailoverMessages = ToInfobipConverter.Convert(_activity, SCENARIO_KEY);
            Assert.NotNull(omniFailoverMessages);
            Assert.Single(omniFailoverMessages);

            var omniFailoverMessage = omniFailoverMessages.First();
            Assert.Equal(omniFailoverMessage.ScenarioKey, SCENARIO_KEY);
            CheckDestinations(omniFailoverMessage.Destinations);

            var whatsAppMessage = omniFailoverMessage.WhatsApp;
            Assert.NotNull(whatsAppMessage);
            Assert.Equal(whatsAppMessage.Text, _activity.Text);
            Assert.Null(whatsAppMessage.FileUrl);
            Assert.Null(whatsAppMessage.LocationName);
            Assert.Null(whatsAppMessage.AudioUrl);
            Assert.Null(whatsAppMessage.Address);
            Assert.Null(whatsAppMessage.Latitude);
            Assert.Null(whatsAppMessage.Longitude);
            Assert.Equal(whatsAppMessage.ImageUrl, attachment.ContentUrl);
            Assert.Null(whatsAppMessage.VideoUrl);
            Assert.Null(whatsAppMessage.TemplateNamespace);
            Assert.Null(whatsAppMessage.TemplateData);
            Assert.Null(whatsAppMessage.TemplateName);
            Assert.Null(whatsAppMessage.Language);
            Assert.Null(whatsAppMessage.MediaTemplateData);
        }

        [Fact]
        public void ConvertActivityWithTextAndMultipleAttachmentsToOmniFailoverMessage()
        {
            var attachment = new Attachment { ContentUrl = "http://dummy-video.com", ContentType = "video/mp4" };
            var attachment2 = new Attachment { ContentUrl = "http://dummy-image.com", ContentType = "image/jpeg" };
            _activity.Attachments = new List<Attachment> { attachment, attachment2 };
            _activity.Text = "Test text";

            var omniFailoverMessages = ToInfobipConverter.Convert(_activity, SCENARIO_KEY);
            Assert.NotNull(omniFailoverMessages);
            Assert.Equal(2, omniFailoverMessages.Count);

            var omniFailoverMessage = omniFailoverMessages.First();
            var omniFailoverMessage2 = omniFailoverMessages.ElementAt(1);
            Assert.Equal(omniFailoverMessage.ScenarioKey, SCENARIO_KEY);
            Assert.Equal(omniFailoverMessage2.ScenarioKey, SCENARIO_KEY);
            CheckDestinations(omniFailoverMessage.Destinations);
            CheckDestinations(omniFailoverMessage2.Destinations);

            var whatsAppMessage = omniFailoverMessage.WhatsApp;
            Assert.NotNull(whatsAppMessage);
            Assert.Equal(whatsAppMessage.Text, _activity.Text);
            Assert.Null(whatsAppMessage.FileUrl);
            Assert.Null(whatsAppMessage.LocationName);
            Assert.Null(whatsAppMessage.AudioUrl);
            Assert.Null(whatsAppMessage.Address);
            Assert.Null(whatsAppMessage.Latitude);
            Assert.Null(whatsAppMessage.Longitude);
            Assert.Null(whatsAppMessage.ImageUrl);
            Assert.Equal(whatsAppMessage.VideoUrl, attachment.ContentUrl);
            Assert.Null(whatsAppMessage.TemplateNamespace);
            Assert.Null(whatsAppMessage.TemplateData);
            Assert.Null(whatsAppMessage.TemplateName);
            Assert.Null(whatsAppMessage.Language);
            Assert.Null(whatsAppMessage.MediaTemplateData);

            var whatsAppMessage2 = omniFailoverMessage2.WhatsApp;
            Assert.NotNull(whatsAppMessage2);
            Assert.Null(whatsAppMessage2.Text);
            Assert.Null(whatsAppMessage2.FileUrl);
            Assert.Null(whatsAppMessage2.LocationName);
            Assert.Null(whatsAppMessage2.AudioUrl);
            Assert.Null(whatsAppMessage2.Address);
            Assert.Null(whatsAppMessage2.Latitude);
            Assert.Null(whatsAppMessage2.Longitude);
            Assert.Equal(whatsAppMessage2.ImageUrl, attachment2.ContentUrl);
            Assert.Null(whatsAppMessage2.VideoUrl);
            Assert.Null(whatsAppMessage2.TemplateNamespace);
            Assert.Null(whatsAppMessage2.TemplateData);
            Assert.Null(whatsAppMessage2.TemplateName);
            Assert.Null(whatsAppMessage2.Language);
            Assert.Null(whatsAppMessage2.MediaTemplateData);
        }

        [Fact]
        public void ConvertActivityWithVideoAttachmentToOmniFailoverMessage()
        {
            var attachment = new Attachment { ContentUrl = "http://dummy-video.com", ContentType = "video/mp4" };
            _activity.Attachments = new List<Attachment> { attachment };

            var omniFailoverMessages = ToInfobipConverter.Convert(_activity, SCENARIO_KEY);
            Assert.NotNull(omniFailoverMessages);
            Assert.Single(omniFailoverMessages);

            var omniFailoverMessage = omniFailoverMessages.First();
            Assert.Equal(omniFailoverMessage.ScenarioKey, SCENARIO_KEY);
            CheckDestinations(omniFailoverMessage.Destinations);

            var whatsAppMessage = omniFailoverMessage.WhatsApp;
            Assert.NotNull(whatsAppMessage);
            Assert.Null(whatsAppMessage.Text);
            Assert.Null(whatsAppMessage.FileUrl);
            Assert.Null(whatsAppMessage.LocationName);
            Assert.Null(whatsAppMessage.AudioUrl);
            Assert.Null(whatsAppMessage.Address);
            Assert.Null(whatsAppMessage.Latitude);
            Assert.Null(whatsAppMessage.Longitude);
            Assert.Null(whatsAppMessage.ImageUrl);
            Assert.Equal(whatsAppMessage.VideoUrl, attachment.ContentUrl);
            Assert.Null(whatsAppMessage.TemplateNamespace);
            Assert.Null(whatsAppMessage.TemplateData);
            Assert.Null(whatsAppMessage.TemplateName);
            Assert.Null(whatsAppMessage.Language);
            Assert.Null(whatsAppMessage.MediaTemplateData);
        }

        [Fact]
        public void ConvertActivityWithVideoAttachmentAndTextToOmniFailoverMessage()
        {
            var attachment = new Attachment { ContentUrl = "http://dummy-video.com", ContentType = "video/mp4" };
            _activity.Attachments = new List<Attachment> { attachment };
            _activity.Text = "Test text";

            var omniFailoverMessages = ToInfobipConverter.Convert(_activity, SCENARIO_KEY);
            Assert.NotNull(omniFailoverMessages);
            Assert.Single(omniFailoverMessages);

            var omniFailoverMessage = omniFailoverMessages.First();
            Assert.Equal(omniFailoverMessage.ScenarioKey, SCENARIO_KEY);
            CheckDestinations(omniFailoverMessage.Destinations);

            var whatsAppMessage = omniFailoverMessage.WhatsApp;
            Assert.NotNull(whatsAppMessage);
            Assert.Equal(whatsAppMessage.Text, _activity.Text);
            Assert.Null(whatsAppMessage.FileUrl);
            Assert.Null(whatsAppMessage.LocationName);
            Assert.Null(whatsAppMessage.AudioUrl);
            Assert.Null(whatsAppMessage.Address);
            Assert.Null(whatsAppMessage.Latitude);
            Assert.Null(whatsAppMessage.Longitude);
            Assert.Null(whatsAppMessage.ImageUrl);
            Assert.Equal(whatsAppMessage.VideoUrl, attachment.ContentUrl);
            Assert.Null(whatsAppMessage.TemplateNamespace);
            Assert.Null(whatsAppMessage.TemplateData);
            Assert.Null(whatsAppMessage.TemplateName);
            Assert.Null(whatsAppMessage.Language);
            Assert.Null(whatsAppMessage.MediaTemplateData);
        }

        [Fact]
        public void ConvertActivityWithAudioAttachmentToOmniFailoverMessage()
        {
            var attachment = new Attachment { ContentUrl = "http://dummy-audio.com", ContentType = "audio/mp3" };
            _activity.Attachments = new List<Attachment> { attachment };

            var omniFailoverMessages = ToInfobipConverter.Convert(_activity, SCENARIO_KEY);
            Assert.NotNull(omniFailoverMessages);
            Assert.Single(omniFailoverMessages);

            var omniFailoverMessage = omniFailoverMessages.First();
            Assert.Equal(omniFailoverMessage.ScenarioKey, SCENARIO_KEY);
            CheckDestinations(omniFailoverMessage.Destinations);

            CheckAudioMessage(omniFailoverMessage, attachment);
        }

        [Fact]
        public void ConvertActivityWithAudioAttachmentAndTextToOmniFailoverMessage()
        {
            var attachment = new Attachment { ContentUrl = "http://dummy-audio.com", ContentType = "audio/mp3" };
            _activity.Attachments = new List<Attachment> { attachment };
            _activity.Text = "Test text";

            var omniFailoverMessages = ToInfobipConverter.Convert(_activity, SCENARIO_KEY);
            Assert.NotNull(omniFailoverMessages);
            Assert.Equal(2, omniFailoverMessages.Count);

            var omniFailoverMessage = omniFailoverMessages.First();
            var omniFailoverMessage2 = omniFailoverMessages.ElementAt(1);
            Assert.Equal(omniFailoverMessage.ScenarioKey, SCENARIO_KEY);
            Assert.Equal(omniFailoverMessage2.ScenarioKey, SCENARIO_KEY);
            CheckDestinations(omniFailoverMessage.Destinations);
            CheckDestinations(omniFailoverMessage2.Destinations);

            var whatsAppMessage = omniFailoverMessage.WhatsApp;
            Assert.NotNull(whatsAppMessage);
            Assert.Equal(whatsAppMessage.Text, _activity.Text);
            Assert.Null(whatsAppMessage.FileUrl);
            Assert.Null(whatsAppMessage.LocationName);
            Assert.Null(whatsAppMessage.AudioUrl);
            Assert.Null(whatsAppMessage.Address);
            Assert.Null(whatsAppMessage.Latitude);
            Assert.Null(whatsAppMessage.Longitude);
            Assert.Null(whatsAppMessage.ImageUrl);
            Assert.Null(whatsAppMessage.VideoUrl);
            Assert.Null(whatsAppMessage.TemplateNamespace);
            Assert.Null(whatsAppMessage.TemplateData);
            Assert.Null(whatsAppMessage.TemplateName);
            Assert.Null(whatsAppMessage.Language);
            Assert.Null(whatsAppMessage.MediaTemplateData);

            CheckAudioMessage(omniFailoverMessage2, attachment);
        }

        [Fact]
        public void ConvertActivityWithFileAttachmentToOmniFailoverMessage()
        {
            var attachment = new Attachment { ContentUrl = "http://dummy-file.com", ContentType = "application/pdf" };
            _activity.Attachments = new List<Attachment> { attachment };

            var omniFailoverMessages = ToInfobipConverter.Convert(_activity, SCENARIO_KEY);
            Assert.NotNull(omniFailoverMessages);
            Assert.Single(omniFailoverMessages);

            var omniFailoverMessage = omniFailoverMessages.First();
            Assert.Equal(omniFailoverMessage.ScenarioKey, SCENARIO_KEY);
            CheckDestinations(omniFailoverMessage.Destinations);

            var whatsAppMessage = omniFailoverMessage.WhatsApp;
            Assert.NotNull(whatsAppMessage);
            Assert.Null(whatsAppMessage.Text);
            Assert.Equal(whatsAppMessage.FileUrl, attachment.ContentUrl);
            Assert.Null(whatsAppMessage.LocationName);
            Assert.Null(whatsAppMessage.AudioUrl);
            Assert.Null(whatsAppMessage.Address);
            Assert.Null(whatsAppMessage.Latitude);
            Assert.Null(whatsAppMessage.Longitude);
            Assert.Null(whatsAppMessage.ImageUrl);
            Assert.Null(whatsAppMessage.VideoUrl);
            Assert.Null(whatsAppMessage.TemplateNamespace);
            Assert.Null(whatsAppMessage.TemplateData);
            Assert.Null(whatsAppMessage.TemplateName);
            Assert.Null(whatsAppMessage.Language);
            Assert.Null(whatsAppMessage.MediaTemplateData);
        }

        [Fact]
        public void ConvertActivityWithFileAttachmentAndTextToOmniFailoverMessage()
        {
            var attachment = new Attachment { ContentUrl = "http://dummy-file.com", ContentType = "application/pdf" };
            _activity.Attachments = new List<Attachment> { attachment };
            _activity.Text = "Test text";

            var omniFailoverMessages = ToInfobipConverter.Convert(_activity, SCENARIO_KEY);
            Assert.NotNull(omniFailoverMessages);
            Assert.Single(omniFailoverMessages);

            var omniFailoverMessage = omniFailoverMessages.First();
            Assert.Equal(omniFailoverMessage.ScenarioKey, SCENARIO_KEY);
            CheckDestinations(omniFailoverMessage.Destinations);

            var whatsAppMessage = omniFailoverMessage.WhatsApp;
            Assert.NotNull(whatsAppMessage);
            Assert.Equal(whatsAppMessage.Text, _activity.Text);
            Assert.Equal(whatsAppMessage.FileUrl, attachment.ContentUrl);
            Assert.Null(whatsAppMessage.LocationName);
            Assert.Null(whatsAppMessage.AudioUrl);
            Assert.Null(whatsAppMessage.Address);
            Assert.Null(whatsAppMessage.Latitude);
            Assert.Null(whatsAppMessage.Longitude);
            Assert.Null(whatsAppMessage.ImageUrl);
            Assert.Null(whatsAppMessage.VideoUrl);
            Assert.Null(whatsAppMessage.TemplateNamespace);
            Assert.Null(whatsAppMessage.TemplateData);
            Assert.Null(whatsAppMessage.TemplateName);
            Assert.Null(whatsAppMessage.Language);
            Assert.Null(whatsAppMessage.MediaTemplateData);
        }

        [Fact]
        public void ConvertActivityWithTemplateAttachmentToOmniFailoverMessage()
        {
            var templateMessage = new InfobipWhatsAppTemplateMessage
            {
                TemplateNamespace = "template_namespace",
                TemplateData = new[] { "one", "two" },
                TemplateName = "template_name",
                Language = "en",
                MediaTemplateData = new InfobipWhatsAppMediaTemplateData
                {
                    MediaTemplateHeader = new InfobipWhatsAppMediaTemplateHeader
                    {
                        DocumentFilename = "Test file name"
                    },
                    MediaTemplateBody = new InfobipWhatsAppMediaTemplateBody
                    {
                        Placeholders = new[] { "three", "four" }
                    }
                }
            };

            _activity.Attachments = new List<Attachment>();
            _activity.Attachments.Add(new InfobipAttachment(templateMessage));

            var omniFailoverMessages = ToInfobipConverter.Convert(_activity, SCENARIO_KEY);
            Assert.NotNull(omniFailoverMessages);
            Assert.Single(omniFailoverMessages);

            var omniFailoverMessage = omniFailoverMessages.First();
            Assert.Equal(omniFailoverMessage.ScenarioKey, SCENARIO_KEY);
            CheckDestinations(omniFailoverMessage.Destinations);

            var whatsAppMessage = omniFailoverMessage.WhatsApp;
            Assert.NotNull(whatsAppMessage);
            Assert.Null(whatsAppMessage.Text);
            Assert.Null(whatsAppMessage.FileUrl);
            Assert.Null(whatsAppMessage.LocationName);
            Assert.Null(whatsAppMessage.AudioUrl);
            Assert.Null(whatsAppMessage.Address);
            Assert.Null(whatsAppMessage.Latitude);
            Assert.Null(whatsAppMessage.Longitude);
            Assert.Null(whatsAppMessage.ImageUrl);
            Assert.Null(whatsAppMessage.VideoUrl);
            Assert.Equal(whatsAppMessage.TemplateNamespace, templateMessage.TemplateNamespace);
            Assert.NotNull(whatsAppMessage.TemplateData);
            Assert.Equal(whatsAppMessage.TemplateData.Length, templateMessage.TemplateData.Length);
            Assert.Equal(whatsAppMessage.TemplateName, templateMessage.TemplateName);
            Assert.Equal(whatsAppMessage.Language, templateMessage.Language);
            Assert.NotNull(whatsAppMessage.MediaTemplateData);
            Assert.Equal(whatsAppMessage.MediaTemplateData.MediaTemplateBody.Placeholders.Length, templateMessage.MediaTemplateData.MediaTemplateBody.Placeholders.Length);
            Assert.Equal(whatsAppMessage.MediaTemplateData.MediaTemplateHeader.DocumentFilename, templateMessage.MediaTemplateData.MediaTemplateHeader.DocumentFilename);
            Assert.Null(whatsAppMessage.MediaTemplateData.MediaTemplateHeader.Latitude);
            Assert.Null(whatsAppMessage.MediaTemplateData.MediaTemplateHeader.Longitude);
            Assert.Null(whatsAppMessage.MediaTemplateData.MediaTemplateHeader.ImageUrl);
            Assert.Null(whatsAppMessage.MediaTemplateData.MediaTemplateHeader.DocumentUrl);
            Assert.Null(whatsAppMessage.MediaTemplateData.MediaTemplateHeader.TextPlaceholder);
            Assert.Null(whatsAppMessage.MediaTemplateData.MediaTemplateHeader.VideoUrl);
        }

        [Fact]
        public void ConvertTextActivityWithCallbackData_Success()
        {
            var callbackData = new Dictionary<string, string>
            {
                {"BoolProperty", "true"},
                {"NumberProperty", "12"},
                {"StringProperty", "string"},
                {"DateProperty", DateTimeOffset.MinValue.ToString()}
            };

            var entityCallbackData = new InfobipCallbackData(callbackData);

            _activity.Text = "Activity with callback data";
            _activity.Entities = new List<Entity>
            {
                entityCallbackData
            };

            var message = ToInfobipConverter.Convert(_activity, SCENARIO_KEY).Single();
            Assert.Equal(message.CallbackData, entityCallbackData.Properties.ToInfobipCallbackDataJson());
        }

        [Fact]
        public void ConvertEmptyActivityWithCallbackData_Success()
        {
            var callbackData = new Dictionary<string, string>
            {
                {"BoolProperty", "true"},
                {"NumberProperty", "12"},
                {"StringProperty", "string"},
                {"DateProperty", DateTimeOffset.MinValue.ToString()}
            };

            var entityCallbackData = new InfobipCallbackData(callbackData);

            _activity.Entities = new List<Entity>
            {
                entityCallbackData
            };

            var messages = ToInfobipConverter.Convert(_activity, SCENARIO_KEY);
            Assert.False(messages.Any());
        }

        private void CheckLocationMessage(InfobipOmniFailoverMessage omniFailoverMessage, GeoCoordinates geoCoordinate)
        {
            Assert.Equal(omniFailoverMessage.ScenarioKey, SCENARIO_KEY);
            CheckDestinations(omniFailoverMessage.Destinations);

            Assert.NotNull(geoCoordinate);

            var whatsAppMessage = omniFailoverMessage.WhatsApp;
            Assert.NotNull(whatsAppMessage);
            Assert.Null(whatsAppMessage.Text);
            Assert.Null(whatsAppMessage.FileUrl);
            Assert.Equal(whatsAppMessage.LocationName, geoCoordinate.Name);
            Assert.Null(whatsAppMessage.AudioUrl);
            Assert.Null(whatsAppMessage.Address);
            Assert.Equal(whatsAppMessage.Latitude, geoCoordinate.Latitude);
            Assert.Equal(whatsAppMessage.Longitude, geoCoordinate.Longitude);
            Assert.Null(whatsAppMessage.ImageUrl);
            Assert.Null(whatsAppMessage.VideoUrl);
            Assert.Null(whatsAppMessage.TemplateNamespace);
            Assert.Null(whatsAppMessage.TemplateData);
            Assert.Null(whatsAppMessage.TemplateName);
            Assert.Null(whatsAppMessage.Language);
            Assert.Null(whatsAppMessage.MediaTemplateData);
        }

        private void CheckLocationMessage(InfobipOmniFailoverMessage omniFailoverMessage, Place place)
        {
            Assert.Equal(omniFailoverMessage.ScenarioKey, SCENARIO_KEY);
            CheckDestinations(omniFailoverMessage.Destinations);

            Assert.NotNull(place);

            var geoCoordinate = JsonConvert.DeserializeObject<GeoCoordinates>(JsonConvert.SerializeObject(place.Geo));
            Assert.NotNull(geoCoordinate);

            var whatsAppMessage = omniFailoverMessage.WhatsApp;
            Assert.NotNull(whatsAppMessage);
            Assert.Null(whatsAppMessage.Text);
            Assert.Null(whatsAppMessage.FileUrl);
            Assert.Equal(whatsAppMessage.LocationName, geoCoordinate.Name);
            Assert.Null(whatsAppMessage.AudioUrl);
            Assert.Equal(whatsAppMessage.Address, place.Address);
            Assert.Equal(whatsAppMessage.Latitude, geoCoordinate.Latitude);
            Assert.Equal(whatsAppMessage.Longitude, geoCoordinate.Longitude);
            Assert.Null(whatsAppMessage.ImageUrl);
            Assert.Null(whatsAppMessage.VideoUrl);
            Assert.Null(whatsAppMessage.TemplateNamespace);
            Assert.Null(whatsAppMessage.TemplateData);
            Assert.Null(whatsAppMessage.TemplateName);
            Assert.Null(whatsAppMessage.Language);
            Assert.Null(whatsAppMessage.MediaTemplateData);
        }

        private void CheckDestinations(InfobipDestination[] destinations)
        {
            Assert.NotNull(destinations);
            Assert.Single(destinations);

            var destination = destinations.First();
            Assert.NotNull(destination.To);
            Assert.Equal(destination.To.PhoneNumber, _activity.Recipient.Id);
        }

        private void CheckAudioMessage(InfobipOmniFailoverMessage omniFailoverMessage, Attachment attachment)
        {
            var whatsAppMessage = omniFailoverMessage.WhatsApp;
            Assert.NotNull(whatsAppMessage);
            Assert.Null(whatsAppMessage.Text);
            Assert.Null(whatsAppMessage.FileUrl);
            Assert.Null(whatsAppMessage.LocationName);
            Assert.Equal(whatsAppMessage.AudioUrl, attachment.ContentUrl);
            Assert.Null(whatsAppMessage.Address);
            Assert.Null(whatsAppMessage.Latitude);
            Assert.Null(whatsAppMessage.Longitude);
            Assert.Null(whatsAppMessage.ImageUrl);
            Assert.Null(whatsAppMessage.VideoUrl);
            Assert.Null(whatsAppMessage.TemplateNamespace);
            Assert.Null(whatsAppMessage.TemplateData);
            Assert.Null(whatsAppMessage.TemplateName);
            Assert.Null(whatsAppMessage.Language);
            Assert.Null(whatsAppMessage.MediaTemplateData);
        }
    }
}
