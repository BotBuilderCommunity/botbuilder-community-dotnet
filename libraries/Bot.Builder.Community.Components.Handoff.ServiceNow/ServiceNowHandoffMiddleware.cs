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
                    serviceNowHandoffRecord.ConversationRecord.ServiceNowUserName,
                    serviceNowHandoffRecord.ConversationRecord.ServiceNowPassword,
                    message).ConfigureAwait(false);
            }
        }
        public override async Task<HandoffRecord> Escalate(ITurnContext turnContext, IEventActivity handoffEvent)
        {
            var serviceNowTenant = _creds.ServiceNowTenant;
            var userName = _creds.UserName; 
            var password = _creds.Password;

            var conversationRecord = await ServiceNowConnector.EscalateToAgentAsync(turnContext, handoffEvent, serviceNowTenant, userName, password, _conversationHandoffRecordMap);

            return new ServiceNowHandoffRecord(turnContext.Activity.GetConversationReference(), conversationRecord);
        }

    }
}
