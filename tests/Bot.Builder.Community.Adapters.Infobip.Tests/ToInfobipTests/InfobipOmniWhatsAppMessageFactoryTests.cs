using System;
using System.Collections.Generic;
using System.Linq;
using Bot.Builder.Community.Adapters.Infobip.Models;
using Bot.Builder.Community.Adapters.Infobip.ToInfobip;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Xunit;

namespace Bot.Builder.Community.Adapters.Infobip.Tests.ToInfobipTests
{
    public class InfobipOmniWhatsAppMessageFactoryTests
    {
        private Activity _activity;

        public InfobipOmniWhatsAppMessageFactoryTests()
        {
            _activity = new Activity
            {
                Type = ActivityTypes.Message,
                Id = "message id",
                Timestamp = DateTimeOffset.Parse("2020-02-26T10:15:48.734+0000"),
                ChannelId = InfobipChannel.WhatsApp,
                Conversation = new ConversationAccount { Id = "subscriber-number" },
                From = new ChannelAccount { Id = "whatsapp-number" },
                Recipient = new ChannelAccount { Id = "subscriber-number" }
            };
        }

        [Fact]
        public void ConvertTextActivityToOmniWhatsAppFailoverMessage()
        {
            _activity.Text = "Test text";

            var whatsAppMessages = InfobipOmniWhatsAppMessageFactory.Create(_activity);
            Assert.NotNull(whatsAppMessages);
            Assert.Single(whatsAppMessages);

            var whatsAppMessage = whatsAppMessages.Single();

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
        public void ConvertActivityWithSingleGeoCoordinateEntityToOmniWhatsAppFailoverMessage()
        {
            _activity.Entities = new List<Entity> { new GeoCoordinates { Latitude = 12.3456789, Longitude = 23.456789, Name = "Test" } };

            var whatsAppMessages = InfobipOmniWhatsAppMessageFactory.Create(_activity);
            Assert.NotNull(whatsAppMessages);
            Assert.Single(whatsAppMessages);

            var whatsAppMessage = whatsAppMessages.First();
            CheckLocationMessage(whatsAppMessage, _activity.Entities.First().GetAs<GeoCoordinates>());
        }

        [Fact]
        public void ConvertActivityWithMultipleGeoCoordinateEntityToOmniWhatsAppFailoverMessage()
        {
            _activity.Entities = new List<Entity>
            {
                new GeoCoordinates {Latitude = 12.3456789, Longitude = 23.456789, Name = "Test"},
                new GeoCoordinates {Latitude = 45.56789, Longitude = 87.12345, Name = "Test2"}
            };

            var whatsAppMessages = InfobipOmniWhatsAppMessageFactory.Create(_activity);
            Assert.NotNull(whatsAppMessages);
            Assert.Equal(2, whatsAppMessages.Count);

            for (var i = 0; i < whatsAppMessages.Count; i++)
                CheckLocationMessage(whatsAppMessages.ElementAt(i), _activity.Entities.ElementAt(i).GetAs<GeoCoordinates>());
        }

        [Fact]
        public void ConvertActivityWithSinglePlaceEntityToOmniWhatsAppFailoverMessage()
        {
            _activity.Entities = new List<Entity>
            {
                new Place
                {
                    Address = "Address",
                    Geo = new GeoCoordinates {Latitude = 12.3456789, Longitude = 23.456789, Name = "Test"}
                }
            };

            var whatsAppMessages = InfobipOmniWhatsAppMessageFactory.Create(_activity);
            Assert.NotNull(whatsAppMessages);
            Assert.Single(whatsAppMessages);

            var whatsAppMessage = whatsAppMessages.First();
            CheckLocationMessage(whatsAppMessage, _activity.Entities.First().GetAs<Place>());
        }

        [Fact]
        public void ConvertActivityWithSinglePlaceEntityWithoutGeoToOmniWhatsAppFailoverMessage()
        {
            _activity.Entities = new List<Entity>
            {
                new Place
                {
                    Address = "Address"
                }
            };

            Assert.Throws<Exception>(() => InfobipOmniWhatsAppMessageFactory.Create(_activity));
        }

        [Fact]
        public void ConvertActivityWithSinglePlaceEntityWithNonStringAddressOmniWhatsAppFailoverMessage()
        {
            _activity.Entities = new List<Entity>
            {
                new Place
                {
                    Address = new { Key = "value" }
                }
            };

            Assert.Throws<Exception>(() => InfobipOmniWhatsAppMessageFactory.Create(_activity));
        }

        [Fact]
        public void ConvertActivityWithImageAttachmentToOmniWhatsAppFailoverMessage()
        {
            var attachment = new Attachment { ContentUrl = "http://dummy-image.com", ContentType = "image/jpeg" };
            _activity.Attachments = new List<Attachment> { attachment };

            var whatsAppMessages = InfobipOmniWhatsAppMessageFactory.Create(_activity);
            Assert.NotNull(whatsAppMessages);
            Assert.Single(whatsAppMessages);

            var whatsAppMessage = whatsAppMessages.Single();

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
        public void ConvertActivityWithImageAttachmentAndTextToOmniWhatsAppFailoverMessage()
        {
            var attachment = new Attachment { ContentUrl = "http://dummy-image.com", ContentType = "image/jpeg" };
            _activity.Attachments = new List<Attachment> { attachment };
            _activity.Text = "Test text";

            var whatsAppMessages = InfobipOmniWhatsAppMessageFactory.Create(_activity);
            Assert.NotNull(whatsAppMessages);
            Assert.Single(whatsAppMessages);

            var whatsAppMessage = whatsAppMessages.Single();
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
        public void ConvertActivityWithTextAndMultipleAttachmentsToOmniWhatsAppFailoverMessage()
        {
            var attachment = new Attachment { ContentUrl = "http://dummy-video.com", ContentType = "video/mp4" };
            var attachment2 = new Attachment { ContentUrl = "http://dummy-image.com", ContentType = "image/jpeg" };
            _activity.Attachments = new List<Attachment> { attachment, attachment2 };
            _activity.Text = "Test text";

            var whatsAppMessages = InfobipOmniWhatsAppMessageFactory.Create(_activity);
            Assert.NotNull(whatsAppMessages);
            Assert.Equal(2, whatsAppMessages.Count);

            var whatsAppMessage = whatsAppMessages.First();

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

            var whatsAppMessage2 = whatsAppMessages.ElementAt(1);

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
        public void ConvertActivityWithVideoAttachmentToOmniWhatsAppFailoverMessage()
        {
            var attachment = new Attachment { ContentUrl = "http://dummy-video.com", ContentType = "video/mp4" };
            _activity.Attachments = new List<Attachment> { attachment };

            var whatsAppMessages = InfobipOmniWhatsAppMessageFactory.Create(_activity);
            Assert.NotNull(whatsAppMessages);
            Assert.Single(whatsAppMessages);

            var whatsAppMessage = whatsAppMessages.Single();
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
        public void ConvertActivityWithVideoAttachmentAndTextToOmniWhatsAppFailoverMessage()
        {
            var attachment = new Attachment { ContentUrl = "http://dummy-video.com", ContentType = "video/mp4" };
            _activity.Attachments = new List<Attachment> { attachment };
            _activity.Text = "Test text";

            var whatsAppMessages = InfobipOmniWhatsAppMessageFactory.Create(_activity);
            Assert.NotNull(whatsAppMessages);
            Assert.Single(whatsAppMessages);

            var whatsAppMessage = whatsAppMessages.Single();
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
        public void ConvertActivityWithAudioAttachmentToOmniWhatsAppFailoverMessage()
        {
            var attachment = new Attachment { ContentUrl = "http://dummy-audio.com", ContentType = "audio/mp3" };
            _activity.Attachments = new List<Attachment> { attachment };

            var whatsAppMessages = InfobipOmniWhatsAppMessageFactory.Create(_activity);
            Assert.NotNull(whatsAppMessages);
            Assert.Single(whatsAppMessages);

            var whatsAppMessage = whatsAppMessages.First();

            CheckAudioMessage(whatsAppMessage, attachment);
        }

        [Fact]
        public void ConvertActivityWithAudioAttachmentAndTextToOmniWhatsAppFailoverMessage()
        {
            var attachment = new Attachment { ContentUrl = "http://dummy-audio.com", ContentType = "audio/mp3" };
            _activity.Attachments = new List<Attachment> { attachment };
            _activity.Text = "Test text";

            var whatsAppMessages = InfobipOmniWhatsAppMessageFactory.Create(_activity);
            Assert.NotNull(whatsAppMessages);
            Assert.Equal(2, whatsAppMessages.Count);

            var whatsAppMessage = whatsAppMessages.First();
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

            CheckAudioMessage(whatsAppMessages.ElementAt(1), attachment);
        }

        [Fact]
        public void ConvertActivityWithFileAttachmentToOmniFailoverMessage()
        {
            var attachment = new Attachment { ContentUrl = "http://dummy-file.com", ContentType = "application/pdf" };
            _activity.Attachments = new List<Attachment> { attachment };

            var whatsAppMessages = InfobipOmniWhatsAppMessageFactory.Create(_activity);
            Assert.NotNull(whatsAppMessages);
            Assert.Single(whatsAppMessages);

            var whatsAppMessage = whatsAppMessages.Single();
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
        public void ConvertActivityWithFileAttachmentAndTextToOmniWhatsAppFailoverMessage()
        {
            var attachment = new Attachment { ContentUrl = "http://dummy-file.com", ContentType = "application/pdf" };
            _activity.Attachments = new List<Attachment> { attachment };
            _activity.Text = "Test text";

            var whatsAppMessages = InfobipOmniWhatsAppMessageFactory.Create(_activity);
            Assert.NotNull(whatsAppMessages);
            Assert.Single(whatsAppMessages);

            var whatsAppMessage = whatsAppMessages.Single();
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
        public void ConvertActivityWithTemplateAttachmentToOmniWhatsAppFailoverMessage()
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

            _activity.AddInfobipWhatsAppTemplateMessage(templateMessage);

            var whatsAppMessages = InfobipOmniWhatsAppMessageFactory.Create(_activity);
            Assert.NotNull(whatsAppMessages);
            Assert.Single(whatsAppMessages);

            var whatsAppMessage = whatsAppMessages.Single();
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

        private void CheckLocationMessage(InfobipOmniWhatsAppMessage whatsAppMessage, GeoCoordinates geoCoordinate)
        {
            Assert.NotNull(geoCoordinate);

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

        private void CheckLocationMessage(InfobipOmniWhatsAppMessage whatsAppMessage, Place place)
        {
            Assert.NotNull(place);

            var geoCoordinate = JsonConvert.DeserializeObject<GeoCoordinates>(JsonConvert.SerializeObject(place.Geo));
            Assert.NotNull(geoCoordinate);

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

        private void CheckAudioMessage(InfobipOmniWhatsAppMessage whatsAppMessage, Attachment attachment)
        {
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
