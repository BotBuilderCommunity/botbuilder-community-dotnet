using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using Bot.Builder.Community.Components.Handoff.ServiceNow.Models;
using Bot.Builder.Community.Components.Handoff.Shared;
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
                var responseMessage = JsonConvert.DeserializeObject<ServiceNowResponseMessage>(body);

                if (responseMessage != null)
                {
                    // Do we have a matching handoff record for the incoming ConversationID from ServiceNow?
                    var handoffRecord = await ConversationHandoffRecordMap.GetByRemoteConversationId(responseMessage.clientSessionId) as ServiceNowHandoffRecord;
                    if (handoffRecord != null)
                    {
                        await HandleContentEvent(responseMessage);

                        // If ServiceNow indicates it's completed handoff from it's perspective we end handoff and return control.
                        if (responseMessage.completed)
                        {
                            var eventActivity = EventFactory.CreateHandoffStatus(handoffRecord.ConversationReference.Conversation, "completed") as Activity;
                            await (_adapter).ContinueConversationAsync(
                                 _credentials.MsAppId,
                                 handoffRecord.ConversationReference,
                                 (turnContext, cancellationToken) => turnContext.SendActivityAsync(eventActivity, cancellationToken), default);

                            var traceActivity = Activity.CreateTraceActivity(
                                "ServiceNowVirtualAgent",
                                label: "ServiceNowHandoff->Handoff completed");

                             await (_adapter).ContinueConversationAsync(
                                  _credentials.MsAppId,
                                  handoffRecord.ConversationReference,
                                  (turnContext, cancellationToken) => turnContext.SendActivityAsync(traceActivity, cancellationToken), default);
                        }

                        Response.StatusCode = (int)HttpStatusCode.OK;
                    }
                    else 
                    {
                        // No matching handoff record for the conversation referenced by ServiceNow
                        Response.StatusCode = (int)HttpStatusCode.NotFound;
                    }
                }
                else
                {
                    // Malformed response from ServiceNow
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
            }
        }

        // A sample mapping from ServiceNow response types to Bot Framework concepts, in many cases this will need to be adapted to a
        // Bot owners specific requirements and styling.
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

                        // Map the picker concept to a basic HeroCard with buttons
                        List<CardAction> cardActions = new List<CardAction>();
                        foreach (var option in item.options)
                        {
                            cardActions.Add(new CardAction("imBack", option.description ?? option.label, value: option.label));
                        }
                        
                        var pickerHeroCard = new HeroCard(buttons: cardActions);
                        responseActivity = MessageFactory.Attachment(pickerHeroCard.ToAttachment());

                        responseActivity.AsMessageActivity().Text = item.promptMsg ?? item.label;
                        break;

                    case "DefaultPicker":

                        // Map the picker concept to a basic HeroCard with buttons
                        List<CardAction> defaultPickerActions = new List<CardAction>();
                        foreach (var option in item.options)
                        {
                            defaultPickerActions.Add(new CardAction("imBack", option.description ?? option.label, value: option.label));
                        }

                        var defaultPickerCard = new HeroCard(buttons: defaultPickerActions);
                        responseActivity = MessageFactory.Attachment(defaultPickerCard.ToAttachment());
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
                    case "OutputHtml":

                        // We can't render HTML inside of conversations.
                        responseActivity = MessageFactory.Text(StripTags(item.value)); 

                        break;
                    case "Boolean":
                        List<CardAction> booleanCardActions = new List<CardAction>();

                        booleanCardActions.Add(new CardAction("imBack", title: "Yes", displayText: "Yes", value:"true" ));
                        booleanCardActions.Add(new CardAction("imBack", title: "No", displayText: "Yes", value:"false"));
                        var booleanHeroCard = new HeroCard(buttons: booleanCardActions);

                        responseActivity = MessageFactory.Attachment(booleanHeroCard.ToAttachment());
                        responseActivity.AsMessageActivity().Text = item.promptMsg ?? item.label;
                        break;

                    case "OutputText":
                        responseActivity = MessageFactory.Text(item.value ?? item.label);
                        break;

                    case "OutputImage":
                        
                        var cardImages = new List<CardImage>();
                        cardImages.Add(new CardImage(url: item.value));

                        var imageHeroCard = new HeroCard(images: cardImages);
                        responseActivity = MessageFactory.Attachment(imageHeroCard.ToAttachment());

                        break;

                    case "OutputLink":

                        var linkHeroCard = new HeroCard(buttons: new List<CardAction> { new CardAction("openUrl", item.value) });
                        responseActivity = MessageFactory.Attachment(linkHeroCard.ToAttachment());
                        responseActivity.AsMessageActivity().Text = item.promptMsg ?? item.label;

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

                        var traceActivity = Activity.CreateTraceActivity(
                            "ServiceNowVirtualAgent",
                            $"Response from ServiceNow Virtual Agent received (Id:{responseMessage.requestId})",
                            label: "ServiceNowHandoff->Response from ServiceNow Virtual Agent");

                        await (_adapter).ContinueConversationAsync(
                            _credentials.MsAppId,
                            handoffRecord.ConversationReference,
                            (turnContext, cancellationToken) => turnContext.SendActivityAsync(traceActivity, cancellationToken), default);
                    }
                }
                else
                {
                    // The bot has no record of this conversation, this should not happen
                    throw new Exception("Cannot find conversation");
                }
            }
        }

        // Crude flattening of HTML content into just it's string content. ServiceNow uses HTML in some scenarios which can't be rendered in most conversational canvases.
        public static string StripTags(string htmlResponse)
        {
            // create whitespace between html elements, so that words do not run together
            htmlResponse = htmlResponse.Replace(">", "> ");
           
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlResponse);

            // strip html decoded text from html
            string text = HttpUtility.HtmlDecode(doc.DocumentNode.InnerText);

            // replace all whitespace with a single space and remove leading and trailing whitespace
            return Regex.Replace(text, @"\s+", " ").Trim();
        }
    }
}
