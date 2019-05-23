using Bot.Builder.Community.Adapters.Twitter.Webhooks.Models.Twitter;

namespace Bot.Builder.Community.Adapters.Twitter.Webhooks.Models
{
    public class DirectMessageEvent : TwitterEvent
    {

        public TwitterUser Recipient { get; set; }
        public TwitterUser Sender { get; set; }

        public string MessageText { get; set; }
        public TwitterEntities MessageEntities { get; set; }

        public string JsonSource { get; set; }
    }
}

