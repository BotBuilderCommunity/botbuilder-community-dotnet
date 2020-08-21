using Bot.Builder.Community.Cards.Management;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Bot.Builder.Community.Cards.Tests.Management.ManagementExtensions
{
    [TestClass]
    public class SetActionBehaviorTests
    {
        private const string OldKey = "OldKey";
        private const string NewKey = "NewKey";
        private const string OldValue = "OldValue";
        private const string NewValue = "NewValue";

        [TestMethod]
        public void SetActionBehavior_New()
        {
            var batch = CreateBatch();

            batch.SetActionBehavior(NewKey, NewValue);

            var expected = new JObject
            {
                { OldKey, OldValue },
                { NewKey, NewValue },
            };

            var actual = GetLibraryData(batch);

            Assert.IsTrue(JToken.DeepEquals(expected, actual));
        }

        [TestMethod]
        public void SetActionBehavior_Existing()
        {
            var batch = CreateBatch();

            batch.SetActionBehavior(OldKey, NewValue);

            var expected = new JObject
            {
                { OldKey, NewValue },
            };

            var actual = GetLibraryData(batch);

            Assert.IsTrue(JToken.DeepEquals(expected, actual));
        }

        [TestMethod]
        public void SetActionBehavior_Null()
        {
            var batch = CreateBatch();

            batch.SetActionBehavior(NewKey, null);

            var expected = new JObject
            {
                { OldKey, OldValue },
                { NewKey, null },
            };

            var actual = GetLibraryData(batch);

            Assert.IsTrue(JToken.DeepEquals(expected, actual));
        }

        private static IEnumerable<IMessageActivity> CreateBatch() => new List<IMessageActivity>
        {
            MessageFactory.Attachment(new HeroCard(buttons: new List<CardAction>
            {
                new CardAction(ActionTypes.PostBack, value: new Dictionary<string, string>
                {
                    { OldKey, OldValue },
                }.WrapLibraryData()),
            }).ToAttachment()),
        };

        private static JToken GetLibraryData(IEnumerable<IMessageActivity> batch)
            => ((HeroCard)batch.Single().Attachments.Single().Content).Buttons.Single().Value.ToJObject()[PropertyNames.LibraryData];
    }
}
