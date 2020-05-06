using Bot.Builder.Community.Adapters.Infobip;
using Bot.Builder.Community.Adapters.Infobip.Models;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bot.Builder.Community.Adapter.Infobip.Tests
{
    [TestFixture(Description = "Tests for conversion from activity to Infobip omni models")]
    public class ToInfobipConverterTest
    {
        private Activity _activity;
        private const string SCENARIO_KEY = TestOptions.ScenarioKey;

        [SetUp]
        public void Setup()
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

        [Test(Description = "convert activity with text to omni failover message with text")]
        public void ConvertTextActivityToOmniFailoverMessage()
        {
            _activity.Text = "Test text";

            var omniFailoverMessages = ToInfobipConverter.Convert(_activity, SCENARIO_KEY);
            Assert.IsNotNull(omniFailoverMessages);
            Assert.AreEqual(omniFailoverMessages.Count, 1);

            var omniFailoverMessage = omniFailoverMessages.First();
            Assert.AreEqual(omniFailoverMessage.ScenarioKey, SCENARIO_KEY);
            CheckDestinations(omniFailoverMessage.Destinations);

            var whatsAppMessage = omniFailoverMessage.WhatsApp;
            Assert.IsNotNull(whatsAppMessage);
            Assert.AreEqual(whatsAppMessage.Text, _activity.Text);
            Assert.IsNull(whatsAppMessage.FileUrl);
            Assert.IsNull(whatsAppMessage.LocationName);
            Assert.IsNull(whatsAppMessage.AudioUrl);
            Assert.IsNull(whatsAppMessage.Address);
            Assert.IsNull(whatsAppMessage.Latitude);
            Assert.IsNull(whatsAppMessage.Longitude);
            Assert.IsNull(whatsAppMessage.ImageUrl);
            Assert.IsNull(whatsAppMessage.VideoUrl);
            Assert.IsNull(whatsAppMessage.TemplateNamespace);
            Assert.IsNull(whatsAppMessage.TemplateData);
            Assert.IsNull(whatsAppMessage.TemplateName);
            Assert.IsNull(whatsAppMessage.Language);
            Assert.IsNull(whatsAppMessage.MediaTemplateData);
        }

        [Test(Description = "convert activity with text to omni failover message with text")]
        public void ConvertActivityWithSingleGeoCoordinateEntityToOmniFailoverMessage()
        {
            _activity.Entities = new List<Entity> { new GeoCoordinates { Latitude = 12.3456789, Longitude = 23.456789, Name = "Test" } };

            var omniFailoverMessages = ToInfobipConverter.Convert(_activity, SCENARIO_KEY);
            Assert.IsNotNull(omniFailoverMessages);
            Assert.AreEqual(omniFailoverMessages.Count, 1);

            var omniFailoverMessage = omniFailoverMessages.First();
            CheckLocationMessage(omniFailoverMessage, _activity.Entities.First().GetAs<GeoCoordinates>());
        }

        [Test(Description = "convert activity with text to omni failover message with text")]
        public void ConvertActivityWithMultipleGeoCoordinateEntityToOmniFailoverMessage()
        {
            _activity.Entities = new List<Entity>
            {
                new GeoCoordinates {Latitude = 12.3456789, Longitude = 23.456789, Name = "Test"},
                new GeoCoordinates {Latitude = 45.56789, Longitude = 87.12345, Name = "Test2"}
            };

            var omniFailoverMessages = ToInfobipConverter.Convert(_activity, SCENARIO_KEY);
            Assert.IsNotNull(omniFailoverMessages);
            Assert.AreEqual(omniFailoverMessages.Count, 2);

            for (var i = 0; i < omniFailoverMessages.Count; i++)
                CheckLocationMessage(omniFailoverMessages.ElementAt(i), _activity.Entities.ElementAt(i).GetAs<GeoCoordinates>());
        }

        [Test(Description = "convert activity with location to omni failover message with text")]
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
            Assert.IsNotNull(omniFailoverMessages);
            Assert.AreEqual(omniFailoverMessages.Count, 1);

            var omniFailoverMessage = omniFailoverMessages.First();
            CheckLocationMessage(omniFailoverMessage, _activity.Entities.First().GetAs<Place>());
        }

        [Test(Description = "convert activity with place entity without geo to omni failover message with location")]
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

        [Test(Description = "convert activity with place entity without geo to omni failover message with location")]
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

        [Test(Description = "convert activity with image to omni failover message with image url")]
        public void ConvertActivityWithImageAttachmentToOmniFailoverMessage()
        {
            var attachment = new Attachment { ContentUrl = "http://dummy-image.com", ContentType = "image/jpeg" };
            _activity.Attachments = new List<Attachment> { attachment };

            var omniFailoverMessages = ToInfobipConverter.Convert(_activity, SCENARIO_KEY);
            Assert.IsNotNull(omniFailoverMessages);
            Assert.AreEqual(omniFailoverMessages.Count, 1);

            var omniFailoverMessage = omniFailoverMessages.First();
            Assert.AreEqual(omniFailoverMessage.ScenarioKey, SCENARIO_KEY);
            CheckDestinations(omniFailoverMessage.Destinations);

            var whatsAppMessage = omniFailoverMessage.WhatsApp;
            Assert.IsNotNull(whatsAppMessage);
            Assert.IsNull(whatsAppMessage.Text);
            Assert.IsNull(whatsAppMessage.FileUrl);
            Assert.IsNull(whatsAppMessage.LocationName);
            Assert.IsNull(whatsAppMessage.AudioUrl);
            Assert.IsNull(whatsAppMessage.Address);
            Assert.IsNull(whatsAppMessage.Latitude);
            Assert.IsNull(whatsAppMessage.Longitude);
            Assert.AreEqual(whatsAppMessage.ImageUrl, attachment.ContentUrl);
            Assert.IsNull(whatsAppMessage.VideoUrl);
            Assert.IsNull(whatsAppMessage.TemplateNamespace);
            Assert.IsNull(whatsAppMessage.TemplateData);
            Assert.IsNull(whatsAppMessage.TemplateName);
            Assert.IsNull(whatsAppMessage.Language);
            Assert.IsNull(whatsAppMessage.MediaTemplateData);
        }

        [Test(Description = "convert activity with image and text to omni failover message with image url and text")]
        public void ConvertActivityWithImageAttachmentAndTextToOmniFailoverMessage()
        {
            var attachment = new Attachment { ContentUrl = "http://dummy-image.com", ContentType = "image/jpeg" };
            _activity.Attachments = new List<Attachment> { attachment };
            _activity.Text = "Test text";

            var omniFailoverMessages = ToInfobipConverter.Convert(_activity, SCENARIO_KEY);
            Assert.IsNotNull(omniFailoverMessages);
            Assert.AreEqual(omniFailoverMessages.Count, 1);

            var omniFailoverMessage = omniFailoverMessages.First();
            Assert.AreEqual(omniFailoverMessage.ScenarioKey, SCENARIO_KEY);
            CheckDestinations(omniFailoverMessage.Destinations);

            var whatsAppMessage = omniFailoverMessage.WhatsApp;
            Assert.IsNotNull(whatsAppMessage);
            Assert.AreEqual(whatsAppMessage.Text, _activity.Text);
            Assert.IsNull(whatsAppMessage.FileUrl);
            Assert.IsNull(whatsAppMessage.LocationName);
            Assert.IsNull(whatsAppMessage.AudioUrl);
            Assert.IsNull(whatsAppMessage.Address);
            Assert.IsNull(whatsAppMessage.Latitude);
            Assert.IsNull(whatsAppMessage.Longitude);
            Assert.AreEqual(whatsAppMessage.ImageUrl, attachment.ContentUrl);
            Assert.IsNull(whatsAppMessage.VideoUrl);
            Assert.IsNull(whatsAppMessage.TemplateNamespace);
            Assert.IsNull(whatsAppMessage.TemplateData);
            Assert.IsNull(whatsAppMessage.TemplateName);
            Assert.IsNull(whatsAppMessage.Language);
            Assert.IsNull(whatsAppMessage.MediaTemplateData);
        }

        [Test(Description = "convert activity with video and image attachments and text to omni failover messages")]
        public void ConvertActivityWithTextAndMultipleAttachmentsToOmniFailoverMessage()
        {
            var attachment = new Attachment { ContentUrl = "http://dummy-video.com", ContentType = "video/mp4" };
            var attachment2 = new Attachment { ContentUrl = "http://dummy-image.com", ContentType = "image/jpeg" };
            _activity.Attachments = new List<Attachment> { attachment, attachment2 };
            _activity.Text = "Test text";

            var omniFailoverMessages = ToInfobipConverter.Convert(_activity, SCENARIO_KEY);
            Assert.IsNotNull(omniFailoverMessages);
            Assert.AreEqual(omniFailoverMessages.Count, 2);

            var omniFailoverMessage = omniFailoverMessages.First();
            var omniFailoverMessage2 = omniFailoverMessages.ElementAt(1);
            Assert.AreEqual(omniFailoverMessage.ScenarioKey, SCENARIO_KEY);
            Assert.AreEqual(omniFailoverMessage2.ScenarioKey, SCENARIO_KEY);
            CheckDestinations(omniFailoverMessage.Destinations);
            CheckDestinations(omniFailoverMessage2.Destinations);

            var whatsAppMessage = omniFailoverMessage.WhatsApp;
            Assert.IsNotNull(whatsAppMessage);
            Assert.AreEqual(whatsAppMessage.Text, _activity.Text);
            Assert.IsNull(whatsAppMessage.FileUrl);
            Assert.IsNull(whatsAppMessage.LocationName);
            Assert.IsNull(whatsAppMessage.AudioUrl);
            Assert.IsNull(whatsAppMessage.Address);
            Assert.IsNull(whatsAppMessage.Latitude);
            Assert.IsNull(whatsAppMessage.Longitude);
            Assert.IsNull(whatsAppMessage.ImageUrl);
            Assert.AreEqual(whatsAppMessage.VideoUrl, attachment.ContentUrl);
            Assert.IsNull(whatsAppMessage.TemplateNamespace);
            Assert.IsNull(whatsAppMessage.TemplateData);
            Assert.IsNull(whatsAppMessage.TemplateName);
            Assert.IsNull(whatsAppMessage.Language);
            Assert.IsNull(whatsAppMessage.MediaTemplateData);

            var whatsAppMessage2 = omniFailoverMessage2.WhatsApp;
            Assert.IsNotNull(whatsAppMessage2);
            Assert.IsNull(whatsAppMessage2.Text);
            Assert.IsNull(whatsAppMessage2.FileUrl);
            Assert.IsNull(whatsAppMessage2.LocationName);
            Assert.IsNull(whatsAppMessage2.AudioUrl);
            Assert.IsNull(whatsAppMessage2.Address);
            Assert.IsNull(whatsAppMessage2.Latitude);
            Assert.IsNull(whatsAppMessage2.Longitude);
            Assert.AreEqual(whatsAppMessage2.ImageUrl, attachment2.ContentUrl);
            Assert.IsNull(whatsAppMessage2.VideoUrl);
            Assert.IsNull(whatsAppMessage2.TemplateNamespace);
            Assert.IsNull(whatsAppMessage2.TemplateData);
            Assert.IsNull(whatsAppMessage2.TemplateName);
            Assert.IsNull(whatsAppMessage2.Language);
            Assert.IsNull(whatsAppMessage2.MediaTemplateData);
        }

        [Test(Description = "convert activity with video to omni failover message with video url")]
        public void ConvertActivityWithVideoAttachmentToOmniFailoverMessage()
        {
            var attachment = new Attachment { ContentUrl = "http://dummy-video.com", ContentType = "video/mp4" };
            _activity.Attachments = new List<Attachment> { attachment };

            var omniFailoverMessages = ToInfobipConverter.Convert(_activity, SCENARIO_KEY);
            Assert.IsNotNull(omniFailoverMessages);
            Assert.AreEqual(omniFailoverMessages.Count, 1);

            var omniFailoverMessage = omniFailoverMessages.First();
            Assert.AreEqual(omniFailoverMessage.ScenarioKey, SCENARIO_KEY);
            CheckDestinations(omniFailoverMessage.Destinations);

            var whatsAppMessage = omniFailoverMessage.WhatsApp;
            Assert.IsNotNull(whatsAppMessage);
            Assert.IsNull(whatsAppMessage.Text);
            Assert.IsNull(whatsAppMessage.FileUrl);
            Assert.IsNull(whatsAppMessage.LocationName);
            Assert.IsNull(whatsAppMessage.AudioUrl);
            Assert.IsNull(whatsAppMessage.Address);
            Assert.IsNull(whatsAppMessage.Latitude);
            Assert.IsNull(whatsAppMessage.Longitude);
            Assert.IsNull(whatsAppMessage.ImageUrl);
            Assert.AreEqual(whatsAppMessage.VideoUrl, attachment.ContentUrl);
            Assert.IsNull(whatsAppMessage.TemplateNamespace);
            Assert.IsNull(whatsAppMessage.TemplateData);
            Assert.IsNull(whatsAppMessage.TemplateName);
            Assert.IsNull(whatsAppMessage.Language);
            Assert.IsNull(whatsAppMessage.MediaTemplateData);
        }

        [Test(Description = "convert activity with video and text to omni failover message with video url and text")]
        public void ConvertActivityWithVideoAttachmentAndTextToOmniFailoverMessage()
        {
            var attachment = new Attachment { ContentUrl = "http://dummy-video.com", ContentType = "video/mp4" };
            _activity.Attachments = new List<Attachment> { attachment };
            _activity.Text = "Test text";

            var omniFailoverMessages = ToInfobipConverter.Convert(_activity, SCENARIO_KEY);
            Assert.IsNotNull(omniFailoverMessages);
            Assert.AreEqual(omniFailoverMessages.Count, 1);

            var omniFailoverMessage = omniFailoverMessages.First();
            Assert.AreEqual(omniFailoverMessage.ScenarioKey, SCENARIO_KEY);
            CheckDestinations(omniFailoverMessage.Destinations);

            var whatsAppMessage = omniFailoverMessage.WhatsApp;
            Assert.IsNotNull(whatsAppMessage);
            Assert.AreEqual(whatsAppMessage.Text, _activity.Text);
            Assert.IsNull(whatsAppMessage.FileUrl);
            Assert.IsNull(whatsAppMessage.LocationName);
            Assert.IsNull(whatsAppMessage.AudioUrl);
            Assert.IsNull(whatsAppMessage.Address);
            Assert.IsNull(whatsAppMessage.Latitude);
            Assert.IsNull(whatsAppMessage.Longitude);
            Assert.IsNull(whatsAppMessage.ImageUrl);
            Assert.AreEqual(whatsAppMessage.VideoUrl, attachment.ContentUrl);
            Assert.IsNull(whatsAppMessage.TemplateNamespace);
            Assert.IsNull(whatsAppMessage.TemplateData);
            Assert.IsNull(whatsAppMessage.TemplateName);
            Assert.IsNull(whatsAppMessage.Language);
            Assert.IsNull(whatsAppMessage.MediaTemplateData);
        }

        [Test(Description = "convert activity with audio to omni failover message with audio url")]
        public void ConvertActivityWithAudioAttachmentToOmniFailoverMessage()
        {
            var attachment = new Attachment { ContentUrl = "http://dummy-audio.com", ContentType = "audio/mp3" };
            _activity.Attachments = new List<Attachment> { attachment };

            var omniFailoverMessages = ToInfobipConverter.Convert(_activity, SCENARIO_KEY);
            Assert.IsNotNull(omniFailoverMessages);
            Assert.AreEqual(omniFailoverMessages.Count, 1);

            var omniFailoverMessage = omniFailoverMessages.First();
            Assert.AreEqual(omniFailoverMessage.ScenarioKey, SCENARIO_KEY);
            CheckDestinations(omniFailoverMessage.Destinations);

            CheckAudioMessage(omniFailoverMessage, attachment);
        }

        [Test(Description = "convert activity with audio and text to two omni failover messages")]
        public void ConvertActivityWithAudioAttachmentAndTextToOmniFailoverMessage()
        {
            var attachment = new Attachment { ContentUrl = "http://dummy-audio.com", ContentType = "audio/mp3" };
            _activity.Attachments = new List<Attachment> { attachment };
            _activity.Text = "Test text";

            var omniFailoverMessages = ToInfobipConverter.Convert(_activity, SCENARIO_KEY);
            Assert.IsNotNull(omniFailoverMessages);
            Assert.AreEqual(omniFailoverMessages.Count, 2);

            var omniFailoverMessage = omniFailoverMessages.First();
            var omniFailoverMessage2 = omniFailoverMessages.ElementAt(1);
            Assert.AreEqual(omniFailoverMessage.ScenarioKey, SCENARIO_KEY);
            Assert.AreEqual(omniFailoverMessage2.ScenarioKey, SCENARIO_KEY);
            CheckDestinations(omniFailoverMessage.Destinations);
            CheckDestinations(omniFailoverMessage2.Destinations);

            var whatsAppMessage = omniFailoverMessage.WhatsApp;
            Assert.IsNotNull(whatsAppMessage);
            Assert.AreEqual(whatsAppMessage.Text, _activity.Text);
            Assert.IsNull(whatsAppMessage.FileUrl);
            Assert.IsNull(whatsAppMessage.LocationName);
            Assert.IsNull(whatsAppMessage.AudioUrl);
            Assert.IsNull(whatsAppMessage.Address);
            Assert.IsNull(whatsAppMessage.Latitude);
            Assert.IsNull(whatsAppMessage.Longitude);
            Assert.IsNull(whatsAppMessage.ImageUrl);
            Assert.IsNull(whatsAppMessage.VideoUrl);
            Assert.IsNull(whatsAppMessage.TemplateNamespace);
            Assert.IsNull(whatsAppMessage.TemplateData);
            Assert.IsNull(whatsAppMessage.TemplateName);
            Assert.IsNull(whatsAppMessage.Language);
            Assert.IsNull(whatsAppMessage.MediaTemplateData);

            CheckAudioMessage(omniFailoverMessage2, attachment);
        }

        [Test(Description = "convert activity with file to omni failover message with file")]
        public void ConvertActivityWithFileAttachmentToOmniFailoverMessage()
        {
            var attachment = new Attachment { ContentUrl = "http://dummy-file.com", ContentType = "application/pdf" };
            _activity.Attachments = new List<Attachment> { attachment };

            var omniFailoverMessages = ToInfobipConverter.Convert(_activity, SCENARIO_KEY);
            Assert.IsNotNull(omniFailoverMessages);
            Assert.AreEqual(omniFailoverMessages.Count, 1);

            var omniFailoverMessage = omniFailoverMessages.First();
            Assert.AreEqual(omniFailoverMessage.ScenarioKey, SCENARIO_KEY);
            CheckDestinations(omniFailoverMessage.Destinations);

            var whatsAppMessage = omniFailoverMessage.WhatsApp;
            Assert.IsNotNull(whatsAppMessage);
            Assert.IsNull(whatsAppMessage.Text);
            Assert.AreEqual(whatsAppMessage.FileUrl, attachment.ContentUrl);
            Assert.IsNull(whatsAppMessage.LocationName);
            Assert.IsNull(whatsAppMessage.AudioUrl);
            Assert.IsNull(whatsAppMessage.Address);
            Assert.IsNull(whatsAppMessage.Latitude);
            Assert.IsNull(whatsAppMessage.Longitude);
            Assert.IsNull(whatsAppMessage.ImageUrl);
            Assert.IsNull(whatsAppMessage.VideoUrl);
            Assert.IsNull(whatsAppMessage.TemplateNamespace);
            Assert.IsNull(whatsAppMessage.TemplateData);
            Assert.IsNull(whatsAppMessage.TemplateName);
            Assert.IsNull(whatsAppMessage.Language);
            Assert.IsNull(whatsAppMessage.MediaTemplateData);
        }

        [Test(Description = "convert activity with file and text to omni failover message with file")]
        public void ConvertActivityWithFileAttachmentAndTextToOmniFailoverMessage()
        {
            var attachment = new Attachment { ContentUrl = "http://dummy-file.com", ContentType = "application/pdf" };
            _activity.Attachments = new List<Attachment> { attachment };
            _activity.Text = "Test text";

            var omniFailoverMessages = ToInfobipConverter.Convert(_activity, SCENARIO_KEY);
            Assert.IsNotNull(omniFailoverMessages);
            Assert.AreEqual(omniFailoverMessages.Count, 1);

            var omniFailoverMessage = omniFailoverMessages.First();
            Assert.AreEqual(omniFailoverMessage.ScenarioKey, SCENARIO_KEY);
            CheckDestinations(omniFailoverMessage.Destinations);

            var whatsAppMessage = omniFailoverMessage.WhatsApp;
            Assert.IsNotNull(whatsAppMessage);
            Assert.AreEqual(whatsAppMessage.Text, _activity.Text);
            Assert.AreEqual(whatsAppMessage.FileUrl, attachment.ContentUrl);
            Assert.IsNull(whatsAppMessage.LocationName);
            Assert.IsNull(whatsAppMessage.AudioUrl);
            Assert.IsNull(whatsAppMessage.Address);
            Assert.IsNull(whatsAppMessage.Latitude);
            Assert.IsNull(whatsAppMessage.Longitude);
            Assert.IsNull(whatsAppMessage.ImageUrl);
            Assert.IsNull(whatsAppMessage.VideoUrl);
            Assert.IsNull(whatsAppMessage.TemplateNamespace);
            Assert.IsNull(whatsAppMessage.TemplateData);
            Assert.IsNull(whatsAppMessage.TemplateName);
            Assert.IsNull(whatsAppMessage.Language);
            Assert.IsNull(whatsAppMessage.MediaTemplateData);
        }

        [Test(Description = "convert activity with template to omni failover message with template object")]
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
            Assert.IsNotNull(omniFailoverMessages);
            Assert.AreEqual(omniFailoverMessages.Count, 1);

            var omniFailoverMessage = omniFailoverMessages.First();
            Assert.AreEqual(omniFailoverMessage.ScenarioKey, SCENARIO_KEY);
            CheckDestinations(omniFailoverMessage.Destinations);

            var whatsAppMessage = omniFailoverMessage.WhatsApp;
            Assert.IsNotNull(whatsAppMessage);
            Assert.IsNull(whatsAppMessage.Text);
            Assert.IsNull(whatsAppMessage.FileUrl);
            Assert.IsNull(whatsAppMessage.LocationName);
            Assert.IsNull(whatsAppMessage.AudioUrl);
            Assert.IsNull(whatsAppMessage.Address);
            Assert.IsNull(whatsAppMessage.Latitude);
            Assert.IsNull(whatsAppMessage.Longitude);
            Assert.IsNull(whatsAppMessage.ImageUrl);
            Assert.IsNull(whatsAppMessage.VideoUrl);
            Assert.AreEqual(whatsAppMessage.TemplateNamespace, templateMessage.TemplateNamespace);
            Assert.IsNotNull(whatsAppMessage.TemplateData);
            Assert.AreEqual(whatsAppMessage.TemplateData.Length, templateMessage.TemplateData.Length);
            Assert.AreEqual(whatsAppMessage.TemplateName, templateMessage.TemplateName);
            Assert.AreEqual(whatsAppMessage.Language, templateMessage.Language);
            Assert.IsNotNull(whatsAppMessage.MediaTemplateData);
            Assert.AreEqual(whatsAppMessage.MediaTemplateData.MediaTemplateBody.Placeholders.Length, templateMessage.MediaTemplateData.MediaTemplateBody.Placeholders.Length);
            Assert.AreEqual(whatsAppMessage.MediaTemplateData.MediaTemplateHeader.DocumentFilename, templateMessage.MediaTemplateData.MediaTemplateHeader.DocumentFilename);
            Assert.IsNull(whatsAppMessage.MediaTemplateData.MediaTemplateHeader.Latitude);
            Assert.IsNull(whatsAppMessage.MediaTemplateData.MediaTemplateHeader.Longitude);
            Assert.IsNull(whatsAppMessage.MediaTemplateData.MediaTemplateHeader.ImageUrl);
            Assert.IsNull(whatsAppMessage.MediaTemplateData.MediaTemplateHeader.DocumentUrl);
            Assert.IsNull(whatsAppMessage.MediaTemplateData.MediaTemplateHeader.TextPlaceholder);
            Assert.IsNull(whatsAppMessage.MediaTemplateData.MediaTemplateHeader.VideoUrl);
        }
        
        [Test(Description = "convert text activity with callback data entity")]
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
            Assert.AreEqual(message.CallbackData, entityCallbackData.Properties.ToInfobipCallbackDataJson());
        }

        [Test(Description = "convert empty activity with callback data entity")]
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
            Assert.AreEqual(messages.Any(), false);
        }

        private void CheckLocationMessage(InfobipOmniFailoverMessage omniFailoverMessage, GeoCoordinates geoCoordinate)
        {
            Assert.AreEqual(omniFailoverMessage.ScenarioKey, SCENARIO_KEY);
            CheckDestinations(omniFailoverMessage.Destinations);

            Assert.NotNull(geoCoordinate);

            var whatsAppMessage = omniFailoverMessage.WhatsApp;
            Assert.IsNotNull(whatsAppMessage);
            Assert.IsNull(whatsAppMessage.Text);
            Assert.IsNull(whatsAppMessage.FileUrl);
            Assert.AreEqual(whatsAppMessage.LocationName, geoCoordinate.Name);
            Assert.IsNull(whatsAppMessage.AudioUrl);
            Assert.IsNull(whatsAppMessage.Address);
            Assert.AreEqual(whatsAppMessage.Latitude, geoCoordinate.Latitude);
            Assert.AreEqual(whatsAppMessage.Longitude, geoCoordinate.Longitude);
            Assert.IsNull(whatsAppMessage.ImageUrl);
            Assert.IsNull(whatsAppMessage.VideoUrl);
            Assert.IsNull(whatsAppMessage.TemplateNamespace);
            Assert.IsNull(whatsAppMessage.TemplateData);
            Assert.IsNull(whatsAppMessage.TemplateName);
            Assert.IsNull(whatsAppMessage.Language);
            Assert.IsNull(whatsAppMessage.MediaTemplateData);
        }

        private void CheckLocationMessage(InfobipOmniFailoverMessage omniFailoverMessage, Place place)
        {
            Assert.AreEqual(omniFailoverMessage.ScenarioKey, SCENARIO_KEY);
            CheckDestinations(omniFailoverMessage.Destinations);

            Assert.NotNull(place);

            var geoCoordinate = JsonConvert.DeserializeObject<GeoCoordinates>(JsonConvert.SerializeObject(place.Geo));
            Assert.NotNull(geoCoordinate);

            var whatsAppMessage = omniFailoverMessage.WhatsApp;
            Assert.IsNotNull(whatsAppMessage);
            Assert.IsNull(whatsAppMessage.Text);
            Assert.IsNull(whatsAppMessage.FileUrl);
            Assert.AreEqual(whatsAppMessage.LocationName, geoCoordinate.Name);
            Assert.IsNull(whatsAppMessage.AudioUrl);
            Assert.AreEqual(whatsAppMessage.Address, place.Address);
            Assert.AreEqual(whatsAppMessage.Latitude, geoCoordinate.Latitude);
            Assert.AreEqual(whatsAppMessage.Longitude, geoCoordinate.Longitude);
            Assert.IsNull(whatsAppMessage.ImageUrl);
            Assert.IsNull(whatsAppMessage.VideoUrl);
            Assert.IsNull(whatsAppMessage.TemplateNamespace);
            Assert.IsNull(whatsAppMessage.TemplateData);
            Assert.IsNull(whatsAppMessage.TemplateName);
            Assert.IsNull(whatsAppMessage.Language);
            Assert.IsNull(whatsAppMessage.MediaTemplateData);
        }

        private void CheckDestinations(InfobipDestination[] destinations)
        {
            Assert.IsNotNull(destinations);
            Assert.AreEqual(destinations.Length, 1);

            var destination = destinations.First();
            Assert.IsNotNull(destination.To);
            Assert.AreEqual(destination.To.PhoneNumber, _activity.Recipient.Id);
        }

        private void CheckAudioMessage(InfobipOmniFailoverMessage omniFailoverMessage, Attachment attachment)
        {
            var whatsAppMessage = omniFailoverMessage.WhatsApp;
            Assert.IsNotNull(whatsAppMessage);
            Assert.IsNull(whatsAppMessage.Text);
            Assert.IsNull(whatsAppMessage.FileUrl);
            Assert.IsNull(whatsAppMessage.LocationName);
            Assert.AreEqual(whatsAppMessage.AudioUrl, attachment.ContentUrl);
            Assert.IsNull(whatsAppMessage.Address);
            Assert.IsNull(whatsAppMessage.Latitude);
            Assert.IsNull(whatsAppMessage.Longitude);
            Assert.IsNull(whatsAppMessage.ImageUrl);
            Assert.IsNull(whatsAppMessage.VideoUrl);
            Assert.IsNull(whatsAppMessage.TemplateNamespace);
            Assert.IsNull(whatsAppMessage.TemplateData);
            Assert.IsNull(whatsAppMessage.TemplateName);
            Assert.IsNull(whatsAppMessage.Language);
            Assert.IsNull(whatsAppMessage.MediaTemplateData);
        }
    }
}
