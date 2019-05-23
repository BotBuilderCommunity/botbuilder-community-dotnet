namespace Bot.Builder.Community.Adapters.Twitter.Webhooks.Models.Twitter
{
    public class UserMentionEntity
    {
        public string screen_name { get; set; }
        public string name { get; set; }
        public long id { get; set; }
        public string id_str { get; set; }
        public int[] indices { get; set; }
 
    }
}
