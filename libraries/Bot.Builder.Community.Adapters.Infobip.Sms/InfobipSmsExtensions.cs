using Bot.Builder.Community.Adapters.Infobip.Sms.Models;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Bot.Builder.Community.Adapters.Infobip.Sms
{
    public static class InfobipSmsExtensions
    {

        public static void AddInfobipOmniSmsMessageOptions(this Activity activity, InfobipOmniSmsMessageOptions options)
        {
            var serializer = new JsonSerializer();
            var entity = new Entity
            {
                Type = InfobipSmsEntityType.SmsMessageOptions,
                Properties = JObject.FromObject(options, serializer)
            };

            AddEntity(activity, entity);
        }

        private static void AddEntity(Activity activity, Entity entity)
        {
            if (activity.Entities == null) activity.Entities = new List<Entity>();
            activity.Entities.Add(entity);
        }
    }
}
