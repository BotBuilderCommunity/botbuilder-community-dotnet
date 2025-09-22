using System;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Infobip.Messages.Models;
using Bot.Builder.Community.Adapters.Infobip.Messages.ToInfobip;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Bot.Builder.Community.Adapters.Infobip.Messages.Tests.ToInfobipTests
{
    public class ToInfobipMessagesConverterTests
    {
        private readonly ToInfobipMessagesConverter _converter;
        private readonly InfobipMessagesAdapterOptions _options;

        public ToInfobipMessagesConverterTests()
        {
            _options = new InfobipMessagesAdapterOptions("test-api-key", "https://api.infobip.com")
            {
                DefaultSender = "447860099299",
                DefaultChannel = "WHATSAPP"
            };

            var mockLogger = new Mock<ILogger>();
            _converter = new ToInfobipMessagesConverter(_options, mockLogger.Object);
        }

        [Fact]
        public async Task ConvertTextMessage_ShouldGenerateCorrectJson()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = "May the Force be with you.",
                Id = "test-message-id",
                Recipient = new ChannelAccount { Id = "447860099299" },
                Conversation = new ConversationAccount { Id = "111111111" }
            };

            // Act
            var result = await _converter.Convert(activity, "111111111");

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Messages);
            Assert.Single(result.Messages);

            var message = result.Messages[0];
            Assert.Equal("WHATSAPP", message.Channel);
            Assert.Equal("447860099299", message.Sender);
            Assert.Single(message.Destinations);
            Assert.Equal("111111111", message.Destinations[0].To);
            Assert.NotNull(message.Content);
            Assert.NotNull(message.Content.Body);
            Assert.Equal("May the Force be with you.", message.Content.Body.Text);
            Assert.Equal("TEXT", message.Content.Body.Type);

            // Test JSON serialization
            var json = JsonConvert.SerializeObject(result, Formatting.None);
            Assert.Contains("\"messages\":", json);
            Assert.Contains("\"channel\":\"WHATSAPP\"", json);
            Assert.Contains("\"sender\":\"447860099299\"", json);
            Assert.Contains("\"destinations\":[{\"to\":\"111111111\"", json); // Accept additional fields
            Assert.Contains("\"text\":\"May the Force be with you.\"", json);
            Assert.Contains("\"type\":\"TEXT\"", json);
        }

        [Fact]
        public async Task ConvertWhatsAppButtonMessage_ShouldGenerateCorrectJson()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = "You take the blue pill, the story ends. You wake up in your bed and believe whatever you want to. You take the red pill, you stay in Wonderland, and I show you how deep the rabbit hole goes. Which one do you choose?",
                Id = "test-message-id",
                Recipient = new ChannelAccount { Id = "447860099299" },
                Conversation = new ConversationAccount { Id = "111111111" },
                SuggestedActions = new SuggestedActions
                {
                    Actions = new[]
                    {
                        new CardAction(ActionTypes.PostBack, "Red", value: "User stayed in Wonderland."),
                        new CardAction(ActionTypes.PostBack, "Blue", value: "User went down the rabbit hole.")
                    }
                }
            };

            // Act
            var result = await _converter.Convert(activity, "111111111");

            // Assert
            var message = result.Messages[0];
            Assert.Equal(2, message.Content.Buttons.Length);
            Assert.Equal("Red", message.Content.Buttons[0].Text);
            Assert.Equal("User stayed in Wonderland.", message.Content.Buttons[0].PostbackData);
            Assert.Equal("QUICK_REPLY", message.Content.Buttons[0].Type);
            Assert.Equal("Blue", message.Content.Buttons[1].Text);
            Assert.Equal("User went down the rabbit hole.", message.Content.Buttons[1].PostbackData);

            // Test JSON serialization matches expected format
            var json = JsonConvert.SerializeObject(result, Formatting.None);
            Assert.Contains("\"buttons\":[{", json);
            Assert.Contains("\"text\":\"Red\"", json);
            Assert.Contains("\"postbackData\":\"User stayed in Wonderland.\"", json);
            Assert.Contains("\"type\":\"QUICK_REPLY\"", json);
            Assert.Contains("\"text\":\"Blue\"", json);
            Assert.Contains("\"postbackData\":\"User went down the rabbit hole.\"", json);
        }

        [Fact]
        public async Task ConvertSmsMessage_ShouldUseSmsChannel()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = "May the Force be with you.",
                Id = "test-message-id",
                Recipient = new ChannelAccount { Id = "447491163443" },
                Conversation = new ConversationAccount { Id = "111111111" }
            };

            activity.SetInfobipChannel("SMS");

            // Act
            var result = await _converter.Convert(activity, "111111111");

            // Assert
            var message = result.Messages[0];
            Assert.Equal("SMS", message.Channel);
        }

        [Fact]
        public async Task ConvertWithCallbackData_ShouldIncludeCallbackData()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = "Test message",
                Id = "test-message-id",
                Recipient = new ChannelAccount { Id = "447860099299" },
                Conversation = new ConversationAccount { Id = "111111111" }
            };

            var callbackData = new System.Collections.Generic.Dictionary<string, string>
            {
                ["orderId"] = "12345",
                ["userId"] = "user123"
            };
            activity.AddInfobipCallbackData(callbackData);

            // Act
            var result = await _converter.Convert(activity, "111111111");

            // Assert
            var message = result.Messages[0];
            Assert.NotNull(message.CallbackData);
            var deserializedCallback = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, string>>(message.CallbackData);
            Assert.Equal("12345", deserializedCallback["orderId"]);
            Assert.Equal("user123", deserializedCallback["userId"]);
        }
    }
}