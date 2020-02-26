using System;
using System.Collections.Generic;
using System.Linq;
using AdaptiveCards;
using Bot.Builder.Community.Cards.Management;
using Bot.Builder.Community.Cards.Management.Tree;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards.Tests
{
    [TestClass]
    public class ManagementExtensionsTests
    {
        [TestMethod]
        public void TestSeparateAttachments()
        {
            var batch = new List<IMessageActivity>
            {
                MessageFactory.Text("text activity text"),
                MessageFactory.Attachment(new Attachment()),
                MessageFactory.Attachment(new Attachment(), "attachment activity text"),
                MessageFactory.Attachment(
                    new List<Attachment>
                    {
                        new Attachment(),
                        new Attachment(),
                    }),
                MessageFactory.Attachment(
                    new List<Attachment>
                    {
                        new Attachment(),
                        new Attachment(),
                    }, "attachments activity text"),
                MessageFactory.Carousel(
                    new List<Attachment>
                    {
                        new Attachment(),
                        new Attachment(),
                    }),
                MessageFactory.Carousel(
                    new List<Attachment>
                    {
                        new Attachment(),
                        new Attachment(),
                    }, "carousel activity text"),
            }.Cast<Activity>().ToList();

            batch.SeparateAttachments();

            AssertIsTextMessage(batch[0]);
            AssertIsAttachmentMessage(batch[1]);
            AssertIsTextMessage(batch[2]);
            AssertIsAttachmentMessage(batch[3]);
            AssertIsAttachmentMessage(batch[4]);
            AssertIsAttachmentMessage(batch[5]);
            AssertIsTextMessage(batch[6]);
            AssertIsAttachmentMessage(batch[7]);
            AssertIsAttachmentMessage(batch[8]);
            AssertIsAttachmentMessage(batch[9], 2);
            AssertIsTextMessage(batch[10]);
            AssertIsAttachmentMessage(batch[11], 2);

            batch = null;

            Assert.ThrowsException<ArgumentNullException>(() => batch.SeparateAttachments());
        }

        [TestMethod]
        public void TestConvertAdaptiveCards()
        {
            var jObject = new JObject();

            var batch = new List<IMessageActivity>
            {
                MessageFactory.Attachment(new List<Attachment>
                {
                    new Attachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0)),
                    },
                }),
                MessageFactory.Attachment(new List<Attachment>
                {
                    new Attachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0)),
                    },
                    new Attachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = jObject,
                    },
                }),
            };

            batch.ConvertAdaptiveCards();

            Assert.IsTrue(batch.All(activity => activity.Attachments.All(attachment => attachment.Content is JObject)));

            // Any preexisting JObject references should remain
            Assert.IsTrue(batch[1].Attachments[1].Content == jObject);

            batch = null;

            Assert.ThrowsException<ArgumentNullException>(() => batch.ConvertAdaptiveCards());
        }

        [TestMethod]
        public void TestApplyIdsToBatch()
        {
            const string ACTIONID = "Action ID";
            const string CARDID = "Card ID";
            const string CAROUSELID = "Carousel ID";
            const string BATCHID = "Batch ID";
            const string EXTRADATA = "Extra Data";

            var jObject = new JObject();

            string json = $"{{'{PayloadIdTypes.GetKey(PayloadIdTypes.Card)}': '{CARDID}',"
                         + $" '{PayloadIdTypes.GetKey(PayloadIdTypes.Carousel)}': '{CAROUSELID}'}}";

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
                new CardAction(ActionTypes.ImBack, value: new JObject(), text: "{}"),
            });

            var batch = new List<IMessageActivity>
            {
                MessageFactory.Attachment(new List<Attachment>
                {
                    new Attachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2))
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
                                        { PayloadIdTypes.GetKey(PayloadIdTypes.Action), ACTIONID },
                                    },
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
                        },
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

            var options = new PayloadIdOptions(PayloadIdTypes.Collection);

            options.Set(PayloadIdTypes.Batch, BATCHID);
            batch.ApplyIdsToBatch(options);

            var adaptiveCard = (AdaptiveCard)batch[0].Attachments[0].Content;
            var selectAction = (AdaptiveSubmitAction)adaptiveCard.SelectAction;
            var data = (JObject)selectAction.Data;
            var batchId = data.GetIdFromPayload(PayloadIdTypes.Batch);
            var carouselId = data.GetIdFromPayload(PayloadIdTypes.Carousel);
            var cardId = data.GetIdFromPayload(PayloadIdTypes.Card);
            var actionId = data.GetIdFromPayload(PayloadIdTypes.Action);
            var carouselIds = new HashSet<string>();
            var cardIds = new HashSet<string>();
            var actionIds = new HashSet<string>();

            carouselIds.Add(carouselId);
            cardIds.Add(cardId);
            actionIds.Add(actionId);

            Assert.IsNotNull(batchId);
            Assert.IsNotNull(carouselId);
            Assert.IsNotNull(cardId);
            Assert.IsNotNull(actionId);

            data = (JObject)((AdaptiveSubmitAction)((AdaptiveActionSet)adaptiveCard.Body.Single()).Actions.Single()).Data;
            actionId = data.GetIdFromPayload(PayloadIdTypes.Action);

            // HashSet.Add returns false if the item was already in the set,
            // so these IsTrue(HashSet.Add) calls are to test for uniqueness of an ID
            Assert.IsTrue(
                actionIds.Add(actionId),
                "The action ID in the action set's submit action is the same as the action ID in the Adaptive Card's select action");
            Assert.AreEqual(batchId, data.GetIdFromPayload(PayloadIdTypes.Batch));
            Assert.AreEqual(carouselId, data.GetIdFromPayload(PayloadIdTypes.Carousel));
            Assert.AreEqual(cardId, data.GetIdFromPayload(PayloadIdTypes.Card));
            Assert.IsNotNull(actionId);
            Assert.IsTrue((string)data["Foo"] == EXTRADATA, "The preexisting 'extra' data was modified");

            data = (JObject)((AdaptiveSubmitAction)adaptiveCard.Actions[0]).Data;
            actionId = data.GetIdFromPayload(PayloadIdTypes.Action);

            Assert.IsTrue(actionIds.Add(actionId));
            Assert.AreEqual(batchId, data.GetIdFromPayload(PayloadIdTypes.Batch));
            Assert.AreEqual(carouselId, data.GetIdFromPayload(PayloadIdTypes.Carousel));
            Assert.AreEqual(cardId, data.GetIdFromPayload(PayloadIdTypes.Card));
            Assert.AreEqual(ACTIONID, actionId, "The preexisting action ID was overwritten");

            data = (JObject)((AdaptiveSubmitAction)((AdaptiveShowCardAction)adaptiveCard.Actions[1]).Card.Actions[0]).Data;
            actionId = data.GetIdFromPayload(PayloadIdTypes.Action);

            Assert.IsTrue(actionIds.Add(actionId));
            Assert.AreEqual(batchId, data.GetIdFromPayload(PayloadIdTypes.Batch));
            Assert.AreEqual(carouselId, data.GetIdFromPayload(PayloadIdTypes.Carousel));
            Assert.AreEqual(cardId, data.GetIdFromPayload(PayloadIdTypes.Card));
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
            carouselId = data.GetIdFromPayload(PayloadIdTypes.Carousel);
            cardId = data.GetIdFromPayload(PayloadIdTypes.Card);
            actionId = data.GetIdFromPayload(PayloadIdTypes.Action);

            Assert.AreSame(oAuthCard, batch[1].Attachments[2].Content);
            Assert.IsNull(oAuthCard.Buttons.Single().Text);
            Assert.IsTrue(carouselIds.Add(carouselId));
            Assert.IsTrue(cardIds.Add(cardId));
            Assert.IsTrue(actionIds.Add(actionId));
            Assert.AreEqual(batchId, data.GetIdFromPayload(PayloadIdTypes.Batch));
            Assert.IsNotNull(carouselId);
            Assert.IsNotNull(cardId);
            Assert.IsNotNull(actionId);

            data = receiptCard.Buttons.Single().Text.ToJObject(true);
            carouselId = data.GetIdFromPayload(PayloadIdTypes.Carousel);
            cardId = data.GetIdFromPayload(PayloadIdTypes.Card);
            actionId = data.GetIdFromPayload(PayloadIdTypes.Action);

            Assert.AreSame(receiptCard, batch[2].Attachments[0].Content);
            Assert.IsNull(receiptCard.Buttons.Single().Value);
            Assert.IsTrue(carouselIds.Add(carouselId));
            Assert.IsTrue(cardIds.Add(cardId));
            Assert.IsTrue(actionIds.Add(actionId));
            Assert.AreEqual(batchId, data.GetIdFromPayload(PayloadIdTypes.Batch));
            Assert.IsNotNull(carouselId);
            Assert.IsNotNull(cardId);
            Assert.IsNotNull(actionId);

            var valueString = (string)signinCard.Buttons.Single().Value;

            data = valueString.ToJObject(true);
            actionId = data.GetIdFromPayload(PayloadIdTypes.Action);

            Assert.AreSame(signinCard, batch[2].Attachments[1].Content);
            Assert.IsNull(signinCard.Buttons.Single().Text);
            Assert.IsTrue(actionIds.Add(actionId));
            Assert.AreEqual(batchId, data.GetIdFromPayload(PayloadIdTypes.Batch));
            Assert.AreEqual(CAROUSELID, data.GetIdFromPayload(PayloadIdTypes.Carousel));
            Assert.AreEqual(CARDID, data.GetIdFromPayload(PayloadIdTypes.Card));
            Assert.IsNotNull(actionId);

            valueString = (string)thumbnailCard.Buttons.Single().Value;

            Assert.AreSame(thumbnailCard, batch[2].Attachments[2].Content);
            Assert.IsNull(thumbnailCard.Buttons.Single().Text);
            Assert.IsNull(valueString.ToJObject(true));

            //  var videoCard = new VideoCard(buttons: new List<CardAction>
            //  {
            //      new CardAction(ActionTypes.MessageBack, text: "Not JSON"),
            //      new CardAction(ActionTypes.PostBack, value: "Not JSON", text: "{}"),
            //      new CardAction(ActionTypes.MessageBack, value: jObject, text: "{}"),
            //      new CardAction(ActionTypes.ImBack, value: new JObject(), text: "{}"),
            //  });

            Assert.AreSame(videoCard, batch[2].Attachments[3].Content);
            Assert.IsNull(videoCard.Buttons[0].Value);
            Assert.IsNotNull(videoCard.Buttons[0].Text);
            Assert.IsNull(videoCard.Buttons[0].Text.ToJObject(true));

            valueString = (string)videoCard.Buttons[1].Value;
            data = videoCard.Buttons[1].Text.ToJObject(true);
            cardId = data.GetIdFromPayload(PayloadIdTypes.Card);
            actionId = data.GetIdFromPayload(PayloadIdTypes.Action);

            Assert.IsNotNull(valueString);
            Assert.IsNull(valueString.ToJObject(true));
            Assert.IsTrue(cardIds.Add(cardId));
            Assert.IsTrue(actionIds.Add(actionId));
            Assert.AreEqual(batchId, data.GetIdFromPayload(PayloadIdTypes.Batch));
            Assert.AreEqual(carouselId, data.GetIdFromPayload(PayloadIdTypes.Carousel));
            Assert.IsNotNull(cardId);
            Assert.IsNotNull(actionId);

            data = (JObject)videoCard.Buttons[2].Value;
            actionId = data.GetIdFromPayload(PayloadIdTypes.Action);

            Assert.AreSame(jObject, data);
            Assert.AreEqual("{}", videoCard.Buttons[2].Text);
            Assert.IsTrue(actionIds.Add(actionId));
            Assert.AreEqual(batchId, data.GetIdFromPayload(PayloadIdTypes.Batch));
            Assert.AreEqual(carouselId, data.GetIdFromPayload(PayloadIdTypes.Carousel));
            Assert.AreEqual(cardId, data.GetIdFromPayload(PayloadIdTypes.Card));
            Assert.IsNotNull(actionId);

            data = (JObject)videoCard.Buttons[3].Value;

            Assert.AreEqual("{}", videoCard.Buttons[3].Text);
            Assert.IsNull(data.GetIdFromPayload(PayloadIdTypes.Batch));
            Assert.IsNull(data.GetIdFromPayload(PayloadIdTypes.Carousel));
            Assert.IsNull(data.GetIdFromPayload(PayloadIdTypes.Card));
            Assert.IsNull(data.GetIdFromPayload(PayloadIdTypes.Action));

            jObject = new AdaptiveCard("1.0")
            {
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Data = new Dictionary<string, object>
                        {
                            { PayloadIdTypes.GetKey(PayloadIdTypes.Batch), BATCHID },
                            { PayloadIdTypes.GetKey(PayloadIdTypes.Action), ACTIONID },
                        }
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
                        ContentType = CardConstants.AdaptiveCardContentType,
                        Content = jObject,
                    },
                }),
                MessageFactory.Attachment(new Attachment
                {
                    ContentType = CardConstants.AdaptiveCardContentType,
                    Content = new HeroCard(buttons: new List<CardAction>
                    {
                        new CardAction(ActionTypes.MessageBack, value: new JObject(), text: "{}"),
                    }),
                }),
            };

            options = new PayloadIdOptions(true);

            options.Set(PayloadIdTypes.Batch);
            options.Set(PayloadIdTypes.Carousel, CAROUSELID);

            batch.ApplyIdsToBatch(options);

            heroCard = (HeroCard)batch[0].Attachments[0].Content;
            data = (JObject)heroCard.Buttons.Single().Value;

            Assert.AreEqual("{}", heroCard.Buttons.Single().Text);
            Assert.IsNull(data.GetIdFromPayload(PayloadIdTypes.Batch));
            Assert.IsNull(data.GetIdFromPayload(PayloadIdTypes.Carousel));
            Assert.IsNull(data.GetIdFromPayload(PayloadIdTypes.Card));
            Assert.IsNull(data.GetIdFromPayload(PayloadIdTypes.Action));

            data = (JObject)jObject["actions"][0]["data"];

            Assert.AreSame(jObject, batch[0].Attachments[1].Content);
            Assert.AreNotEqual(BATCHID, data.GetIdFromPayload(PayloadIdTypes.Batch), "Batch ID was not generated");
            Assert.AreNotEqual(CAROUSELID, data.GetIdFromPayload(PayloadIdTypes.Carousel), "Carousel ID was not generated");
            Assert.IsNotNull(data.GetIdFromPayload(PayloadIdTypes.Carousel), "Carousel ID was not applied");
            Assert.IsNull(data.GetIdFromPayload(PayloadIdTypes.Card), "Card ID was applied");
            Assert.AreEqual(ACTIONID, data.GetIdFromPayload(PayloadIdTypes.Action), "Action ID was generated/applied");

            heroCard = (HeroCard)batch[1].Attachments.Single().Content;
            data = (JObject)heroCard.Buttons.Single().Value;

            Assert.AreEqual("{}", heroCard.Buttons.Single().Text);
            Assert.IsNull(data.GetIdFromPayload(PayloadIdTypes.Batch));
            Assert.IsNull(data.GetIdFromPayload(PayloadIdTypes.Carousel));
            Assert.IsNull(data.GetIdFromPayload(PayloadIdTypes.Card));
            Assert.IsNull(data.GetIdFromPayload(PayloadIdTypes.Action));

            batch = new List<IMessageActivity>
            {
                MessageFactory.Attachment(heroCard.ToAttachment()),
            };

            batch.ApplyIdsToBatch();

            data = (JObject)heroCard.Buttons.Single().Value;

            Assert.AreSame(heroCard, batch.Single().Attachments.Single().Content);
            Assert.AreEqual("{}", heroCard.Buttons.Single().Text);
            Assert.IsNotNull(data.GetIdFromPayload(PayloadIdTypes.Batch), "Batch ID was not generated/applied");
            Assert.IsNull(data.GetIdFromPayload(PayloadIdTypes.Carousel));
            Assert.IsNull(data.GetIdFromPayload(PayloadIdTypes.Card));
            Assert.IsNull(data.GetIdFromPayload(PayloadIdTypes.Action));

            batch = null;

            Assert.ThrowsException<ArgumentNullException>(() => batch.ApplyIdsToBatch());
        }

        [TestMethod]
        public void TestGetIdsFromBatch()
        {
            const string ACTIONID = "Action ID";
            const string ACTIONID2 = "Action ID 2";
            const string CARDID = "Card ID";
            const string CARDID2 = "Card ID 2";
            const string CAROUSELID = "Carousel ID";
            const string CAROUSELID2 = "Carousel ID 2";
            const string BATCHID = "Batch ID";
            const string BATCHID2 = "Batch ID 2";

            var batch = new List<IMessageActivity>
            {
                MessageFactory.Carousel(new List<Attachment>
                {
                    new Attachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = new AdaptiveCard(new AdaptiveSchemaVersion())
                        {
                            Actions = new List<AdaptiveAction>
                            {
                                new AdaptiveSubmitAction
                                {
                                    Data = new Dictionary<string, string>
                                    {
                                        { PayloadIdTypes.GetKey(PayloadIdTypes.Action), ACTIONID },
                                    },
                                },
                            },
                        },
                    },
                    new AnimationCard(buttons: new List<CardAction>
                    {
                        new CardAction(ActionTypes.PostBack, value: new Dictionary<string, string>
                        {
                            { PayloadIdTypes.GetKey(PayloadIdTypes.Card), CARDID },
                        }),
                    }).ToAttachment(),
                    new Attachment
                    {
                        ContentType = CardConstants.AdaptiveCardContentType,
                        Content = new AudioCard(buttons: new List<CardAction>
                        {
                            new CardAction(ActionTypes.PostBack, value: new Dictionary<string, string>
                            {
                                { PayloadIdTypes.GetKey(PayloadIdTypes.Card), CARDID2 },
                            }),
                        }),
                    },
                    new Attachment
                    {
                        ContentType = HeroCard.ContentType,
                        Content = new OAuthCard(buttons: new List<CardAction>
                        {
                            new CardAction(ActionTypes.PostBack, value: new Dictionary<string, string>
                            {
                                { PayloadIdTypes.GetKey(PayloadIdTypes.Card), CAROUSELID },
                            }),
                        }),
                    },
                }),
                MessageFactory.Attachment(new AudioCard(buttons: new List<CardAction>
                {
                    new CardAction(ActionTypes.ImBack, value: new Dictionary<string, string>
                    {
                        { PayloadIdTypes.GetKey(PayloadIdTypes.Carousel), CAROUSELID2 },
                    }),
                    new CardAction(ActionTypes.PostBack, value: new Dictionary<string, string>
                    {
                        { PayloadIdTypes.GetKey(PayloadIdTypes.Action), ACTIONID2 },
                        { PayloadIdTypes.GetKey(PayloadIdTypes.Batch), BATCHID },
                    }),
                    new CardAction(ActionTypes.MessageBack, value: new Dictionary<string, string>
                    {
                        { PayloadIdTypes.GetKey(PayloadIdTypes.Action), ACTIONID },
                        { PayloadIdTypes.GetKey(PayloadIdTypes.Batch), BATCHID2 },
                    }),
                }).ToAttachment()),
            };

            var ids = batch.GetIdsFromBatch();

            Assert.AreEqual(5, ids.Count);
            Assert.IsTrue(ids.Contains(new PayloadItem(PayloadIdTypes.Action, ACTIONID)));
            Assert.IsTrue(ids.Contains(new PayloadItem(PayloadIdTypes.Action, ACTIONID2)));
            Assert.IsTrue(ids.Contains(new PayloadItem(PayloadIdTypes.Card, CARDID)));
            Assert.IsFalse(ids.Contains(new PayloadItem(PayloadIdTypes.Card, CARDID2)));
            Assert.IsFalse(ids.Contains(new PayloadItem(PayloadIdTypes.Carousel, CAROUSELID)));
            Assert.IsFalse(ids.Contains(new PayloadItem(PayloadIdTypes.Carousel, CAROUSELID2)));
            Assert.IsTrue(ids.Contains(new PayloadItem(PayloadIdTypes.Batch, BATCHID)));
            Assert.IsTrue(ids.Contains(new PayloadItem(PayloadIdTypes.Batch, BATCHID2)));

            batch = null;

            Assert.ThrowsException<ArgumentNullException>(() => batch.GetIdsFromBatch());
        }

        [TestMethod]
        public void TestAdaptOutgoingCardActions()
        {
            var batch = new List<IMessageActivity>
            {
                new Activity(channelId: Channels.Directline, attachments: new List<Attachment>
                {

                }),
            };


            batch = null;

            Assert.ThrowsException<ArgumentNullException>(() => batch.AdaptOutgoingCardActions());
        }

        private static void AssertIsTextMessage(IMessageActivity activity)
            => Assert.IsTrue(activity.Attachments?.Any() != true && !string.IsNullOrEmpty(activity.Text));

        private static void AssertIsAttachmentMessage(IMessageActivity activity, int count = 1)
            => Assert.IsTrue(activity.Attachments.Count == count && string.IsNullOrEmpty(activity.Text));
    }
}
