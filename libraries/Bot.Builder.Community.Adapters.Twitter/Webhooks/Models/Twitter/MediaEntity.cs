namespace Bot.Builder.Community.Adapters.Twitter.Webhooks.Models.Twitter
{
 
    public class MediaEntity
    {
        public long id { get; set; }
        public string id_str { get; set; }
        public int[] indices { get; set; }
        public string media_url { get; set; }
        public string media_url_https { get; set; }
        public string url { get; set; }
        public string display_url { get; set; }
        public string expanded_url { get; set; }
        public string type { get; set; }
        public Sizes sizes { get; set; }
    }

    public class Sizes
    {
        public TwitterSizeMedium medium { get; set; }
        public TwitterSizeThumb thumb { get; set; }
        public TwitterSizeSmall small { get; set; }
        public TwitterSizeLarge large { get; set; }
    }

    public class TwitterSizeMedium
    {
        public int w { get; set; }
        public int h { get; set; }
        public string resize { get; set; }
    }

    public class TwitterSizeThumb
    {
        public int w { get; set; }
        public int h { get; set; }
        public string resize { get; set; }
    }

    public class TwitterSizeSmall
    {
        public int w { get; set; }
        public int h { get; set; }
        public string resize { get; set; }
    }

    public class TwitterSizeLarge
    {
        public int w { get; set; }
        public int h { get; set; }
        public string resize { get; set; }
    }

}
