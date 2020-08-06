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

                var actionActivity = new Activity(value: new JObject
                {
                    {
                        PropertyNames.LibraryData,
                        new JObject { { DataIdScopes.Action, ActionId } }
                    },
                });

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

            var actionActivity = new Activity(value: new JObject
            {
                {
                    PropertyNames.LibraryData,
                    new JObject { { DataIdScopes.Action, ActionId } }
                },
            });

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

        private UserState CreateUserState() => new UserState(new MemoryStorage());

        private CardManager CreateManager() => new CardManager(CreateUserState());

        private CardManagerMiddleware CreateMiddleware() => new CardManagerMiddleware(CreateManager());
    }
}
