using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Xml;
using System.Xml.Linq;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Bot.Builder.Community.Adapters.Alexa.Core.Attachments;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Alexa.Core
{
    public class AlexaHelper
    {
        public static Activity RequestToActivity(SkillRequest skillRequest, string defaultIntentSlotName = "phrase")
        {
            var system = skillRequest.Context.System;

            var activity = new Activity
            {
                ChannelId = "alexa",
                ServiceUrl = $"{system.ApiEndpoint}?token={system.ApiAccessToken}",
                Recipient = new ChannelAccount(system.Application.ApplicationId, "skill"),
                From = new ChannelAccount(system.User.UserId, "user"),
                Conversation = new ConversationAccount(false, "conversation", skillRequest.Session.SessionId),
                Type = skillRequest.Request.Type,
                Id = skillRequest.Request.RequestId,
                Timestamp = skillRequest.Request.Timestamp,
                Locale = skillRequest.Request.Locale,
                ChannelData = skillRequest
            };

            if (skillRequest.Request is IntentRequest intentRequest
                && intentRequest.Intent.Slots != null
                && intentRequest.Intent.Slots.ContainsKey(defaultIntentSlotName))
            {
                activity.Type = ActivityTypes.Message;
                activity.Text = intentRequest.Intent.Slots[defaultIntentSlotName].Value;
                activity.Value = intentRequest;
            }
            else if (skillRequest.Request is LaunchRequest launchRequest)
            {
                activity.Type = ActivityTypes.ConversationUpdate;
                activity.MembersAdded = new List<ChannelAccount>() { new ChannelAccount() { Id = skillRequest.Session.User.UserId } };
                activity.Value = launchRequest;
            }
            else
            {
                activity.Type = ActivityTypes.Event;
                activity.Name = skillRequest.Request.Type;

                switch (skillRequest.Request)
                {
                    case IntentRequest skillIntentRequest:
                        activity.Value = skillIntentRequest;
                        break;
                    case AccountLinkSkillEventRequest accountLinkSkillEventRequest:
                        activity.Value = accountLinkSkillEventRequest;
                        break;
                    case AudioPlayerRequest audioPlayerRequest:
                        activity.Value = audioPlayerRequest;
                        break;
                    case DisplayElementSelectedRequest displayElementSelectedRequest:
                        activity.Value = displayElementSelectedRequest;
                        break;
                    case PermissionSkillEventRequest permissionSkillEventRequest:
                        activity.Value = permissionSkillEventRequest;
                        break;
                    case PlaybackControllerRequest playbackControllerRequest:
                        activity.Value = playbackControllerRequest;
                        break;
                    case SessionEndedRequest sessionEndedRequest:
                        activity.Value = sessionEndedRequest;
                        break;
                    case SkillEventRequest skillEventRequest:
                        activity.Value = skillEventRequest;
                        break;
                    case SystemExceptionRequest systemExceptionRequest:
                        activity.Value = systemExceptionRequest;
                        break;
                }
            }

            return activity;
        }

        public static SkillResponse CreateResponseFromActivity(Activity activity, SkillRequest alexaRequest, bool shouldEndSessionByDefault)
        {
            if (alexaRequest.Request.Type == "SessionEndedRequest" || activity == null)
            {
                return ResponseBuilder.Tell(string.Empty);
            }

            var response = new SkillResponse()
            {
                Version = "1.0",
                Response = new ResponseBody()
            };

            if (!SecurityElement.IsValidText(activity.Text))
            {
                activity.Text = SecurityElement.Escape(activity.Text);
            }

            if (!string.IsNullOrEmpty(activity.Speak))
            {
                response.Response.OutputSpeech = new SsmlOutputSpeech(activity.Speak);
            }
            else
            {
                response.Response.OutputSpeech = new PlainTextOutputSpeech(activity.Text);
            }

            ProcessActivityAttachments(activity, response);

            if (ShouldSetEndSession(response))
            {
                switch (activity.InputHint)
                {
                    case InputHints.IgnoringInput:
                        response.Response.ShouldEndSession = true;
                        break;
                    case InputHints.ExpectingInput:
                        response.Response.ShouldEndSession = false;
                        response.Response.Reprompt = new Reprompt(activity.Text);
                        break;
                    default:
                        response.Response.ShouldEndSession = shouldEndSessionByDefault;
                        break;
                }
            }

            return response;
        }

        /// <summary>
        /// Concatenates outgoing activities into a single activity. If any of the activities being process
        /// contain an outer SSML speak tag within the value of the Speak property, these are removed from the individual activities and a <speak>
        /// tag is wrapped around the resulting concatenated string.  An SSML strong break tag is added between activity
        /// content. For more infomation about the supported SSML for Alexa see 
        /// https://developer.amazon.com/en-US/docs/alexa/custom-skills/speech-synthesis-markup-language-ssml-reference.html#break
        /// </summary>
        /// <param name="activities">The list of one or more outgoing activities</param>
        /// <returns></returns>
        public static Activity ProcessOutgoingActivities(List<Activity> activities)
        {
            if (activities.Count == 0)
            {
                return null;
            }

            var activity = activities.Last();

            if (activities.Any(a => !string.IsNullOrEmpty(a.Speak)))
            {
                var speakText = string.Join("<break strength=\"strong\"/>", activities
                    .Select(a => !string.IsNullOrEmpty(a.Speak) ? StripSpeakTag(a.Speak) : a.Text)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Select(s => s));

                activity.Speak = $"<speak>{speakText}</speak>";
            }

            activity.Text = string.Join(". ", activities
                .Select(a => a.Text)
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(s => s.Trim(new char[] { ' ', '.' })));

            return activity;
        }

        private static void ProcessActivityAttachments(Activity activity, SkillResponse response)
        {
            var cardAttachment = activity.Attachments?.FirstOrDefault(a => a.GetType() == typeof(CardAttachment)) as CardAttachment;
            if (cardAttachment != null)
            {
                response.Response.Card = cardAttachment.Card;
            }

            var directiveAttachments = activity.Attachments?.Where(a => a.GetType() == typeof(DirectiveAttachment))
                .Select(d => d as DirectiveAttachment);
            var directives = directiveAttachments?.Select(d => d.Directive).ToList();
            if (directives != null && directives.Any())
            {
                response.Response.Directives = directives;
            }
        }

        /// <summary>
        /// Checks a string to see if it is XML and if the outer tag is a speak tag
        /// indicating it is SSML.  If an outer speak tag is found, the inner XML is
        /// returned, otherwise the original string is returned
        /// </summary>
        /// <param name="speakText">String to be checked for an outer speak XML tag and stripped if found</param>
        private static string StripSpeakTag(string speakText)
        {
            try
            {
                var speakSsmlDoc = XDocument.Parse(speakText);
                if (speakSsmlDoc != null && speakSsmlDoc.Root.Name.ToString().ToLowerInvariant() == "speak")
                {
                    using (var reader = speakSsmlDoc.Root.CreateReader())
                    {
                        reader.MoveToContent();
                        return reader.ReadInnerXml();
                    }
                }

                return speakText;
            }
            catch (XmlException)
            {
                return speakText;
            }
        }

        /// <summary>
        /// Under certain circumstances, such as the inclusion of certain types of directives
        /// on a response, should force the 'ShouldEndSession' property not be included on
        /// an outgoing response. This method determines if this property is allowed to have
        /// a value assigned.
        /// </summary>
        /// <param name="response">Boolean indicating if the 'ShouldEndSession' property can be populated on the response.'</param>
        /// <returns>bool</returns>
        private static bool ShouldSetEndSession(SkillResponse response)
        {
            if (response.Response.Directives.Any(d => d is IEndSessionDirective))
            {
                return false;
            }

            return true;
        }
    }
}