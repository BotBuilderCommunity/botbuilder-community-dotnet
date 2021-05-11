using System.Threading.Tasks;
using Bot.Builder.Community.Components.Handoff.LivePerson.Models;
using Bot.Builder.Community.Components.Handoff.Shared;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Components.Handoff.LivePerson
{
    public class LivePersonHandoffMiddleware : HandoffMiddleware
    {
        private readonly ConversationHandoffRecordMap _conversationHandoffRecordMap;
        private readonly ILivePersonCredentialsProvider _creds;

        public LivePersonHandoffMiddleware(ConversationHandoffRecordMap conversationHandoffRecordMap, ILivePersonCredentialsProvider creds) : base(conversationHandoffRecordMap)
        {
            _conversationHandoffRecordMap = conversationHandoffRecordMap;
            _creds = creds;
        }

        public override async Task RouteActivityToExistingHandoff(ITurnContext turnContext, HandoffRecord handoffRecord)
        {
            var livePersonHandoffRecord = handoffRecord as LivePersonHandoffRecord;

            if (livePersonHandoffRecord != null)
            {
                var account = _creds.LpAccount;
                var message = LivePersonConnector.MakeLivePersonMessage(0,
                    livePersonHandoffRecord.RemoteConversationId,
                    turnContext.Activity.Text);

                await LivePersonConnector.SendMessageToConversationAsync(account,
                    livePersonHandoffRecord.ConversationRecord.MessageDomain,
                    livePersonHandoffRecord.ConversationRecord.AppJWT,
                    livePersonHandoffRecord.ConversationRecord.ConsumerJWS,
                    message).ConfigureAwait(false);
            }
        }

        public override async Task<HandoffRecord> Escalate(ITurnContext turnContext, IEventActivity handoffEvent)
        {
            var account = _creds.LpAccount;
            var clientId = _creds.LpAppId;
            var clientSecret = _creds.LpAppSecret;
            
            var conversationRecord = await LivePersonConnector.EscalateToAgentAsync(turnContext, handoffEvent, account, clientId, clientSecret, _conversationHandoffRecordMap);
            
            return new LivePersonHandoffRecord(turnContext.Activity.GetConversationReference(), conversationRecord);
        }
    }
}
