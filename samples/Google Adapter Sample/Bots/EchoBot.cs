using Bot.Builder.Community.Adapters.Google.Core.Model.Response;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Google.Core.Attachments;
using Bot.Builder.Community.Adapters.Google.Core.Helpers;
using Bot.Builder.Community.Adapters.Google.Core.Model.SystemIntents;

namespace Bot.Builder.Community.Samples.Google.Bots
{
    public class EchoBot : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            switch (turnContext.Activity.Text.ToLower())
            {
                default:
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Echo: {turnContext.Activity.Text}. What's next?", inputHint: InputHints.ExpectingInput), cancellationToken);
                    break;

                case "finish":
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Ok, I won't ask anymore.", inputHint: InputHints.IgnoringInput), cancellationToken);
                    break;

                case "card":
                    var activityWithCard = MessageFactory.Text($"Ok, I included a simple card.");
                    var basicCard = GoogleCardFactory.CreateBasicCard("card title", "card subtitle", "some text for the content");
                    activityWithCard.Attachments.Add(basicCard.ToAttachment());
                    await turnContext.SendActivityAsync(activityWithCard, cancellationToken);
                    break;

                case "list":
                    var activityWithListAttachment = MessageFactory.Text($"Ok, I included a list.");
                    var listIntent = GoogleHelperIntentFactory.CreateListIntent(
                            "List title",
                            new List<OptionItem>()
                            {
                                new OptionItem(
                                    "List item 1",
                                    "This is the List Item 1 description",
                                    new OptionItemInfo() {Key = "Item1", Synonyms = new List<string>() {"first"}},
                                    new OptionItemImage()
                                    {
                                        AccessibilityText = "Item 1 image",
                                        Url = "https://storage.googleapis.com/actionsresources/logo_assistant_2x_64dp.png"
                                    }),
                                new OptionItem(
                                    "List item 2",
                                    "This is the List Item 2 description",
                                    new OptionItemInfo() {Key = "Item2", Synonyms = new List<string>() {"second"}},
                                    new OptionItemImage()
                                    {
                                        AccessibilityText = "Item 2 image",
                                        Url = "https://storage.googleapis.com/actionsresources/logo_assistant_2x_64dp.png"
                                    })
                            });
                    activityWithListAttachment.Attachments.Add(listIntent.ToAttachment());
                    await turnContext.SendActivityAsync(activityWithListAttachment, cancellationToken);
                    break;

                case "carousel":
                    var activityWithCarouselAttachment = MessageFactory.Text($"Ok, I included a carousel.");
                    var carouselIntent = GoogleHelperIntentFactory.CreateCarouselIntent(
                        "List title",
                        new List<OptionItem>()
                        {
                            new OptionItem(
                                "List item 1",
                                "This is the List Item 1 description",
                                new OptionItemInfo() {Key = "Item1", Synonyms = new List<string>() {"first"}},
                                new OptionItemImage()
                                {
                                    AccessibilityText = "Item 1 image",
                                    Url = "https://storage.googleapis.com/actionsresources/logo_assistant_2x_64dp.png"
                                }),
                            new OptionItem(
                                "List item 2",
                                "This is the List Item 2 description",
                                new OptionItemInfo() {Key = "Item2", Synonyms = new List<string>() {"second"}},
                                new OptionItemImage()
                                {
                                    AccessibilityText = "Item 2 image",
                                    Url = "https://storage.googleapis.com/actionsresources/logo_assistant_2x_64dp.png"
                                })
                        });
                    activityWithCarouselAttachment.Attachments.Add(carouselIntent.ToAttachment());
                    await turnContext.SendActivityAsync(activityWithCarouselAttachment, cancellationToken);
                    break;

                case "table":
                    var activityWithTableCardAttachment = MessageFactory.Text($"Ok, I included a table card.");
                    var tableCard = GoogleCardFactory.CreateTableCard(
                        new List<ColumnProperties>()
                        {
                            new ColumnProperties() { Header = "Column 1" },
                            new ColumnProperties() { Header = "Column 2" }
                        },
                        new List<Row>()
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
                        },
                        "Table Card Title",
                        "Table card subtitle",
                        new List<Button>() { new Button() { Title = "Click here", OpenUrlAction = new OpenUrlAction() { Url = "https://www.microsoft.com" }}  });
                    activityWithTableCardAttachment.Attachments.Add(tableCard.ToAttachment());
                    await turnContext.SendActivityAsync(activityWithTableCardAttachment, cancellationToken);
                    break;
            }
        }

        protected override async Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text($"Event Received. Name: {turnContext.Activity.Name}, Value: {turnContext.Activity.Value}"), cancellationToken);
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
