using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Models;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Models.Twitter;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bot.Builder.Community.Adapters.Twitter.Tests
{
    [TestClass]
    public class ActivityExtensionsTests
    {
        [TestMethod]
        public void ShouldFailWhenMessageEmpty()
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
        public void ShouldFailWhenMessageTooLong()
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
        public void ShouldReturnNewDirectMessage()
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
        public void ShouldReturnNewDirectMessageWithQuickReply()
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
