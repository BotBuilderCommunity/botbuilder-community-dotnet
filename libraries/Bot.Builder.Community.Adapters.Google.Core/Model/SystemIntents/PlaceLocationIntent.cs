using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Google.Core.Model.SystemIntents
{
    public class PlaceLocationIntent : SystemIntent
    {
        public PlaceLocationIntent()
        {
            Intent = "actions.intent.PLACE";
        }

        public PlaceLocationInputValueData InputValueData { get; set; }
    }

    public class PlaceLocationInputValueData : IntentInputValueData
    {
        public PlaceLocationInputValueData()
        {
            Type = "type.googleapis.com/google.actions.v2.PlaceValueSpec";
        }

        public PlaceLocationIntentDialogSpec DialogSpec { get; set; }
    }

    public class PlaceLocationIntentDialogSpec
    {
        public PlaceLocationIntentDialogSpecExtension Extension { get; set; }
    }

    public class PlaceLocationIntentDialogSpecExtension
    {
        [JsonProperty(PropertyName = "@type")]
        public string Type => "type.googleapis.com/google.actions.v2.PlaceValueSpec.PlaceDialogSpec";

        public string PermissionContext { get; set; }

        public string RequestPrompt { get; set; }
    }
}