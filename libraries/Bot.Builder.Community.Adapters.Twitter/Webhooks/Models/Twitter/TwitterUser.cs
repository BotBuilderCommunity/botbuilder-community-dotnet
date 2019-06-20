using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Twitter.Webhooks.Models.Twitter
{
    public class TwitterUser
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("created_timestamp")]
        public long CreatedTimestamp { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("screen_name")]
        public string ScreenName { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("_protected")]
        public bool IsProtected { get; set; }

        [JsonProperty("verified")]
        public bool IsVerified { get; set; }

        [JsonProperty("followers_count")]
        public int FollowersCount { get; set; }

        [JsonProperty("friends_count")]
        public int FriendsCount { get; set; }

        [JsonProperty("statuses_count")]
        public int StatusesCount { get; set; }

        [JsonProperty("profile_image_url")]
        public string ProfileImage { get; set; }

        [JsonProperty("profile_image_url_https")]
        public string ProfileImageHttps { get; set; }
    }

}
