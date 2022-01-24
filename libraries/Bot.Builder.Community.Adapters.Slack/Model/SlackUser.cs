using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Slack.Model
{
    public class SlackUser
    {
        public string Id { get; set; }

        public string Username { get; set; }

        [JsonProperty(PropertyName = "team_id")]
        public string TeamId { get; set; }
    }
}
