using System.Collections.Generic;
using Bot.Builder.Community.Adapters.Zoom.Models;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Linq;
using Bot.Builder.Community.Adapters.Zoom.Attachments;

namespace Bot.Builder.Community.Adapters.Zoom
{
    public class ZoomRequestMapper
    {
        private readonly ZoomRequestMapperOptions _options;
        private ILogger _logger;

        public ZoomRequestMapper(ZoomRequestMapperOptions options, ILogger logger)
        {
            _options = options;
            _logger = logger ?? NullLogger.Instance;
        }

        public Activity RequestToActivity(ZoomRequest request)
        {
            var activity = new Activity();
            activity = SetGeneralActivityProperties(activity, request);

            switch (request.Event)
            {
                case "bot_notification":
                    var botNotificationPayload = request.Payload.ToObject<BotNotificationPayload>();
                    activity.Type = ActivityTypes.Message;
                    activity.Text = botNotificationPayload.Cmd;
                    break;
                case "interactive_message_actions":
                    var messageActionsPayload = request.Payload.ToObject<InteractiveMessageActionsPayload>();
                    activity.Type = ActivityTypes.Message;
                    activity.Text = messageActionsPayload.ActionItem.Text;
                    break;
                case "interactive_message_fields_editable":
                    var fieldsEditablePayload = request.Payload.ToObject<InteractiveMessageFieldsEditablePayload>();
                    activity.Value = fieldsEditablePayload;
                    activity.Type = ActivityTypes.Event;
                    activity.Name = request.Event;
                    break;
                case "interactive_message_select":
                    var selectPayload = request.Payload.ToObject<InteractiveMessageSelectPayload>();
                    activity.Value = selectPayload;
                    activity.Type = ActivityTypes.Event;
                    activity.Name = request.Event;
                    break;
                default:
                    activity.Type = ActivityTypes.Event;
                    activity.Name = request.Event;
                    activity.Value = request.Payload;
                    break;
            }

            return activity;
        }

        private Activity SetGeneralActivityProperties(Activity activity, ZoomRequest request)
        {
            var botNotificationPayload = request.Payload.ToObject<Payload>();
            activity.Conversation = new ConversationAccount();
            activity.From = new ChannelAccount(botNotificationPayload.UserJid, botNotificationPayload.UserName);
            activity.ChannelData = request;
            activity.Recipient = new ChannelAccount(botNotificationPayload.ToJid);
            activity.ChannelId = "zoom";
            activity.Conversation = new ConversationAccount(id: $"{botNotificationPayload.AccountId}:{botNotificationPayload.ChannelName}");
            return activity;
        }

        public ChatResponse ActivityToZoom(Activity activity)
        {
            var message = new ChatResponse()
            {
                ToJid = activity.From.Id,
                AccountId = activity.Conversation.Id.Split(':')[0],
                RobotJid = _options.RobotJid,
                Content = new ChatResponseContent()
                {
                    Head = !string.IsNullOrEmpty(activity.Text) ? new Head() { Text = $"{activity.Text}" } : null,
                    Body = new List<BodyItem>()
                }
            };

            if (activity.SuggestedActions?.Actions != null && activity.SuggestedActions.Actions.Any())
            {
                var actions = new List<ZoomAction>();

                foreach (var suggestedAction in activity.SuggestedActions.Actions)
                {
                    actions.Add(new ZoomAction()
                    {
                        Style = "Default", 
                        Text = suggestedAction.DisplayText, 
                        Value = suggestedAction.Text
                    });
                }

                message.Content.Body.Add(new ActionsItem()
                {
                    Actions = actions
                });
            }

            ProcessActivityAttachments(activity, message);

            return message;
        }

        private void ProcessActivityAttachments(Activity activity, ChatResponse response)
        {
            activity.ConvertAttachmentContent();

            ProcessAttachment<MessageBodyItemWithLink>(ZoomAttachmentContentTypes.MessageWithLink, activity, response);
            ProcessAttachment<FieldsBodyItem>(ZoomAttachmentContentTypes.Fields, activity, response);
            ProcessAttachment<AttachmentBodyItem>(ZoomAttachmentContentTypes.Attachment, activity, response);
            ProcessAttachment<DropdownBodyItem>(ZoomAttachmentContentTypes.Dropdown, activity, response);
        }

        private static void ProcessAttachment<T>(string contentType, Activity activity, ChatResponse response) where T : BodyItem
        {
            var messageWithLinks = activity.Attachments?
                .Where(a => a.ContentType == contentType)
                .Select(a => (T)a.Content).ToList();

            if (messageWithLinks != null && messageWithLinks.Any())
                response.Content.Body.AddRange(messageWithLinks);
        }
    }
}
