using System;
using System.Collections.Generic;
using System.Linq;
using AdaptiveCards;
using Bot.Builder.Community.Cards.Management;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards.Tests
{
    [TestClass]
    public class ManagementExtensionsTests
    {
        private enum ActionMod
        {
            None,
            ConvertedToPostBack,
            EnsuredText,
            EnsuredValue,
            EnsuredStringValue,
            EnsuredObjectValue,
        }

        [TestMethod]
        public void TestSeparateAttachments()
        {
            var textActivity = MessageFactory.Text("text activity text");
            var heroCardAttachment = new HeroCard(buttons: new List<CardAction> { new CardAction(value: new Entity()) }).ToAttachment();

            var batch = new List<IMessageActivity>
            {
                textActivity,
                MessageFactory.Attachment(new Attachment()),
                MessageFactory.Attachment(new Attachment(), "attachment activity text"),
                new Activity(
                    attachments: new List<Attachment>
                    {
                        new Attachment(),
                        new Attachment(),
                    },
                    channelData: new Dictionary<string, object>(),
                    id: "Activity ID"),
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
                        heroCardAttachment,
                    }, "carousel activity text"),
            }.Cast<Activity>().ToList();

            batch.SeparateAttachments();

            static void AssertIsTextMessage(IMessageActivity activity)
                => Assert.IsTrue(activity.Attachments?.Any() != true && !string.IsNullOrEmpty(activity.Text));

            static void AssertIsAttachmentMessage(IMessageActivity activity, int count = 1)
                => Assert.IsTrue(activity.Attachments.Count == count && string.IsNullOrEmpty(activity.Text));

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

            Assert.AreSame(textActivity, batch[0]);
            Assert.IsNull(batch[3].ChannelData);
            Assert.IsInstanceOfType(batch[4].ChannelData, typeof(Dictionary<string, object>), "The channel data's type was changed");
            Assert.IsInstanceOfType(batch[5].ChannelData, typeof(Dictionary<string, object>), "The channel data's type was changed");
            Assert.IsNull(batch[4].Id);
            Assert.IsNull(batch[5].Id);
            Assert.IsNull(batch[6].ChannelData);

            var lastAttachment = batch[11].Attachments[1];

            Assert.IsInstanceOfType(((HeroCard)lastAttachment.Content).Buttons.Single().Value, typeof(Entity), "The value's type was changed");
            Assert.AreSame(heroCardAttachment, lastAttachment, "Hero card reference was broken");

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

            var json = $"{{'{DataId.GetKey(DataIdTypes.Card)}': '{CARDID}',"
                         + $" '{DataId.GetKey(DataIdTypes.Carousel)}': '{CAROUSELID}'}}";

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
                            { DataId.GetKey(DataIdTypes.Action), ACTIONID },
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

            var options = new DataIdOptions(DataId.Types);

            options.Set(DataIdTypes.Batch, BATCHID);
            batch.ApplyIdsToBatch(options);

            var newAdaptiveCard = (AdaptiveCard)batch[0].Attachments[0].Content;
            var selectAction = (AdaptiveSubmitAction)newAdaptiveCard.SelectAction;
            var data = (JObject)selectAction.Data;
            var batchId = data.GetIdFromActionData(DataIdTypes.Batch);
            var carouselId = data.GetIdFromActionData(DataIdTypes.Carousel);
            var cardId = data.GetIdFromActionData(DataIdTypes.Card);
            var actionId = data.GetIdFromActionData(DataIdTypes.Action);
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
            actionId = data.GetIdFromActionData(DataIdTypes.Action);

            // HashSet.Add returns false if the item was already in the set,
            // so these IsTrue(HashSet.Add) calls are to test for uniqueness of an ID
            Assert.IsTrue(
                actionIds.Add(actionId),
                "The action ID in the action set's submit action is the same as the action ID in the Adaptive Card's select action");
            Assert.AreEqual(batchId, data.GetIdFromActionData(DataIdTypes.Batch));
            Assert.AreEqual(carouselId, data.GetIdFromActionData(DataIdTypes.Carousel));
            Assert.AreEqual(cardId, data.GetIdFromActionData(DataIdTypes.Card));
            Assert.IsNotNull(actionId);
            Assert.AreEqual(EXTRADATA, (string)data["Foo"], "The preexisting 'extra' data was modified");

            data = (JObject)((AdaptiveSubmitAction)newAdaptiveCard.Actions[0]).Data;
            actionId = data.GetIdFromActionData(DataIdTypes.Action);

            Assert.IsTrue(actionIds.Add(actionId));
            Assert.AreEqual(batchId, data.GetIdFromActionData(DataIdTypes.Batch));
            Assert.AreEqual(carouselId, data.GetIdFromActionData(DataIdTypes.Carousel));
            Assert.AreEqual(cardId, data.GetIdFromActionData(DataIdTypes.Card));
            Assert.AreEqual(ACTIONID, actionId, "The preexisting action ID was overwritten");

            data = (JObject)((AdaptiveSubmitAction)((AdaptiveShowCardAction)newAdaptiveCard.Actions[1]).Card.Actions[0]).Data;
            actionId = data.GetIdFromActionData(DataIdTypes.Action);

            Assert.IsTrue(actionIds.Add(actionId));
            Assert.AreEqual(batchId, data.GetIdFromActionData(DataIdTypes.Batch));
            Assert.AreEqual(carouselId, data.GetIdFromActionData(DataIdTypes.Carousel));
            Assert.AreEqual(cardId, data.GetIdFromActionData(DataIdTypes.Card));
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
            carouselId = data.GetIdFromActionData(DataIdTypes.Carousel);
            cardId = data.GetIdFromActionData(DataIdTypes.Card);
            actionId = data.GetIdFromActionData(DataIdTypes.Action);

            Assert.AreSame(oAuthCard, batch[1].Attachments[2].Content);
            Assert.IsNull(oAuthCard.Buttons.Single().Text);
            Assert.IsTrue(carouselIds.Add(carouselId));
            Assert.IsTrue(cardIds.Add(cardId));
            Assert.IsTrue(actionIds.Add(actionId));
            Assert.AreEqual(batchId, data.GetIdFromActionData(DataIdTypes.Batch));
            Assert.IsNotNull(carouselId);
            Assert.IsNotNull(cardId);
            Assert.IsNotNull(actionId);

            data = receiptCard.Buttons.Single().Text.ToJObject(true);
            carouselId = data.GetIdFromActionData(DataIdTypes.Carousel);
            cardId = data.GetIdFromActionData(DataIdTypes.Card);
            actionId = data.GetIdFromActionData(DataIdTypes.Action);

            Assert.AreSame(receiptCard, batch[2].Attachments[0].Content);
            Assert.IsNull(receiptCard.Buttons.Single().Value);
            Assert.IsTrue(carouselIds.Add(carouselId));
            Assert.IsTrue(cardIds.Add(cardId));
            Assert.IsTrue(actionIds.Add(actionId));
            Assert.AreEqual(batchId, data.GetIdFromActionData(DataIdTypes.Batch));
            Assert.IsNotNull(carouselId);
            Assert.IsNotNull(cardId);
            Assert.IsNotNull(actionId);

            var valueString = (string)signinCard.Buttons.Single().Value;

            data = valueString.ToJObject(true);
            actionId = data.GetIdFromActionData(DataIdTypes.Action);

            Assert.AreSame(signinCard, batch[2].Attachments[1].Content);
            Assert.IsNull(signinCard.Buttons.Single().Text);
            Assert.IsTrue(actionIds.Add(actionId));
            Assert.AreEqual(batchId, data.GetIdFromActionData(DataIdTypes.Batch));
            Assert.AreEqual(CAROUSELID, data.GetIdFromActionData(DataIdTypes.Carousel));
            Assert.AreEqual(CARDID, data.GetIdFromActionData(DataIdTypes.Card));
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
            cardId = data.GetIdFromActionData(DataIdTypes.Card);
            actionId = data.GetIdFromActionData(DataIdTypes.Action);

            Assert.IsNotNull(valueString);
            Assert.IsNull(valueString.ToJObject(true));
            Assert.IsTrue(cardIds.Add(cardId));
            Assert.IsTrue(actionIds.Add(actionId));
            Assert.AreEqual(batchId, data.GetIdFromActionData(DataIdTypes.Batch));
            Assert.AreEqual(carouselId, data.GetIdFromActionData(DataIdTypes.Carousel));
            Assert.IsNotNull(cardId);
            Assert.IsNotNull(actionId);

            data = (JObject)videoCard.Buttons[2].Value;
            actionId = data.GetIdFromActionData(DataIdTypes.Action);

            Assert.AreSame(jObject, data);
            Assert.AreEqual("{}", videoCard.Buttons[2].Text);
            Assert.IsTrue(actionIds.Add(actionId));
            Assert.AreEqual(batchId, data.GetIdFromActionData(DataIdTypes.Batch));
            Assert.AreEqual(carouselId, data.GetIdFromActionData(DataIdTypes.Carousel));
            Assert.AreEqual(cardId, data.GetIdFromActionData(DataIdTypes.Card));
            Assert.IsNotNull(actionId);

            data = (JObject)videoCard.Buttons[3].Value;
            actionId = data.GetIdFromActionData(DataIdTypes.Action);

            Assert.IsTrue(actionIds.Add(actionId));
            Assert.AreEqual(batchId, data.GetIdFromActionData(DataIdTypes.Batch));
            Assert.AreEqual(carouselId, data.GetIdFromActionData(DataIdTypes.Carousel));
            Assert.AreEqual(cardId, data.GetIdFromActionData(DataIdTypes.Card));
            Assert.IsNotNull(actionId);
            Assert.AreEqual(EXTRADATA, (string)data["key"]);

            data = (JObject)videoCard.Buttons[4].Value;

            Assert.AreEqual("{}", videoCard.Buttons[4].Text);
            Assert.IsNull(data.GetIdFromActionData(DataIdTypes.Batch));
            Assert.IsNull(data.GetIdFromActionData(DataIdTypes.Carousel));
            Assert.IsNull(data.GetIdFromActionData(DataIdTypes.Card));
            Assert.IsNull(data.GetIdFromActionData(DataIdTypes.Action));

            jObject = new AdaptiveCard("1.0")
            {
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Data = new Dictionary<string, object>
                        {
                            { DataId.GetKey(DataIdTypes.Batch), BATCHID },
                            { DataId.GetKey(DataIdTypes.Action), ACTIONID },
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

            options = new DataIdOptions(DataIdTypes.Batch, true);

            options.Set(DataIdTypes.Carousel, CAROUSELID);
            batch.ApplyIdsToBatch(options);

            heroCard = (HeroCard)batch[0].Attachments[0].Content;
            data = (JObject)heroCard.Buttons.Single().Value;

            Assert.AreEqual("{}", heroCard.Buttons.Single().Text);
            Assert.IsNull(data.GetIdFromActionData(DataIdTypes.Batch));
            Assert.IsNull(data.GetIdFromActionData(DataIdTypes.Carousel));
            Assert.IsNull(data.GetIdFromActionData(DataIdTypes.Card));
            Assert.IsNull(data.GetIdFromActionData(DataIdTypes.Action));

            data = (JObject)jObject["actions"][0]["data"];

            Assert.AreSame(jObject, batch[0].Attachments[1].Content, "New Adaptive Card JObject reference was assigned");
            Assert.AreNotEqual(BATCHID, data.GetIdFromActionData(DataIdTypes.Batch), "Batch ID was not generated");
            Assert.AreEqual(CAROUSELID, data.GetIdFromActionData(DataIdTypes.Carousel), "Carousel ID was not applied");
            Assert.IsNull(data.GetIdFromActionData(DataIdTypes.Card), "Card ID was applied");
            Assert.AreEqual(ACTIONID, data.GetIdFromActionData(DataIdTypes.Action), "Action ID was generated/applied");

            heroCard = (HeroCard)batch[1].Attachments.Single().Content;
            data = (JObject)heroCard.Buttons.Single().Value;

            Assert.AreEqual("{}", heroCard.Buttons.Single().Text);
            Assert.IsNull(data.GetIdFromActionData(DataIdTypes.Batch));
            Assert.IsNull(data.GetIdFromActionData(DataIdTypes.Carousel));
            Assert.IsNull(data.GetIdFromActionData(DataIdTypes.Card));
            Assert.IsNull(data.GetIdFromActionData(DataIdTypes.Action));

            // Empty options shouldn't apply any ID's

            batch = new List<IMessageActivity>
            {
                MessageFactory.Attachment(new HeroCard(buttons: new List<CardAction>
                {
                    new CardAction(ActionTypes.PostBack, value: new JObject()),
                }).ToAttachment()),
            };

            batch.ApplyIdsToBatch(new DataIdOptions());

            heroCard = (HeroCard)batch.Single().Attachments.Single().Content;
            data = (JObject)heroCard.Buttons.Single().Value;

            Assert.IsNull(data.GetIdFromActionData(DataIdTypes.Batch));
            Assert.IsNull(data.GetIdFromActionData(DataIdTypes.Carousel));
            Assert.IsNull(data.GetIdFromActionData(DataIdTypes.Card));
            Assert.IsNull(data.GetIdFromActionData(DataIdTypes.Action));

            // Null options should default to applying an action ID

            batch = new List<IMessageActivity>
            {
                MessageFactory.Attachment(new HeroCard(buttons: new List<CardAction>
                {
                    new CardAction(ActionTypes.MessageBack, value: new JObject()),
                }).ToAttachment()),
            };

            batch.ApplyIdsToBatch();

            heroCard = (HeroCard)batch.Single().Attachments.Single().Content;
            data = (JObject)heroCard.Buttons.Single().Value;

            Assert.IsNull(data.GetIdFromActionData(DataIdTypes.Batch));
            Assert.IsNull(data.GetIdFromActionData(DataIdTypes.Carousel));
            Assert.IsNull(data.GetIdFromActionData(DataIdTypes.Card));
            Assert.IsNotNull(data.GetIdFromActionData(DataIdTypes.Action));

            // Null batch should throw an exception

            batch = null;

            Assert.ThrowsException<ArgumentNullException>(() => batch.ApplyIdsToBatch(options));
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

            var adaptiveCard = new AdaptiveCard(new AdaptiveSchemaVersion())
            {
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Data = new Dictionary<string, string>
                        {
                            { DataId.GetKey(DataIdTypes.Action), ACTIONID },
                        },
                    },
                },
            };

            var batch = new List<IMessageActivity>
            {
                MessageFactory.Carousel(new List<Attachment>
                {
                    new Attachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = adaptiveCard,
                    },
                    new AnimationCard(buttons: new List<CardAction>
                    {
                        new CardAction(ActionTypes.PostBack, value: new Dictionary<string, string>
                        {
                            { DataId.GetKey(DataIdTypes.Card), CARDID },
                        }),
                    }).ToAttachment(),
                    new Attachment
                    {
                        ContentType = ContentTypes.AdaptiveCard,
                        Content = new AudioCard(buttons: new List<CardAction>
                        {
                            new CardAction(ActionTypes.PostBack, value: new Dictionary<string, string>
                            {
                                { DataId.GetKey(DataIdTypes.Card), CARDID2 },
                            }),
                        }),
                    },
                    new Attachment
                    {
                        ContentType = HeroCard.ContentType, // Content does not match content type
                        Content = new OAuthCard(buttons: new List<CardAction>
                        {
                            new CardAction(ActionTypes.PostBack, value: new Dictionary<string, string>
                            {
                                { DataId.GetKey(DataIdTypes.Card), CAROUSELID },
                            }),
                        }),
                    },
                }),
                MessageFactory.Attachment(new AudioCard(buttons: new List<CardAction>
                {
                    new CardAction(ActionTypes.ImBack, value: new Dictionary<string, string>
                    {
                        { DataId.GetKey(DataIdTypes.Carousel), CAROUSELID2 },
                    }),
                    new CardAction(ActionTypes.PostBack, value: new Dictionary<string, string>
                    {
                        { DataId.GetKey(DataIdTypes.Action), ACTIONID2 },
                        { DataId.GetKey(DataIdTypes.Batch), BATCHID },
                    }),
                    new CardAction(ActionTypes.MessageBack, value: new Dictionary<string, string>
                    {
                        { DataId.GetKey(DataIdTypes.Action), ACTIONID },
                        { DataId.GetKey(DataIdTypes.Batch), BATCHID2 },
                    }),
                }).ToAttachment()),
            };

            var ids = batch.GetIdsFromBatch();

            Assert.AreSame(adaptiveCard, batch[0].Attachments[0].Content, "Adaptive Card reference was broken");
            Assert.AreEqual(5, ids.Count);
            Assert.IsTrue(ids.Contains(new DataId(DataIdTypes.Action, ACTIONID)));
            Assert.IsTrue(ids.Contains(new DataId(DataIdTypes.Action, ACTIONID2)));
            Assert.IsTrue(ids.Contains(new DataId(DataIdTypes.Card, CARDID)));
            Assert.IsFalse(ids.Contains(new DataId(DataIdTypes.Card, CARDID2)));
            Assert.IsFalse(ids.Contains(new DataId(DataIdTypes.Carousel, CAROUSELID)));
            Assert.IsFalse(ids.Contains(new DataId(DataIdTypes.Carousel, CAROUSELID2)));
            Assert.IsTrue(ids.Contains(new DataId(DataIdTypes.Batch, BATCHID)));
            Assert.IsTrue(ids.Contains(new DataId(DataIdTypes.Batch, BATCHID2)));

            batch = null;

            Assert.ThrowsException<ArgumentNullException>(() => batch.GetIdsFromBatch());
        }

        [TestMethod]
        public void TestAdaptOutgoingCardActions()
        {
            var notJson = "Not JSON";
            var json = "{'foo':'bar'}";
            var parsedJson = JObject.Parse(json);
            var obj = new { };
            var serializedObj = JsonConvert.SerializeObject(obj);

            var actionTypes = new List<string>
            {
                ActionTypes.MessageBack,
                ActionTypes.PostBack,
                ActionTypes.ImBack,
                ActionTypes.OpenUrl,
            };

            var channels = new List<string>
            {
                Channels.Cortana,
                Channels.Directline,
                Channels.DirectlineSpeech,
                Channels.Email,
                Channels.Emulator,
                Channels.Facebook,
                Channels.Groupme,
                Channels.Kik,
                Channels.Line,
                Channels.Msteams,
                Channels.Skype,
                Channels.Skypeforbusiness,
                Channels.Slack,
                Channels.Sms,
                Channels.Telegram,
                Channels.Test,
                Channels.Twilio,
                Channels.Webchat,
            };

            var modifiedCardActions = new Dictionary<ActionMod, IEnumerable<CardAction>>
            {
                {
                    ActionMod.None,
                    new List<CardAction>
                    {
                        new CardAction(),
                        new CardAction(value: obj),
                        new CardAction(value: json),
                        new CardAction(value: notJson),
                        new CardAction(text: json),
                        new CardAction(text: notJson),
                        new CardAction(value: obj, text: json),
                        new CardAction(value: obj, text: notJson),
                        new CardAction(value: json, text: json),
                        new CardAction(value: json, text: notJson),
                        new CardAction(value: notJson, text: json),
                        new CardAction(value: notJson, text: notJson),
                    }
                },
                {
                    ActionMod.EnsuredText,
                    new List<CardAction>
                    {
                        new CardAction(),
                        new CardAction(value: obj, text: serializedObj),
                        new CardAction(value: json, text: json),
                        new CardAction(value: notJson, text: notJson),
                        new CardAction(text: json),
                        new CardAction(text: notJson),
                        new CardAction(value: obj, text: json),
                        new CardAction(value: obj, text: notJson),
                        new CardAction(value: json, text: json),
                        new CardAction(value: json, text: notJson),
                        new CardAction(value: notJson, text: json),
                        new CardAction(value: notJson, text: notJson),
                    }
                },
                {
                    ActionMod.EnsuredValue,
                    new List<CardAction>
                    {
                        new CardAction(),
                        new CardAction(value: obj),
                        new CardAction(value: json),
                        new CardAction(value: notJson),
                        new CardAction(text: json, value: json),
                        new CardAction(text: notJson, value: notJson),
                        new CardAction(value: obj, text: json),
                        new CardAction(value: obj, text: notJson),
                        new CardAction(value: json, text: json),
                        new CardAction(value: json, text: notJson),
                        new CardAction(value: notJson, text: json),
                        new CardAction(value: notJson, text: notJson),
                    }
                },
                {
                    ActionMod.EnsuredStringValue,
                    new List<CardAction>
                    {
                        new CardAction(),
                        new CardAction(value: serializedObj),
                        new CardAction(value: json),
                        new CardAction(value: notJson),
                        new CardAction(text: json, value: json),
                        new CardAction(text: notJson, value: notJson),
                        new CardAction(value: serializedObj, text: json),
                        new CardAction(value: serializedObj, text: notJson),
                        new CardAction(value: json, text: json),
                        new CardAction(value: json, text: notJson),
                        new CardAction(value: notJson, text: json),
                        new CardAction(value: notJson, text: notJson),
                    }
                },
                {
                    ActionMod.EnsuredObjectValue,
                    new List<CardAction>
                    {
                        new CardAction(),
                        new CardAction(value: obj),
                        new CardAction(value: parsedJson),
                        new CardAction(value: notJson),
                        new CardAction(text: json, value: parsedJson),
                        new CardAction(text: notJson),
                        new CardAction(value: obj, text: json),
                        new CardAction(value: obj, text: notJson),
                        new CardAction(value: parsedJson, text: json),
                        new CardAction(value: parsedJson, text: notJson),
                        new CardAction(value: parsedJson, text: json),
                        new CardAction(value: notJson, text: notJson),
                    }
                },
            };

            IList<Attachment> GenerateAttachments() => new List<Attachment>
            {
                new HeroCard(
                    buttons: actionTypes
                        .SelectMany(actionType => modifiedCardActions[ActionMod.None]
                            .Select(action => new CardAction(
                                actionType,
                                value: action.Value,
                                text: action.Text)))
                        .ToList())
                    .ToAttachment(),
            };

            var batch = channels.Select(channel => new Activity(channelId: channel, attachments: GenerateAttachments())).ToList();

            batch.AdaptOutgoingCardActions();

            Assert.AreEqual(channels.Count, batch.Count);

            int activityIndex = 0;

            void CheckChannelActions(
                string channelId,
                ActionMod messageBackMod,
                ActionMod postBackMod,
                ActionMod imBackMod)
            {
                var modifications = new Dictionary<string, ActionMod>
                {
                    { ActionTypes.MessageBack, messageBackMod },
                    { ActionTypes.PostBack, postBackMod },
                    { ActionTypes.ImBack, imBackMod },
                    { ActionTypes.OpenUrl, ActionMod.None },
                };

                var activity = batch[activityIndex];

                Assert.AreEqual(channelId, activity.ChannelId);

                var buttons = ((HeroCard)activity.Attachments.Single().Content).Buttons;
                var buttonIndex = 0;

                Assert.AreEqual(48, buttons.Count);

                foreach (var actionType in actionTypes)
                {
                    var expectedType = actionType;

                    if (modifications[expectedType] == ActionMod.ConvertedToPostBack)
                    {
                        expectedType = ActionTypes.PostBack;
                    }

                    var modification = modifications[expectedType];
                    var expectedActions = modifiedCardActions[modification];

                    Assert.AreEqual(12, expectedActions.Count());

                    foreach (var expectedAction in expectedActions)
                    {
                        var actualAction = buttons[buttonIndex];
                        var expectedValue = expectedAction.Value;
                        var actualValue = actualAction.Value;

                        Assert.AreEqual(expectedType, actualAction.Type, $"Wrong type for {channelId} action #{buttonIndex}");
                        Assert.AreEqual(expectedAction.Text, actualAction.Text, $"Wrong text for {channelId} action #{buttonIndex}");

                        if (expectedValue is JObject)
                        {
                            Assert.IsTrue(JToken.DeepEquals(parsedJson, (JObject)actualValue), $"Wrong JObject for {channelId} action #{buttonIndex}");
                        }
                        else
                        {
                            Assert.AreEqual(expectedValue, actualValue, $"Wrong value for {channelId} action #{buttonIndex}");
                        }

                        buttonIndex++;
                    }
                }

                activityIndex++;
            }

            CheckChannelActions(Channels.Cortana, ActionMod.ConvertedToPostBack, ActionMod.EnsuredStringValue, ActionMod.EnsuredStringValue);
            CheckChannelActions(Channels.Directline, ActionMod.EnsuredValue, ActionMod.EnsuredValue, ActionMod.EnsuredStringValue);
            CheckChannelActions(Channels.DirectlineSpeech, ActionMod.None, ActionMod.None, ActionMod.None);
            CheckChannelActions(Channels.Email, ActionMod.EnsuredText, ActionMod.EnsuredValue, ActionMod.EnsuredValue);
            CheckChannelActions(Channels.Emulator, ActionMod.EnsuredValue, ActionMod.EnsuredValue, ActionMod.EnsuredStringValue);
            CheckChannelActions(Channels.Facebook, ActionMod.EnsuredStringValue, ActionMod.EnsuredStringValue, ActionMod.EnsuredStringValue);
            CheckChannelActions(Channels.Groupme, ActionMod.None, ActionMod.None, ActionMod.None);
            CheckChannelActions(Channels.Kik, ActionMod.None, ActionMod.None, ActionMod.None);
            CheckChannelActions(Channels.Line, ActionMod.EnsuredValue, ActionMod.EnsuredValue, ActionMod.EnsuredValue);
            CheckChannelActions(Channels.Msteams, ActionMod.EnsuredObjectValue, ActionMod.EnsuredObjectValue, ActionMod.EnsuredStringValue);
            CheckChannelActions(Channels.Skype, ActionMod.ConvertedToPostBack, ActionMod.EnsuredValue, ActionMod.EnsuredStringValue);
            CheckChannelActions(Channels.Skypeforbusiness, ActionMod.None, ActionMod.None, ActionMod.None);
            CheckChannelActions(Channels.Slack, ActionMod.EnsuredText, ActionMod.EnsuredStringValue, ActionMod.EnsuredStringValue);
            CheckChannelActions(Channels.Sms, ActionMod.None, ActionMod.None, ActionMod.None);
            CheckChannelActions(Channels.Telegram, ActionMod.EnsuredText, ActionMod.EnsuredStringValue, ActionMod.EnsuredStringValue);
            CheckChannelActions(Channels.Test, ActionMod.None, ActionMod.None, ActionMod.None);
            CheckChannelActions(Channels.Twilio, ActionMod.None, ActionMod.None, ActionMod.None);
            CheckChannelActions(Channels.Webchat, ActionMod.EnsuredValue, ActionMod.EnsuredValue, ActionMod.EnsuredStringValue);

            Assert.AreEqual(channels.Count, activityIndex);

            batch = new List<Activity>
            {
                new Activity(channelId: Channels.Directline, attachments: GenerateAttachments()),
                new Activity(channelId: Channels.DirectlineSpeech, attachments: GenerateAttachments()),
            };

            activityIndex = 0;

            batch.AdaptOutgoingCardActions(Channels.Msteams);

            CheckChannelActions(Channels.Directline, ActionMod.EnsuredObjectValue, ActionMod.EnsuredObjectValue, ActionMod.EnsuredStringValue);
            CheckChannelActions(Channels.DirectlineSpeech, ActionMod.EnsuredObjectValue, ActionMod.EnsuredObjectValue, ActionMod.EnsuredStringValue);

            Assert.AreEqual(2, activityIndex);

            batch = null;

            Assert.ThrowsException<ArgumentNullException>(() => batch.AdaptOutgoingCardActions());
        }

        [TestMethod]
        public void TestGetIncomingActionData()
        {
            var adapter = new TestAdapter();
            var json = "{'foo':'bar'}";
            var notJson = "Not JSON";
            var parsedJson = json.TryParseJObject();

            ITurnContext GenerateTurnContext(
                string channelId = null,
                object value = null,
                string text = null,
                object channelData = null,
                string entityType = null,
                string type = ActivityTypes.Message)
            {
                var activity = new Activity
                {
                    ChannelId = channelId,
                    Value = value,
                    Text = text,
                    ChannelData = channelData,
                    Entities = entityType is null ? null : new List<Entity> { new Entity(entityType) },
                    Type = type,
                };

                return new TurnContext(adapter, activity);
            }

            static object GenerateChannelData(string key, object value = null) => new Dictionary<string, object> { { key, value } };

            // CORTANA / TURN STATE CACHE (note that the data is no longer cached)

            var turnContext = GenerateTurnContext(Channels.Cortana, null, json, null, "Non-intent");
            var incomingData = turnContext.GetIncomingActionData();

            Assert.IsTrue(JToken.DeepEquals(parsedJson, (JObject)incomingData));

            var newIncomingData = turnContext.GetIncomingActionData();

            Assert.IsTrue(JToken.DeepEquals((JObject)incomingData, (JObject)newIncomingData));
            Assert.AreNotSame(incomingData, newIncomingData, "Cached data was used");

            turnContext = GenerateTurnContext(Channels.Cortana, json, json);

            Assert.IsTrue(JToken.DeepEquals(parsedJson, (JObject)turnContext.GetIncomingActionData()));

            turnContext = GenerateTurnContext(Channels.Cortana, json, notJson);

            Assert.IsNull(turnContext.GetIncomingActionData());

            turnContext = GenerateTurnContext(Channels.Cortana, parsedJson, json, null, EntityTypes.Intent);

            Assert.AreSame(parsedJson, turnContext.GetIncomingActionData());

            // DIRECTLINE / EMULATOR / WEBCHAT

            turnContext = GenerateTurnContext(Channels.Directline, null, json);

            Assert.IsNull(turnContext.GetIncomingActionData());

            turnContext = GenerateTurnContext(Channels.Directline, parsedJson, json);

            Assert.AreSame(parsedJson, turnContext.GetIncomingActionData());

            turnContext = GenerateTurnContext(Channels.Emulator, null, notJson, GenerateChannelData(ChannelData.PostBack));

            Assert.IsNull(turnContext.GetIncomingActionData());

            turnContext = GenerateTurnContext(Channels.Emulator, parsedJson, notJson, GenerateChannelData(ChannelData.PostBack));

            Assert.AreSame(parsedJson, turnContext.GetIncomingActionData());

            turnContext = GenerateTurnContext(Channels.Directline, json, null, GenerateChannelData(ChannelData.PostBack));

            Assert.IsTrue(JToken.DeepEquals(parsedJson, (JObject)turnContext.GetIncomingActionData()));

            turnContext = GenerateTurnContext(Channels.Webchat, null, json, GenerateChannelData(ChannelData.MessageBack));

            Assert.IsTrue(JToken.DeepEquals(parsedJson, (JObject)turnContext.GetIncomingActionData()));

            turnContext = GenerateTurnContext(Channels.Webchat, null, json, JsonConvert.SerializeObject(GenerateChannelData(ChannelData.MessageBack)));

            Assert.IsTrue(JToken.DeepEquals(parsedJson, (JObject)turnContext.GetIncomingActionData()));

            // KIK

            void AssertBasedOnChannelData(string channelId, string channelDataProperty)
            {
                turnContext = GenerateTurnContext(channelId, parsedJson, json);

                Assert.AreSame(parsedJson, turnContext.GetIncomingActionData());

                turnContext = GenerateTurnContext(channelId, null, notJson, GenerateChannelData(channelDataProperty));

                Assert.IsNull(turnContext.GetIncomingActionData());

                turnContext = GenerateTurnContext(channelId, null, json, GenerateChannelData(channelDataProperty));

                Assert.IsTrue(JToken.DeepEquals(parsedJson, (JObject)turnContext.GetIncomingActionData()));
            }

            AssertBasedOnChannelData(Channels.Kik, ChannelData.Metadata);

            // LINE

            AssertBasedOnChannelData(Channels.Line, ChannelData.LinePostback);

            // SKYPE

            turnContext = GenerateTurnContext(Channels.Skype, parsedJson);

            Assert.AreSame(parsedJson, turnContext.GetIncomingActionData());

            turnContext = GenerateTurnContext(Channels.Skype, null, json, GenerateChannelData(ChannelData.Text, json));

            Assert.IsNull(turnContext.GetIncomingActionData());

            turnContext = GenerateTurnContext(Channels.Skype, null, notJson, GenerateChannelData(ChannelData.Text, json));

            Assert.IsNull(turnContext.GetIncomingActionData());

            turnContext = GenerateTurnContext(Channels.Skype, null, json, GenerateChannelData(ChannelData.Text, notJson));

            Assert.IsTrue(JToken.DeepEquals(parsedJson, (JObject)turnContext.GetIncomingActionData()));

            // SLACK

            AssertBasedOnChannelData(Channels.Slack, ChannelData.Payload);

            // TELEGRAM

            AssertBasedOnChannelData(Channels.Telegram, ChannelData.CallbackQuery);

            // ANY CHANNELS NOT LISTED

            turnContext = GenerateTurnContext(Channels.Test, null, json);

            Assert.IsNull(turnContext.GetIncomingActionData());

            turnContext = GenerateTurnContext(Channels.DirectlineSpeech, notJson);

            Assert.IsNull(turnContext.GetIncomingActionData());

            turnContext = GenerateTurnContext(Channels.Email, json);

            Assert.IsTrue(JToken.DeepEquals(parsedJson, (JObject)turnContext.GetIncomingActionData()));

            turnContext = GenerateTurnContext(Channels.Sms, parsedJson);

            Assert.AreSame(parsedJson, turnContext.GetIncomingActionData());

            turnContext = GenerateTurnContext(Channels.Console, parsedJson, type: ActivityTypes.Event);

            Assert.IsNull(turnContext.GetIncomingActionData(), "Action data was returned for a non-message activity");

            turnContext = null;

            Assert.ThrowsException<ArgumentNullException>(() => turnContext.GetIncomingActionData());
        }
    }
}
