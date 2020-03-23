﻿using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Xml;
using System.Xml.Linq;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Bot.Builder.Community.Adapters.Alexa.Core.Attachments;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Rest;
using AlexaResponse = Alexa.NET.Response;

namespace Bot.Builder.Community.Adapters.Alexa.Core
{
    public class AlexaRequestMapper
    {
        private AlexaRequestMapperOptions _options;
        private ILogger _logger;

        public AlexaRequestMapper(AlexaRequestMapperOptions options = null, ILogger logger = null)
        {
            _options = options ?? new AlexaRequestMapperOptions();
            _logger = logger ?? NullLogger.Instance;
        }

        /// <summary>
        /// Returns an Activity object created by using properties on a SkillRequest.
        /// A base set of properties based on the SkillRequest are applied to a new Activity object
        /// for all request types with the activity type, and additional properties, set depending 
        /// on the specific request type.
        /// </summary>
        /// <param name="skillRequest">The SkillRequest to be used to create an Activity object.</param>
        /// <returns>Activity</returns>
        public Activity RequestToActivity(SkillRequest skillRequest)
        {
            if (skillRequest.Request == null)
            {
                throw new ValidationException("Bad Request. Skill request missing Request property.");
            }

            switch (skillRequest.Request)
            {
                case IntentRequest intentRequest:
                    if (intentRequest.Intent.Slots != null && intentRequest.Intent.Slots.ContainsKey(_options.DefaultIntentSlotName))
                    {
                        return RequestToMessageActivity(skillRequest, intentRequest);
                    }
                    else
                    {
                        if (intentRequest.Intent.Name == "AMAZON.StopIntent")
                        {
                            return RequestToEndOfConversationActivity(skillRequest);
                        }

                        return RequestToEventActivity(skillRequest);
                    }
                case LaunchRequest launchRequest:
                    return RequestToConversationUpdateActivity(skillRequest);
                case SessionEndedRequest sessionEndedRequest:
                    return RequestToEndOfConversationActivity(skillRequest);
                default:
                    return RequestToEventActivity(skillRequest);
            }
        }
        
        /// <summary>
        /// Creates a SkillResponse based on an Activity and original SkillRequest. 
        /// </summary>
        /// <param name="activity">The Activity to use to create the SkillResponse</param>
        /// <param name="alexaRequest">Original SkillRequest received from Alexa Skills service. This is used
        /// to check if the original request was a SessionEndedRequest which should not return a response.</param>
        /// <returns>SkillResponse</returns>
        public SkillResponse ActivityToResponse(Activity activity, SkillRequest alexaRequest)
        {
            var response = new SkillResponse()
            {
                Version = "1.0",
                Response = new ResponseBody()
            };

            if (activity == null || alexaRequest.Request is SessionEndedRequest)
            {
                response.Response.ShouldEndSession = true;
                response.Response.OutputSpeech = new PlainTextOutputSpeech 
                {
                    Text = string.Empty
                };
                return response;
            }

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
                response.Response.OutputSpeech = new PlainTextOutputSpeech(activity.Text ?? string.Empty);
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
                        response.Response.ShouldEndSession = _options.ShouldEndSessionByDefault;
                        break;
                }
            }

            return response;
        }

        /// <summary>
        /// Concatenates activities into a single activity. Uses the last activity in the list as the base activity.
        /// If any of the activities being process contain an outer SSML speak tag within the value of the Speak property, 
        /// these are removed from the individual activities and a <speak> tag is wrapped around the resulting 
        /// concatenated string.  An SSML strong break tag is added between activity content. For more infomation 
        /// about the supported SSML for Alexa see 
        /// https://developer.amazon.com/en-US/docs/alexa/custom-skills/speech-synthesis-markup-language-ssml-reference.html#break
        /// </summary>
        /// <param name="activities">The list of one or more outgoing activities</param>
        /// <returns>Activity</returns>
        public Activity MergeActivities(IList<Activity> activities)
        {
            if (activities == null || activities.Count == 0)
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

        private Activity RequestToEndOfConversationActivity(SkillRequest skillRequest)
        {
            var activity = Activity.CreateEndOfConversationActivity() as Activity;
            activity = SetGeneralActivityProperties(activity, skillRequest);
            return activity;
        }

        private Activity RequestToConversationUpdateActivity(SkillRequest skillRequest)
        {
            var activity = Activity.CreateConversationUpdateActivity() as Activity;
            activity = SetGeneralActivityProperties(activity, skillRequest);
            activity.MembersAdded.Add(new ChannelAccount(id: skillRequest.Session.User.UserId));
            return activity;
        }

        private Activity RequestToMessageActivity(SkillRequest skillRequest, IntentRequest intentRequest)
        {
            var activity = Activity.CreateMessageActivity() as Activity;
            activity = SetGeneralActivityProperties(activity, skillRequest);
            activity.Text = intentRequest.Intent.Slots[_options.DefaultIntentSlotName].Value;
            activity.Locale = intentRequest.Locale;
            return activity;
        }

        private Activity RequestToEventActivity(SkillRequest skillRequest)
        {
            var activity = Activity.CreateEventActivity() as Activity;
            activity = SetGeneralActivityProperties(activity, skillRequest);
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
                case SkillEventRequest skillEventRequest:
                    activity.Value = skillEventRequest;
                    break;
                case SystemExceptionRequest systemExceptionRequest:
                    activity.Value = systemExceptionRequest;
                    break;
            }

            return activity;
        }

        /// <summary>
        /// Set the general properties, based on an incoming SkillRequest that can be applied
        /// irresepective of the resulting Activity type.
        /// </summary>
        /// <param name="activity">The Activity on which to set the properties on.</param>
        /// <param name="skillRequest">The incoming SkillRequest</param>
        /// <returns>Activity</returns>
        private Activity SetGeneralActivityProperties(Activity activity, SkillRequest skillRequest)
        {
            var alexaSystem = skillRequest.Context.System;

            activity.ChannelId = _options.ChannelId;
            activity.Id = skillRequest.Request.RequestId;
            activity.DeliveryMode = DeliveryModes.ExpectReplies;
            activity.ServiceUrl = _options.ServiceUrl ?? $"{alexaSystem.ApiEndpoint}?token={alexaSystem.ApiAccessToken}";
            activity.Recipient = new ChannelAccount(alexaSystem.Application.ApplicationId);
            activity.From = new ChannelAccount(alexaSystem.Person?.PersonId ?? alexaSystem.User.UserId);
            activity.Conversation = new ConversationAccount(false, "conversation", skillRequest.Session?.SessionId ?? skillRequest.Request.RequestId);
            activity.Timestamp = skillRequest.Request.Timestamp.ToUniversalTime();
            activity.ChannelData = skillRequest;

            return activity;
        }

        /// <summary>
        /// Checks a string to see if it is XML and if the outer tag is a speak tag
        /// indicating it is SSML.  If an outer speak tag is found, the inner XML is
        /// returned, otherwise the original string is returned
        /// </summary>
        /// <param name="speakText">String to be checked for an outer speak XML tag and stripped if found</param>
        private string StripSpeakTag(string speakText)
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
        private bool ShouldSetEndSession(SkillResponse response)
        {
            if (response.Response.Directives.Any(d => d is IEndSessionDirective))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Processes any attachments on the Activity in order to amend the SkillResponse appropriately.
        /// The current process for processing activity attachments is;
        /// 1. Check for an instance of a SigninCard. Set the Card property on the SkillResponse to a LinkAccountCard.
        /// 2. If no SigninCard is found, check for an instance of a HeroCard. Transform the first instance of a HeroCard 
        /// into an Alexa Card and set the Card property on the response.
        /// 3. If no Signin or HeroCard instances were found, check for Alexa specific CardAttachment and
        /// set the content of the Card property on the response.
        /// 4. Look for any instances of DirectiveAttachments and add the appropriate directives to the response.
        /// </summary>
        /// <param name="activity">The Activity for which to process activities.</param>
        /// <param name="response">The SkillResponse to be modified based on the attachments on the Activity object.</param>
        private void ProcessActivityAttachments(Activity activity, SkillResponse response)
        {
            var bfCard = activity.Attachments?.FirstOrDefault(a => a.ContentType == HeroCard.ContentType || a.ContentType == SigninCard.ContentType);

            if (bfCard?.ContentType == SigninCard.ContentType)
            {
                response.Response.Card = new LinkAccountCard();
            }

            if (bfCard?.ContentType == HeroCard.ContentType)
            {
                response.Response.Card = CreateAlexaCardFromHeroCard(bfCard.Content as HeroCard);
            }

            if (response.Response.Card == null)
            {
                if (activity.Attachments?.FirstOrDefault(a => a.GetType() == typeof(CardAttachment)) is CardAttachment cardAttachment)
                {
                    response.Response.Card = cardAttachment.Card;
                }
            }

            var directiveAttachments = activity.Attachments?
                .Where(a => a.GetType() == typeof(DirectiveAttachment))
                .Select(d => d as DirectiveAttachment);

            var directives = directiveAttachments?.Select(d => d.Directive).ToList();
            if (directives != null && directives.Any())
            {
                response.Response.Directives = directives;
            }
        }

        /// <summary>
        /// Uses a HeroCard to create an instance of ICard (either StandardCard or SimpleCard).
        /// </summary>
        /// <param name="heroCard">The HeroCard to be transformed.</param>
        /// <returns>An instance of ICard - either SimpleCard or StandardCard.</returns>
        private ICard CreateAlexaCardFromHeroCard(HeroCard heroCard)
        {
            if (heroCard.Images != null && heroCard.Images.Any())
            {
                return new StandardCard()
                {
                    Content = heroCard.Text,
                    Image = new AlexaResponse.CardImage()
                    {
                        SmallImageUrl = heroCard.Images[0].Url,
                        LargeImageUrl = heroCard.Images.Count > 1 ? heroCard.Images[1].Url : null
                    },
                    Title = heroCard.Title
                };
            }
            else
            {
                return new SimpleCard()
                {
                    Content = heroCard.Text,
                    Title = heroCard.Title
                };
            }
        }
    }
}