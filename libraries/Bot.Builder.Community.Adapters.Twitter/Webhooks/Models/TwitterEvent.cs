using System;

namespace Bot.Builder.Community.Adapters.Twitter.Webhooks.Models
{
    public class TwitterEvent
    {
        public string Id { get; internal set; }
        public TwitterEventType Type { get; internal set; }
        public DateTime Created { get; internal set; }

    }
}
