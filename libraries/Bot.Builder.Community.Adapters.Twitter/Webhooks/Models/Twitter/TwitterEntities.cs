using System.Collections.Generic;

namespace Bot.Builder.Community.Adapters.Twitter.Webhooks.Models.Twitter
{
    public class TwitterEntities
    {
        public List<HashtagEntity> hashtags { get; set; }
        public List<SymbolEntity> symbols { get; set; }
        public List<UserMentionEntity> user_mentions { get; set; }
        public List<UrlEntity> urls { get; set; }
        public List<MediaEntity> media { get; set; }
    }

}
