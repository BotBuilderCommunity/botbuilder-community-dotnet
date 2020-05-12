using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapters.Google.Core.Model.Request
{
    public class DialogFlowRequest
    {
        public string ResponseId { get; set; }
        public QueryResult QueryResult { get; set; }
        public OriginalDetectIntentRequest OriginalDetectIntentRequest { get; set; }
        public string Session { get; set; }
    }

    public class QueryResult
    {
        public string QueryText { get; set; }
        public string Action { get; set; }
        public JObject Parameters { get; set; }
        public bool AllRequiredParamsPresent { get; set; }
        public string FulfilmentText { get; set; }
        public string[] FulfilmentMessages { get; set; }
        public OutputContext[] OutputContexts { get; set; }
        public Intent Intent { get; set; }
        public double IntentDetectionConfidence { get; set; }
        public JObject DiagnosticInfo { get; set; }
        public string LanguageCode { get; set; }
    }

    public class OriginalDetectIntentRequest
    {
        public string Source { get; set; }
        public string Version { get; set; }
        public ConversationRequest Payload { get; set; }
    }

    public class Intent
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool IsFallback { get; set; }
    }

    public class OutputContext
    {
        public string Name { get; set; }
    }
}
