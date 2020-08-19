using Bot.Builder.Community.Cards.Management;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bot.Builder.Community.Cards.Tests.Management.ManagementExtensions
{
    [TestClass]
    public class SetLibraryDataTests
    {
        private const string OldKeyInNewData = "OldKeyInNewData";
        private const string OldKeyNotInNewData = "OldKeyNotInNewData";
        private const string NewKey = "NewKey";
        private const string OldValue = "OldValue";
        private const string NewValue = "NewValue";

        [TestMethod]
        public void SetLibraryData_MergeTrue()
        {
            var batch = CreateBatch();

            batch.SetLibraryData(CreateNewData(), true);

            var expected = new JObject
            {
                { OldKeyInNewData, NewValue },
                { OldKeyNotInNewData, OldValue },
                { NewKey, NewValue },
            };

            var actual = GetLibraryData(batch);

            Assert.IsTrue(JToken.DeepEquals(expected, actual));
        }

        [TestMethod]
        public void SetLibraryData_MergeFalse()
        {
            var batch = CreateBatch();

            batch.SetLibraryData(CreateNewData(), false);

            var expected = new JObject
            {
                { OldKeyInNewData, NewValue },
                { NewKey, NewValue },
            };

            var actual = GetLibraryData(batch);

            Assert.IsTrue(JToken.DeepEquals(expected, actual));
        }

        [TestMethod]
        public void SetLibraryData_Null()
        {
            var batch = CreateBatch();

            batch.SetLibraryData(null);

            var expected = JValue.CreateNull();
            var actual = GetLibraryData(batch);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SetLibraryData_ThrowsOnNullActivities()
            => Assert.ThrowsException<ArgumentNullException>(() => ((IEnumerable<IMessageActivity>)null).SetLibraryData(null));

        private static IEnumerable<IMessageActivity> CreateBatch() => new List<IMessageActivity>
        {
            MessageFactory.Attachment(new HeroCard(buttons: new List<CardAction>
            {
                new CardAction(ActionTypes.PostBack, value: new Dictionary<string, string>
                {
                    { OldKeyInNewData, OldValue },
                    { OldKeyNotInNewData, OldValue },
                }.WrapLibraryData()),
            }).ToAttachment()),
        };

        private static Dictionary<string, object> CreateNewData() => new Dictionary<string, object>
        {
            { OldKeyInNewData, NewValue },
            { NewKey, NewValue },
        };

        private static JToken GetLibraryData(IEnumerable<IMessageActivity> batch)
            => ((HeroCard)batch.Single().Attachments.Single().Content).Buttons.Single().Value.ToJObject()[PropertyNames.LibraryData];
    }
}
