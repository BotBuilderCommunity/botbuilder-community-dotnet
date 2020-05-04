using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Zoom.Attachments;
using Bot.Builder.Community.Adapters.Zoom.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Samples.Zoom.Bots
{
    public class EchoBot : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            switch (turnContext.Activity.Text.ToLower())
            {
                default:
                    await turnContext.SendActivityAsync(MessageFactory.Text($"_Echo: {turnContext.Activity.Text}. What's next?_", inputHint: InputHints.ExpectingInput), cancellationToken);
                    break;

                case "finish":
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Ok, I *won't ask anymore.*"), cancellationToken);
                    break;

                case "multiple":
                    var multiPartMessage = MessageFactory.SuggestedActions(
                        new List<CardAction>() {
                            new CardAction(text: "This is the text", displayText: "display text"),
                            new CardAction(text: "This is the text for action 2", displayText: "action 2 display text")
                        },
                        "Here are some actions.");
                    multiPartMessage.Attachments.Add(new MessageBodyItemWithLink()
                    {
                        Link = "https://www.microsoft.com",
                        Text = "Visit the Microsoft web site"
                    }.ToAttachment());
                    await turnContext.SendActivityAsync(multiPartMessage, cancellationToken);
                    break;

                case "actions":
                    var message = MessageFactory.SuggestedActions(
                        new List<CardAction>() {
                            new CardAction(text: "This is the text", displayText: "display text"),
                            new CardAction(text: "This is the text for action 2", displayText: "action 2 display text")
                        },
                        "Here are some actions.");
                    await turnContext.SendActivityAsync(message, cancellationToken);
                    break;

                case "fields":
                    var fieldsMessage = MessageFactory.Text("This is the text");
                    fieldsMessage.Attachments.Add(new FieldsBodyItem()
                    {
                        Fields = new List<ZoomField>()
                        {
                            new ZoomField() { Key="item1", Value = "value 1"},
                            new ZoomField() { Key="item2", Value = "value 2"},
                        }
                    }.ToAttachment());
                    await turnContext.SendActivityAsync(fieldsMessage, cancellationToken);
                    break;

                case "fields-noedit":
                    var fieldsMessage2 = MessageFactory.Text("This is the text");
                    fieldsMessage2.Attachments.Add(new FieldsBodyItem()
                    {
                        Fields = new List<ZoomField>()
                        {
                            new ZoomField() { Key="item 1", Value = "*value 1*", Editable = false},
                            new ZoomField() { Key="item 2", Value = "_value 2_", Editable = false},
                        }
                    }.ToAttachment());
                    await turnContext.SendActivityAsync(fieldsMessage2, cancellationToken);
                    break;

                case "link":
                    var messageWithLink = MessageFactory.Text("Here is a link.");
                    messageWithLink.Attachments.Add(new MessageBodyItemWithLink()
                    {
                        Link = "https://www.microsoft.com",
                        Text = "Visit the Microsoft web site"
                    }.ToAttachment());
                    await turnContext.SendActivityAsync(messageWithLink, cancellationToken);
                    break;

                case "dropdown":
                    var messageWithDropdown = MessageFactory.Text("Here is a dropdown.");
                    messageWithDropdown.Attachments.Add(new DropdownBodyItem()
                    {
                        Text = "Visit the Microsoft web site",
                        SelectItems = new List<ZoomSelectItem>()
                        {
                            new ZoomSelectItem() { Text = "Item 1", Value = "item1" },
                            new ZoomSelectItem() { Text = "Item 2", Value = "item2" }
                        }
                    }.ToAttachment());
                    await turnContext.SendActivityAsync(messageWithDropdown, cancellationToken);
                    break;

                case "attachment":
                    var messageWithAttachment = MessageFactory.Text("Here is an attachment.");
                    messageWithAttachment.Attachments.Add(new AttachmentBodyItem()
                    {
                        ImageUrl = new Uri("https://s3.amazonaws.com/user-content.stoplight.io/19808/1560465782721"),
                        Size = 250,
                        Information = new ZoomAttachmentInfo()
                        {
                            Description = new ZoomAttachmentInfoContent() { Text = "This is a description" },
                            Title = new ZoomAttachmentInfoContent() { Text = "This is a title" }
                        },
                        Ext = FileExtensions.jpeg,
                        ResourceUrl = new Uri("https://www.microsoft.com")
                    }.ToAttachment());
                    await turnContext.SendActivityAsync(messageWithAttachment, cancellationToken);
                    break;
            }
        }

        protected override async Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(
                $"Event received. Name: {turnContext.Activity.Name}. Value: {turnContext.Activity.Value}").ConfigureAwait(false);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and welcome!"), cancellationToken);
                }
            }
        }
    }
}
