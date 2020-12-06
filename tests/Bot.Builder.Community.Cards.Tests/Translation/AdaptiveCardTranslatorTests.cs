using AdaptiveCards;
using Bot.Builder.Community.Cards.Translation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Cards.Tests.Translation
{
    [TestClass]
    public class AdaptiveCardTranslatorTests
    {
        private const string Translated = "Translated";
        private const string Untranslated = "Untranslated";

        [TestMethod]
        public async Task PropertiesToTranslate_Default()
        {
            await TestPropertiesToTranslate();
        }

        [TestMethod]
        public async Task PropertiesToTranslate_Custom()
        {
            await TestPropertiesToTranslate(new AdaptiveCardTranslatorSettings
            {
                PropertiesToTranslate = new[] { "lang", "speak", "id", "iconUrl", "style" },
            });
        }

        [TestMethod]
        public async Task ShouldUseMicrosoftTranslatorApi()
        {
            const string SubscriptionKey = "12345";
            const string TargetLocale = "mud and rock language";

            HttpRequestMessage message = null;

            var card = new AdaptiveCard("1.0")
            {
                FallbackText = Untranslated,
            };

            var handler = new TestHandler(request =>
            {
                message = request;

                Assert.AreEqual(JsonConvert.SerializeObject(new[]
                {
                    new
                    {
                        Text = Untranslated
                    }
                }), message.Content.ReadAsStringAsync().Result);

                return new HttpResponseMessage
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new[]
                    {
                        new TranslatorResponse
                        {
                            Translations = new[]
                            {
                                new TranslatorResult
                                {
                                    Text = Translated,
                                },
                            },
                        },
                    })),
                };
            });

            var client = new HttpClient(handler);
            var config = new MicrosoftTranslatorConfig(SubscriptionKey, TargetLocale, client);
            var result = await AdaptiveCardTranslator.TranslateAsync(card, config);

            var expectedUri = new Uri(new Uri("https://api.cognitive.microsofttranslator.com"),
                $"translate?api-version=3.0&to={TargetLocale}");

            Assert.AreEqual(HttpMethod.Post, message.Method);
            Assert.AreEqual(expectedUri, message.RequestUri);
            Assert.AreEqual(SubscriptionKey, message.Headers.GetValues("Ocp-Apim-Subscription-Key").Single());
            Assert.AreEqual(Untranslated, card.FallbackText);
            Assert.AreEqual(Translated, result.FallbackText);
        }

        [TestMethod]
        public async Task ArrayElementsShouldOnlyBeTranslatedInInlines()
        {
            var jObject = new JObject
            {
                { "inlines", new JArray { Untranslated } },
                { "notInlines", new JArray { Untranslated } },
            };

            var result = await AdaptiveCardTranslator.TranslateAsync(jObject, TranslateOneAsync);

            Assert.AreEqual(Translated, result["inlines"].Single().ToString(), "Inlines weren't translated");
            Assert.AreEqual(Untranslated, result["notInlines"].Single().ToString(), "A non-inline array element was translated");
        }

        [TestMethod]
        public async Task ValuePropertyShouldOnlyBeTranslatedInSpecificCases()
        {
            var card = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveFactSet
                    {
                        Facts = new List<AdaptiveFact>
                        {
                            new AdaptiveFact(string.Empty, Untranslated),
                        }
                    },
                    new AdaptiveChoiceSetInput
                    {
                        Id = string.Empty,
                        Choices = new List<AdaptiveChoice>
                        {
                            new AdaptiveChoice
                            {
                                Title = string.Empty,
                                Value = Untranslated,
                            }
                        },
                        Value = Untranslated,
                    },
                    new AdaptiveDateInput
                    {
                        Id = string.Empty,
                        Value = Untranslated,
                    },
                    new AdaptiveTextInput
                    {
                        Id = string.Empty,
                        Value = Untranslated,
                    },
                    new AdaptiveTimeInput
                    {
                        Id = string.Empty,
                        Value = Untranslated,
                    },
                    new AdaptiveToggleInput
                    {
                        Id = string.Empty,
                        Title = string.Empty,
                        Value = Untranslated,
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Data = new Dictionary<string, object>
                        {
                            {
                                "msteams", new Dictionary<string, object>
                                {
                                    { "type", "imBack" },
                                    { "value", Untranslated },
                                }
                            },
                        },
                    },
                    new AdaptiveSubmitAction
                    {
                        Data = new Dictionary<string, object>
                        {
                            {
                                "msteams", new Dictionary<string, object>
                                {
                                    { "type", "messageBack" },
                                    { "value", Untranslated },
                                }
                            },
                        },
                    },
                },
            };

            var result = await AdaptiveCardTranslator.TranslateAsync(card, TranslateOneAsync);
            var body = result.Body;
            var actions = result.Actions;
            
            Assert.AreEqual(Translated, ((AdaptiveFactSet)body[0]).Facts.Single().Value, "Fact was not translated");
            Assert.AreEqual(Untranslated, ((AdaptiveChoiceSetInput)body[1]).Choices.Single().Value, "Choice was translated");
            Assert.AreEqual(Untranslated, ((AdaptiveChoiceSetInput)body[1]).Value, "Choice set input was translated");
            Assert.AreEqual(Untranslated, ((AdaptiveDateInput)body[2]).Value, "Date input was translated");
            Assert.AreEqual(Translated, ((AdaptiveTextInput)body[3]).Value, "Text input was not translated");
            Assert.AreEqual(Untranslated, ((AdaptiveTimeInput)body[4]).Value, "Time input was translated");
            Assert.AreEqual(Untranslated, ((AdaptiveToggleInput)body[5]).Value, "Toggle input was translated");
            Assert.AreEqual(Translated, ((JToken)((AdaptiveSubmitAction)actions[0]).Data)["msteams"]["value"].ToString(), "ImBack was not translated");
            Assert.AreEqual(Untranslated, ((JToken)((AdaptiveSubmitAction)actions[1]).Data)["msteams"]["value"].ToString(), "MessageBack was translated");
        }

        private async Task TestPropertiesToTranslate(AdaptiveCardTranslatorSettings settings = null)
        {
            settings ??= AdaptiveCardTranslator.DefaultSettings;

            var card = new AdaptiveCard("1.0")
            {
                Lang = Untranslated,
                Speak = Untranslated,
                FallbackText = Untranslated,
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = Untranslated,
                    },
                    new AdaptiveImage
                    {
                        AltText = Untranslated,
                        Url = new Uri("url", UriKind.Relative),
                    },
                    new AdaptiveTextInput
                    {
                        Id = Untranslated,
                        Placeholder = Untranslated,
                        Value = Untranslated,
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = Untranslated,
                        IconUrl = Untranslated,
                        Style = Untranslated,
                        Data = Untranslated,
                    },
                    new AdaptiveSubmitAction
                    {
                        Data = new Dictionary<string, object>
                        {
                            {
                                "msteams", new Dictionary<string, object>
                                {
                                    { "type", "messageBack" },
                                    { "displayText", Untranslated },
                                }
                            },
                        },
                    },
                },
            };

            var result = await AdaptiveCardTranslator.TranslateAsync(card, TranslateOneAsync, settings);
            var body = result.Body;
            var actions = result.Actions;

            void AssertTranslation(object propertyContainer, string propertyName)
            {
                var shouldBeTranslated = settings.PropertiesToTranslate.Contains(propertyName);
                var expected = shouldBeTranslated ? Translated : Untranslated;
                var actual = propertyContainer.ToJObject()[propertyName].ToString();
                var message = $"{propertyName} was{(shouldBeTranslated ? " not" : string.Empty)} translated";

                Assert.AreEqual(expected, actual, message);
            }

            AssertTranslation(result, "lang");
            AssertTranslation(result, "speak");
            AssertTranslation(result, "fallbackText");
            AssertTranslation(body[0], "text");
            AssertTranslation(body[1], "altText");
            AssertTranslation(body[2], "id");
            AssertTranslation(body[2], "placeholder");
            AssertTranslation(body[2], "value");
            AssertTranslation(actions[0], "title");
            AssertTranslation(actions[0], "iconUrl");
            AssertTranslation(actions[0], "style");
            AssertTranslation(actions[0], "data");
            AssertTranslation(((JToken)((AdaptiveSubmitAction)actions[1]).Data)["msteams"], "displayText");
        }

        private Task<string> TranslateOneAsync(string input, CancellationToken cancellationToken) => Task.FromResult(Translated);
    }
}
