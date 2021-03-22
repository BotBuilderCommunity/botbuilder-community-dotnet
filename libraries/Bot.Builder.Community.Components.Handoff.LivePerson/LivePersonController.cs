using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Bot.Builder.Community.Components.Handoff.LivePerson.Models;
using Bot.Builder.Community.Components.Handoff.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Components.Handoff.LivePerson
{
    [ApiController]
    [Route("api/liveperson")]
    public class LivePersonHandoffController : HandoffController
    {
        private readonly BotAdapter _adapter;
        private readonly ILivePersonCredentialsProvider _credentials;
        private readonly IBot _bot;

        public LivePersonHandoffController(BotAdapter adapter, IBot bot, ILivePersonCredentialsProvider credentials, ConversationHandoffRecordMap conversationHandoffRecordMap) : base(conversationHandoffRecordMap)
        {
            _credentials = credentials;
            _adapter = adapter;
            _bot = bot;
        }

        [HttpPost]
        [HttpGet]
        public async Task PostAsync()
        {
            using (var sr = new StreamReader(Request.Body))
            {
                var body = await sr.ReadToEndAsync();

                if (!Authenticate(Request, Response, body))
                {
                    return;
                }

                var webhookData = JsonConvert.DeserializeObject<WebhookData>(body);

                if (webhookData != null)
                {
                    switch (webhookData.type)
                    {
                        case "cqm.ExConversationChangeNotification":
                            await HandleExConversationChangeNotification(body);
                            break;
                        case "ms.MessagingEventNotification":
                            switch (webhookData.body?.changes?.FirstOrDefault()?.@event.type)
                            {
                                case "ChatStateEvent":
                                    await HandleChatStateEvent(body);
                                    break;
                                case "AcceptStatusEvent":
                                    await HandleAcceptStatusEvent(body);
                                    break;
                                case "ContentEvent":
                                    await HandleContentEvent(body);
                                    break;
                                case "RichContentEvent":
                                    break;
                            }

                            break;
                    }
                }
            }

            Response.StatusCode = (int)HttpStatusCode.OK;
        }

        private async Task HandleContentEvent(string body)
        {
            var webhookData = JsonConvert.DeserializeObject<WebhookData>(body);

            foreach (var change in webhookData.body.changes)
            {
                if (change?.@event?.type == "ContentEvent" && change?.originatorMetadata?.role == "ASSIGNED_AGENT")
                {
                    if (change.@event.message != null)
                    {
                        var humanActivity = MessageFactory.Text(change.@event.message);

                        if (await ConversationHandoffRecordMap.GetByRemoteConversationId(change.conversationId) is LivePersonHandoffRecord handoffRecord)
                        {
                            if (!handoffRecord.ConversationRecord.IsClosed)
                            {
                                MicrosoftAppCredentials.TrustServiceUrl(handoffRecord.ConversationReference.ServiceUrl);
                                
                                await (_adapter).ContinueConversationAsync(
                                    _credentials.MsAppId,
                                    handoffRecord.ConversationReference,
                                    (turnContext, cancellationToken) => turnContext.SendActivityAsync(humanActivity, cancellationToken), default);
                            }
                        }
                        else
                        {
                            // The bot has no record of this conversation, this should not happen
                            throw new Exception("Cannot find conversation");
                        }
                    }
                }
            }
        }

        private async Task HandleAcceptStatusEvent(string body)
        {
            var webhookData = JsonConvert.DeserializeObject<Models.AcceptStatusEvent.WebhookData>(body);

            foreach (var change in webhookData.body.changes)
            {
                if (change?.originatorMetadata?.role == "ASSIGNED_AGENT")
                {
                    // Agent has accepted the conversation
                    var convId = change?.conversationId;

                    if (await ConversationHandoffRecordMap.GetByRemoteConversationId(change.conversationId) is LivePersonHandoffRecord handoffRecord)
                    {
                        if (handoffRecord.ConversationRecord.IsAcknowledged || handoffRecord.ConversationRecord.IsClosed)
                        {
                            // Already acknowledged this one
                            break;
                        }

                        var conversationRecord = new LivePersonConversationRecord()
                        {
                            AppJWT = handoffRecord.ConversationRecord.AppJWT,
                            ConsumerJWS = handoffRecord.ConversationRecord.ConsumerJWS,
                            MessageDomain = handoffRecord.ConversationRecord.MessageDomain,
                            ConversationId = handoffRecord.ConversationRecord.ConversationId,
                            IsClosed = handoffRecord.ConversationRecord.IsClosed,
                            IsAcknowledged = true
                        };

                        var updatedHandoffRecord = new LivePersonHandoffRecord(handoffRecord.ConversationReference, conversationRecord);

                        // Update atomically -- only one will succeed
                        if (ConversationHandoffRecordMap.TryUpdate(convId, updatedHandoffRecord, handoffRecord))
                        {
                            var eventActivity = EventFactory.CreateHandoffStatus(
                                updatedHandoffRecord.ConversationReference.Conversation, "accepted") as Activity;

                            //await _adapter.ContinueConversationAsync(_credentials.MsAppId, eventActivity, _bot.OnTurnAsync, default);

                            //TEMPORARY WORKAROUND UNTIL CLOUDADAPTER IS IN PLACE SO ABOVE LINE WILL WORK
                            await (_adapter).ContinueConversationAsync(
                                _credentials.MsAppId,
                                handoffRecord.ConversationReference,
                                (turnContext, cancellationToken) => turnContext.SendActivityAsync(eventActivity, cancellationToken), default);
                        }
                    }
                }
            }
        }

        private async Task HandleChatStateEvent(string body)
        {
            var webhookData = JsonConvert.DeserializeObject<Models.ChatStateEvent.WebhookData>(body);

            foreach (var change in webhookData.body.changes)
            {
                if (change?.@event?.chatState == "COMPOSING")
                {
                    if (await ConversationHandoffRecordMap.GetByRemoteConversationId(change.conversationId) is LivePersonHandoffRecord handoffRecord)
                    {
                        var typingActivity = new Activity
                        {
                            Type = ActivityTypes.Typing
                        };

                        await _adapter.ContinueConversationAsync(
                            _credentials.MsAppId,
                            handoffRecord.ConversationReference,
                            (turnContext, cancellationToken) => turnContext.SendActivityAsync(typingActivity, cancellationToken), default);
                    }
                }
            }
        }

        private async Task HandleExConversationChangeNotification(string body)
        {
            var webhookData = JsonConvert.DeserializeObject<Models.ExConversationChangeNotification.WebhookData>(body);

            foreach (var change in webhookData.body.changes)
            {
                string state = change?.result?.conversationDetails?.state;
                switch (state)
                {
                    case "CLOSE":
                        {
                            // Agent has closed the conversation
                            var conversationId = change?.result?.convId;

                            if (await ConversationHandoffRecordMap.GetByRemoteConversationId(conversationId) is LivePersonHandoffRecord handoffRecord)
                            {
                                var eventActivity = EventFactory.CreateHandoffStatus(handoffRecord.ConversationReference.Conversation, "completed") as Activity;

                                //await _adapter.ContinueConversationAsync(_credentials.MsAppId, eventActivity, _bot.OnTurnAsync, default);

                                //TEMPORARY WORKAROUND UNTIL CLOUDADAPTER IS IN PLACE SO ABOVE LINE WILL WORK
                                await (_adapter).ContinueConversationAsync(
                                    _credentials.MsAppId,
                                    handoffRecord.ConversationReference,
                                    (turnContext, cancellationToken) => turnContext.SendActivityAsync(eventActivity, cancellationToken), default);
                            }
                        }

                        break;

                    case "OPEN":
                        break;
                }
            }
        }

        private bool Authenticate(HttpRequest request, HttpResponse response, string body)
        {
            // https://github.com/LivePersonInc/developers-community/blob/ae8890694cb9b3be797ca382e9ad7395382aed25/pages/documents/MessagingChannels/ConnectorAPI/webhooks/security.md#authentication
            if (!request.Headers.ContainsKey("X-Liveperson-Signature") || !request.Headers.ContainsKey("X-Liveperson-Client-Id") || !request.Headers.ContainsKey("X-Liveperson-Account-Id"))
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                return false;
            }

            using (var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(_credentials.LpAppSecret)))
            {
                var hash = hmac.ComputeHash(Encoding.ASCII.GetBytes(body));
                var signature = $"sha1={Convert.ToBase64String(hash)}";
                if (signature != request.Headers["X-Liveperson-Signature"])
                {
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return false;
                }
            }

            var account = request.Headers["X-Liveperson-Account-Id"];
            var clientId = request.Headers["X-Liveperson-Client-Id"];
            if (account != _credentials.LpAccount || clientId != _credentials.LpAppId)
            {
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return false;
            }

            response.StatusCode = (int)HttpStatusCode.OK;
            return true;
        }
    }
}
