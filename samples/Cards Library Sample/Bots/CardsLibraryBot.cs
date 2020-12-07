using AdaptiveCards;
using Bot.Builder.Community.Cards.Management;
using Bot.Builder.Community.Cards.Translation;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cards_Library_Sample.Bots
{
    internal class CardsLibraryBot : ActivityHandler
    {
        private const string DemoDeactivateActions = "Deactivate actions";
        private const string DemoDeactivateCards = "Deactivate cards";
        private const string DemoDeactivateCarousels = "Deactivate carousels";
        private const string DemoDeactivateBatch = "Deactivate batch";
        private const string DemoPreserveValues = "Preserve values";
        private const string DemoTranslateCards = "Translate cards";

        private const string BehaviorSubmit = "submit";
        private const string BehaviorPreserve = "preserve";
        private const string BehaviorTranslate = "translate";

        private const string IdText = "id-text";
        private const string IdNumber = "id-number";
        private const string IdDate = "id-date";

        public ConversationState ConversationState { get; }

        public CardManager CardManager { get; }

        public AdaptiveCardTranslator Translator { get; }

        public CardsLibraryBot(ConversationState conversationState, CardManager cardManager, AdaptiveCardTranslator adaptiveCardTranslator)
        {
            ConversationState = conversationState;
            CardManager = cardManager;
            Translator = adaptiveCardTranslator;
        }

        protected override async Task OnMembersAddedAsync(
            IList<ChannelAccount> membersAdded,
            ITurnContext<IConversationUpdateActivity> turnContext,
            CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync("Welcome to the cards library sample!", cancellationToken: cancellationToken);
            await ShowMenu(turnContext, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            if (turnContext.GetIncomingActionData() is object incomingData)
            {
                var jObject = JObject.FromObject(incomingData);

                async Task SendPreservationFeedback()
                {
                    await turnContext.SendActivityAsync(
                        $"You sent the following data: {jObject[IdText]}, {jObject[IdNumber]}, {jObject[IdDate]}",
                        cancellationToken: cancellationToken);
                }

                switch (jObject["behavior"]?.ToString())
                {
                    case BehaviorSubmit:

                        await SendPreservationFeedback();

                        break;

                    case BehaviorPreserve:

                        await CardManager.PreserveValuesAsync(turnContext, cancellationToken);
                        await SendPreservationFeedback();

                        break;

                    case BehaviorTranslate:

                        var language = jObject["language"]?.ToString();
                        var card = CreateTranslationCard();

                        if (string.IsNullOrWhiteSpace(Translator.MicrosoftTranslatorConfig.SubscriptionKey))
                        {
                            if (string.IsNullOrWhiteSpace(language))
                            {
                                language = "Undefined";
                            }

                            Task<string> translateOne(string text, CancellationToken ct)
                            {
                                return Task.FromResult($"{language}: {text}");
                            }

                            card = await AdaptiveCardTranslator.TranslateAsync(card, translateOne, cancellationToken: cancellationToken);

                            await turnContext.SendActivityAsync("No subscription key was configured, so the card has been modified without a translation.");
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(language))
                            {
                                card = await Translator.TranslateAsync(card, cancellationToken);
                            }
                            else
                            {
                                card = await Translator.TranslateAsync(card, language, cancellationToken);
                            }
                        }

                        // There's no need to convert the card to a JObject
                        // since the library will do that for us
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(new Attachment
                        {
                            ContentType = AdaptiveCard.ContentType,
                            Content = card,
                        }), cancellationToken);

                        break;

                    default:

                        if (jObject["label"] is JToken label)
                        {
                            await turnContext.SendActivityAsync(
                                $"Thank you for choosing {label}!",
                                cancellationToken: cancellationToken);
                        }

                        break;
                }
            }
            else
            {
                switch (turnContext.Activity.Text)
                {
                    case DemoDeactivateActions:
                        await ShowDeactivationSample(turnContext, DataIdScopes.Action, cancellationToken);
                        break;
                    case DemoDeactivateCards:
                        await ShowDeactivationSample(turnContext, DataIdScopes.Card, cancellationToken);
                        break;
                    case DemoDeactivateCarousels:
                        await ShowDeactivationSample(turnContext, DataIdScopes.Carousel, cancellationToken);
                        break;
                    case DemoDeactivateBatch:
                        await ShowDeactivationSample(turnContext, DataIdScopes.Batch, cancellationToken);
                        break;
                    case DemoPreserveValues:
                        await ShowPreservationSample(turnContext, cancellationToken);
                        break;
                    case DemoTranslateCards:
                        await ShowTranslationSample(turnContext, cancellationToken);
                        break;
                    default:
                        await ShowMenu(turnContext, cancellationToken);
                        break;
                } 
            }

            // The card manager will not work if its state is not saved
            await ConversationState.SaveChangesAsync(turnContext, cancellationToken: cancellationToken);
        }

        private async Task ShowDeactivationSample(ITurnContext turnContext, string idScope, CancellationToken cancellationToken = default)
        {
            static IList<CardAction> MakeButtons(IEnumerable<string> labels) =>
                labels.Select((string label) =>
                    new CardAction
                    {
                        Type = ActionTypes.PostBack,
                        Title = label,
                        Value = new { label },
                    }).ToList();

            var batch = new IMessageActivity[]
            {
                MessageFactory.Carousel(new Attachment[]
                {
                    new ThumbnailCard
                    {
                        Title = $"Deactivate-{idScope} demo",
                        Subtitle = "Thumbnail Card 1",
                        Text = $"If you click a button then the {idScope} will get disabled or deleted.",
                        Images = new List<CardImage>
                        {
                            new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg")
                        },
                        Buttons = MakeButtons(new[] { "Hello World", "Get Started", "Help" })
                    }.ToAttachment(),
                    new ThumbnailCard
                    {
                        Title = $"Deactivate-{idScope} demo",
                        Subtitle = "Thumbnail Card 2",
                        Text = "Try clicking these ones too.",
                        Images = new List<CardImage>
                        {
                            new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg")
                        },
                        Buttons = MakeButtons(new[] { "More Information", "Try Again", "Cancel" })
                    }.ToAttachment(),
                }),
                MessageFactory.Carousel(new Attachment[]
                {
                    new HeroCard
                    {
                        Buttons = MakeButtons(new[] { "Second Carousel", "Same Batch" })
                    }.ToAttachment(),
                    new HeroCard
                    {
                        Buttons = MakeButtons(new[] { "More Buttons", "More Disabling" })
                    }.ToAttachment(),
                    new HeroCard
                    {
                        Buttons = MakeButtons(new[] { "Go Back", "Submit" })
                    }.ToAttachment(),
                }),
            };

            DataId.SetInBatch(batch, new DataIdOptions(idScope));

            await turnContext.SendActivitiesAsync(batch, cancellationToken);
        }

        private async Task ShowPreservationSample(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            object card = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Cards library demo",
                        Size = AdaptiveTextSize.ExtraLarge,
                        Weight = AdaptiveTextWeight.Bolder,
                        Wrap = true,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = "Adaptive Card translation",
                        Size = AdaptiveTextSize.Large,
                        Weight = AdaptiveTextWeight.Bolder,
                        Wrap = true,
                    },
                    new AdaptiveTextInput
                    {
                        Id = IdText,
                        Placeholder = "Text",
                    },
                    new AdaptiveNumberInput
                    {
                        Id = IdNumber,
                        Placeholder = "Number",
                    },
                    new AdaptiveDateInput
                    {
                        Id = IdDate,
                        Placeholder = "Date",
                    },
                    new AdaptiveTextBlock
                    {
                        Text = "Fill in the inputs and then click a button to see if the values are preserved."
                                + " (This demo only applies to Microsoft Teams)",
                        Wrap = true,
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = "Submit normally",
                        Data = new
                        {
                            behavior = BehaviorSubmit,
                        },
                    },
                    new AdaptiveSubmitAction
                    {
                        Title = "Submit and preserve",
                        Data = new
                        {
                            behavior = BehaviorPreserve,
                        },
                    },
                },
            };

            DataId.SetInAdaptiveCard(ref card);
            ActionBehavior.SetInAdaptiveCard(ref card, Behaviors.AutoDeactivate, BehaviorSwitch.Off);

            await turnContext.SendActivityAsync(MessageFactory.Attachment(new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            }), cancellationToken);
        }

        private async Task ShowTranslationSample(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var card = CreateTranslationCard();

            // There's no need to convert the card to a JObject
            // since the library will do that for us
            await turnContext.SendActivityAsync(MessageFactory.Attachment(new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            }), cancellationToken);
        }

        private static AdaptiveCard CreateTranslationCard() => new AdaptiveCard("1.0")
        {
            Body = new List<AdaptiveElement>
            {
                new AdaptiveTextBlock
                {
                    Text = "Cards library demo",
                    Size = AdaptiveTextSize.ExtraLarge,
                    Weight = AdaptiveTextWeight.Bolder,
                    Wrap = true,
                },
                new AdaptiveTextBlock
                {
                    Text = "Adaptive Card translation",
                    Size = AdaptiveTextSize.Large,
                    Weight = AdaptiveTextWeight.Bolder,
                    Wrap = true,
                },
                new AdaptiveTextBlock
                {
                    Text = "Go ahead and try typing a language tag here (example: pt-br)",
                    Wrap = true,
                },
                // TODO: Use a choice set instead of text
                new AdaptiveTextInput
                {
                    Id = "language",
                    Placeholder = "Example: zh-cn",
                },
                new AdaptiveTextBlock
                {
                    Text = "Then click the button to see what happens!",
                    Wrap = true,
                },
            },
            Actions = new List<AdaptiveAction>
            {
                new AdaptiveSubmitAction
                {
                    Title = "Translate",
                    Data = new
                    {
                        behavior = BehaviorTranslate,
                    },
                },
            },
        };

        private async Task ShowMenu(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            var mainDemos = new string[]
            {
                DemoDeactivateActions,
                DemoDeactivateCards,
                DemoDeactivateCarousels,
                DemoDeactivateBatch,
            };

            var adaptiveCardDemos = new string[]
            {
                DemoPreserveValues,
                DemoTranslateCards,
            };

            static IList<CardAction> MakeButtons(string[] demos)
            {
                return demos.Select(demo => new CardAction(ActionTypes.ImBack, demo, value: demo)).ToList();
            }

            await turnContext.SendActivityAsync(MessageFactory.Carousel(new List<Attachment>
            {
                new HeroCard
                {
                    Text = "These demos should work on most channels",
                    Buttons = MakeButtons(mainDemos),
                }.ToAttachment(),
                new HeroCard
                {
                    Text = "These demos will only work on channels that support Adaptive Cards",
                    Buttons = MakeButtons(adaptiveCardDemos)
                }.ToAttachment(),
            }, "Please select a demo (these options will not be deactivated)"), cancellationToken);
        }
    }
}