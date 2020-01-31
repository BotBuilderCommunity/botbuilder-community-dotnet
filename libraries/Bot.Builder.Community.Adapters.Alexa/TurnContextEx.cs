using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Linq;

namespace Bot.Builder.Community.Adapters.Alexa
{
    internal class TurnContextEx : TurnContext
    {
        public TurnContextEx(BotAdapter adapter, Activity activity) : base(adapter, activity)
        {
            this.OnSendActivities(async (ctx, activities, nextSend) =>
            {
                var responses = await nextSend().ConfigureAwait(false);

                SentActivities = activities.Where(a => a.Type == ActivityTypes.Message).ToList();

                return responses;
            });
        }

        public List<Activity> SentActivities { get; internal set; } = new List<Activity>();
    }


}
