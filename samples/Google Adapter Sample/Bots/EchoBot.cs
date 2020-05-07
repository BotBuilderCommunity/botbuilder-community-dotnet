using Bot.Builder.Community.Adapters.Google;
using Bot.Builder.Community.Adapters.Google.Core.Model;
using Bot.Builder.Community.Adapters.Google.Core.Model.Response;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Google.Core.Attachments;
using Bot.Builder.Community.Adapters.Google.Core.Model.SystemIntents;
using BasicCard = Bot.Builder.Community.Adapters.Google.Core.Model.Response.BasicCard;

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
                        new BasicCard()
                            {
                                Content = new BasicCardContent()
                                {

                                    Title = "This is a simple card",
                                    Subtitle = "This is a simple card subtitle",
                                    FormattedText = "This is the simple card content"
                                }
                            }.ToAttachment());
                    await turnContext.SendActivityAsync(activityWithCard, cancellationToken);
                    break;

                case "table":
                    var activityWithTableCardAttachment = MessageFactory.Text($"Ok, I included a table card.");
                    var tableCardAttachment = new TableCard()
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
                        }.ToAttachment();
                    activityWithTableCardAttachment.Attachments.Add(tableCardAttachment);
                    await turnContext.SendActivityAsync(activityWithTableCardAttachment, cancellationToken);
                    break;

                case "list":
                    var activityWithListAttachment = MessageFactory.Text($"Ok, I included a list.");
                    var listAttachment = new ListIntent()
                    {
                        InputValueData = new ListOptionIntentInputValueData()
                        {
                            ListSelect = new OptionIntentSelect()
                            {
                                Title = "This is the list title",
                                Items = new List<OptionItem>() {
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
                                }
                            }
                        }
                    }.ToAttachment();
                    activityWithListAttachment.Attachments.Add(listAttachment);
                    await turnContext.SendActivityAsync(activityWithListAttachment, cancellationToken);
                    break;

                case "carousel":
                    var activityWithCarouselAttachment = MessageFactory.Text($"Ok, I included a carousel.");
                    var carouselAttachment = new CarouselIntent()
                    {
                        InputValueData = new CarouselOptionIntentInputValueData()
                        {
                            CarouselSelect = new OptionIntentSelect()
                            {
                                Title = "This is the list title",
                                Items = new List<OptionItem>() {
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
                                }
                            }
                        }
                    }.ToAttachment();
                    activityWithCarouselAttachment.Attachments.Add(carouselAttachment);
                    await turnContext.SendActivityAsync(activityWithCarouselAttachment, cancellationToken);
                    break;

                case "datetime":
                    var activityWithDateTimeIntent = MessageFactory.Text($"Ask for date time");
                    activityWithDateTimeIntent.Attachments.Add(
                        new DateTimeIntent()
                        {
                            InputValueData = new DateTimeInputValueData()
                            {
                                DialogSpec = new DateTimeIntentDialogSpec()
                                {
                                    RequestDateText = "What date again?",
                                    RequestDatetimeText = "When would you like?",
                                    RequestTimeText = "What time?"
                                }
                            }
                        }.ToAttachment());
                    await turnContext.SendActivityAsync(activityWithDateTimeIntent, cancellationToken);
                    break;

                case "permissions":
                    var activityWithPermissionsIntent = MessageFactory.Text($"Ask for permissions");
                    activityWithPermissionsIntent.Attachments.Add(
                        new PermissionsIntent()
                        {
                            InputValueData = new PermissionsInputValueData()
                            {
                                Permissions = new List<Permission>()
                                {
                                    Permission.DEVICE_COARSE_LOCATION,
                                    Permission.DEVICE_PRECISE_LOCATION,
                                    Permission.NAME,
                                    Permission.UPDATE
                                },
                                OptContext = "Ask for permissions"
                            }
                        }.ToAttachment());
                    await turnContext.SendActivityAsync(activityWithPermissionsIntent, cancellationToken);
                    break;

                    case "location":
                    var activityWithLocationIntent = MessageFactory.Text($"Ask for place or location");
                    activityWithLocationIntent.Attachments.Add(
                        new PlaceLocationIntent()
                        {
                            InputValueData = new PlaceLocationInputValueData()
                            {
                                DialogSpec = new PlaceLocationIntentDialogSpec()
                                {
                                    Extension = new PlaceLocationIntentDialogSpecExtension()
                                    {
                                        PermissionContext = "To help with your request",
                                        RequestPrompt = "Sorry, where again?"
                                    }
                                }
                            }
                        }.ToAttachment());
                    await turnContext.SendActivityAsync(activityWithLocationIntent, cancellationToken);
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
