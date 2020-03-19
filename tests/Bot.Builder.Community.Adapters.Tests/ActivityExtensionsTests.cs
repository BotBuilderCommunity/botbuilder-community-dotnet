using System.Collections.Generic;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Models;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Models.Twitter;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bot.Builder.Community.Adapters.Twitter.Tests
{
    [TestClass]
    [TestCategory("Twitter")]
    public class ActivityExtensionsTests
    {
        [TestMethod]
        public void AsTwitterMessageWithEmptyMessageShouldFail()
        {
            var activity = new Activity()
            {
                Text = null,
                Type = ActivityTypes.Message
            };

            Assert.ThrowsException<TwitterException>(
                () =>
            {
                activity.AsTwitterMessage();
            }, "You can't send an empty message.");
        }

        [TestMethod]
        public void AsTwitterMessageWithLongMessageShouldFail()
        {
            var activity = new Activity()
            {
                Text = new string('a', 10001),
                Type = ActivityTypes.Message
            };

            Assert.ThrowsException<TwitterException>(
                () =>
            {
                activity.AsTwitterMessage();
            }, "Invalid message, the length of the message should be less than 10000 chars.");
        }

        [TestMethod]
        public void AsTwitterMessageShouldReturnNewDirectMessage()
        {
            var activity = new Activity()
            {
                Text = "test",
                Type = ActivityTypes.Message,
                Recipient = new ChannelAccount()
                {
                    Id = null
                }
            };

            Assert.IsInstanceOfType(activity.AsTwitterMessage(), typeof(NewDirectMessageObject));
        }

        [TestMethod]
        public void AsTwitterMessageShouldReturnNewDirectMessageWithQuickReply()
        {
            var activity = new Activity()
            {
                Text = "test",
                Type = ActivityTypes.Message,
                Recipient = new ChannelAccount()
                {
                    Id = null
                },
                SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Get Started", value: "https://docs.microsoft.com/bot-framework") }
                }
            };

            Assert.IsInstanceOfType(activity.AsTwitterMessage().Event.MessageCreate.MessageData.QuickReply, typeof(NewEvent_QuickReply));
        }
    }
}
