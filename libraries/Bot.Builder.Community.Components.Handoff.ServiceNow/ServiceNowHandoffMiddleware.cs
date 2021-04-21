using System.Threading.Tasks;
using Bot.Builder.Community.Components.Handoff.ServiceNow.Models;
using Bot.Builder.Community.Components.Handoff.Shared;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Components.Handoff.ServiceNow
{
    public class ServiceNowHandoffMiddleware : HandoffMiddleware
    {
        private readonly ConversationHandoffRecordMap _conversationHandoffRecordMap;
        private readonly IServiceNowCredentialsProvider _creds;

        public ServiceNowHandoffMiddleware(ConversationHandoffRecordMap conversationHandoffRecordMap, IServiceNowCredentialsProvider creds) : base(conversationHandoffRecordMap)
        {
            _conversationHandoffRecordMap = conversationHandoffRecordMap;
            _creds = creds;
        }

        public override async Task RouteActivityToExistingHandoff(ITurnContext turnContext, HandoffRecord handoffRecord)
        {
            var serviceNowHandoffRecord = handoffRecord as ServiceNowHandoffRecord;

            // Retrieve an oAuth token for ServiceNow which we'll pass on this turn
            var botAdapter = (BotFrameworkAdapter)turnContext.Adapter;
            var tokenResponse = await botAdapter.GetUserTokenAsync(turnContext, serviceNowHandoffRecord.ConversationRecord.ServiceNowAuthConnectionName, null);

            if (serviceNowHandoffRecord != null)
            {
                var message = ServiceNowConnector.MakeServiceNowMessage(0,
                    serviceNowHandoffRecord.RemoteConversationId,
                    turnContext.Activity.Text,
                    serviceNowHandoffRecord.ConversationRecord.Timezone,
                    turnContext.Activity.Locale,
                    serviceNowHandoffRecord.ConversationRecord.UserId,
                    serviceNowHandoffRecord.ConversationRecord.EmailId);

                await ServiceNowConnector.SendMessageToConversationAsync(
                    serviceNowHandoffRecord.ConversationRecord.ServiceNowTenant,
                    tokenResponse.Token,
                    message).ConfigureAwait(false);
            }
        }
        public override async Task<HandoffRecord> Escalate(ITurnContext turnContext, IEventActivity handoffEvent)
        {
            var serviceNowTenant = _creds.ServiceNowTenant;
            var serviceNowAuthConnectionName = _creds.ServiceNowAuthConnectionName;

            var conversationRecord = await ServiceNowConnector.EscalateToAgentAsync(turnContext, handoffEvent, serviceNowTenant, serviceNowAuthConnectionName, _conversationHandoffRecordMap);

            await turnContext.SendActivityAsync(Activity.CreateTraceActivity("ServiceNowVirtualAgent", "Handoff initiated"));

            return new ServiceNowHandoffRecord(turnContext.Activity.GetConversationReference(), conversationRecord);
        }

    }
}
