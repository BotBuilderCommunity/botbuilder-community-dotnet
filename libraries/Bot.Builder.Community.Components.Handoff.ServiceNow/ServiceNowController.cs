using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Bot.Builder.Community.Components.Handoff.ServiceNow.Models;
using Bot.Builder.Community.Components.Handoff.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Components.Handoff.ServiceNow
{
    // Controller to receive async responses from ServiceNow Virtual Agent
    [ApiController]
    [Route("api/ServiceNow")]
    public class ServiceNowHandoffController : HandoffController
    {
        private readonly BotAdapter _adapter;
        private readonly IServiceNowCredentialsProvider _credentials;
        private readonly IBot _bot;

        public ServiceNowHandoffController(BotAdapter adapter, IBot bot, IServiceNowCredentialsProvider credentials, ConversationHandoffRecordMap conversationHandoffRecordMap) : base(conversationHandoffRecordMap)
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

                // Doesn't appear to be a way with ServiceNow to authenticate incoming responses
                //if (!Authenticate(Request, Response, body))
                //{
                //    return;
                //}

                var responseMessage = JsonConvert.DeserializeObject<ServiceNowResponseMessage>(body);

                if (responseMessage != null)
                {
                    await HandleContentEvent(responseMessage);

                    // If ServiceNow indicates it's completed handoff from it's perspective we end handoff
                    if (responseMessage.completed)
                    {
                        if (await ConversationHandoffRecordMap.GetByRemoteConversationId(responseMessage.clientSessionId) is ServiceNowHandoffRecord handoffRecord)
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

                    Response.StatusCode = (int)HttpStatusCode.OK;
                }
                else
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
            }
        }

        private async Task HandleContentEvent(ServiceNowResponseMessage responseMessage)
        {
            foreach (var item in responseMessage.body)
            {
                IMessageActivity responseActivity;

                // Map ServiceNow UX controls to Bot Framework concepts. This will need refinement as broader experiences are used but this covers a broad range of out-of-box ServiceNow response types.
                switch (item.uiType)
                {
                    case "TopicPickerControl":
                    case "ItemPicker":
                    case "Picker":

                        var options = item.options.Select(o => o.label);

                        responseActivity = MessageFactory.SuggestedActions(options);
                        responseActivity.AsMessageActivity().Text = item.promptMsg ?? item.label;
                        break;
                    case "GroupedPartsOutputControl":
                        responseActivity = MessageFactory.Text(item.header);
                        responseActivity.AttachmentLayout = "carousel";
                        responseActivity.Attachments = new List<Microsoft.Bot.Schema.Attachment>();
                        foreach (var action in item.values)
                        {
                            var cardAction = new CardAction("openUrl", action.label, value: action.action);
                            var card = new HeroCard(action.label, null, action.description, null, null, cardAction);
                            responseActivity.Attachments.Add(card.ToAttachment());
                        }
                        break;
                    case "OutputText":
                        responseActivity = MessageFactory.Text(item.value ?? item.label);
                        break;

                    default:
                        responseActivity = MessageFactory.Text(item.value ?? item.label);
                        break;
                }

                if (await ConversationHandoffRecordMap.GetByRemoteConversationId(responseMessage.clientSessionId) is ServiceNowHandoffRecord handoffRecord)
                {
                    if (!handoffRecord.ConversationRecord.IsClosed)
                    {
                        MicrosoftAppCredentials.TrustServiceUrl(handoffRecord.ConversationReference.ServiceUrl);

                        await (_adapter).ContinueConversationAsync(
                            _credentials.MsAppId,
                            handoffRecord.ConversationReference,
                            (turnContext, cancellationToken) => turnContext.SendActivityAsync(responseActivity, cancellationToken), default);
                    }
                }
                else
                {
                    // The bot has no record of this conversation, this should not happen
                    throw new Exception("Cannot find conversation");
                }
            }
        }

        private bool Authenticate(HttpRequest request, HttpResponse response, string body)
        {
            throw new NotImplementedException();
        }
    }
}
