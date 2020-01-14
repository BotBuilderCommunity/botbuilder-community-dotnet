using Bot.Builder.Community.Adapters.Google;
using Bot.Builder.Community.Adapters.Google.Model;
using Bot.Builder.Community.Adapters.Google.Model.Attachments;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Samples.Google.Bots
{
    public class EchoBot : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            switch (turnContext.Activity.Text.ToLower())
            {
                case "finish":
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Ok, I won't ask anymore.", inputHint: InputHints.IgnoringInput), cancellationToken);
                    break;

                case "card":
                    var activityWithCard = MessageFactory.Text($"Ok, I included a simple card.");
                    activityWithCard.Attachments.Add(
                        new BasicCardAttachment(
                            new Adapters.Google.BasicCard()
                            {
                                Content = new BasicCardContent()
                                {

                                    Title = "This is a simple card",
                                    Subtitle = "This is a simple card subtitle",
                                    FormattedText = "This is the simple card content"
                                }
                            }));
                    await turnContext.SendActivityAsync(activityWithCard, cancellationToken);
                    break;

                case "table":
                    var activityWithTableCardAttachment = MessageFactory.Text($"Ok, I included a table card.");
                    var tableCardAttachment = new TableCardAttachment(
                        new TableCard()
                        {
                            Content = new TableCardContent()
                            {
                                ColumnProperties = new List<ColumnProperties>()
                                {
                                    new ColumnProperties() { Header = "Column 1" },
                                    new ColumnProperties() { Header = "Column 2" }
                                },
                                Rows = new List<Row>()
                                {
                                    new Row() {
                                        Cells = new List<Cell>
                                        {
                                            new Cell { Text = "Row 1, Item 1" },
                                            new Cell { Text = "Row 1, Item 2" }
                                        }
                                    },
                                    new Row() {
                                        Cells = new List<Cell>
                                        {
                                            new Cell { Text = "Row 2, Item 1" },
                                            new Cell { Text = "Row 2, Item 2" }
                                        }
                                    }
                                }
                            }
                        });
                    activityWithTableCardAttachment.Attachments.Add(tableCardAttachment);
                    await turnContext.SendActivityAsync(activityWithTableCardAttachment, cancellationToken);
                    break;

                case "list":
                    var activityWithListAttachment = MessageFactory.Text($"Ok, I included a list.");
                    var listAttachment = new ListAttachment(
                        "This is the list title",
                        new List<OptionItem>() {
                            new OptionItem() {
                                Title = "List item 1",
                                Description = "This is the List Item 1 description",
                                Image = new OptionItemImage() { AccessibilityText = "Item 1 image", Url = "https://storage.googleapis.com/actionsresources/logo_assistant_2x_64dp.png"},
                                OptionInfo = new OptionItemInfo() { Key = "Item1", Synonyms = new List<string>(){ "first" } }
                            },
                        new OptionItem() {
                                Title = "List item 2",
                                Description = "This is the List Item 2 description",
                                Image = new OptionItemImage() { AccessibilityText = "Item 1 image", Url = "https://storage.googleapis.com/actionsresources/logo_assistant_2x_64dp.png"},
                                OptionInfo = new OptionItemInfo() { Key = "Item2", Synonyms = new List<string>(){ "second" } }
                            }
                        },
                        ListAttachmentStyle.List);
                    activityWithListAttachment.Attachments.Add(listAttachment);
                    await turnContext.SendActivityAsync(activityWithListAttachment, cancellationToken);
                    break;

                case "carousel":
                    var activityWithCarouselAttachment = MessageFactory.Text($"Ok, I included a carousel.");
                    var carouselAttachment = new ListAttachment(
                        "This is the list title",
                        new List<OptionItem>() {
                            new OptionItem() {
                                Title = "List item 1",
                                Description = "This is the List Item 1 description",
                                Image = new OptionItemImage() { AccessibilityText = "Item 1 image", Url = "https://storage.googleapis.com/actionsresources/logo_assistant_2x_64dp.png"},
                                OptionInfo = new OptionItemInfo() { Key = "Item1", Synonyms = new List<string>(){ "first" } }
                            },
                        new OptionItem() {
                                Title = "List item 2",
                                Description = "This is the List Item 2 description",
                                Image = new OptionItemImage() { AccessibilityText = "Item 1 image", Url = "https://storage.googleapis.com/actionsresources/logo_assistant_2x_64dp.png"},
                                OptionInfo = new OptionItemInfo() { Key = "Item2", Synonyms = new List<string>(){ "second" } }
                            }
                        },
                        ListAttachmentStyle.Carousel);
                    activityWithCarouselAttachment.Attachments.Add(carouselAttachment);
                    await turnContext.SendActivityAsync(activityWithCarouselAttachment, cancellationToken);
                    break;

                default:
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Echo: {turnContext.Activity.Text}. What's next?", inputHint: InputHints.ExpectingInput), cancellationToken);
                    break;

            }
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
