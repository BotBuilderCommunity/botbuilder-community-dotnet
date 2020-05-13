using System.Collections.Generic;
using Alexa.NET;
using Alexa.NET.ConnectionTasks.Inputs;
using Alexa.NET.Response;
using Alexa.NET.Response.Directive;
using Alexa.NET.Response.Directive.Templates.Types;
using Bot.Builder.Community.Adapters.Alexa.Core.Attachments;
using Bot.Builder.Community.Adapters.Alexa.Core.Helpers;
using Bot.Builder.Community.Adapters.Alexa.Tests.Helpers;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using Xunit;
using AlexaCardImage = Alexa.NET.Response.CardImage;
using AlexaIntent = Alexa.NET.Request.Intent;

namespace Bot.Builder.Community.Adapters.Alexa.Tests
{
    public class AttachmentHelperTests
    {
        [Fact]
        public void ConvertNullActivityDoesNothing()
        {
            AttachmentHelper.ConvertAttachmentContent(null);
        }

        [Fact]
        public void ConvertNullAttachmentsDoesNothing()
        {
            var activity = new Activity
            {
                Attachments = null
            };
            activity.ConvertAttachmentContent();
        }

        [Fact]
        public void ConvertAttachmentContentTypeNullDoesNothing()
        {
            var activity = new Activity
            {
                Attachments = new List<Attachment> { new TestAttachment { ContentType = null } }
            };
            activity.ConvertAttachmentContent();

            Assert.Equal(typeof(TestAttachment), activity.Attachments[0].GetType());
        }

        [Fact]
        public void ConvertAttachmentAlexaSimpleCard() =>
            VerifyAttachmentContentConversion<SimpleCard>(new SimpleCard
            {
                Content = "Simple card content",
                Title = "Simple card title"
            }.ToAttachment());

        [Fact]
        public void ConvertAttachmentAlexaLinkAccountCard() =>
            VerifyAttachmentContentConversion<LinkAccountCard>(new LinkAccountCard { }.ToAttachment());

        [Fact]
        public void ConvertAttachmentAlexaAskForPermissionsConsentCard() =>
            VerifyAttachmentContentConversion<AskForPermissionsConsentCard>(new AskForPermissionsConsentCard
            {
                Permissions = new List<string> { "permission 1", "permission 2" }
            }.ToAttachment());

        [Fact]
        public void ConvertAttachmentAlexaStandardCard() =>
            VerifyAttachmentContentConversion<StandardCard>(new StandardCard
            {
                Content = "Standard card content",
                Title = "Standard card title",
                Image = new AlexaCardImage { LargeImageUrl = "https://someimagehost/largeimage.png", SmallImageUrl = "https://someimagehost/smallimage.png" }
            }.ToAttachment());

        [Fact]
        public void NonSupportedAttachmentCardIsIgnored() =>
            VerifyAttachmentContentConversion<NonSupportedCard>(new NonSupportedCard
            {
                Value = "non-supported card value"
            }.ToAttachment(), false);

        [Fact]
        public void ConvertAttachmentAudioPlayerPlayDirective() =>
            VerifyAttachmentContentConversion<AudioPlayerPlayDirective>(new AudioPlayerPlayDirective
            {
                AudioItem = new AudioItem { Stream = new AudioItemStream { Url = "https://streamhost/stream", Token = "123" } },
                PlayBehavior = PlayBehavior.ReplaceAll
            }.ToAttachment());

        [Fact]
        public void ConvertAttachmentClearQueueDirective() =>
            VerifyAttachmentContentConversion<ClearQueueDirective>(new ClearQueueDirective
            {
                ClearBehavior = ClearBehavior.ClearEnqueued
            }.ToAttachment());

        [Fact]
        public void ConvertAttachmentDialogConfirmIntent() =>
            VerifyAttachmentContentConversion<DialogConfirmIntent>(new DialogConfirmIntent
            {
                UpdatedIntent = new AlexaIntent { Name = "Intent name", ConfirmationStatus = "success" }
            }.ToAttachment());

        [Fact]
        public void ConvertAttachmentDialogConfirmSlot() =>
            VerifyAttachmentContentConversion<DialogConfirmSlot>(new DialogConfirmSlot("slot name").ToAttachment());

        [Fact]
        public void ConvertAttachmentDialogDelegate() =>
            VerifyAttachmentContentConversion<DialogDelegate>(new DialogDelegate
            {
                UpdatedIntent = new AlexaIntent { Name = "Intent name", ConfirmationStatus = "success" }
            }.ToAttachment());

        [Fact]
        public void ConvertAttachmentDialogElicitSlot() =>
            VerifyAttachmentContentConversion<DialogElicitSlot>(new DialogElicitSlot("slot name")
            {
                UpdatedIntent = new AlexaIntent { Name = "Intent name", ConfirmationStatus = "success" }
            }.ToAttachment());

        [Fact]
        public void ConvertAttachmentDisplayRenderTemplateDirective() =>
            VerifyAttachmentContentConversion<DisplayRenderTemplateDirective>(new DisplayRenderTemplateDirective
            {
                Template = new BodyTemplate1 { Title = "body template 1" }
            }.ToAttachment());

        [Fact]
        public void ConvertAttachmentHintDirective() =>
            VerifyAttachmentContentConversion<HintDirective>(new HintDirective
            {
                Hint = new Hint { Text = "plain text hint" }
            }.ToAttachment());

        [Fact]
        public void ConvertAttachmentStopDirective() =>
            VerifyAttachmentContentConversion<StopDirective>(new StopDirective().ToAttachment());

        [Fact]
        public void ConvertAttachmentVideoAppDirective() =>
            VerifyAttachmentContentConversion<VideoAppDirective>(new VideoAppDirective
            {
                VideoItem = new VideoItem("https://videoitemhost/videoitem")
            }.ToAttachment());

        [Fact]
        public void ConvertAttachmentStartConnectionDirective() =>
            VerifyAttachmentContentConversion<StartConnectionDirective>(new StartConnectionDirective
            {
                Token = "123",
                Uri = "https://somewhere/somewhere",
                Input = new PrintPdfV1 { Url = "https://somewhere/url" }
            }.ToAttachment());

        [Fact]
        public void ConvertAttachmentCompleteTaskDirective() =>
            VerifyAttachmentContentConversion<CompleteTaskDirective>(new CompleteTaskDirective
            {
                Status = new ConnectionStatus { Code = 123, Message = "done" }
            }.ToAttachment());

        [Fact]
        public void ConvertAttachmentDialogUpdateDynamicEntities() =>
            VerifyAttachmentContentConversion<DialogUpdateDynamicEntities>(new DialogUpdateDynamicEntities
            {
                Types = new List<SlotType> { new SlotType { Name = "slot name", Values = new [] { new SlotTypeValue { Id = "id" } } } },
                UpdateBehavior = UpdateBehavior.Replace
            }.ToAttachment());

        private void VerifyAttachmentContentConversion<T>(Attachment attachment, bool supportedType = true)
        {
            var activity = new Activity
            {
                Attachments = new List<Attachment> { attachment }
            };

            // Prior to serialization type is as expected.
            Assert.Equal(typeof(JObject), activity.Attachments[0].Content.GetType());

            var anonymizedActivity = ActivityHelper.GetAnonymizedActivity(activity);

            // After conversion the type information is lost.
            Assert.Equal(typeof(JObject), anonymizedActivity.Attachments[0].Content.GetType());

            anonymizedActivity.ConvertAttachmentContent();

            // After converting Alexa attachments the type information is back.
            Assert.Equal(supportedType ? typeof(T) : typeof(JObject), anonymizedActivity.Attachments[0].Content.GetType());
        }

        class TestAttachment : Attachment
        {
        }

        class NonSupportedCard : ICard
        {
            string IResponse.Type => nameof(NonSupportedCard);
            public string Value;
        }
    }
}
