using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Alexa.Tests.Helpers
{
    public static class ActivityHelper
    {
        /// <summary>
        /// Anonymize the activity by removing type information from properties that don't include type information (ie are passed as objects).
        /// </summary>
        public static Activity GetAnonymizedActivity(Activity activity) => JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(activity, HttpHelper.BotMessageSerializerSettings), HttpHelper.BotMessageSerializerSettings);
    }
}
