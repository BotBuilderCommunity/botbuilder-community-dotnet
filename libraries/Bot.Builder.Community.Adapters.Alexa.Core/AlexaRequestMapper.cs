using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Xml;
using System.Xml.Linq;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Bot.Builder.Community.Adapters.Alexa.Core.Attachments;
using Bot.Builder.Community.Adapters.Shared;
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
        /// <param name="mergedActivityResult">The Merged Activity Result to use to create the SkillResponse</param>
        /// <param name="alexaRequest">Original SkillRequest received from Alexa Skills service. This is used
        /// to check if the original request was a SessionEndedRequest which should not return a response.</param>
        /// <returns>SkillResponse</returns>
        public SkillResponse ActivityToResponse(MergedActivityResult mergedActivityResult, SkillRequest alexaRequest)
        {
            var response = new SkillResponse()
            {
                Version = "1.0",
                Response = new ResponseBody()
            };

            var activity = mergedActivityResult?.MergedActivity;

            if (activity == null || activity.Type != ActivityTypes.Message || alexaRequest.Request is SessionEndedRequest)
            {
                response.Response.ShouldEndSession = true;
                response.Response.OutputSpeech = new PlainTextOutputSpeech 
                {
                    Text = string.Empty
                };
                return response;
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
                // If end of conversation was flagged use that, othwerwise look at the InputHint.
                if (mergedActivityResult.EndOfConversationFlagged)
                {
                    response.Response.ShouldEndSession = true;
                }
                else
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
        /// <returns>MergedActivityResult</returns>
        public MergedActivityResult MergeActivities(IList<Activity> activities)
        {
            if (activities == null || activities.Count == 0)
            {
                return null;
            }

            var mergedActivityResult = new MergedActivityResult();
            bool mergedActivityEndsWithPeriod = false;

            bool hasSpeakField = false;
            var speakFields = new List<string>();
            var textFields = new List<string>();
            var attachments = new List<Attachment>();

            foreach (var activity in activities)
            {
                if (activity == null)
                {
                    continue;
                }

                switch (activity.Type)
                {
                    case ActivityTypes.Message:
                        mergedActivityResult.MergedActivity = activity;

                        if (!string.IsNullOrEmpty(activity.Speak))
                        {
                            hasSpeakField = true;
                            speakFields.Add(StripSpeakTag(activity.Speak));
                        }
                        else if (!string.IsNullOrEmpty(activity.Text))
                        {
                            speakFields.Add(NormalizeActivityText(activity.TextFormat, activity.Text, forSsml: true));
                        }

                        if (!string.IsNullOrEmpty(activity.Text))
                        {
                            var text = NormalizeActivityText(activity.TextFormat, activity.Text, forSsml: false);
                            if (!string.IsNullOrEmpty(text))
                            {
                                mergedActivityEndsWithPeriod = activity.Text?.TrimEnd().EndsWith(".") ?? false;

                                textFields.Add(text.Trim(new char[] { ' ' }));
                            }
                        }

                        if (activity.Attachments != null && activity.Attachments.Count > 0)
                        {
                            attachments.AddRange(activity.Attachments);
                        }
                        break;
                    case ActivityTypes.EndOfConversation:
                        mergedActivityResult.EndOfConversationFlagged = true;
                        break;
                }
            }

            if (mergedActivityResult.MergedActivity != null)
            {
                if (hasSpeakField)
                {
                    mergedActivityResult.MergedActivity.Speak = $"<speak>{string.Join("<break strength=\"strong\"/>", speakFields)}</speak>";
                }

                mergedActivityResult.MergedActivity.Text = string.Join(" ", textFields);
                mergedActivityResult.MergedActivity.Attachments = attachments;

                if (mergedActivityResult.MergedActivity.Text.EndsWith(".") && !mergedActivityEndsWithPeriod)
                {
                    mergedActivityResult.MergedActivity.Text = mergedActivityResult.MergedActivity.Text.TrimEnd('.');
                }
            }

            return mergedActivityResult;
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
                default:
                    activity.Value = skillRequest.Request;
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
            activity.Conversation = new ConversationAccount(isGroup: false, id: skillRequest.Session?.SessionId ?? skillRequest.Request.RequestId);
            activity.Timestamp = skillRequest.Request.Timestamp.ToUniversalTime();
            activity.ChannelData = skillRequest;
            activity.Locale = skillRequest.Request.Locale;

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

        private string NormalizeActivityText(string textFormat, string text, bool forSsml)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            // Default to markdown if it isn't specified.
            if (textFormat == null)
            {
                textFormat = TextFormatTypes.Markdown;
            }

            string plainText;
            if (textFormat.Equals(TextFormatTypes.Plain, StringComparison.Ordinal))
            {
                plainText = text;
            }
            else if (textFormat.Equals(TextFormatTypes.Markdown, StringComparison.Ordinal))
            {
                plainText = MarkdownToPlaintextRenderer.Render(text);
            }
            else // xml format or other unknown and unsupported format.
            {
                plainText = string.Empty;
            }

            if (forSsml && !SecurityElement.IsValidText(plainText))
            {
                plainText = SecurityElement.Escape(plainText);
            }
            return plainText;
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
           activity.ConvertAttachmentContent();

            var bfCard = activity.Attachments?.FirstOrDefault(a => a.ContentType == HeroCard.ContentType || a.ContentType == SigninCard.ContentType);

            if (bfCard != null)
            {
                switch (bfCard.Content)
                {
                    case SigninCard signinCard:
                        response.Response.Card = new LinkAccountCard();
                        break;
                    case HeroCard heroCard:
                        response.Response.Card = CreateAlexaCardFromHeroCard(heroCard);
                        break;
                }
            }
            else
            {
                var cardAttachment = activity.Attachments?.FirstOrDefault(a => a.ContentType == AlexaAttachmentContentTypes.Card);
                if (cardAttachment != null)
                {
                    response.Response.Card = cardAttachment.Content as ICard;
                }
            }

            var directiveAttachments = activity.Attachments?.Where(a => a.ContentType == AlexaAttachmentContentTypes.Directive).ToList();
            if (directiveAttachments != null && directiveAttachments.Any())
            {
                response.Response.Directives = directiveAttachments.Select(d => d.Content as IDirective).ToList();
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
