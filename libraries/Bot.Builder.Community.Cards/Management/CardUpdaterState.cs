using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Cards.Management
{
    public class CardUpdaterState
    {
        [JsonProperty("activitiesByCard")]
        public Dictionary<string, CardActivity> ActivitiesByCard { get; set; }
    }
}