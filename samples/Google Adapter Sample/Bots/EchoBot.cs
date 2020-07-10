using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.ActionsSDK.Core.Attachments;
using Bot.Builder.Community.Adapters.ActionsSDK.Core.Helpers;
using Bot.Builder.Community.Adapters.ActionsSDK.Core.Model;
using Bot.Builder.Community.Adapters.ActionsSDK.Core.Model.ContentItems;

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

                //case "card":
                //    var activityWithCard = MessageFactory.Text($"Ok, I included a simple card.");
                //    var card = ContentItemFactory.CreateCard("card title", "card subtitle", new Link()
                //    {
                //        Name = "Microsoft", 
                //        Open = new OpenUrl() { Url = "https://www.microsoft.com"}
                //    });
                //    activityWithCard.Attachments.Add(card.ToAttachment());
                //    await turnContext.SendActivityAsync(activityWithCard, cancellationToken);
                //    break;

                case "signin":
                    var activityWithSigninCard = MessageFactory.Text($"Ok, I included a signin card.");
                    var signinCard = new SigninCard();
                    activityWithSigninCard.Attachments.Add(signinCard.ToAttachment());
                    await turnContext.SendActivityAsync(activityWithSigninCard, cancellationToken);
                    break;

                case "chips":
                    var activityWithChips = MessageFactory.Text($"Ok, I included some suggested actions.");
                    activityWithChips.SuggestedActions = new SuggestedActions(actions: new List<CardAction>
                    {
                        new CardAction { Title = "Yes", Type= ActionTypes.ImBack, Value = "Y" },
                        new CardAction { Title = "No", Type= ActionTypes.ImBack, Value = "N" },
                        new CardAction { Title = "Click to learn more", Type= ActionTypes.OpenUrl, Value = "http://www.progressive.com" }
                    });
                    await turnContext.SendActivityAsync(activityWithChips, cancellationToken);
                    break;

                //case "list":
                //    var activityWithListAttachment = MessageFactory.Text($"This is a list.");
                //    var list = new ListContentItem()
                //    {
                //        Title = "InternalList title",
                //        Subtitle = "InternalList subtitle",
                //        Items = new List<ListItem>()
                //        {
                //            new ListItem()
                //            {
                //                Key = "ITEM_1",
                //                Synonyms = new List<string>() { "Item 1", "First item" },
                //                Item = new EntryDisplay()
                //                {
                //                    Title = "Item #1",
                //                    Description = "Description of Item #1",
                //                    Image = new Image()
                //                    {
                //                        Url = "https://developers.google.com/assistant/assistant_96.png",
                //                        Height = 0,
                //                        Width = 0,
                //                        Alt = "Google Assistant logo"
                //                    }
                //                }
                //            },
                //            new ListItem()
                //            {
                //                Key = "ITEM_2",
                //                Synonyms = new List<string>() { "Item 2", "Second item" },
                //                Item = new EntryDisplay()
                //                {
                //                    Title = "Item #2",
                //                    Description = "Description of Item #2",
                //                    Image = new Image()
                //                    {
                //                        Url = "https://developers.google.com/assistant/assistant_96.png",
                //                        Height = 0,
                //                        Width = 0,
                //                        Alt = "Google Assistant logo"
                //                    }
                //                }
                //            },
                //            new ListItem()
                //            {
                //                Key = "ITEM_3",
                //                Synonyms = new List<string>() { "Item 3", "Third item" },
                //                Item = new EntryDisplay()
                //                {
                //                    Title = "Item #3",
                //                    Description = "Description of Item #3",
                //                    Image = new Image()
                //                    {
                //                        Url = "https://developers.google.com/assistant/assistant_96.png",
                //                        Height = 0,
                //                        Width = 0,
                //                        Alt = "Google Assistant logo"
                //                    }
                //                }
                //            },
                //            new ListItem()
                //            {
                //                Key = "ITEM_4",
                //                Synonyms = new List<string>() { "Item 4", "Fourth item" },
                //                Item = new EntryDisplay()
                //                {
                //                    Title = "Item #4",
                //                    Description = "Description of Item #4",
                //                    Image = new Image()
                //                    {
                //                        Url = "https://developers.google.com/assistant/assistant_96.png",
                //                        Height = 0,
                //                        Width = 0,
                //                        Alt = "Google Assistant logo"
                //                    }
                //                }
                //            },
                //        }
                //    };
                //    activityWithListAttachment.Attachments.Add(list.ToAttachment());
                //    await turnContext.SendActivityAsync(activityWithListAttachment, cancellationToken);
                //    break;

                //case "collection":
                //    var activityWithCollectionAttachment = MessageFactory.Text($"Ok, I included a collection.");
                //    var collection = new CollectionContentItem()
                //    {
                //        Title = "InternalList title",
                //        Subtitle = "InternalList subtitle",
                //        Items = new List<CollectionItem>()
                //        {
                //            new CollectionItem()
                //            {
                //                Key = "ITEM_1",
                //                Synonyms = new List<string>() { "Item 1", "First item" },
                //                Item = new EntryDisplay()
                //                {
                //                    Title = "Item #1",
                //                    Description = "Description of Item #1",
                //                    Image = new Image()
                //                    {
                //                        Url = "https://developers.google.com/assistant/assistant_96.png",
                //                        Height = 0,
                //                        Width = 0,
                //                        Alt = "Google Assistant logo"
                //                    }
                //                }
                //            },
                //            new CollectionItem()
                //            {
                //                Key = "ITEM_2",
                //                Synonyms = new List<string>() { "Item 2", "Second item" },
                //                Item = new EntryDisplay()
                //                {
                //                    Title = "Item #2",
                //                    Description = "Description of Item #2",
                //                    Image = new Image()
                //                    {
                //                        Url = "https://developers.google.com/assistant/assistant_96.png",
                //                        Height = 0,
                //                        Width = 0,
                //                        Alt = "Google Assistant logo"
                //                    }
                //                }
                //            },
                //            new CollectionItem()
                //            {
                //                Key = "ITEM_3",
                //                Synonyms = new List<string>() { "Item 3", "Third item" },
                //                Item = new EntryDisplay()
                //                {
                //                    Title = "Item #3",
                //                    Description = "Description of Item #3",
                //                    Image = new Image()
                //                    {
                //                        Url = "https://developers.google.com/assistant/assistant_96.png",
                //                        Height = 0,
                //                        Width = 0,
                //                        Alt = "Google Assistant logo"
                //                    }
                //                }
                //            },
                //            new CollectionItem()
                //            {
                //                Key = "ITEM_4",
                //                Synonyms = new List<string>() { "Item 4", "Fourth item" },
                //                Item = new EntryDisplay()
                //                {
                //                    Title = "Item #4",
                //                    Description = "Description of Item #4",
                //                    Image = new Image()
                //                    {
                //                        Url = "https://developers.google.com/assistant/assistant_96.png",
                //                        Height = 0,
                //                        Width = 0,
                //                        Alt = "Google Assistant logo"
                //                    }
                //                }
                //            },
                //        }
                //    };
                //    activityWithCollectionAttachment.Attachments.Add(collection.ToAttachment());
                //    await turnContext.SendActivityAsync(activityWithCollectionAttachment, cancellationToken);
                //    break;

                //case "carousel":
                //    var activityWithCarouselAttachment = MessageFactory.Text($"Ok, I included a carousel.");
                //    var carouselIntent = GoogleHelperIntentFactory.CreateCarouselIntent(
                //        "InternalList title",
                //        new InternalList<OptionItem>()
                //        {
                //            new OptionItem(
                //                "InternalList item 1",
                //                "This is the InternalList Item 1 description",
                //                new OptionItemInfo() {Key = "Item1", Synonyms = new InternalList<string>() {"first"}},
                //                new OptionItemImage()
                //                {
                //                    AccessibilityText = "Item 1 image",
                //                    Url = "https://storage.googleapis.com/actionsresources/logo_assistant_2x_64dp.png"
                //                }),
                //            new OptionItem(
                //                "InternalList item 2",
                //                "This is the InternalList Item 2 description",
                //                new OptionItemInfo() {Key = "Item2", Synonyms = new InternalList<string>() {"second"}},
                //                new OptionItemImage()
                //                {
                //                    AccessibilityText = "Item 2 image",
                //                    Url = "https://storage.googleapis.com/actionsresources/logo_assistant_2x_64dp.png"
                //                })
                //        });
                //    activityWithCarouselAttachment.Attachments.Add(carouselIntent.ToAttachment());
                //    await turnContext.SendActivityAsync(activityWithCarouselAttachment, cancellationToken);
                //    break;

                //case "table":
                //    var activityWithTableCardAttachment = MessageFactory.Text($"Ok, I included a table.");
                //    var table = ContentItemFactory.CreateTable(
                //        new List<TableColumn>()
                //        {
                //            new TableColumn() { Header = "Column 1" },
                //            new TableColumn() { Header = "Column 2" }
                //        },
                //        new List<TableRow>()
                //        {
                //            new TableRow() {
                //                Cells = new List<TableCell>
                //                {
                //                    new TableCell { Text = "Row 1, Item 1" },
                //                    new TableCell { Text = "Row 1, Item 2" }
                //                }
                //            },
                //            new TableRow() {
                //                Cells = new List<TableCell>
                //                {
                //                    new TableCell { Text = "Row 2, Item 1" },
                //                    new TableCell { Text = "Row 2, Item 2" }
                //                }
                //            }
                //        },
                //        "Table Card Title",
                //        "Table card subtitle",
                //        new Link { Name = "Microsoft", Open = new OpenUrl() { Url = "https://www.microsoft.com" } });
                //    activityWithTableCardAttachment.Attachments.Add(table.ToAttachment());
                //    await turnContext.SendActivityAsync(activityWithTableCardAttachment, cancellationToken);
                //    break;
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
