using System;

namespace Bot.Builder.Community.Components.Adapters.GoogleBusiness.Core.Model
{
    public class GoogleBusinessRequest
    {
        public Message Message { get; set; }
        public Context Context { get; set; }
        public DateTime SendTime { get; set; }
        public string ConversationId { get; set; }
        public string RequestId { get; set; }
        public string Agent { get; set; }
        public UserStatus UserStatus { get; set; }
        public SuggestionResponse SuggestionResponse { get; set; }
        public AuthenticationResponse AuthenticationResponse { get; set; }
        public SurveyResponse SurveyResponse { get; set; }
        public MessageReceipts Receipts { get; set; }
    }
}