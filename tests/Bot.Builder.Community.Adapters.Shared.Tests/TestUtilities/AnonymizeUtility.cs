using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Shared.Tests.TestUtilities
{
    public static class AnonymizeUtility
    {
        public static Attachment Anonymize(this Attachment attachment) => JsonConvert.DeserializeObject<Attachment>(JsonConvert.SerializeObject(attachment));

        public static Activity Anonymize(this Activity activity) => JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(activity));
    }
}
