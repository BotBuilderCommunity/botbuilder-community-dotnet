﻿using System;
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

namespace Bot.Builder.Community.Cards.Tests.Management
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
            var dataId = new DataId(DataIdScopes.Action, "action ID");

            Assert.IsNull(await manager.StateAccessor.GetAsync(turnContext));

            await manager.EnableIdAsync(turnContext, dataId);

            var state = await manager.StateAccessor.GetAsync(turnContext);
            var actionIds = state.DataIdsByScope[DataIdScopes.Action];

            Assert.AreEqual(dataId.Value, actionIds.Single());

            await manager.EnableIdAsync(turnContext, new DataId(DataIdScopes.Action, "different action ID"), TrackingStyle.TrackDisabled);

            Assert.AreEqual(dataId.Value, actionIds.Single());

            await manager.EnableIdAsync(turnContext, dataId, TrackingStyle.TrackDisabled);

            Assert.AreEqual(0, actionIds.Count);

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await manager.EnableIdAsync(turnContext, null));
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await manager.EnableIdAsync(null, dataId));
        }

        [TestMethod]
        public async Task TestDisableIdAsync()
        {
            var manager = CreateManager();
            var turnContext = CreateTurnContext();
            var dataId = new DataId(DataIdScopes.Card, "card ID");

            Assert.IsNull(await manager.StateAccessor.GetAsync(turnContext));

            await manager.DisableIdAsync(turnContext, dataId, TrackingStyle.TrackDisabled);

            var state = await manager.StateAccessor.GetAsync(turnContext);
            var actionIds = state.DataIdsByScope[DataIdScopes.Card];

            Assert.AreEqual(dataId.Value, actionIds.Single());

            await manager.DisableIdAsync(turnContext, new DataId(DataIdScopes.Card, "different card ID"));

            Assert.AreEqual(dataId.Value, actionIds.Single());

            await manager.DisableIdAsync(turnContext, dataId);

            Assert.AreEqual(0, actionIds.Count);

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await manager.DisableIdAsync(turnContext, null));
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await manager.DisableIdAsync(null, dataId));
        }

        [TestMethod]
        public async Task TestTrackIdAsync()
        {
            var manager = CreateManager();
            var turnContext = CreateTurnContext();
            var dataId = new DataId(DataIdScopes.Carousel, "carousel ID");

            Assert.IsNull(await manager.StateAccessor.GetAsync(turnContext));

            await manager.TrackIdAsync(turnContext, dataId);

            var state = await manager.StateAccessor.GetAsync(turnContext);
            var actionIds = state.DataIdsByScope[DataIdScopes.Carousel];

            Assert.AreEqual(dataId.Value, actionIds.Single());

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await manager.TrackIdAsync(turnContext, null));
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await manager.TrackIdAsync(null, dataId));
        }

        [TestMethod]
        public async Task TestForgetIdAsync()
        {
            var manager = CreateManager();
            var turnContext = CreateTurnContext();
            var dataId = new DataId(DataIdScopes.Batch, "batch ID");

            Assert.IsNull(await manager.StateAccessor.GetAsync(turnContext));

            await manager.ForgetIdAsync(turnContext, dataId);

            var state = await manager.StateAccessor.GetAsync(turnContext);

            Assert.AreEqual(0, state.DataIdsByScope.Count);

            var actionIds = state.DataIdsByScope[DataIdScopes.Batch] = new HashSet<string> { dataId.Value };

            await manager.ForgetIdAsync(turnContext, new DataId(DataIdScopes.Batch, "different batch ID"));

            Assert.AreEqual(dataId.Value, actionIds.Single());

            await manager.ForgetIdAsync(turnContext, dataId);

            Assert.AreEqual(0, actionIds.Count);

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await manager.ForgetIdAsync(turnContext, null));
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await manager.ForgetIdAsync(null, dataId));
        }

        [TestMethod]
        public async Task TestClearTrackedIdsAsync()
        {
            var manager = CreateManager();
            var turnContext = CreateTurnContext();

            Assert.IsNull(await manager.StateAccessor.GetAsync(turnContext));

            await manager.ClearTrackedIdsAsync(turnContext);

            var state = await manager.StateAccessor.GetAsync(turnContext);

            Assert.AreEqual(0, state.DataIdsByScope.Count);

            state.DataIdsByScope[DataIdScopes.Action] = new HashSet<string> { "action ID" };
            state.DataIdsByScope[DataIdScopes.Card] = new HashSet<string> { "card ID" };
            state.DataIdsByScope[DataIdScopes.Carousel] = new HashSet<string> { "carousel ID" };
            state.DataIdsByScope[DataIdScopes.Batch] = new HashSet<string> { "batch ID" };

            Assert.AreEqual(4, state.DataIdsByScope.Count);

            await manager.ClearTrackedIdsAsync(turnContext);

            Assert.AreEqual(0, state.DataIdsByScope.Count);

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
                { "foo", "bar" }, // No data ID's
            };

            var activities1 = new List<Activity>
            {
                new Activity
                {
                    Attachments = new List<Attachment>
                    {
                        new HeroCard(buttons: new List<CardAction>
                        {
                            // This acitivity shouldn't be saved because the value has no data ID's
                            new CardAction(type: ActionTypes.PostBack, value: value.WrapLibraryData()),
                        }).ToAttachment(),
                    },
                },
            };

            Assert.IsNull(await manager.StateAccessor.GetAsync(turnContext));

            await manager.SaveActivitiesAsync(turnContext, activities1);

            var state = await manager.StateAccessor.GetAsync(turnContext);

            Assert.AreEqual(0, state.SavedActivities.Count, "An activity was saved despite having no data ID's");

            value[DataIdScopes.Action] = "action ID";

            await manager.SaveActivitiesAsync(turnContext, activities1);

            Assert.AreSame(activities1.Single(), state.SavedActivities.Single());

            await manager.SaveActivitiesAsync(turnContext, activities1);

            Assert.AreEqual(1, state.SavedActivities.Count, "One activity was saved as multiple activities");

            var activities2 = new List<Activity>
            {
                new Activity(id: "2.0"),
                new Activity
                {
                    Id = "2.1",
                    Attachments = new List<Attachment>
                    {
                        new Attachment
                        {
                            ContentType = ContentTypes.AdaptiveCard,
                            Content = new AdaptiveCard(new AdaptiveSchemaVersion())
                            {
                                Actions = new List<AdaptiveAction>
                                {
                                    new AdaptiveSubmitAction
                                    {
                                        Data = new Dictionary<string, string>
                                        {
                                            { DataIdScopes.Card, "card ID" },
                                        }.WrapLibraryData(),
                                    },
                                },
                            },
                        },
                    },
                },
                new Activity
                {
                    Id = "2.2",
                    Attachments = new List<Attachment>
                    {
                        new Attachment
                        {
                            ContentType = HeroCard.ContentType, // Wrong content type
                            Content = new AdaptiveCard(new AdaptiveSchemaVersion())
                            {
                                Actions = new List<AdaptiveAction>
                                {
                                    new AdaptiveSubmitAction
                                    {
                                        Data = new Dictionary<string, string>
                                        {
                                            { DataIdScopes.Carousel, "carousel ID" },
                                        }.WrapLibraryData(),
                                    },
                                },
                            },
                        },
                    },
                },
                new Activity
                {
                    Id = "2.3",
                    Attachments = new List<Attachment>
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
                                            { DataIdScopes.Carousel, "other carousel ID" },
                                        }.WrapLibraryData(),
                                    },
                                },
                            },
                        },
                        new AnimationCard(buttons: new List<CardAction>
                        {
                            new CardAction(type: ActionTypes.MessageBack, value: $"{{'{PropertyNames.LibraryData}':{{'{DataIdScopes.Batch}':'batch ID'}}}}"),
                        }).ToAttachment(),
                    },
                },
                new Activity
                {
                    Id = "2.3", // Duplicate ID
                    Attachments = new List<Attachment>
                    {
                        new AudioCard(buttons: new List<CardAction>
                        {
                            new CardAction(type: ActionTypes.PostBack, value: $"{{'{PropertyNames.LibraryData}':{{'{DataIdScopes.Batch}':'batch ID'}}}}"),
                        }).ToAttachment(),
                    },
                },
            };

            await manager.SaveActivitiesAsync(turnContext, activities2);

            Assert.AreEqual(3, state.SavedActivities.Count);
            Assert.IsTrue(state.SavedActivities.Contains(activities1.Single()));
            Assert.IsTrue(state.SavedActivities.Contains(activities2[1]));
            Assert.IsTrue(state.SavedActivities.Contains(activities2[4]));

            var activities3 = new List<Activity>
            {
                new Activity
                {
                    Attachments = new List<Attachment>
                    {
                        new ReceiptCard(buttons: new List<CardAction>
                        {
                            new CardAction(type: ActionTypes.PostBack, value: new Dictionary<string, string>
                            {
                                { DataIdScopes.Action, "other action ID" },
                            }.WrapLibraryData()),
                        }).ToAttachment(),
                    },
                },
            };

            await manager.SaveActivitiesAsync(turnContext, activities3);

            Assert.AreEqual(4, state.SavedActivities.Count, "Null ID collided with other null ID");

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await manager.SaveActivitiesAsync(turnContext, null));
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await manager.SaveActivitiesAsync(null, activities1));
        }

        [TestMethod]
        public async Task TestUnsaveActivityAsync()
        {
            const string ACTIVITYID1 = "activity ID 1";
            const string ACTIVITYID2 = "activity ID 2";

            var manager = CreateManager();
            var turnContext = CreateTurnContext();
            var state = new CardManagerState();
            var activity1 = new Activity(id: ACTIVITYID1);
            var activity2 = new Activity(id: ACTIVITYID2);
            var activity3 = new Activity(id: ACTIVITYID2);

            await manager.StateAccessor.SetAsync(turnContext, state);
            await manager.UnsaveActivityAsync(turnContext, ACTIVITYID1);

            Assert.AreEqual(0, state.SavedActivities.Count);

            state.SavedActivities.Add(activity1);
            state.SavedActivities.Add(activity2);
            state.SavedActivities.Add(activity3);

            Assert.AreEqual(3, state.SavedActivities.Count);

            await manager.UnsaveActivityAsync(turnContext, ACTIVITYID1);

            Assert.AreEqual(2, state.SavedActivities.Count, "Did not remove one activity");

            await manager.UnsaveActivityAsync(turnContext, ACTIVITYID2);

            Assert.AreEqual(0, state.SavedActivities.Count, "Did not remove two activities");

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await manager.UnsaveActivityAsync(turnContext, null));
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await manager.UnsaveActivityAsync(null, "ID"));
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
                                        { DataIdScopes.Action, ACTIONID1 },
                                    }.WrapLibraryData(),
                                },
                                new AdaptiveSubmitAction
                                {
                                    Data = new Dictionary<string, string>
                                    {
                                        { DataIdScopes.Action, ACTIONID2 },
                                    }.WrapLibraryData(),
                                },
                            },
                            SelectAction = new AdaptiveSubmitAction
                            {
                                Data = new Dictionary<string, string>
                                {
                                    { DataIdScopes.Action, "Irrelevant action ID" },
                                }.WrapLibraryData(),
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
                                { DataIdScopes.Action, ACTIONID3 },
                            }.WrapLibraryData()),
                            new CardAction(ActionTypes.MessageBack, value: new Dictionary<string, string>
                            {
                                { DataIdScopes.Action, ACTIONID4 },
                            }.WrapLibraryData()),
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
                                        { DataIdScopes.Action, ACTIONID5 },
                                    }.WrapLibraryData(),
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
                                { DataIdScopes.Action, ACTIONID6 },
                            }.WrapLibraryData()),
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
                                        { DataIdScopes.Carousel, CAROUSELID },
                                    }.WrapLibraryData(),
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
                                { DataIdScopes.Card, CARDID },
                            }.WrapLibraryData()),
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
                                { DataIdScopes.Batch, BATCHID },
                            }.WrapLibraryData()),
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
                                        { DataIdScopes.Action, "Another irrelevant action ID" },
                                        { DataIdScopes.Card, "Irrelevant card ID" },
                                        { DataIdScopes.Carousel, "Irrelevant carousel ID" },
                                        { DataIdScopes.Batch, BATCHID },
                                    }.WrapLibraryData(),
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
                                        { DataIdScopes.Action, "Yet another irrelevant action ID" },
                                    }.WrapLibraryData(),
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
                                { DataIdScopes.Batch, BATCHID },
                            }.WrapLibraryData()),
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
                                { DataIdScopes.Batch, "Irrelevant batch ID" },
                            }.WrapLibraryData()),
                        }),
                    },
                },
            };

            var activity8 = new Activity
            {
                Id = "Irrelevant activity ID",
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

            // The first 7 activities should be saved in state as well as the active queue
            state.SavedActivities.UnionWith(queue);

            // But this one shouldn't be
            queue.Enqueue(activity8);

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
                // This shouldn't match any action data
                { DataIdScopes.Card, ACTIONID1 },
            }.WrapLibraryData();

            await manager.StateAccessor.SetAsync(turnContext, state);
            await manager.DeleteActionSourceAsync(turnContext, DataIdScopes.Action);

            Assert.IsFalse(updated);
            Assert.AreEqual(0, deletedCount);
            Assert.IsFalse(state.SavedActivities.Contains(activity8));
            Assert.AreEqual(7, state.SavedActivities.Count);
            Assert.IsTrue(queue.Contains(activity8));
            Assert.AreEqual(8, queue.Count());

            turnContext.Activity.Value = new Dictionary<string, object>
            {
                { DataIdScopes.Action, ACTIONID1 },
            }.WrapLibraryData();

            await manager.DeleteActionSourceAsync(turnContext, DataIdScopes.Action);

            var actionId = ((JObject)((AdaptiveSubmitAction)((AdaptiveCard)activity1.Attachments[0].Content)
                    .Actions.Single()).Data).GetIdFromActionData(DataIdScopes.Action);

            Assert.IsTrue(updated);
            Assert.AreEqual(0, deletedCount);
            Assert.AreEqual(ACTIONID2, actionId, "Wrong action was deleted");
            Assert.AreEqual(7, state.SavedActivities.Count);
            Assert.AreEqual(8, queue.Count());

            updated = false;

            turnContext.Activity.Value = new Dictionary<string, object>
            {
                { DataIdScopes.Action, ACTIONID2 },
            }.WrapLibraryData();

            await manager.DeleteActionSourceAsync(turnContext, DataIdScopes.Action);

            Assert.IsTrue(updated);
            Assert.AreEqual(0, deletedCount);
            Assert.IsInstanceOfType(activity1.Attachments.Single().Content, typeof(HeroCard), "Card wasn't deleted");
            Assert.AreEqual(7, state.SavedActivities.Count);
            Assert.AreEqual(8, queue.Count());

            updated = false;

            turnContext.Activity.Value = new Dictionary<string, object>
            {
                { DataIdScopes.Action, ACTIONID3 },
            }.WrapLibraryData();

            await manager.DeleteActionSourceAsync(turnContext, DataIdScopes.Action);

            actionId = ((Dictionary<string, string>)((Dictionary<string, object>)((HeroCard)activity1.Attachments.Single().Content)
                .Buttons.Single().Value)[PropertyNames.LibraryData])[DataIdScopes.Action];

            Assert.IsTrue(updated);
            Assert.AreEqual(0, deletedCount);
            Assert.AreEqual(ACTIONID4, actionId, "Wrong action was deleted");
            Assert.AreEqual(7, state.SavedActivities.Count);
            Assert.AreEqual(8, queue.Count());

            updated = false;

            turnContext.Activity.Value = new Dictionary<string, object>
            {
                { DataIdScopes.Action, ACTIONID4 },
            }.WrapLibraryData();

            await manager.DeleteActionSourceAsync(turnContext, DataIdScopes.Action);

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
                {
                    PropertyNames.LibraryData,
                    new Dictionary<string, string>
                    {
                        { DataIdScopes.Action, ACTIONID5 },
                    }
                },
            };

            await manager.DeleteActionSourceAsync(turnContext, DataIdScopes.Action);

            Assert.IsTrue(updated);
            Assert.AreEqual(0, deletedCount);
            Assert.IsFalse(((AdaptiveCard)activity2.Attachments[0].Content).Actions.Any(), "Action wasn't deleted");
            Assert.AreEqual(6, state.SavedActivities.Count);
            Assert.AreEqual(7, queue.Count());

            updated = false;

            turnContext.Activity.Value = new Dictionary<string, object>
            {
                { DataIdScopes.Action, ACTIONID6 },
            }.WrapLibraryData();

            await manager.DeleteActionSourceAsync(turnContext, DataIdScopes.Action);

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
                {
                    PropertyNames.LibraryData,
                    new Dictionary<string, string>
                    {
                        { DataIdScopes.Carousel, CAROUSELID },
                    }
                },
            };

            // We are deleting the card using a carousel ID
            await manager.DeleteActionSourceAsync(turnContext, DataIdScopes.Card);

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
                { DataIdScopes.Card, CARDID },
            }.WrapLibraryData();

            // We are deleting the carousel using a card ID
            await manager.DeleteActionSourceAsync(turnContext, DataIdScopes.Carousel);

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
                { DataIdScopes.Batch, BATCHID },
            }.WrapLibraryData();

            // We are deleting the carousel using a card ID
            await manager.DeleteActionSourceAsync(turnContext, DataIdScopes.Batch);

            Assert.IsFalse(updated);
            Assert.AreEqual(2, deletedCount);
            Assert.AreEqual(activity7, state.SavedActivities.Single(), "Saved activities weren't cleaned correctly");
            Assert.IsFalse(queue.Contains(activity5), "Activity 5 not removed from the queue");
            Assert.IsFalse(queue.Contains(activity6), "Activity 6 activity not removed from the queue");
            Assert.AreEqual(4, queue.Count(), "Deleted activities not removed from the queue");

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await manager.DeleteActionSourceAsync(null, DataIdScopes.Action));
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await manager.DeleteActionSourceAsync(turnContext, string.Empty));
        }

        private UserState CreateUserState() => new UserState(new MemoryStorage());

        private CardManager CreateManager() => new CardManager(CreateUserState());

        private ITurnContext CreateTurnContext() => new TurnContext(
            new TestAdapter(),
            new Activity(ActivityTypes.Message, from: new ChannelAccount("CardManagerTests"), channelId: Channels.Test));
    }
}
