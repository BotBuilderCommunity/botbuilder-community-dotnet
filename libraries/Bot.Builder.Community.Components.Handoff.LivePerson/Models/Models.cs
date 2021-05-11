namespace Bot.Builder.Community.Components.Handoff.LivePerson.Models
{
    // Start conversation

    // Send conversation

    // Webhook models:

    namespace Webhook
    {
        public class Body
        {
            public Change[] changes { get; set; }
        }
    }

    namespace ChatStateEvent
    {
        public class WebhookData
        {
            public string kind { get; set; }
            public Body body { get; set; }
            public string type { get; set; }
        }

        public class Body
        {
            public Change[] changes { get; set; }
        }

        public class Change
        {
            public string originatorId { get; set; }
            public Originatormetadata originatorMetadata { get; set; }
            public Event @event { get; set; }
            public string conversationId { get; set; }
            public string dialogId { get; set; }
        }

        public class Originatormetadata
        {
            public string id { get; set; }
            public string role { get; set; }
        }

        public class Event
        {
            public string type { get; set; }
            public string chatState { get; set; }
        }
    }

    namespace AcceptStatusEvent
    {
        public class WebhookData
        {
            public string kind { get; set; }
            public Body body { get; set; }
            public string type { get; set; }
        }

        public class Body
        {
            public Change[] changes { get; set; }
        }

        public class Change
        {
            public int sequence { get; set; }
            public string originatorId { get; set; }
            public Originatormetadata originatorMetadata { get; set; }
            public long serverTimestamp { get; set; }
            public Event _event { get; set; }
            public string conversationId { get; set; }
            public string dialogId { get; set; }
        }

        public class Originatormetadata
        {
            public string id { get; set; }
            public string role { get; set; }
        }

        public class Event
        {
            public string type { get; set; }
            public string status { get; set; }
            public int[] sequenceList { get; set; }
        }
    }

    namespace ExConversationChangeNotification
    {
        public class WebhookData
        {
            public string kind { get; set; }
            public Body body { get; set; }
            public string type { get; set; }
        }

        public class Body
        {
            public long sentTs { get; set; }
            public Change[] changes { get; set; }
        }

        public class Change
        {
            public string type { get; set; }
            public Result result { get; set; }
        }

        public class Result
        {
            public string convId { get; set; }
            public long effectiveTTR { get; set; }
            public Conversationdetails conversationDetails { get; set; }
        }

        public class Conversationdetails
        {
            public string skillId { get; set; }
            public Participant[] participants { get; set; }
            public Dialog[] dialogs { get; set; }
            public string brandId { get; set; }
            public string state { get; set; }
            public string stage { get; set; }
            public long startTs { get; set; }
            public long metaDataLastUpdateTs { get; set; }
            public Ttr ttr { get; set; }
            public Conversationhandlerdetails conversationHandlerDetails { get; set; }
        }

        public class Ttr
        {
            public string ttrType { get; set; }
            public int value { get; set; }
        }

        public class Conversationhandlerdetails
        {
            public string accountId { get; set; }
            public string skillId { get; set; }
        }

        public class Participant
        {
            public string id { get; set; }
            public string role { get; set; }
        }

        public class Dialog
        {
            public string dialogId { get; set; }
            public Participantsdetail[] participantsDetails { get; set; }
            public string dialogType { get; set; }
            public string channelType { get; set; }
            public Metadata metaData { get; set; }
            public string state { get; set; }
            public long creationTs { get; set; }
            public long metaDataLastUpdateTs { get; set; }
        }

        public class Metadata
        {
            public string appInstallId { get; set; }
        }

        public class Participantsdetail
        {
            public string id { get; set; }
            public string role { get; set; }
            public string state { get; set; }
        }

    }
}

