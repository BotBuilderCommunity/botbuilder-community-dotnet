namespace Bot.Builder.Community.Components.Handoff.ServiceNow.Models
{
    public class ServiceNowRequestMessage
    {
        public string clientSessionId { get; set; }
        public string requestId { get; set; }
        public string action { get; set; }
        public bool botToBot { get; set; }
        public ServiceNowRequestMessageContent message { get; set; }
        public string userId { get; set; }
        public string emailId { get; set; }
        public long timestamp { get; set; }
        public string timezone { get; set; }
        public contextVariables contextVariables { get; set; }
        public ClientVariables clientVariables { get; set; }
    }
}
