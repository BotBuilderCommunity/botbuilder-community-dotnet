namespace Bot.Builder.Community.Components.Handoff.ServiceNow.Models
{
    public class ServiceNowConversationRecord
    {
        public string ConversationId { get; set; }
        public string ServiceNowTenant { get; set; }
        public string ServiceNowUserName { get; set; }
        public string ServiceNowPassword { get; set; }
        public string ServiceNowAuthConnectionName { get; set; }
        public string UserId { get; set; }
        public string EmailId { get; set; }
        public bool IsClosed { get; set; }
        public string Timezone { get; set; }
    }
}
