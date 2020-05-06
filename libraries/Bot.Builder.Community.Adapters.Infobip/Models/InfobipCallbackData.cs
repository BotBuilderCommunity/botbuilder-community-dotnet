using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Bot.Builder.Community.Adapters.Infobip.Models
{
    public class InfobipCallbackData: Entity
    {
        public InfobipCallbackData(Dictionary<string, string> callbackData)
        {
            var serializer = new JsonSerializer();
            Type = InfobipConstants.InfobipCallbackDataEntityType;
            Properties = JObject.FromObject(callbackData, serializer);
        }
    }
}
