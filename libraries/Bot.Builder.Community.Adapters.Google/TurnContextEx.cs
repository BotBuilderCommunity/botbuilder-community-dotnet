using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;

namespace Bot.Builder.Community.Adapters.Google
{
    internal class TurnContextEx : TurnContext
    {
        public TurnContextEx(BotAdapter adapter, Activity activity) : base(adapter, activity)
        {
            this.OnSendActivities(async (ctx, activities, nextSend) =>
            {
                var responses = await nextSend().ConfigureAwait(false);

                SentActivities.AddRange(activities);

                return responses;
            });
        }

        public List<Activity> SentActivities { get; internal set; } = new List<Activity>();
    }
}