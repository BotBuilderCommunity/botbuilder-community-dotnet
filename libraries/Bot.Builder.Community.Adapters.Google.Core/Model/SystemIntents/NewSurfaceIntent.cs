using System.Collections.Generic;
using System.Linq;
using Bot.Builder.Community.Adapters.Google.Core.Model.Request;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Google.Core.Model.SystemIntents
{
    public class NewSurfaceIntent : SystemIntent
    {
        public NewSurfaceIntent()
        {
            Intent = "actions.intent.NEW_SURFACE";
        }

        public NewSurfaceInputValueData InputValueData { get; set; }
    }

    public class NewSurfaceInputValueData : IntentInputValueData
    {
        public NewSurfaceInputValueData()
        {
            Type = "type.googleapis.com/google.actions.v2.NewSurfaceValueSpec";
        }

        public List<string> Capabilities { get; } = new List<string>();

        public string Context { get; set; }

        public string NotificationTitle { get; set; }
    }
}