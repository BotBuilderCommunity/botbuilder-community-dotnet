using AdaptiveCards;
using Bot.Builder.Community.Cards.Management;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Bot.Builder.Community.Cards.Tests.Management
{
    [TestClass]
    public class DataIdTests
    {
        [TestMethod]
        public void TestSetInBatch()
        {
            const string ACTIONID = "Action ID";
            const string CARDID = "Card ID";
            const string CAROUSELID = "Carousel ID";
            const string BATCHID = "Batch ID";
            const string EXTRADATA = "Extra Data";

            var jObject = new JObject();

            var json = $"{{'{PropertyNames.LibraryData}': {{'{DataIdScopes.Card}': '{CARDID}',"
                         + $" '{DataIdScopes.Carousel}': '{CAROUSELID}'}}}}";

            var animationCard = new AnimationCard();

            var audioCard = new AudioCard(buttons: new List<CardAction>
            {
                new CardAction(),
            });

            var heroCard = new HeroCard(buttons: new List<CardAction>
            {
                new CardAction(ActionTypes.PostBack),
            });

            var oAuthCard = new OAuthCard(buttons: new List<CardAction>
            {
                new CardAction(ActionTypes.MessageBack, value: new object()),
            });

            var receiptCard = new ReceiptCard(buttons: new List<CardAction>
            {
                new CardAction(ActionTypes.PostBack, text: "{}"),
            });

            var signinCard = new SigninCard(buttons: new List<CardAction>
            {
                new CardAction(ActionTypes.MessageBack, value: json),
            });

            var thumbnailCard = new ThumbnailCard(buttons: new List<CardAction>
            {
                new CardAction(ActionTypes.PostBack, value: "Not JSON"),
            });

            var videoCard = new VideoCard(buttons: new List<CardAction>
            {
                new CardAction(ActionTypes.MessageBack, text: "Not JSON"),
                new CardAction(ActionTypes.PostBack, value: "Not JSON", text: "{}"),
                new CardAction(ActionTypes.MessageBack, value: jObject, text: "{}"),
                new CardAction(ActionTypes.PostBack, value: new { key = EXTRADATA }),
                new CardAction(ActionTypes.ImBack, value: new JObject(), text: "{}"),
            });

            var adaptiveCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2))
            {
                SelectAction = new AdaptiveSubmitAction(),
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveActionSet
                    {
                        Actions = new List<AdaptiveAction>
                        {
                            new AdaptiveSubmitAction
                            {
                                Data = new
                                {
                                    Foo = EXTRADATA,
                                },
                            },
                        },
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Data = new Dictionary<string, object>
                        {
                            { DataIdScopes.Action, ACTIONID },
                        }.WrapLibraryData(),
                    },
                    new AdaptiveShowCardAction
                    {
                        Card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
                        {
                            SelectAction = new AdaptiveOpenUrlAction
                            {
                                UrlString = "https://adaptivecards.io/",
                            },
                            Actions = new List<AdaptiveAction>
                            {
                                new AdaptiveSubmitAction(),
                                new AdaptiveOpenUrlAction
                                {
                                    UrlString = "https://adaptivecards.io/",
                                },
                            }
                        }
                    }
                },
            };

            var batch = new List<IMessageActivity>
            {
                MessageFactory.Attachment(new List<Attachment>
                {
                    new Attachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = adaptiveCard,
                    },
                    animationCard.ToAttachment(),
                }),
                MessageFactory.Attachment(new List<Attachment>
                {
                    audioCard.ToAttachment(),
                    heroCard.ToAttachment(),
                    new Attachment
                    {
                        ContentType = OAuthCard.ContentType,
                        Content = oAuthCard,
                    },
                }),
                MessageFactory.Attachment(new List<Attachment>
                {
                    receiptCard.ToAttachment(),
                    signinCard.ToAttachment(),
                    thumbnailCard.ToAttachment(),
                    videoCard.ToAttachment(),
                }),
            };

            var options = new DataIdOptions(DataId.Scopes);

            options.Set(DataIdScopes.Batch, BATCHID);
            DataId.SetInBatch(batch, options);

            var newAdaptiveCard = (AdaptiveCard)batch[0].Attachments[0].Content;
            var selectAction = (AdaptiveSubmitAction)newAdaptiveCard.SelectAction;
            var data = (JObject)selectAction.Data;
            var batchId = data.GetIdFromActionData(DataIdScopes.Batch);
            var carouselId = data.GetIdFromActionData(DataIdScopes.Carousel);
            var cardId = data.GetIdFromActionData(DataIdScopes.Card);
            var actionId = data.GetIdFromActionData(DataIdScopes.Action);
            var carouselIds = new HashSet<string>();
            var cardIds = new HashSet<string>();
            var actionIds = new HashSet<string>();

            carouselIds.Add(carouselId);
            cardIds.Add(cardId);
            actionIds.Add(actionId);

            Assert.AreNotSame(adaptiveCard, newAdaptiveCard, "New Adaptive Card reference was not assigned");
            Assert.IsNotNull(batchId);
            Assert.IsNotNull(carouselId);
            Assert.IsNotNull(cardId);
            Assert.IsNotNull(actionId);

            data = (JObject)((AdaptiveSubmitAction)((AdaptiveActionSet)newAdaptiveCard.Body.Single()).Actions.Single()).Data;
            actionId = data.GetIdFromActionData(DataIdScopes.Action);

            // HashSet.Add returns false if the item was already in the set,
            // so these IsTrue(HashSet.Add) calls are to test for uniqueness of an ID
            Assert.IsTrue(
                actionIds.Add(actionId),
                "The action ID in the action set's submit action is the same as the action ID in the Adaptive Card's select action");
            Assert.AreEqual(batchId, data.GetIdFromActionData(DataIdScopes.Batch));
            Assert.AreEqual(carouselId, data.GetIdFromActionData(DataIdScopes.Carousel));
            Assert.AreEqual(cardId, data.GetIdFromActionData(DataIdScopes.Card));
            Assert.IsNotNull(actionId);
            Assert.AreEqual(EXTRADATA, (string)data["Foo"], "The preexisting 'extra' data was modified");

            data = (JObject)((AdaptiveSubmitAction)newAdaptiveCard.Actions[0]).Data;
            actionId = data.GetIdFromActionData(DataIdScopes.Action);

            Assert.IsTrue(actionIds.Add(actionId));
            Assert.AreEqual(batchId, data.GetIdFromActionData(DataIdScopes.Batch));
            Assert.AreEqual(carouselId, data.GetIdFromActionData(DataIdScopes.Carousel));
            Assert.AreEqual(cardId, data.GetIdFromActionData(DataIdScopes.Card));
            Assert.AreEqual(ACTIONID, actionId, "The preexisting action ID was overwritten");

            data = (JObject)((AdaptiveSubmitAction)((AdaptiveShowCardAction)newAdaptiveCard.Actions[1]).Card.Actions[0]).Data;
            actionId = data.GetIdFromActionData(DataIdScopes.Action);

            Assert.IsTrue(actionIds.Add(actionId));
            Assert.AreEqual(batchId, data.GetIdFromActionData(DataIdScopes.Batch));
            Assert.AreEqual(carouselId, data.GetIdFromActionData(DataIdScopes.Carousel));
            Assert.AreEqual(cardId, data.GetIdFromActionData(DataIdScopes.Card));
            Assert.IsNotNull(actionId);

            Assert.AreSame(animationCard, batch[0].Attachments[1].Content);
            Assert.IsNull(animationCard.Buttons);

            Assert.AreSame(audioCard, batch[1].Attachments[0].Content);
            Assert.IsNull(audioCard.Buttons.Single().Value);
            Assert.IsNull(audioCard.Buttons.Single().Text);

            Assert.AreSame(heroCard, batch[1].Attachments[1].Content);
            Assert.IsNull(heroCard.Buttons.Single().Value);
            Assert.IsNull(heroCard.Buttons.Single().Text);

            data = (JObject)oAuthCard.Buttons.Single().Value;
            carouselId = data.GetIdFromActionData(DataIdScopes.Carousel);
            cardId = data.GetIdFromActionData(DataIdScopes.Card);
            actionId = data.GetIdFromActionData(DataIdScopes.Action);

            Assert.AreSame(oAuthCard, batch[1].Attachments[2].Content);
            Assert.IsNull(oAuthCard.Buttons.Single().Text);
            Assert.IsTrue(carouselIds.Add(carouselId));
            Assert.IsTrue(cardIds.Add(cardId));
            Assert.IsTrue(actionIds.Add(actionId));
            Assert.AreEqual(batchId, data.GetIdFromActionData(DataIdScopes.Batch));
            Assert.IsNotNull(carouselId);
            Assert.IsNotNull(cardId);
            Assert.IsNotNull(actionId);

            data = receiptCard.Buttons.Single().Text.ToJObject(true);
            carouselId = data.GetIdFromActionData(DataIdScopes.Carousel);
            cardId = data.GetIdFromActionData(DataIdScopes.Card);
            actionId = data.GetIdFromActionData(DataIdScopes.Action);

            Assert.AreSame(receiptCard, batch[2].Attachments[0].Content);
            Assert.IsNull(receiptCard.Buttons.Single().Value);
            Assert.IsTrue(carouselIds.Add(carouselId));
            Assert.IsTrue(cardIds.Add(cardId));
            Assert.IsTrue(actionIds.Add(actionId));
            Assert.AreEqual(batchId, data.GetIdFromActionData(DataIdScopes.Batch));
            Assert.IsNotNull(carouselId);
            Assert.IsNotNull(cardId);
            Assert.IsNotNull(actionId);

            var valueString = (string)signinCard.Buttons.Single().Value;

            data = valueString.ToJObject(true);
            actionId = data.GetIdFromActionData(DataIdScopes.Action);

            Assert.AreSame(signinCard, batch[2].Attachments[1].Content);
            Assert.IsNull(signinCard.Buttons.Single().Text);
            Assert.IsTrue(actionIds.Add(actionId));
            Assert.AreEqual(batchId, data.GetIdFromActionData(DataIdScopes.Batch));
            Assert.AreEqual(CAROUSELID, data.GetIdFromActionData(DataIdScopes.Carousel));
            Assert.AreEqual(CARDID, data.GetIdFromActionData(DataIdScopes.Card));
            Assert.IsNotNull(actionId);

            valueString = (string)thumbnailCard.Buttons.Single().Value;

            Assert.AreSame(thumbnailCard, batch[2].Attachments[2].Content);
            Assert.IsNull(thumbnailCard.Buttons.Single().Text);
            Assert.IsNull(valueString.ToJObject(true));

            Assert.AreSame(videoCard, batch[2].Attachments[3].Content);
            Assert.IsNull(videoCard.Buttons[0].Value);
            Assert.IsNotNull(videoCard.Buttons[0].Text);
            Assert.IsNull(videoCard.Buttons[0].Text.ToJObject(true));

            valueString = (string)videoCard.Buttons[1].Value;
            data = videoCard.Buttons[1].Text.ToJObject(true);
            cardId = data.GetIdFromActionData(DataIdScopes.Card);
            actionId = data.GetIdFromActionData(DataIdScopes.Action);

            Assert.IsNotNull(valueString);
            Assert.IsNull(valueString.ToJObject(true));
            Assert.IsTrue(cardIds.Add(cardId));
            Assert.IsTrue(actionIds.Add(actionId));
            Assert.AreEqual(batchId, data.GetIdFromActionData(DataIdScopes.Batch));
            Assert.AreEqual(carouselId, data.GetIdFromActionData(DataIdScopes.Carousel));
            Assert.IsNotNull(cardId);
            Assert.IsNotNull(actionId);

            data = (JObject)videoCard.Buttons[2].Value;
            actionId = data.GetIdFromActionData(DataIdScopes.Action);

            Assert.AreSame(jObject, data);
            Assert.AreEqual("{}", videoCard.Buttons[2].Text);
            Assert.IsTrue(actionIds.Add(actionId));
            Assert.AreEqual(batchId, data.GetIdFromActionData(DataIdScopes.Batch));
            Assert.AreEqual(carouselId, data.GetIdFromActionData(DataIdScopes.Carousel));
            Assert.AreEqual(cardId, data.GetIdFromActionData(DataIdScopes.Card));
            Assert.IsNotNull(actionId);

            data = (JObject)videoCard.Buttons[3].Value;
            actionId = data.GetIdFromActionData(DataIdScopes.Action);

            Assert.IsTrue(actionIds.Add(actionId));
            Assert.AreEqual(batchId, data.GetIdFromActionData(DataIdScopes.Batch));
            Assert.AreEqual(carouselId, data.GetIdFromActionData(DataIdScopes.Carousel));
            Assert.AreEqual(cardId, data.GetIdFromActionData(DataIdScopes.Card));
            Assert.IsNotNull(actionId);
            Assert.AreEqual(EXTRADATA, (string)data["key"]);

            data = (JObject)videoCard.Buttons[4].Value;

            Assert.AreEqual("{}", videoCard.Buttons[4].Text);
            Assert.IsNull(data.GetIdFromActionData(DataIdScopes.Batch));
            Assert.IsNull(data.GetIdFromActionData(DataIdScopes.Carousel));
            Assert.IsNull(data.GetIdFromActionData(DataIdScopes.Card));
            Assert.IsNull(data.GetIdFromActionData(DataIdScopes.Action));

            jObject = new AdaptiveCard("1.0")
            {
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Data = new Dictionary<string, object>
                        {
                            { DataIdScopes.Batch, BATCHID },
                            { DataIdScopes.Action, ACTIONID },
                        }.WrapLibraryData(),
                    }
                }
            }.ToJObject();

            batch = new List<IMessageActivity>
            {
                MessageFactory.Carousel(new List<Attachment>
                {
                    new Attachment
                    {
                        ContentType = "This attachment will be ignored",
                        Content = new HeroCard(buttons: new List<CardAction>
                        {
                            new CardAction(ActionTypes.PostBack, value: new JObject(), text: "{}"),
                        }),
                    },
                    new Attachment
                    {
                        ContentType = ContentTypes.AdaptiveCard,
                        Content = jObject,
                    },
                }),
                MessageFactory.Attachment(new Attachment
                {
                    ContentType = ContentTypes.AdaptiveCard,
                    Content = new HeroCard(buttons: new List<CardAction>
                    {
                        new CardAction(ActionTypes.MessageBack, value: new JObject(), text: "{}"),
                    }),
                }),
            };

            options = new DataIdOptions(DataIdScopes.Batch, true);

            options.Set(DataIdScopes.Carousel, CAROUSELID);
            DataId.SetInBatch(batch, options);

            heroCard = (HeroCard)batch[0].Attachments[0].Content;
            data = (JObject)heroCard.Buttons.Single().Value;

            Assert.AreEqual("{}", heroCard.Buttons.Single().Text);
            Assert.IsNull(data.GetIdFromActionData(DataIdScopes.Batch));
            Assert.IsNull(data.GetIdFromActionData(DataIdScopes.Carousel));
            Assert.IsNull(data.GetIdFromActionData(DataIdScopes.Card));
            Assert.IsNull(data.GetIdFromActionData(DataIdScopes.Action));

            data = (JObject)jObject["actions"][0]["data"];

            Assert.AreSame(jObject, batch[0].Attachments[1].Content, "New Adaptive Card JObject reference was assigned");
            Assert.AreNotEqual(BATCHID, data.GetIdFromActionData(DataIdScopes.Batch), "Batch ID was not generated");
            Assert.AreEqual(CAROUSELID, data.GetIdFromActionData(DataIdScopes.Carousel), "Carousel ID was not applied");
            Assert.IsNull(data.GetIdFromActionData(DataIdScopes.Card), "Card ID was applied");
            Assert.AreEqual(ACTIONID, data.GetIdFromActionData(DataIdScopes.Action), "Action ID was generated/applied");

            heroCard = (HeroCard)batch[1].Attachments.Single().Content;
            data = (JObject)heroCard.Buttons.Single().Value;

            Assert.AreEqual("{}", heroCard.Buttons.Single().Text);
            Assert.IsNull(data.GetIdFromActionData(DataIdScopes.Batch));
            Assert.IsNull(data.GetIdFromActionData(DataIdScopes.Carousel));
            Assert.IsNull(data.GetIdFromActionData(DataIdScopes.Card));
            Assert.IsNull(data.GetIdFromActionData(DataIdScopes.Action));

            // Empty options shouldn't apply any ID's

            batch = new List<IMessageActivity>
            {
                MessageFactory.Attachment(new HeroCard(buttons: new List<CardAction>
                {
                    new CardAction(ActionTypes.PostBack, value: new JObject()),
                }).ToAttachment()),
            };

            DataId.SetInBatch(batch, new DataIdOptions());

            heroCard = (HeroCard)batch.Single().Attachments.Single().Content;
            data = (JObject)heroCard.Buttons.Single().Value;

            Assert.IsNull(data.GetIdFromActionData(DataIdScopes.Batch));
            Assert.IsNull(data.GetIdFromActionData(DataIdScopes.Carousel));
            Assert.IsNull(data.GetIdFromActionData(DataIdScopes.Card));
            Assert.IsNull(data.GetIdFromActionData(DataIdScopes.Action));

            // Null options should default to applying an action ID

            batch = new List<IMessageActivity>
            {
                MessageFactory.Attachment(new HeroCard(buttons: new List<CardAction>
                {
                    new CardAction(ActionTypes.MessageBack, value: new JObject()),
                }).ToAttachment()),
            };

            DataId.SetInBatch(batch);

            heroCard = (HeroCard)batch.Single().Attachments.Single().Content;
            data = (JObject)heroCard.Buttons.Single().Value;

            Assert.IsNull(data.GetIdFromActionData(DataIdScopes.Batch));
            Assert.IsNull(data.GetIdFromActionData(DataIdScopes.Carousel));
            Assert.IsNull(data.GetIdFromActionData(DataIdScopes.Card));
            Assert.IsNotNull(data.GetIdFromActionData(DataIdScopes.Action));
        }

        [TestMethod]
        public void SetInBatch_SetsCorrectly()
        {
            var batch = CreateBatch();

            DataId.SetInBatch(batch);

            AssertIdsWereSetCorrectly(batch, batch);
        }

        [TestMethod]
        public void SetInActivity_SetsCorrectly()
        {
            var batch = CreateBatch();
            var activity = batch[0];

            DataId.SetInActivity(activity);

            AssertIdsWereSetCorrectly(batch, activity);
        }

        [TestMethod]
        public void SetInCarousel_SetsCorrectly()
        {
            var batch = CreateBatch();
            var activity = batch[0];

            DataId.SetInCarousel(activity.Attachments);

            AssertIdsWereSetCorrectly(batch, activity);
        }

        [TestMethod]
        public void SetInAttachment_SetsCorrectly()
        {
            var batch = CreateBatch();
            var attachment = batch[0].Attachments[0];

            DataId.SetInAttachment(attachment);

            AssertIdsWereSetCorrectly(batch, attachment);
        }

        [TestMethod]
        public void SetInAdaptiveCard_SetsCorrectly()
        {
            var batch = CreateBatch();
            var attachment = batch[0].Attachments[0];

            object adaptiveCard = attachment.Content;

            DataId.SetInAdaptiveCard(ref adaptiveCard);

            attachment.Content = adaptiveCard;

            AssertIdsWereSetCorrectly(batch, attachment);
        }

        [TestMethod]
        public void SetInAnimationCard_SetsCorrectly() => TestBotFrameworkCard<AnimationCard>(DataId.SetInAnimationCard, 0, 1);

        [TestMethod]
        public void SetInAudioCard_SetsCorrectly() => TestBotFrameworkCard<AudioCard>(DataId.SetInAudioCard, 0, 2);

        [TestMethod]
        public void SetInHeroCard_SetsCorrectly() => TestBotFrameworkCard<HeroCard>(DataId.SetInHeroCard, 0, 3);

        [TestMethod]
        public void SetInOAuthCard_SetsCorrectly() => TestBotFrameworkCard<OAuthCard>(DataId.SetInOAuthCard, 1, 0);

        [TestMethod]
        public void SetInReceiptCard_SetsCorrectly() => TestBotFrameworkCard<ReceiptCard>(DataId.SetInReceiptCard, 1, 1);

        [TestMethod]
        public void SetInSigninCard_SetsCorrectly() => TestBotFrameworkCard<SigninCard>(DataId.SetInSigninCard, 1, 2);

        [TestMethod]
        public void SetInThumbnailCard_SetsCorrectly() => TestBotFrameworkCard<ThumbnailCard>(DataId.SetInThumbnailCard, 1, 3);

        [TestMethod]
        public void SetInVideoCard_SetsCorrectly() => TestBotFrameworkCard<VideoCard>(DataId.SetInVideoCard, 1, 4);

        [TestMethod]
        public void SetInSubmitAction_SetsCorrectly()
        {
            var batch = CreateBatch();
            var actions = ((AdaptiveCard)batch[0].Attachments[0].Content).Actions;

            object submitAction = actions[0];

            DataId.SetInSubmitAction(ref submitAction);

            actions[0] = JObject.FromObject(submitAction).ToObject<AdaptiveSubmitAction>();

            AssertIdsWereSetCorrectly(batch, actions[0]);
        }

        [TestMethod]
        public void SetInCardAction_SetsCorrectly()
        {
            var batch = CreateBatch();
            var action = ((AnimationCard)batch[0].Attachments[1].Content).Buttons[0];

            DataId.SetInCardAction(action);

            AssertIdsWereSetCorrectly(batch, action);
        }

        [TestMethod]
        public void SetInActionData_SetsCorrectly()
        {
            var batch = CreateBatch();
            var submitAction = (AdaptiveSubmitAction)((AdaptiveCard)batch[0].Attachments[0].Content).Actions[0];

            object actionData = submitAction.Data;

            DataId.SetInActionData(ref actionData);

            submitAction.Data = actionData;

            AssertIdsWereSetCorrectly(batch, submitAction);
        }

        private static void TestBotFrameworkCard<T>(SetIdsIn<T> setIdsIn, int activityIndex, int attachmentIndex)
        {
            var batch = CreateBatch();
            var attachment = batch[activityIndex].Attachments[attachmentIndex];

            setIdsIn((T)attachment.Content);

            AssertIdsWereSetCorrectly(batch, attachment);
        }

        private static void AssertIdsWereSetCorrectly(IList<IMessageActivity> batch, object scopeRoot)
        {
            var anyIdsExpected = true;
            var anyIdsFound = false;
            var totalAssertions = 0;
            var scope = DataIdScopes.Action;

            if (scopeRoot is Attachment) scope = DataIdScopes.Card;
            if (scopeRoot is IMessageActivity) scope = DataIdScopes.Carousel;
            if (scopeRoot is IList<IMessageActivity>) scope = DataIdScopes.Batch;

            foreach (var activity in batch)
            {
                if (scope == DataIdScopes.Carousel)
                {
                    anyIdsExpected = activity == scopeRoot as IMessageActivity;
                }

                foreach (var attachment in activity.Attachments)
                {
                    if (scope == DataIdScopes.Card)
                    {
                        anyIdsExpected = attachment == scopeRoot as Attachment;
                    }

                    IEnumerable<object> actions = null;

                    var card = attachment.Content;

                    switch (attachment.ContentType)
                    {
                        case ContentTypes.AdaptiveCard: actions = ((AdaptiveCard)card).Actions; break;
                        case AnimationCard.ContentType: actions = ((AnimationCard)card).Buttons; break;
                        case AudioCard.ContentType: actions = ((AudioCard)card).Buttons; break;
                        case HeroCard.ContentType: actions = ((HeroCard)card).Buttons; break;
                        case OAuthCard.ContentType: actions = ((OAuthCard)card).Buttons; break;
                        case ReceiptCard.ContentType: actions = ((ReceiptCard)card).Buttons; break;
                        case SigninCard.ContentType: actions = ((SigninCard)card).Buttons; break;
                        case ThumbnailCard.ContentType: actions = ((ThumbnailCard)card).Buttons; break;
                        case VideoCard.ContentType: actions = ((VideoCard)card).Buttons; break;
                    }

                    foreach (var action in actions)
                    {
                        if (scope == DataIdScopes.Action)
                        {
                            anyIdsExpected = action == scopeRoot;
                        }

                        var actionData = action is CardAction cardAction
                            ? JObject.FromObject(cardAction.Value)
                            : JObject.FromObject(action)["data"] as JObject;

                        Assert.AreEqual(anyIdsExpected, actionData.GetIdFromActionData() != null);

                        if (anyIdsExpected)
                        {
                            anyIdsFound = true;
                        }

                        totalAssertions++;
                    }
                }
            }

            Assert.IsTrue(anyIdsFound);
            Assert.AreEqual(18, totalAssertions);
        }

        private static IList<IMessageActivity> CreateBatch() => new List<IMessageActivity>
        {
            MessageFactory.Attachment(new List<Attachment>
            {
                new Attachment
                {
                    ContentType = ContentTypes.AdaptiveCard,
                    Content = new AdaptiveCard("1.0")
                    {
                        Actions = new List<AdaptiveAction>
                        {
                            new AdaptiveSubmitAction
                            {
                                Data = new object(),
                            },
                            new AdaptiveSubmitAction
                            {
                                Data = new object(),
                            },
                        },
                    },
                },
                new AnimationCard
                {
                    Buttons = CreateButtons(),
                }.ToAttachment(),
                new AudioCard
                {
                    Buttons = CreateButtons(),
                }.ToAttachment(),
                new HeroCard
                {
                    Buttons = CreateButtons(),
                }.ToAttachment(),
            }),
            MessageFactory.Carousel(new List<Attachment>
            {
                new Attachment
                {
                    ContentType = OAuthCard.ContentType,
                    Content = new OAuthCard
                    {
                        Buttons = CreateButtons(),
                    },
                },
                new ReceiptCard
                {
                    Buttons = CreateButtons(),
                }.ToAttachment(),
                new SigninCard
                {
                    Buttons = CreateButtons(),
                }.ToAttachment(),
                new ThumbnailCard
                {
                    Buttons = CreateButtons(),
                }.ToAttachment(),
                new VideoCard
                {
                    Buttons = CreateButtons(),
                }.ToAttachment(),
            }),
        };

        private static List<CardAction> CreateButtons()
        {
            return new List<CardAction>
            {
                new CardAction
                {
                    Type = ActionTypes.PostBack,
                    Value = new object(),
                },
                new CardAction
                {
                    Type = ActionTypes.PostBack,
                    Value = new object(),
                },
            };
        }

        private delegate void SetIdsIn<T>(T entryValue, DataIdOptions options = null);
    }
}
