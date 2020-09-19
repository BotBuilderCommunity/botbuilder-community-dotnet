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

namespace Bot.Builder.Community.Cards.Tests.Management
{
    [TestClass]
    public class CardManagerMiddlewareTests
    {
        [TestMethod]
        public async Task ChannelsWithMessageUpdates_CanBeChanged()
        {
            var shortCircuited = false;
            var deleted = false;

            await RunTest();

            Assert.IsTrue(shortCircuited, "Non-updating channel didn't short-circuit");
            Assert.IsFalse(deleted, "Non-updating channel deleted activity");

            await RunTest(middleware => middleware.ChannelsWithMessageUpdates.Add(Channels.Test));

            Assert.IsFalse(shortCircuited, "Updating channel short-circuited");
            Assert.IsTrue(deleted, "Updating channel didn't delete activity");

            async Task RunTest(Action<CardManagerMiddleware> action = null)
            {
                const string ActionId = "action ID";

                var botState = CreateUserState();
                var middleware = new CardManagerMiddleware(new CardManager(botState));
                var adapter = new TestAdapter().Use(middleware);

                var cardActivity = MessageFactory.Attachment(new HeroCard(buttons: new List<CardAction>
                {
                    new CardAction(ActionTypes.PostBack, value: new object()),
                }).ToAttachment());

                var actionActivity = new Activity(value: new JObject { { DataIdScopes.Action, ActionId } }.WrapLibraryData());

                action?.Invoke(middleware);

                middleware.UpdatingOptions.IdOptions.Set(DataIdScopes.Action, ActionId);
                middleware.NonUpdatingOptions.IdOptions.Set(DataIdScopes.Action, ActionId);

                await new TestFlow(adapter, async (turnContext, cancellationToken) =>
                {
                    if (turnContext.Activity.Value == null)
                    {
                        await turnContext.SendActivityAsync(cardActivity);
                    }

                    shortCircuited = false;

                    await botState.SaveChangesAsync(turnContext);
                })
                    .Send("hi")
                        .Do(() => Assert.IsTrue(adapter.ActiveQueue.Contains(cardActivity)))
                    .Send(actionActivity)
                        .Do(() => deleted = !adapter.ActiveQueue.Contains(cardActivity))
                        .Do(() => shortCircuited = true)
                    .Send(actionActivity)
                    .StartTestAsync();
            }
        }

        [TestMethod]
        public async Task Options_AutoAdaptOutgoingCardActions()
        {
            await RunTest(true, typeof(string));
            await RunTest(false, typeof(object));

            async Task RunTest(bool autoAdaptOutgoingCardActions, Type expectedType)
            {
                var middleware = CreateMiddleware();
                var adapter = new TestAdapter().Use(middleware);

                var cardActivity = MessageFactory.Attachment(new HeroCard(buttons: new List<CardAction>
                {
                    new CardAction(ActionTypes.ImBack, value: new object()),
                }).ToAttachment());

                middleware.NonUpdatingOptions.AutoAdaptOutgoingCardActions = autoAdaptOutgoingCardActions;

                // No adaptations take place in the test channel, so we'll use Direct Line as an example
                adapter.Conversation.ChannelId = Channels.Directline;

                await new TestFlow(adapter, async (turnContext, cancellationToken) =>
                {
                    await turnContext.SendActivityAsync(cardActivity);
                })
                    .Send("hi")
                        .AssertReply(activity => Assert.AreEqual(expectedType, ((HeroCard)((Activity)activity).Attachments.Single().Content).Buttons.Single().Value.GetType()))
                    .StartTestAsync();
            }
        }

        [TestMethod]
        public async Task Options_AutoApplyIds()
        {
            await RunTest(true, 1);
            await RunTest(false, 0);

            async Task RunTest(bool autoApplyIds, int idCount)
            {
                var middleware = CreateMiddleware();
                var adapter = new TestAdapter().Use(middleware);

                var cardActivity = MessageFactory.Attachment(new HeroCard(buttons: new List<CardAction>
                {
                    new CardAction(ActionTypes.MessageBack, value: new JObject()),
                }).ToAttachment());

                middleware.NonUpdatingOptions.AutoApplyIds = autoApplyIds;

                await new TestFlow(adapter, async (turnContext, cancellationToken) =>
                {
                    await turnContext.SendActivityAsync(cardActivity);
                })
                    .Send("hi")
                        .AssertReply(activity => Assert.AreEqual(idCount, ((JObject)((HeroCard)((Activity)activity).Attachments.Single().Content).Buttons.Single().Value).Count))
                    .StartTestAsync();
            }
        }

        [TestMethod]
        public async Task Options_AutoConvertAdaptiveCards()
        {
            await RunTest(true, typeof(JObject));
            await RunTest(false, typeof(AdaptiveCard));

            async Task RunTest(bool autoConvertAdaptiveCards, Type expectedType)
            {
                var middleware = CreateMiddleware();
                var adapter = new TestAdapter().Use(middleware);

                var cardActivity = MessageFactory.Attachment(new Attachment
                {
                    ContentType = ContentTypes.AdaptiveCard,
                    Content = new AdaptiveCard("1.0"),
                });

                middleware.NonUpdatingOptions.AutoConvertAdaptiveCards = autoConvertAdaptiveCards;

                await new TestFlow(adapter, async (turnContext, cancellationToken) =>
                {
                    await turnContext.SendActivityAsync(cardActivity);
                })
                    .Send("hi")
                        .AssertReply(activity => Assert.AreEqual(expectedType, ((Activity)activity).Attachments.Single().Content.GetType()))
                    .StartTestAsync();
            }
        }

        [TestMethod]
        public async Task IdTrackingStyle_Disabled()
        {
            const string ActionId = "action ID";

            var botState = CreateUserState();
            var accessor = botState.CreateProperty<CardManagerState>(nameof(CardManagerState));
            var middleware = new CardManagerMiddleware(new CardManager(botState));
            var adapter = new TestAdapter().Use(middleware);

            var cardActivity = MessageFactory.Attachment(new HeroCard(buttons: new List<CardAction>
            {
                new CardAction(ActionTypes.PostBack, value: new object()),
            }).ToAttachment());

            var actionActivity = new Activity(value: new JObject { { DataIdScopes.Action, ActionId } }.WrapLibraryData());

            middleware.NonUpdatingOptions.IdTrackingStyle = TrackingStyle.TrackDisabled;
            middleware.NonUpdatingOptions.IdOptions.Set(DataIdScopes.Action, ActionId);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                if (turnContext.Activity.Value == null)
                {
                    await turnContext.SendActivityAsync(cardActivity);
                }

                var state = await accessor.GetNotNullAsync(turnContext, () => new CardManagerState());

                await turnContext.SendActivityAsync(
                    $"Tracked: {state.DataIdsByScope.TryGetValue(DataIdScopes.Action, out var set) && set.Contains(ActionId)}");
            })
                .Send("hi")
                    .AssertReply(cardActivity, EqualityComparer<IActivity>.Default)
                    .AssertReply($"Tracked: {false}")
                .Send(actionActivity)
                    .AssertReply($"Tracked: {true}")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task OnUpdateActivity_Ignore()
        {
            const string UserSays_PleaseUpdate = "please update";

            await RunTest(true, 0);
            await RunTest(false, 1);

            async Task RunTest(bool ignore, int idCount)
            {
                var middleware = CreateMiddleware();
                var adapter = new TestAdapter().Use(middleware);

                var cardActivity = MessageFactory.Attachment(new HeroCard(buttons: new List<CardAction>
                {
                    new CardAction(ActionTypes.MessageBack, value: new JObject()),
                }).ToAttachment());

                ResourceResponse response = null;

                middleware.ChannelsWithMessageUpdates.Add(Channels.Test);

                await new TestFlow(adapter, async (turnContext, cancellationToken) =>
                {
                    if (turnContext.Activity.Text == UserSays_PleaseUpdate)
                    {
                        cardActivity.Id = response.Id;

                        if (ignore)
                        {
                            var ignoreUpdate = turnContext.TurnState.Get<CardManagerTurnState>()?.MiddlewareIgnoreUpdate;

                            ignoreUpdate?.Add(cardActivity);
                        }

                        await turnContext.UpdateActivityAsync(cardActivity);
                    }
                    else
                    {
                        response = await turnContext.SendActivityAsync("This will get updated");
                    }
                })
                    .Send("hi")
                    .Send(UserSays_PleaseUpdate)
                        .AssertReply(activity => Assert.AreEqual(idCount, ((JObject)((HeroCard)((Activity)activity).Attachments.Single().Content).Buttons.Single().Value).Count))
                    .StartTestAsync();
            }
        }

        [TestMethod]
        public async Task OnDeleteActivity_Ignore()
        {
            const string UserSays_PleaseDelete = "please delete";

            await RunTest(true, 1);
            await RunTest(false, 0);

            async Task RunTest(bool ignore, int activityCount)
            {
                var botState = CreateUserState();
                var accessor = botState.CreateProperty<CardManagerState>(nameof(CardManagerState));
                var middleware = new CardManagerMiddleware(new CardManager(botState));
                var adapter = new TestAdapter().Use(middleware);

                var cardActivity = MessageFactory.Attachment(new HeroCard(buttons: new List<CardAction>
                {
                    new CardAction(ActionTypes.PostBack, value: new JObject()),
                }).ToAttachment());

                ResourceResponse response = null;

                middleware.ChannelsWithMessageUpdates.Add(Channels.Test);

                await new TestFlow(adapter, async (turnContext, cancellationToken) =>
                {
                    if (turnContext.Activity.Text == UserSays_PleaseDelete)
                    {
                        if (ignore)
                        {
                            var ignoreDelete = turnContext.TurnState.Get<CardManagerTurnState>()?.MiddlewareIgnoreDelete;

                            ignoreDelete?.Add(response.Id);
                        }

                        await turnContext.DeleteActivityAsync(response.Id);
                    }
                    else
                    {
                        response = await turnContext.SendActivityAsync(cardActivity);
                    }

                    var state = await accessor.GetNotNullAsync(turnContext, () => new CardManagerState());

                    await turnContext.SendActivityAsync($"Saved: {state.SavedActivities.Count}");

                    await botState.SaveChangesAsync(turnContext);
                })
                    .Send("hi")
                        .AssertReply(cardActivity, EqualityComparer<IActivity>.Default)
                        .AssertReply($"Saved: {1}")
                    .Send(UserSays_PleaseDelete)
                        .AssertReply($"Saved: {activityCount}")
                    .StartTestAsync();
            }
        }

        [TestMethod]
        public async Task AutoDeactivate_DisablesWhenTrue() => await TestAutoDeactivate(false, true, false);

        [TestMethod]
        public async Task AutoDeactivate_DoesNotDisableWhenFalse() => await TestAutoDeactivate(false, false, true);

        [TestMethod]
        public async Task AutoDeactivate_DisablesWhenDefault() => await TestAutoDeactivate(false, null, true);

        [TestMethod]
        public async Task AutoDeactivate_DoesNotDisableWhenDefault() => await TestAutoDeactivate(false, null, false);

        [TestMethod]
        public async Task AutoDeactivate_DeletesWhenTrue() => await TestAutoDeactivate(true, true, false);

        [TestMethod]
        public async Task AutoDeactivate_DoesNotDeleteWhenFalse() => await TestAutoDeactivate(true, false, true);

        [TestMethod]
        public async Task AutoDeactivate_DeletesWhenDefault() => await TestAutoDeactivate(true, null, true);

        [TestMethod]
        public async Task AutoDeactivate_DoesNotDeleteWhenDefault() => await TestAutoDeactivate(true, null, false);

        private static async Task TestAutoDeactivate(
            bool useChannelWithMessageUpdates,
            bool? autoDeactivateInAction,
            bool autoDeactivateInOptions)
        {
            const string ActionId = "action ID";

            var botState = CreateUserState();
            var accessor = botState.CreateProperty<CardManagerState>(nameof(CardManagerState));
            var middleware = new CardManagerMiddleware(new CardManager(botState));
            var adapter = new TestAdapter().Use(middleware);
            var expectedToDeactivate = autoDeactivateInAction == true || (autoDeactivateInAction != false && autoDeactivateInOptions);
            var expectedInStateBefore = !useChannelWithMessageUpdates;
            var expectedInQueueBefore = true;
            var expectedInStateAfter = !(useChannelWithMessageUpdates || expectedToDeactivate);
            var expectedInQueueAfter = !(useChannelWithMessageUpdates && expectedToDeactivate);
            var isInState = false;
            var isInQueue = false;
            var activitiesProcessed = 0;

            var cardActivity = MessageFactory.Attachment(new HeroCard(buttons: new List<CardAction>
            {
                new CardAction(ActionTypes.PostBack, value: new Dictionary<string, object>
                {
                    { Behaviors.AutoDeactivate, autoDeactivateInAction },
                }.WrapLibraryData()),
            }).ToAttachment());

            var actionActivity = new Activity(value: new JObject
            {
                { Behaviors.AutoDeactivate, autoDeactivateInAction },
                { DataIdScopes.Action, ActionId },
            }.WrapLibraryData());

            if (useChannelWithMessageUpdates)
            {
                middleware.ChannelsWithMessageUpdates.Add(Channels.Test);
                middleware.UpdatingOptions.IdOptions.Set(DataIdScopes.Action, ActionId);
                middleware.UpdatingOptions.AutoDeleteOnAction = autoDeactivateInOptions;
            }
            else
            {
                middleware.NonUpdatingOptions.IdOptions.Set(DataIdScopes.Action, ActionId);
                middleware.NonUpdatingOptions.AutoDisableOnAction = autoDeactivateInOptions;
                middleware.NonUpdatingOptions.AutoClearEnabledOnSend = false;
            }

            void AssertCard(bool expected, bool useState) => Assert.AreEqual(
                    expected,
                    useState ? isInState : isInQueue,
                    $"Card activity {(expected ? "wasn't" : "was")} found in {(useState ? "state" : "queue")}");

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                if (turnContext.Activity.Value == null)
                {
                    await turnContext.SendActivityAsync(cardActivity);
                }

                var state = await accessor.GetNotNullAsync(turnContext, () => new CardManagerState());

                isInState = state.DataIdsByScope.TryGetValue(DataIdScopes.Action, out var set) && set.Contains(ActionId);
                isInQueue = adapter.ActiveQueue.Contains(cardActivity);

                activitiesProcessed++;

                await botState.SaveChangesAsync(turnContext);
            })
                .Send("hi")
                    .Do(() => AssertCard(expectedInStateBefore, true))
                    .Do(() => AssertCard(expectedInQueueBefore, false))
                .Send(actionActivity)
                    .Do(() => AssertCard(expectedInStateAfter, true))
                    .Do(() => AssertCard(expectedInQueueAfter, false))
                .StartTestAsync();

            Assert.AreEqual(2, activitiesProcessed);
        }

        private static UserState CreateUserState() => new UserState(new MemoryStorage());

        private static CardManager CreateManager() => new CardManager(CreateUserState());

        private static CardManagerMiddleware CreateMiddleware() => new CardManagerMiddleware(CreateManager());
    }
}
