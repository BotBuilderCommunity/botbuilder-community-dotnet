using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Components.Handoff.ServiceNow.Models;
using Bot.Builder.Community.Components.Handoff.Shared;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Components.Handoff.ServiceNow
{
    public class ServiceNowHandoffMiddleware : HandoffMiddleware
    {
        private readonly ConversationHandoffRecordMap _conversationHandoffRecordMap;
        private readonly IServiceNowCredentialsProvider _creds;
        private readonly BotFrameworkAuthentication _botFrameworkAuth;

        public ServiceNowHandoffMiddleware(ConversationHandoffRecordMap conversationHandoffRecordMap, IServiceNowCredentialsProvider creds, BotFrameworkAuthentication botFrameworkAuth) : base(conversationHandoffRecordMap)
        {
            _conversationHandoffRecordMap = conversationHandoffRecordMap;
            _creds = creds;
            _botFrameworkAuth = botFrameworkAuth;
        }

        public override async Task RouteActivityToExistingHandoff(ITurnContext turnContext, HandoffRecord handoffRecord)
        {
            var serviceNowHandoffRecord = handoffRecord as ServiceNowHandoffRecord;

            // Retrieve an oAuth token for ServiceNow which we'll pass on this turn
            var claimsIdentity = (ClaimsIdentity)turnContext.TurnState.Get<IIdentity>(BotFrameworkAdapter.BotIdentityKey);
            var userTokenClient = await _botFrameworkAuth.CreateUserTokenClientAsync(claimsIdentity, default(CancellationToken));
            var tokenResponse = await userTokenClient.GetUserTokenAsync(turnContext.Activity.From.Id, _creds.ServiceNowAuthConnectionName, null, null, default(CancellationToken));

            if (tokenResponse != null)
            {
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

                    var traceActivity = Activity.CreateTraceActivity("ServiceNowVirtualAgent", label: "ServiceNowHandoff->Activity forwarded to ServiceNow");
                    await turnContext.SendActivityAsync(traceActivity);

                }
            }
            else
            {
                var traceActivity = Activity.CreateTraceActivity("ServiceNowVirtualAgent", label: "ServiceNowHandoff->No ServiceNow authentication token available.");
                await turnContext.SendActivityAsync(traceActivity);

                throw new Exception("No ServiceNow authentication token available for this user");
            }
        }
        public override async Task<HandoffRecord> Escalate(ITurnContext turnContext, IEventActivity handoffEvent)
        {
            var serviceNowTenant = _creds.ServiceNowTenant;
            var serviceNowAuthConnectionName = _creds.ServiceNowAuthConnectionName;

            var conversationRecord = await ServiceNowConnector.EscalateToAgentAsync(turnContext, handoffEvent, serviceNowTenant, serviceNowAuthConnectionName, _conversationHandoffRecordMap);

            // Forward the activating activity onto ServiceNow
            var handoffRecord = new ServiceNowHandoffRecord(turnContext.Activity.GetConversationReference(), conversationRecord);
            await RouteActivityToExistingHandoff(turnContext, handoffRecord);

            var traceActivity = Activity.CreateTraceActivity("ServiceNowVirtualAgent", label: "ServiceNowHandoff->Handoff initiated");
            await turnContext.SendActivityAsync(traceActivity);

            return handoffRecord;
        }

    }
}
