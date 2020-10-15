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
    public class ActionBehaviorTests
    {
        private const string OldKey = "OldKey";
        private const string NewKey = "NewKey";
        private const string OldValue = "OldValue";
        private const string NewValue = "NewValue";

        [TestMethod]
        public void SetActionBehavior_New()
        {
            var actionData = CreateActionData();

            ActionBehavior.SetInActionData(ref actionData, NewKey, NewValue);

            var expected = new JObject
            {
                { OldKey, OldValue },
                { NewKey, NewValue },
            };

            var actual = JObject.FromObject(actionData)[PropertyNames.LibraryData];

            Assert.IsTrue(JToken.DeepEquals(expected, actual));
        }

        [TestMethod]
        public void SetActionBehavior_Existing()
        {
            var actionData = CreateActionData();

            ActionBehavior.SetInActionData(ref actionData, OldKey, NewValue);

            var expected = new JObject
            {
                { OldKey, NewValue },
            };

            var actual = GetLibraryData(actionData);

            Assert.IsTrue(JToken.DeepEquals(expected, actual));
        }

        [TestMethod]
        public void SetActionBehavior_Null()
        {
            var actionData = CreateActionData();

            ActionBehavior.SetInActionData(ref actionData, NewKey, null);

            var expected = new JObject
            {
                { OldKey, OldValue },
                { NewKey, null },
            };

            var actual = GetLibraryData(actionData);

            Assert.IsTrue(JToken.DeepEquals(expected, actual));
        }

        [TestMethod]
        public void SetInBatch_SetsCorrectly()
        {
            var batch = CreateBatch();

            ActionBehavior.SetInBatch(batch, NewKey, NewValue);

            AssertBehaviorWasSetCorrectly(batch, batch);
        }

        [TestMethod]
        public void SetInActivity_SetsCorrectly()
        {
            var batch = CreateBatch();
            var activity = batch[0];

            ActionBehavior.SetInActivity(activity, NewKey, NewValue);

            AssertBehaviorWasSetCorrectly(batch, activity);
        }

        [TestMethod]
        public void SetInCarousel_SetsCorrectly()
        {
            var batch = CreateBatch();
            var activity = batch[0];

            ActionBehavior.SetInCarousel(activity.Attachments, NewKey, NewValue);

            AssertBehaviorWasSetCorrectly(batch, activity);
        }

        [TestMethod]
        public void SetInAttachment_SetsCorrectly()
        {
            var batch = CreateBatch();
            var attachment = batch[0].Attachments[0];

            ActionBehavior.SetInAttachment(attachment, NewKey, NewValue);

            AssertBehaviorWasSetCorrectly(batch, attachment);
        }

        [TestMethod]
        public void SetInAdaptiveCard_SetsCorrectly()
        {
            var batch = CreateBatch();
            var attachment = batch[0].Attachments[0];

            object adaptiveCard = attachment.Content;

            ActionBehavior.SetInAdaptiveCard(ref adaptiveCard, NewKey, NewValue);

            attachment.Content = adaptiveCard;

            AssertBehaviorWasSetCorrectly(batch, attachment);
        }

        [TestMethod]
        public void SetInAnimationCard_SetsCorrectly() => TestBotFrameworkCard<AnimationCard>(ActionBehavior.SetInAnimationCard, 0, 1);

        [TestMethod]
        public void SetInAudioCard_SetsCorrectly() => TestBotFrameworkCard<AudioCard>(ActionBehavior.SetInAudioCard, 0, 2);

        [TestMethod]
        public void SetInHeroCard_SetsCorrectly() => TestBotFrameworkCard<HeroCard>(ActionBehavior.SetInHeroCard, 0, 3);

        [TestMethod]
        public void SetInOAuthCard_SetsCorrectly() => TestBotFrameworkCard<OAuthCard>(ActionBehavior.SetInOAuthCard, 1, 0);

        [TestMethod]
        public void SetInReceiptCard_SetsCorrectly() => TestBotFrameworkCard<ReceiptCard>(ActionBehavior.SetInReceiptCard, 1, 1);

        [TestMethod]
        public void SetInSigninCard_SetsCorrectly() => TestBotFrameworkCard<SigninCard>(ActionBehavior.SetInSigninCard, 1, 2);

        [TestMethod]
        public void SetInThumbnailCard_SetsCorrectly() => TestBotFrameworkCard<ThumbnailCard>(ActionBehavior.SetInThumbnailCard, 1, 3);

        [TestMethod]
        public void SetInVideoCard_SetsCorrectly() => TestBotFrameworkCard<VideoCard>(ActionBehavior.SetInVideoCard, 1, 4);

        [TestMethod]
        public void SetInSubmitAction_SetsCorrectly()
        {
            var batch = CreateBatch();
            var actions = ((AdaptiveCard)batch[0].Attachments[0].Content).Actions;

            object submitAction = actions[0];

            ActionBehavior.SetInSubmitAction(ref submitAction, NewKey, NewValue);

            actions[0] = JObject.FromObject(submitAction).ToObject<AdaptiveSubmitAction>();

            AssertBehaviorWasSetCorrectly(batch, actions[0]);
        }

        [TestMethod]
        public void SetInCardAction_SetsCorrectly()
        {
            var batch = CreateBatch();
            var action = ((AnimationCard)batch[0].Attachments[1].Content).Buttons[0];

            ActionBehavior.SetInCardAction(action, NewKey, NewValue);

            AssertBehaviorWasSetCorrectly(batch, action);
        }

        [TestMethod]
        public void SetInActionData_SetsCorrectly()
        {
            var batch = CreateBatch();
            var submitAction = (AdaptiveSubmitAction)((AdaptiveCard)batch[0].Attachments[0].Content).Actions[0];

            object actionData = submitAction.Data;

            ActionBehavior.SetInActionData(ref actionData, NewKey, NewValue);

            submitAction.Data = actionData;

            AssertBehaviorWasSetCorrectly(batch, submitAction);
        }

        private static void TestBotFrameworkCard<T>(SetIdsIn<T> setIdsIn, int activityIndex, int attachmentIndex)
        {
            var batch = CreateBatch();
            var attachment = batch[activityIndex].Attachments[attachmentIndex];

            setIdsIn((T)attachment.Content, NewKey, NewValue);

            AssertBehaviorWasSetCorrectly(batch, attachment);
        }

        // TODO: Rework tests to only check the object that was modified
        // because it doesn't make sense to check everything in the whole batch
        private static void AssertBehaviorWasSetCorrectly(IList<IMessageActivity> batch, object scopeRoot)
        {
            var anyNewBehaviorsExpected = true;
            var anyNewBehaviorsFound = false;
            var totalAssertions = 0;
            var scope = DataIdScopes.Action;

            if (scopeRoot is Attachment) scope = DataIdScopes.Card;
            if (scopeRoot is IMessageActivity) scope = DataIdScopes.Carousel;
            if (scopeRoot is IList<IMessageActivity>) scope = DataIdScopes.Batch;

            foreach (var activity in batch)
            {
                if (scope == DataIdScopes.Carousel)
                {
                    anyNewBehaviorsExpected = activity == scopeRoot as IMessageActivity;
                }

                foreach (var attachment in activity.Attachments)
                {
                    if (scope == DataIdScopes.Card)
                    {
                        anyNewBehaviorsExpected = attachment == scopeRoot as Attachment;
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
                            anyNewBehaviorsExpected = action == scopeRoot;
                        }

                        var actionData = action is CardAction cardAction
                            ? JObject.FromObject(cardAction.Value)
                            : JObject.FromObject(action)[AdaptiveProperties.Data] as JObject;

                        Assert.AreEqual(OldValue, actionData[PropertyNames.LibraryData][OldKey].ToString());
                        Assert.AreEqual(anyNewBehaviorsExpected, actionData[PropertyNames.LibraryData][NewKey]?.ToString() == NewValue);

                        if (anyNewBehaviorsExpected)
                        {
                            anyNewBehaviorsFound = true;
                        }

                        totalAssertions++;
                    }
                }
            }

            Assert.IsTrue(anyNewBehaviorsFound);
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
                                Data = CreateActionData(),
                            },
                            new AdaptiveSubmitAction
                            {
                                Data = CreateActionData(),
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
                    Value = CreateActionData(),
                },
                new CardAction
                {
                    Type = ActionTypes.PostBack,
                    Value = CreateActionData(),
                },
            };
        }

        private static object CreateActionData() => new Dictionary<string, object>
        {
            { OldKey, OldValue },
        }.WrapLibraryData();

        private delegate void SetIdsIn<T>(T entryValue, string name, string value);

        private static JToken GetLibraryData(object actionData)
            => JObject.FromObject(actionData)[PropertyNames.LibraryData];
    }
}
