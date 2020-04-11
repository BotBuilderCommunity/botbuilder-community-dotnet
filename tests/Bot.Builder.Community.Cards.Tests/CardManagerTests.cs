using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdaptiveCards;
using Bot.Builder.Community.Cards.Management;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards.Tests
{
    [TestClass]
    public class CardManagerTests
    {
        [TestMethod]
        public void TestCreate()
        {
            var name = "Custom card manager state";
            var manager = CardManager.Create(CreateUserState().CreateProperty<CardManagerState>(name));

            Assert.AreEqual(name, manager.StateAccessor.Name);
            Assert.ThrowsException<ArgumentNullException>(() => CardManager.Create(null));
        }

        // --------------------
        // NON-UPDATING METHODS
        // --------------------

        [TestMethod]
        public async Task TestEnableIdAsync()
        {
            var manager = CreateManager();
            var turnContext = CreateTurnContext();
            var payloadId = new PayloadItem(PayloadIdTypes.Action, "action ID");

            Assert.IsNull(await manager.StateAccessor.GetAsync(turnContext));

            await manager.EnableIdAsync(turnContext, payloadId);

            var state = await manager.StateAccessor.GetAsync(turnContext);
            var actionIds = state.PayloadIdsByType[PayloadIdTypes.Action];

            Assert.AreEqual(payloadId.Value, actionIds.Single());

            await manager.EnableIdAsync(turnContext, new PayloadItem(PayloadIdTypes.Action, "different action ID"), TrackingStyle.TrackDisabled);

            Assert.AreEqual(payloadId.Value, actionIds.Single());

            await manager.EnableIdAsync(turnContext, payloadId, TrackingStyle.TrackDisabled);

            Assert.AreEqual(0, actionIds.Count);

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await manager.EnableIdAsync(turnContext, null));
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await manager.EnableIdAsync(null, payloadId));
        }

        [TestMethod]
        public async Task TestDisableIdAsync()
        {
            var manager = CreateManager();
            var turnContext = CreateTurnContext();
            var payloadId = new PayloadItem(PayloadIdTypes.Card, "card ID");

            Assert.IsNull(await manager.StateAccessor.GetAsync(turnContext));

            await manager.DisableIdAsync(turnContext, payloadId, TrackingStyle.TrackDisabled);

            var state = await manager.StateAccessor.GetAsync(turnContext);
            var actionIds = state.PayloadIdsByType[PayloadIdTypes.Card];

            Assert.AreEqual(payloadId.Value, actionIds.Single());

            await manager.DisableIdAsync(turnContext, new PayloadItem(PayloadIdTypes.Card, "different card ID"));

            Assert.AreEqual(payloadId.Value, actionIds.Single());

            await manager.DisableIdAsync(turnContext, payloadId);

            Assert.AreEqual(0, actionIds.Count);

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await manager.DisableIdAsync(turnContext, null));
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await manager.DisableIdAsync(null, payloadId));
        }

        [TestMethod]
        public async Task TestTrackIdAsync()
        {
            var manager = CreateManager();
            var turnContext = CreateTurnContext();
            var payloadId = new PayloadItem(PayloadIdTypes.Carousel, "carousel ID");

            Assert.IsNull(await manager.StateAccessor.GetAsync(turnContext));

            await manager.TrackIdAsync(turnContext, payloadId);

            var state = await manager.StateAccessor.GetAsync(turnContext);
            var actionIds = state.PayloadIdsByType[PayloadIdTypes.Carousel];

            Assert.AreEqual(payloadId.Value, actionIds.Single());

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await manager.TrackIdAsync(turnContext, null));
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await manager.TrackIdAsync(null, payloadId));
        }

        [TestMethod]
        public async Task TestForgetIdAsync()
        {
            var manager = CreateManager();
            var turnContext = CreateTurnContext();
            var payloadId = new PayloadItem(PayloadIdTypes.Batch, "batch ID");

            Assert.IsNull(await manager.StateAccessor.GetAsync(turnContext));

            await manager.ForgetIdAsync(turnContext, payloadId);

            var state = await manager.StateAccessor.GetAsync(turnContext);

            Assert.AreEqual(0, state.PayloadIdsByType.Count);

            var actionIds = state.PayloadIdsByType[PayloadIdTypes.Batch] = new HashSet<string> { payloadId.Value };

            await manager.ForgetIdAsync(turnContext, new PayloadItem(PayloadIdTypes.Batch, "different batch ID"));

            Assert.AreEqual(payloadId.Value, actionIds.Single());

            await manager.ForgetIdAsync(turnContext, payloadId);

            Assert.AreEqual(0, actionIds.Count);

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await manager.ForgetIdAsync(turnContext, null));
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await manager.ForgetIdAsync(null, payloadId));
        }

        [TestMethod]
        public async Task TestClearTrackedIdsAsync()
        {
            var manager = CreateManager();
            var turnContext = CreateTurnContext();

            Assert.IsNull(await manager.StateAccessor.GetAsync(turnContext));

            await manager.ClearTrackedIdsAsync(turnContext);

            var state = await manager.StateAccessor.GetAsync(turnContext);

            Assert.AreEqual(0, state.PayloadIdsByType.Count);

            state.PayloadIdsByType[PayloadIdTypes.Action] = new HashSet<string> { "action ID" };
            state.PayloadIdsByType[PayloadIdTypes.Card] = new HashSet<string> { "card ID" };
            state.PayloadIdsByType[PayloadIdTypes.Carousel] = new HashSet<string> { "carousel ID" };
            state.PayloadIdsByType[PayloadIdTypes.Batch] = new HashSet<string> { "batch ID" };

            Assert.AreEqual(4, state.PayloadIdsByType.Count);

            await manager.ClearTrackedIdsAsync(turnContext);

            Assert.AreEqual(0, state.PayloadIdsByType.Count);

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await manager.ClearTrackedIdsAsync(null));
        }

        // ----------------
        // UPDATING METHODS
        // ----------------

        [TestMethod]
        public async Task TestSaveActivitiesAsync()
        {
            var manager = CreateManager();
            var turnContext = CreateTurnContext();

            var value = new Dictionary<string, object>
            {
                { "foo", "bar" }, // No ID's
            };

            var activities = new List<IMessageActivity>
            {
                MessageFactory.Attachment(new HeroCard(buttons: new List<CardAction>
                {
                    new CardAction(type: ActionTypes.PostBack, value: value),
                }).ToAttachment()),
            };

            Assert.IsNull(await manager.StateAccessor.GetAsync(turnContext));

            await manager.SaveActivitiesAsync(turnContext, activities);

            var state = await manager.StateAccessor.GetAsync(turnContext);

            Assert.AreEqual(0, state.SavedActivities.Count, "An activity was saved despite having no payload ID's");

            value[PayloadIdTypes.GetKey(PayloadIdTypes.Action)] = "action ID";

            await manager.SaveActivitiesAsync(turnContext, activities);

            Assert.AreSame(activities.Single(), state.SavedActivities.Single());

            await manager.SaveActivitiesAsync(turnContext, activities);

            Assert.AreEqual(1, state.SavedActivities.Count, "One activity was saved as multiple activities");

            var activities2 = new List<IMessageActivity>
            {
                new Activity(),
                MessageFactory.Attachment(new Attachment
                {
                    ContentType = CardConstants.AdaptiveCardContentType,
                    Content = new AdaptiveCard(new AdaptiveSchemaVersion())
                    {
                        Actions = new List<AdaptiveAction>
                        {
                            new AdaptiveSubmitAction
                            {
                                Data = new Dictionary<string, string>
                                {
                                    { PayloadIdTypes.GetKey(PayloadIdTypes.Card), "card ID" },
                                },
                            },
                        },
                    },
                }),
                MessageFactory.Attachment(new Attachment
                {
                    ContentType = HeroCard.ContentType,
                    Content = new AdaptiveCard(new AdaptiveSchemaVersion())
                    {
                        Actions = new List<AdaptiveAction>
                        {
                            new AdaptiveSubmitAction
                            {
                                Data = new Dictionary<string, string>
                                {
                                    { PayloadIdTypes.GetKey(PayloadIdTypes.Carousel), "carousel ID" },
                                },
                            },
                        },
                    },
                }),
                MessageFactory.Carousel(new List<Attachment>
                {
                    new Attachment
                    {
                        ContentType = HeroCard.ContentType,
                        Content = new AdaptiveCard(new AdaptiveSchemaVersion())
                        {
                            Actions = new List<AdaptiveAction>
                            {
                                new AdaptiveSubmitAction
                                {
                                    Data = new Dictionary<string, string>
                                    {
                                        { PayloadIdTypes.GetKey(PayloadIdTypes.Carousel), "carousel ID" },
                                    },
                                },
                            },
                        },
                    },
                    new AnimationCard(buttons: new List<CardAction>
                    {
                        new CardAction(type: ActionTypes.PostBack, value: $"{{'{PayloadIdTypes.GetKey(PayloadIdTypes.Batch)}':'batch ID'}}"),
                    }).ToAttachment(),
                }),
            };

            await manager.SaveActivitiesAsync(turnContext, activities2);

            Assert.AreEqual(3, state.SavedActivities.Count);
            Assert.IsTrue(state.SavedActivities.Contains(activities.Single()));
            Assert.IsTrue(state.SavedActivities.Contains(activities2[1]));
            Assert.IsTrue(state.SavedActivities.Contains(activities2[3]));

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await manager.SaveActivitiesAsync(turnContext, null));
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await manager.SaveActivitiesAsync(null, activities));
        }

        [TestMethod]
        public async Task TestPreserveValuesAsync()
        {
            const string ACTIONID = "action ID";
            const string CARDID = "card ID";
            const string CAROUSELID = "carousel ID";
            const string BATCHID = "batch ID";
            const string CHOICEINPUTID = "choice set input ID";
            const string DATEINPUTID = "date input ID";
            const string NUMBERINPUTID = "number input ID";
            const string TEXTINPUTID = "text input ID";
            const string TIMEINPUTID = "time input ID";
            const string TOGGLEINPUTID = "toggle input ID";
            const string USERENTEREDCHOICE = "User-entered choice";
            const string USERENTEREDDATE = "User-entered date";
            const double USERENTEREDNUMBER1 = 1;
            const double USERENTEREDNUMBER2 = 2;
            const string USERENTEREDTEXT = "User-entered text";
            const string USERENTEREDTIME = "User-entered time";
            const string USERENTEREDTOGGLE = "User-entered toggle";

            var manager = CreateManager();
            var turnContext = CreateTurnContext();
            var state = new CardManagerState();
            var updated = false;

            var adaptiveCardJObject = JObject.FromObject(new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveChoiceSetInput
                    {
                        Id = CHOICEINPUTID,
                    },
                },
                SelectAction = new AdaptiveSubmitAction
                {
                    Data = new Dictionary<string, string>
                    {
                        { PayloadIdTypes.GetKey(PayloadIdTypes.Batch), BATCHID },
                    },
                },
            });

            var activity1 = MessageFactory.Carousel(new List<Attachment>
            {
                new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = new AdaptiveCard("1.0")
                    {
                        Body = new List<AdaptiveElement>
                        {
                            new AdaptiveChoiceSetInput
                            {
                                Id = CHOICEINPUTID,
                            },
                            new AdaptiveDateInput
                            {
                                Id = DATEINPUTID,
                            },
                            new AdaptiveNumberInput
                            {
                                Id = NUMBERINPUTID,
                            },
                            new AdaptiveTextInput
                            {
                                Id = TEXTINPUTID,
                            },
                            new AdaptiveTimeInput
                            {
                                Id = TIMEINPUTID,
                            },
                            new AdaptiveToggleInput
                            {
                                Id = TOGGLEINPUTID,
                                Title = "toggle",
                            },
                        },
                        Actions = new List<AdaptiveAction>
                        {
                            new AdaptiveSubmitAction
                            {
                                Data = new Dictionary<string, string>
                                {
                                    { PayloadIdTypes.GetKey(PayloadIdTypes.Card), CARDID },
                                    { PayloadIdTypes.GetKey(PayloadIdTypes.Carousel), CAROUSELID },
                                    { PayloadIdTypes.GetKey(PayloadIdTypes.Batch), BATCHID },
                                },
                            },
                        },
                    },
                },
                new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = new AdaptiveCard("1.1")
                    {
                        Body = new List<AdaptiveElement>
                        {
                            new AdaptiveChoiceSetInput
                            {
                                Id = CHOICEINPUTID,
                            },
                        },
                        SelectAction = new AdaptiveSubmitAction(),
                    },
                },
                new Attachment
                {
                    // No content type
                    Content = new AdaptiveCard("1.2")
                    {
                        Body = new List<AdaptiveElement>
                        {
                            new AdaptiveDateInput
                            {
                                Id = DATEINPUTID,
                            },
                        },
                        SelectAction = new AdaptiveSubmitAction(),
                    },
                },
                new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = new AdaptiveCard("1.1")
                    {
                        Body = new List<AdaptiveElement>
                        {
                            new AdaptiveNumberInput
                            {
                                Id = NUMBERINPUTID,
                            },
                            new AdaptiveTextInput
                            {
                                Id = TEXTINPUTID,
                            },
                        },
                        SelectAction = new AdaptiveSubmitAction(),
                    },
                },
                new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = new AdaptiveCard("1.0")
                    {
                        Body = new List<AdaptiveElement>
                        {
                            new AdaptiveTextInput
                            {
                                Id = TEXTINPUTID,
                            },
                        },
                        SelectAction = new AdaptiveSubmitAction
                        {
                            Data = new Dictionary<string, double>
                            {
                                { NUMBERINPUTID, USERENTEREDNUMBER1 },
                            },
                        },
                    },
                },
                new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = new AdaptiveCard("1.1")
                    {
                        Body = new List<AdaptiveElement>
                        {
                            new AdaptiveTimeInput
                            {
                                Id = TIMEINPUTID,
                            },
                        },
                        SelectAction = new AdaptiveOpenUrlAction
                        {
                            UrlString = "https://adaptivecards.io/",
                        },
                    },
                },
                new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = new AdaptiveCard("1.2")
                    {
                        SelectAction = new AdaptiveSubmitAction
                        {
                            Data = new Dictionary<string, string>
                            {
                                { PayloadIdTypes.GetKey(PayloadIdTypes.Action), ACTIONID },
                            },
                        },
                    },
                },
                new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = new AdaptiveCard("1.1")
                    {
                        Body = new List<AdaptiveElement>
                        {
                            new AdaptiveToggleInput
                            {
                                Id = TOGGLEINPUTID,
                                Title = "toggle",
                            },
                        },
                        SelectAction = new AdaptiveSubmitAction(),
                    },
                },
                new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = new AdaptiveCard("1.0")
                    {
                        Body = new List<AdaptiveElement>
                        {
                            new AdaptiveToggleInput
                            {
                                Id = TOGGLEINPUTID,
                                Title = "toggle",
                            },
                        },
                        SelectAction = new AdaptiveSubmitAction
                        {
                            Data = new Dictionary<string, string>
                            {
                                { PayloadIdTypes.GetKey(PayloadIdTypes.Batch), BATCHID },
                            },
                        },
                    },
                },
            });

            var activity2 = MessageFactory.Attachment(new List<Attachment>
            {
                new HeroCard(buttons: new List<CardAction>
                {
                    new CardAction(ActionTypes.PostBack, value: new Dictionary<string, string>
                    {
                        { TOGGLEINPUTID, USERENTEREDTOGGLE },
                        { PayloadIdTypes.GetKey(PayloadIdTypes.Batch), BATCHID },
                    }),
                }).ToAttachment(),
                new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = adaptiveCardJObject,
                }
            });

            state.SavedActivities.Add(activity1);
            state.SavedActivities.Add(activity2);

            var expectedActivity = activity1;
            var body = ((AdaptiveCard)activity1.Attachments[0].Content).Body;

            turnContext.OnUpdateActivity((turnContext, activity, next) =>
            {
                Assert.AreSame(expectedActivity, activity);
                Assert.IsFalse(updated, "UpdateActivityAsync was called an extra time");

                updated = true;

                return next();
            });

            turnContext.Activity.Value = new Dictionary<string, object>
            {
                { CHOICEINPUTID, USERENTEREDCHOICE },
                { DATEINPUTID, USERENTEREDDATE },
                { NUMBERINPUTID, USERENTEREDNUMBER1 },
                { TEXTINPUTID, USERENTEREDTEXT },
                { TIMEINPUTID, USERENTEREDTIME },
                { TOGGLEINPUTID, USERENTEREDTOGGLE },
                { PayloadIdTypes.GetKey(PayloadIdTypes.Action), ACTIONID },
                { PayloadIdTypes.GetKey(PayloadIdTypes.Card), CARDID },
                { PayloadIdTypes.GetKey(PayloadIdTypes.Carousel), CAROUSELID },
                { PayloadIdTypes.GetKey(PayloadIdTypes.Batch), BATCHID },
            };

            await manager.StateAccessor.SetAsync(turnContext, state);
            await manager.PreserveValuesAsync(turnContext);

            Assert.IsFalse(updated, "Payload matched with additional field");

            turnContext.Activity.Value = new Dictionary<string, object>
            {
                { CHOICEINPUTID, USERENTEREDCHOICE },
                { DATEINPUTID, USERENTEREDDATE },
                { NUMBERINPUTID, USERENTEREDNUMBER1 },
                { TEXTINPUTID, USERENTEREDTEXT },
                { TIMEINPUTID, USERENTEREDTIME },
                { PayloadIdTypes.GetKey(PayloadIdTypes.Card), CARDID },
                { PayloadIdTypes.GetKey(PayloadIdTypes.Carousel), CAROUSELID },
                { PayloadIdTypes.GetKey(PayloadIdTypes.Batch), BATCHID },
            };

            await manager.PreserveValuesAsync(turnContext);

            Assert.IsFalse(updated, "Payload matched with missing field");

            turnContext.Activity.Value = new Dictionary<string, object>
            {
                { CHOICEINPUTID, USERENTEREDCHOICE },
                { DATEINPUTID, USERENTEREDDATE },
                { NUMBERINPUTID, USERENTEREDNUMBER1 },
                { TEXTINPUTID, USERENTEREDTEXT },
                { TIMEINPUTID, USERENTEREDTIME },
                { TOGGLEINPUTID, USERENTEREDTOGGLE },
                { PayloadIdTypes.GetKey(PayloadIdTypes.Card), CARDID },
                { PayloadIdTypes.GetKey(PayloadIdTypes.Carousel), CAROUSELID },
                { PayloadIdTypes.GetKey(PayloadIdTypes.Batch), BATCHID },
            };

            await manager.PreserveValuesAsync(turnContext);

            Assert.IsTrue(updated);
            Assert.AreNotSame(body, ((AdaptiveCard)activity1.Attachments[0].Content).Body, "Adaptive card body reference was not broken");

            body = ((AdaptiveCard)activity1.Attachments[0].Content).Body;

            Assert.AreEqual(USERENTEREDCHOICE, ((AdaptiveChoiceSetInput)body[0]).Value);
            Assert.AreEqual(USERENTEREDDATE, ((AdaptiveDateInput)body[1]).Value);
            Assert.AreEqual(USERENTEREDNUMBER1, ((AdaptiveNumberInput)body[2]).Value);
            Assert.AreEqual(USERENTEREDTEXT, ((AdaptiveTextInput)body[3]).Value);
            Assert.AreEqual(USERENTEREDTIME, ((AdaptiveTimeInput)body[4]).Value);
            Assert.AreEqual(USERENTEREDTOGGLE, ((AdaptiveToggleInput)body[5]).Value);
            Assert.AreNotEqual(
                USERENTEREDCHOICE,
                ((AdaptiveChoiceSetInput)((AdaptiveCard)activity1.Attachments[1].Content).Body.Single()).Value,
                "An input was updated in the wrong card");

            updated = false;

            turnContext.Activity.Value = new Dictionary<string, string>
            {
                { CHOICEINPUTID, USERENTEREDCHOICE },
            };

            await manager.PreserveValuesAsync(turnContext);

            Assert.IsTrue(updated);
            Assert.AreSame(body, ((AdaptiveCard)activity1.Attachments[0].Content).Body, "Adaptive Card body reference was broken");
            Assert.AreEqual(USERENTEREDCHOICE, ((AdaptiveChoiceSetInput)((AdaptiveCard)activity1.Attachments[1].Content).Body.Single()).Value);

            updated = false;

            turnContext.Activity.Value = new Dictionary<string, string>
            {
                { DATEINPUTID, USERENTEREDDATE },
            };

            await manager.PreserveValuesAsync(turnContext);

            Assert.IsFalse(updated, "Card was updated despite having no content type");

            turnContext.Activity.Value = new Dictionary<string, object>
            {
                { NUMBERINPUTID, USERENTEREDNUMBER1 },
                { TEXTINPUTID, USERENTEREDTEXT },
            };

            await manager.PreserveValuesAsync(turnContext);

            Assert.IsFalse(updated, "Card was updated despite having same payload as other Adaptive Card");

            turnContext.Activity.Value = new Dictionary<string, object>
            {
                { NUMBERINPUTID, USERENTEREDNUMBER2 },
                { TEXTINPUTID, USERENTEREDTEXT },
            };

            await manager.PreserveValuesAsync(turnContext);

            Assert.IsTrue(updated);
            Assert.AreEqual(USERENTEREDNUMBER2, ((AdaptiveNumberInput)((AdaptiveCard)activity1.Attachments[3].Content).Body[0]).Value);
            Assert.AreEqual(USERENTEREDTEXT, ((AdaptiveTextInput)((AdaptiveCard)activity1.Attachments[3].Content).Body[1]).Value);

            updated = false;

            turnContext.Activity.Value = new Dictionary<string, string>
            {
                { TIMEINPUTID, USERENTEREDTIME },
            };

            await manager.PreserveValuesAsync(turnContext);

            Assert.IsFalse(updated, "Card was updated despite having no submit action");

            turnContext.Activity.Value = new Dictionary<string, string>
            {
                { PayloadIdTypes.GetKey(PayloadIdTypes.Action), ACTIONID },
            };

            await manager.PreserveValuesAsync(turnContext);

            Assert.IsFalse(updated, "Card was updated despite having no inputs");

            turnContext.Activity.Value = $"{{ '{TOGGLEINPUTID}': '{USERENTEREDTOGGLE}' }}";

            await manager.PreserveValuesAsync(turnContext);

            Assert.IsFalse(updated, "Card was updated by a serialized payload");

            turnContext.Activity.Value = new Dictionary<string, string>
            {
                { TOGGLEINPUTID, USERENTEREDTOGGLE },
                { PayloadIdTypes.GetKey(PayloadIdTypes.Batch), BATCHID },
            };

            await manager.PreserveValuesAsync(turnContext);

            Assert.IsFalse(updated, "Card was updated despite having the same payload as a hero card");

            expectedActivity = activity2;

            turnContext.Activity.Value = new Dictionary<string, string>
            {
                { CHOICEINPUTID, USERENTEREDCHOICE },
                { PayloadIdTypes.GetKey(PayloadIdTypes.Batch), BATCHID },
            };

            await manager.PreserveValuesAsync(turnContext);

            Assert.IsTrue(updated);
            Assert.AreSame(adaptiveCardJObject, activity2.Attachments[1].Content);
            Assert.AreEqual(USERENTEREDCHOICE, adaptiveCardJObject.SelectToken("body[0].value").ToString());

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await manager.PreserveValuesAsync(null));
        }

        [TestMethod]
        public async Task TestDeleteAsync()
        {
            const string ACTIONID1 = "action ID 1";
            const string ACTIONID2 = "action ID 2";
            const string ACTIONID3 = "action ID 3";
            const string ACTIONID4 = "action ID 4";
            const string ACTIONID5 = "action ID 5";
            const string ACTIONID6 = "action ID 6";
            const string CARDID = "card ID";
            const string CAROUSELID = "carousel ID";
            const string BATCHID = "batch ID";
            const string CHOICEINPUTID = "choice set input ID";
            const string DATEINPUTID = "date input ID";

            var manager = CreateManager();
            var turnContext = CreateTurnContext();
            var state = new CardManagerState();
            var updated = false;
            var deletedCount = 0;

            var activity1 = new Activity
            {
                Id = "activity ID 1",
                Attachments = new List<Attachment>
                {
                    // This attachment should be deleted after both actions in the actions property are deleted
                    // despite having a select action
                    new Attachment
                    {
                        Name = "0,0",
                        ContentType = AdaptiveCard.ContentType,
                        Content = new AdaptiveCard("1.0")
                        {
                            Actions = new List<AdaptiveAction>
                            {
                                new AdaptiveSubmitAction
                                {
                                    Data = new Dictionary<string, string>
                                    {
                                        { PayloadIdTypes.GetKey(PayloadIdTypes.Action), ACTIONID1 },
                                    },
                                },
                                new AdaptiveSubmitAction
                                {
                                    Data = new Dictionary<string, string>
                                    {
                                        { PayloadIdTypes.GetKey(PayloadIdTypes.Action), ACTIONID2 },
                                    },
                                },
                            },
                            SelectAction = new AdaptiveSubmitAction
                            {
                                Data = new Dictionary<string, string>
                                {
                                    { PayloadIdTypes.GetKey(PayloadIdTypes.Action), "Irrelevant action ID" },
                                },
                            },
                        },
                    },

                    // When this attachment is deleted, the whole activity should be deleted
                    new Attachment
                    {
                        Name = "0,1",
                        ContentType = HeroCard.ContentType,
                        Content = new HeroCard(buttons: new List<CardAction>
                        {
                            new CardAction(ActionTypes.PostBack, value: new Dictionary<string, string>
                            {
                                { PayloadIdTypes.GetKey(PayloadIdTypes.Action), ACTIONID3 },
                            }),
                            new CardAction(ActionTypes.MessageBack, value: new Dictionary<string, string>
                            {
                                { PayloadIdTypes.GetKey(PayloadIdTypes.Action), ACTIONID4 },
                            }),
                        }),
                    },
                }
            };

            var activity2 = new Activity
            {
                Id = "activity ID 2",
                Attachments = new List<Attachment>
                {
                    // This attachment shouldn't be deleted because of its body
                    new Attachment
                    {
                        Name = "1,0",
                        ContentType = AdaptiveCard.ContentType,
                        Content = new AdaptiveCard("1.2")
                        {
                            Body = new List<AdaptiveElement>
                            {
                                new AdaptiveChoiceSetInput
                                {
                                    Id = CHOICEINPUTID,
                                },
                            },
                            Actions = new List<AdaptiveAction>
                            {
                                new AdaptiveSubmitAction
                                {
                                    Data = new Dictionary<string, string>
                                    {
                                        { PayloadIdTypes.GetKey(PayloadIdTypes.Action), ACTIONID5 },
                                    },
                                },
                            },
                        },
                    },

                    // This attachment shouldn't be deleted because of its title
                    new Attachment
                    {
                        Name = "1,1",
                        ContentType = HeroCard.ContentType,
                        Content = new HeroCard(title: "Irrelevant title", buttons: new List<CardAction>
                        {
                            new CardAction(ActionTypes.PostBack, value: new Dictionary<string, string>
                            {
                                { PayloadIdTypes.GetKey(PayloadIdTypes.Action), ACTIONID6 },
                            }),
                        }),
                    },
                },
            };

            var activity3 = new Activity
            {
                Id = "activity ID 3",
                Attachments = new List<Attachment>
                {
                    // When this attachment is deleted, the activity won't be deleted
                    // but it should be removed from state
                    new Attachment
                    {
                        Name = "2,0",
                        ContentType = AdaptiveCard.ContentType,
                        Content = new AdaptiveCard("1.1")
                        {
                            Body = new List<AdaptiveElement>
                            {
                                new AdaptiveDateInput
                                {
                                    Id = DATEINPUTID,
                                },
                            },
                            Actions = new List<AdaptiveAction>
                            {
                                new AdaptiveSubmitAction
                                {
                                    Data = new Dictionary<string, string>
                                    {
                                        // Even though a carousel ID is used to identify an activity,
                                        // it can still be used to delete a single attachment
                                        { PayloadIdTypes.GetKey(PayloadIdTypes.Carousel), CAROUSELID },
                                    },
                                },
                            },
                        },
                    },

                    new Attachment
                    {
                        Name = "2,1",
                    },
                },
            };

            var activity4 = new Activity
            {
                Id = "activity ID 4",
                Attachments = new List<Attachment>
                {
                    new Attachment
                    {
                        Name = "3,0",
                        ContentType = HeroCard.ContentType,
                        Content = new HeroCard(title: "Irrelevant title", buttons: new List<CardAction>
                        {
                            new CardAction(ActionTypes.MessageBack, value: new Dictionary<string, string>
                            {
                                // Even though a card ID is used to identify a single attachment,
                                // it can still be used to delete an activity
                                { PayloadIdTypes.GetKey(PayloadIdTypes.Card), CARDID },
                            }),
                        }),
                    },
                    new Attachment
                    {
                        Name = "3,1",
                    },
                },
            };

            var activity5 = new Activity
            {
                Id = "activity ID 5",
                Attachments = new List<Attachment>
                {
                    new Attachment
                    {
                        Name = "4,0",
                        ContentType = HeroCard.ContentType,
                        Content = new HeroCard(title: "Irrelevant title", buttons: new List<CardAction>
                        {
                            new CardAction(ActionTypes.PostBack, value: new Dictionary<string, string>
                            {
                                { PayloadIdTypes.GetKey(PayloadIdTypes.Batch), BATCHID },
                            }),
                        }),
                    },
                    new Attachment
                    {
                        Name = "4,1",
                        ContentType = AdaptiveCard.ContentType,
                        Content = new AdaptiveCard("1.0")
                        {
                            Actions = new List<AdaptiveAction>
                            {
                                new AdaptiveSubmitAction
                                {
                                    Data = new Dictionary<string, string>
                                    {
                                        { PayloadIdTypes.GetKey(PayloadIdTypes.Action), "Another irrelevant action ID" },
                                        { PayloadIdTypes.GetKey(PayloadIdTypes.Card), "Irrelevant card ID" },
                                        { PayloadIdTypes.GetKey(PayloadIdTypes.Carousel), "Irrelevant carousel ID" },
                                        { PayloadIdTypes.GetKey(PayloadIdTypes.Batch), BATCHID },
                                    },
                                },
                            },
                        },
                    },
                },
            };

            var activity6 = new Activity
            {
                Id = "activity ID 6",
                Attachments = new List<Attachment>
                {
                    // Not all attachments need to have the batch ID
                    new Attachment
                    {
                        Name = "5,0",
                        ContentType = AdaptiveCard.ContentType,
                        Content = new AdaptiveCard("1.1")
                        {
                            Actions = new List<AdaptiveAction>
                            {
                                new AdaptiveSubmitAction
                                {
                                    Data = new Dictionary<string, string>
                                    {
                                        { PayloadIdTypes.GetKey(PayloadIdTypes.Action), "Yet another irrelevant action ID" },
                                    },
                                },
                            },
                        },
                    },

                    // As long as one attachment contains the batch ID,
                    // the activity will be included in the batch
                    new Attachment
                    {
                        Name = "5,1",
                        ContentType = HeroCard.ContentType,
                        Content = new HeroCard(title: "Irrelevant title", buttons: new List<CardAction>
                        {
                            new CardAction(ActionTypes.MessageBack, value: new Dictionary<string, string>
                            {
                                { PayloadIdTypes.GetKey(PayloadIdTypes.Batch), BATCHID },
                            }),
                        }),
                    },
                },
            };

            var activity7 = new Activity
            {
                Id = "activity ID 7",
                Attachments = new List<Attachment>
                {
                    new Attachment
                    {
                        Name = "6,0",
                        ContentType = HeroCard.ContentType,
                        Content = new HeroCard(buttons: new List<CardAction>
                        {
                            new CardAction(ActionTypes.PostBack, value: new Dictionary<string, string>
                            {
                                { PayloadIdTypes.GetKey(PayloadIdTypes.Batch), "Irrelevant batch ID" },
                            }),
                        }),
                    },
                },
            };

            var activity8 = new Activity
            {
                Id = "Irrelevant activity ID",
                Attachments = new List<Attachment>
                {
                    new Attachment
                    {
                        Name = "7,0",
                        ContentType = HeroCard.ContentType,
                        Content = new HeroCard(buttons: new List<CardAction>
                        {
                            new CardAction(ActionTypes.ImBack, value: new Dictionary<string, string>
                            {
                                // This ID won't be visible because it's in an imBack
                                // and so the activity should immediately be removed from state
                                { PayloadIdTypes.GetKey(PayloadIdTypes.Action), "Invisible action ID" },
                            }),
                        }),
                    },
                },
            };

            // We need to use the active queue or else the test adapter will remove the ID
            // of each activity not in the queue when it tries to update them
            var queue = ((TestAdapter)turnContext.Adapter).ActiveQueue;

            queue.Enqueue(activity1);
            queue.Enqueue(activity2);
            queue.Enqueue(activity3);
            queue.Enqueue(activity4);
            queue.Enqueue(activity5);
            queue.Enqueue(activity6);
            queue.Enqueue(activity7);
            queue.Enqueue(activity8);

            state.SavedActivities.UnionWith(queue);

            var expectedActivities = new[] { activity1 };

            turnContext.OnUpdateActivity((turnContext, activity, next) =>
            {
                Assert.AreSame(expectedActivities.Single(), activity);
                Assert.IsFalse(updated, "UpdateActivityAsync was called an extra time");

                updated = true;

                return next();
            });

            turnContext.OnDeleteActivity((turnContext, reference, next) =>
            {
                Assert.IsTrue(expectedActivities.Any(activity => activity.Id == reference.ActivityId));

                deletedCount++;

                return next();
            });

            turnContext.Activity.Value = new Dictionary<string, object>
            {
                // This shouldn't match any payloads
                { PayloadIdTypes.GetKey(PayloadIdTypes.Card), ACTIONID1 },
            };

            await manager.StateAccessor.SetAsync(turnContext, state);

            Assert.IsTrue(state.SavedActivities.Contains(activity8));
            Assert.AreEqual(8, state.SavedActivities.Count);

            await manager.DeleteAsync(turnContext, PayloadIdTypes.Action);

            Assert.IsFalse(updated);
            Assert.AreEqual(0, deletedCount);
            Assert.IsFalse(state.SavedActivities.Contains(activity8), "Saved activities weren't cleaned correctly");
            Assert.AreEqual(7, state.SavedActivities.Count, "Saved activities weren't cleaned correctly");
            Assert.IsTrue(queue.Contains(activity8), "Cleaned activity removed from the queue");
            Assert.AreEqual(8, queue.Count(), "Cleaned activity removed from the queue");

            turnContext.Activity.Value = new Dictionary<string, object>
            {
                { PayloadIdTypes.GetKey(PayloadIdTypes.Action), ACTIONID1 },
            };

            await manager.DeleteAsync(turnContext, PayloadIdTypes.Action);

            var actionId = ((JObject)((AdaptiveSubmitAction)((AdaptiveCard)activity1.Attachments[0].Content)
                    .Actions.Single()).Data)[PayloadIdTypes.GetKey(PayloadIdTypes.Action)];

            Assert.IsTrue(updated);
            Assert.AreEqual(0, deletedCount);
            Assert.AreEqual(ACTIONID2, actionId, "Wrong action was deleted");
            Assert.AreEqual(7, state.SavedActivities.Count);
            Assert.AreEqual(8, queue.Count());

            updated = false;

            turnContext.Activity.Value = new Dictionary<string, object>
            {
                { PayloadIdTypes.GetKey(PayloadIdTypes.Action), ACTIONID2 },
            };

            await manager.DeleteAsync(turnContext, PayloadIdTypes.Action);

            Assert.IsTrue(updated);
            Assert.AreEqual(0, deletedCount);
            Assert.IsInstanceOfType(activity1.Attachments.Single().Content, typeof(HeroCard), "Card wasn't deleted");
            Assert.AreEqual(7, state.SavedActivities.Count);
            Assert.AreEqual(8, queue.Count());

            updated = false;

            turnContext.Activity.Value = new Dictionary<string, object>
            {
                { PayloadIdTypes.GetKey(PayloadIdTypes.Action), ACTIONID3 },
            };

            await manager.DeleteAsync(turnContext, PayloadIdTypes.Action);

            actionId = ((Dictionary<string, string>)((HeroCard)activity1.Attachments.Single().Content)
                .Buttons.Single().Value)[PayloadIdTypes.GetKey(PayloadIdTypes.Action)];

            Assert.IsTrue(updated);
            Assert.AreEqual(0, deletedCount);
            Assert.AreEqual(ACTIONID4, actionId, "Wrong action was deleted");
            Assert.AreEqual(7, state.SavedActivities.Count);
            Assert.AreEqual(8, queue.Count());

            updated = false;

            turnContext.Activity.Value = new Dictionary<string, object>
            {
                { PayloadIdTypes.GetKey(PayloadIdTypes.Action), ACTIONID4 },
            };

            await manager.DeleteAsync(turnContext, PayloadIdTypes.Action);

            Assert.IsFalse(updated);
            Assert.AreEqual(1, deletedCount, "Activity wasn't deleted even though it was empty");
            Assert.IsFalse(state.SavedActivities.Contains(activity1), "Activity wasn't removed from state");
            Assert.AreEqual(6, state.SavedActivities.Count);
            Assert.IsFalse(queue.Contains(activity1), "Deleted activity not removed from the queue");
            Assert.AreEqual(7, queue.Count(), "Deleted activity not removed from the queue");

            deletedCount = 0;
            expectedActivities = new[] { activity2 };

            turnContext.Activity.Value = new Dictionary<string, object>
            {
                { CHOICEINPUTID, "User-entered choice" },
                { PayloadIdTypes.GetKey(PayloadIdTypes.Action), ACTIONID5 },
            };

            await manager.DeleteAsync(turnContext, PayloadIdTypes.Action);

            Assert.IsTrue(updated);
            Assert.AreEqual(0, deletedCount);
            Assert.IsFalse(((AdaptiveCard)activity2.Attachments[0].Content).Actions.Any(), "Action wasn't deleted");
            Assert.AreEqual(6, state.SavedActivities.Count);
            Assert.AreEqual(7, queue.Count());

            updated = false;

            turnContext.Activity.Value = new Dictionary<string, object>
            {
                { PayloadIdTypes.GetKey(PayloadIdTypes.Action), ACTIONID6 },
            };

            await manager.DeleteAsync(turnContext, PayloadIdTypes.Action);

            Assert.IsTrue(updated);
            Assert.AreEqual(0, deletedCount, "Activity was deleted even though it still had attachments");
            Assert.IsFalse(((AdaptiveCard)activity2.Attachments[0].Content).Actions.Any(), "Action wasn't deleted");
            Assert.IsFalse(state.SavedActivities.Contains(activity2), "Activity wasn't removed from state");
            Assert.AreEqual(5, state.SavedActivities.Count, "Saved activities weren't cleaned correctly");
            Assert.IsTrue(queue.Contains(activity2), "Cleaned activity removed from the queue");
            Assert.AreEqual(7, queue.Count(), "Cleaned activity removed from the queue");

            updated = false;
            expectedActivities = new[] { activity3 };

            turnContext.Activity.Value = new Dictionary<string, object>
            {
                { DATEINPUTID, "User-entered date" },
                { PayloadIdTypes.GetKey(PayloadIdTypes.Carousel), CAROUSELID },
            };

            // We are deleting the card using a carousel ID
            await manager.DeleteAsync(turnContext, PayloadIdTypes.Card);

            Assert.IsTrue(updated);
            Assert.AreEqual(0, deletedCount, "Activity was deleted even though it still had attachments");
            Assert.AreEqual(1, activity3.Attachments.Count(), "Attachment wasn't deleted");
            Assert.IsFalse(state.SavedActivities.Contains(activity3), "Activity wasn't removed from state");
            Assert.AreEqual(4, state.SavedActivities.Count, "Saved activities weren't cleaned correctly");
            Assert.IsTrue(queue.Contains(activity3), "Cleaned activity removed from the queue");
            Assert.AreEqual(7, queue.Count(), "Cleaned activity removed from the queue");

            updated = false;
            expectedActivities = new[] { activity4 };

            turnContext.Activity.Value = new Dictionary<string, object>
            {
                { PayloadIdTypes.GetKey(PayloadIdTypes.Card), CARDID },
            };

            // We are deleting the carousel using a card ID
            await manager.DeleteAsync(turnContext, PayloadIdTypes.Carousel);

            Assert.IsFalse(updated);
            Assert.AreEqual(1, deletedCount);
            Assert.IsFalse(state.SavedActivities.Contains(activity4), "Activity wasn't removed from state");
            Assert.AreEqual(3, state.SavedActivities.Count, "Saved activities weren't cleaned correctly");
            Assert.IsFalse(queue.Contains(activity4), "Deleted activity not removed from the queue");
            Assert.AreEqual(6, queue.Count(), "Deleted activity not removed from the queue");

            deletedCount = 0;
            expectedActivities = new[] { activity5, activity6 };

            turnContext.Activity.Value = new Dictionary<string, object>
            {
                { PayloadIdTypes.GetKey(PayloadIdTypes.Batch), BATCHID },
            };

            // We are deleting the carousel using a card ID
            await manager.DeleteAsync(turnContext, PayloadIdTypes.Batch);

            Assert.IsFalse(updated);
            Assert.AreEqual(2, deletedCount);
            Assert.AreEqual(activity7, state.SavedActivities.Single(), "Saved activities weren't cleaned correctly");
            Assert.IsFalse(queue.Contains(activity5), "Activity 5 not removed from the queue");
            Assert.IsFalse(queue.Contains(activity6), "Activity 6 activity not removed from the queue");
            Assert.AreEqual(4, queue.Count(), "Deleted activities not removed from the queue");

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await manager.DeleteAsync(null, PayloadIdTypes.Action));
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await manager.DeleteAsync(turnContext, string.Empty));
        }

        private UserState CreateUserState() => new UserState(new MemoryStorage());

        private CardManager CreateManager() => new CardManager(CreateUserState());

        private ITurnContext CreateTurnContext() => new TurnContext(
            new TestAdapter(),
            new Activity(ActivityTypes.Message, from: new ChannelAccount("CardManagerTests"), channelId: Channels.Test));
    }
}
