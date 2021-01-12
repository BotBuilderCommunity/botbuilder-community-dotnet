using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Bot.Builder.Community.Adapters.Infobip.Core
{
    public static class InfobipExtensionsCore
    {
        public static void AddInfobipCallbackData(this Activity activity, IDictionary<string, string> callbackData)
        {
            var serializer = new JsonSerializer();
            var entity = new Entity
            {
                Type = InfobipEntityType.CallbackData,
                Properties = JObject.FromObject(callbackData, serializer)
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
