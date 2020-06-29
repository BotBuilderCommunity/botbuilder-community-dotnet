using AdaptiveCards;
using Bot.Builder.Community.Cards.Management;
using Bot.Builder.Community.Cards.Translation;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cards_Library_Sample.Bots
{
    internal class CardsLibraryBot : ActivityHandler
    {
        private const string DemoDisableActions = "Disable actions";
        private const string DemoDisableCards = "Disable cards";
        private const string DemoDisableCarousels = "Disable carousels";
        private const string DemoDisableBatch = "Disable batch";
        private const string DemoTranslateCards = "Translate cards";

        public ConversationState ConversationState { get; }

        public AdaptiveCardTranslator Translator { get; }

        public CardsLibraryBot(ConversationState conversationState, AdaptiveCardTranslator adaptiveCardTranslator)
        {
            ConversationState = conversationState;
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

                switch (jObject["behavior"].ToString())
                {
                    case "translate":

                        var language = jObject["language"]?.ToString();
                        var card = CreateAdaptiveCard();

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
                    case DemoDisableActions:
                        await ShowSampleBatch(turnContext, DataIdTypes.Action, cancellationToken);
                        break;
                    case DemoDisableCards:
                        await ShowSampleBatch(turnContext, DataIdTypes.Card, cancellationToken);
                        break;
                    case DemoDisableCarousels:
                        await ShowSampleBatch(turnContext, DataIdTypes.Carousel, cancellationToken);
                        break;
                    case DemoDisableBatch:
                        await ShowSampleBatch(turnContext, DataIdTypes.Batch, cancellationToken);
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

        private static AdaptiveCard CreateAdaptiveCard() => new AdaptiveCard("1.0")
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
                        behavior = "translate",
                    },
                }
            },
        };

        private async Task ShowTranslationSample(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var card = CreateAdaptiveCard();

            // There's no need to convert the card to a JObject
            // since the library will do that for us
            await turnContext.SendActivityAsync(MessageFactory.Attachment(new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            }), cancellationToken);
        }

        private async Task ShowSampleBatch(ITurnContext turnContext, string idType, CancellationToken cancellationToken = default)
        {
            static CardAction ToButton(string label) => new CardAction
            {
                Type = ActionTypes.PostBack,
                Title = label,
                Value = new { label },
            };

            var batch = new IMessageActivity[]
            {
                MessageFactory.Carousel(new Attachment[]
                {
                    new ThumbnailCard
                    {
                        Title = $"Cards library demo: {idType}",
                        Subtitle = "Thumbnail Card 1",
                        Text = "Go ahead and try clicking these buttons to see what gets disabled.",
                        Images = new List<CardImage>
                        {
                            new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg")
                        },
                        Buttons = (new[] { "Hello World", "Get Started", "Help" }).Select(ToButton).ToList()
                    }.ToAttachment(),
                    new ThumbnailCard
                    {
                        Title = $"Cards library demo: {idType}",
                        Subtitle = "Thumbnail Card 2",
                        Text = "Try clicking these ones too.",
                        Images = new List<CardImage>
                        {
                            new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg")
                        },
                        Buttons = (new[] { "More Information", "Try Again", "Cancel" }).Select(ToButton).ToList()
                    }.ToAttachment(),
                }),
                MessageFactory.Carousel(new Attachment[]
                {
                    new HeroCard
                    {
                        Buttons = (new[] { "Second Carousel", "Same Batch" }).Select(ToButton).ToList()
                    }.ToAttachment(),
                    new HeroCard
                    {
                        Buttons = (new[] { "More Buttons", "More Disabling" }).Select(ToButton).ToList()
                    }.ToAttachment(),
                    new HeroCard
                    {
                        Buttons = (new[] { "Go Back", "Submit" }).Select(ToButton).ToList()
                    }.ToAttachment(),
                }),
            };

            batch.ApplyIdsToBatch(new DataIdOptions(idType));

            await turnContext.SendActivitiesAsync(batch, cancellationToken);
        }

        private async Task ShowMenu(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            var options = new string[]
            {
                DemoDisableActions,
                DemoDisableCards,
                DemoDisableCarousels,
                DemoDisableBatch,
                DemoTranslateCards,
            };

            var card = new HeroCard
            {
                Text = "Please select a demo (these options will not be disabled)",
                Buttons = options.Select(option => new CardAction(ActionTypes.ImBack, option, value: option)).ToList(),
            };

            await turnContext.SendActivityAsync(MessageFactory.Attachment(card.ToAttachment()), cancellationToken);
        }
    }
}