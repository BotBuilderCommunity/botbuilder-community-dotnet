using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Models;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Models.Twitter;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Twitter
{
    public static class ActivityExtensions
    {
        public static NewDirectMessageObject AsTwitterMessage(this Activity activity)
        {
            if (string.IsNullOrEmpty(activity.Text))
            {
                throw new TwitterException("You can't send an empty message.");
            }

            if (activity.Text.Length > 10000)
            {
                throw new TwitterException(
                    "Invalid message, the length of the message should be less than 10000 chars.");
            }

            var newDmEvent = new NewDirectMessageObject
            {
                Event = new Event
                {
                    EventType = "message_create",
                    MessageCreate = new NewEvent_MessageCreate
                    {
                        MessageData = new NewEvent_MessageData {Text = activity.Text},
                        target = new Target {recipient_id = activity.Recipient.Id}
                    }
                }
            };

            if (activity.SuggestedActions?.Actions != null && activity.SuggestedActions.Actions.Any())
            {
                newDmEvent.Event.MessageCreate.MessageData.QuickReply = new NewEvent_QuickReply
                {
                    Options = activity.SuggestedActions.Actions
                        .Select(x => new NewEvent_QuickReplyOption {Label = x.Title}).ToList()
                };
            }

            return newDmEvent;
        }
    }
}
