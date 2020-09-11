using System;
using System.Collections.Generic;
using Bot.Builder.Community.Adapters.Google.Core;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Xunit;

namespace Bot.Builder.Community.Adapter.Google.Core.Tests
{
    public class GoogleRequestMapperBaseMergeTests
    {
        /// <summary>
        /// Single activity with empty text, when merged, should give back that activity.
        /// </summary>
        [Fact]
        public void MergeActivitiesNoText()
        {
            var firstActivity = MessageFactory.Text("");
            var mergedActivity = GoogleRequestMapperBase.MergeActivities(new List<Activity>() { firstActivity });

            Assert.Equal("", mergedActivity.Text);
        }

        /// <summary>
        /// If 2nd to last activity ends with a period, and the last activity is blank, the merged activity should still have the period.
        /// </summary>
        [Fact]
        public void MergeActivitiesTextWithPeriodAndEmptyText()
        {
            var firstActivity = MessageFactory.Text("This is the first activity.");
            var secondActivity = MessageFactory.Text("");

            var mergedActivity = GoogleRequestMapperBase.MergeActivities(new List<Activity>() { firstActivity, secondActivity });

            // We want to preserve the period here even though it is merged with empty text.
            Assert.Equal("This is the first activity.", mergedActivity.Text);
        }

        /// <summary>
        /// If 2nd to last activity doesn't end with a period, and the last activity is blank, the merged activity should not have a period.
        /// </summary>
        [Fact]
        public void MergeActivitiesTextWithNoPeriodAndEmptyText()
        {
            var firstActivity = MessageFactory.Text("This is the first activity");
            var secondActivity = MessageFactory.Text("");

            var mergedActivity = GoogleRequestMapperBase.MergeActivities(new List<Activity>() { firstActivity, secondActivity });

            // We want to preserve the missing period here even though it is merged with empty text.
            Assert.Equal("This is the first activity", mergedActivity.Text);
        }
    }
}
