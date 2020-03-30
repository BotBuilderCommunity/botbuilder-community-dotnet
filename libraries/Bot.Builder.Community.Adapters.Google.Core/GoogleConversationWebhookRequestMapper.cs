using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Xml;
using System.Xml.Linq;
using Bot.Builder.Community.Adapters.Google.Core.Helpers;
using Bot.Builder.Community.Adapters.Google.Core.Model;
using Bot.Builder.Community.Adapters.Google.Core.Model.Attachments;
using Bot.Builder.Community.Adapters.Google.Model;
using Bot.Builder.Community.Adapters.Google.Model.Attachments;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapters.Google.Core
{
    public class GoogleConversationWebhookRequestMapper
    {
        private GoogleRequestMapperOptions _options;
        private ILogger _logger;

        public GoogleConversationWebhookRequestMapper(GoogleRequestMapperOptions options = null, ILogger logger = null)
        {
            _options = options ?? new GoogleRequestMapperOptions();
            _logger = logger ?? NullLogger.Instance;
        }

        public Activity RequestToActivity(ActionsPayload payload)
        {
            var activity = new Activity()
            {
                ChannelId = _options.ChannelId,
                ServiceUrl = _options.ServiceUrl,
                Recipient = new ChannelAccount(payload.User.),
                Conversation = new ConversationAccount(false, "conversation", $"{payload.Conversation.ConversationId}"),
                Id = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                Locale = payload.User.Locale,
                Value = payload.Inputs[0]?.Intent,
                ChannelData = payload
            };

            if (!string.IsNullOrEmpty(payload.User.UserId))
            {
                activity.From = new ChannelAccount(payload.User.UserId, "user");
            }
            else
            {
                if (!string.IsNullOrEmpty(payload.User.UserStorage))
                {
                    var values = JObject.Parse(payload.User.UserStorage);
                    if (values.ContainsKey("UserId"))
                    {
                        activity.From = new ChannelAccount(values["UserId"].ToString(), "user");
                    }
                }
                else
                {
                    activity.From = new ChannelAccount(Guid.NewGuid().ToString(), "user");
                }
            }

            activity.Text = GoogleHelper.StripInvocation(payload.Inputs[0]?.RawInputs[0]?.Query, _options.ActionInvocationName);

            if (string.IsNullOrEmpty(activity.Text))
            {
                activity.Type = ActivityTypes.ConversationUpdate;
                activity.MembersAdded = new List<ChannelAccount>() { new ChannelAccount() { Id = activity.From.Id } };
            }
            else
            {
                activity.Type = ActivityTypes.Message;
                activity.Text = GoogleHelper.StripInvocation(payload.Inputs[0]?.RawInputs[0]?.Query, _options.ActionInvocationName);
            }

            if (payload.Inputs.FirstOrDefault()?.Arguments?.FirstOrDefault()?.Name == "OPTION")
            {
                activity.Value = payload.Inputs.First().Arguments.First().TextValue;
            }

            activity.ChannelData = payload;

            return activity;
        }

        public ConversationWebhookResponse ActivityToResponse(Activity activity, ActionsPayload alexaRequest, string googleUserId)
        {
            if (!SecurityElement.IsValidText(activity.Text))
            {
                activity.Text = SecurityElement.Escape(activity.Text);
            }

            var response = new ConversationWebhookResponse();

            var userStorage = new JObject
            {
                { "UserId", googleUserId }
            };
            response.UserStorage = userStorage?.ToString();

            if (activity?.Attachments != null
                && activity.Attachments.Any(a => a.ContentType == "google/list-attachment"))
            {
                var listAttachment = activity.Attachments?.FirstOrDefault(a => a.ContentType == "google/list-attachment") as ListAttachment;
                var optionIntentData = GoogleHelper.GetOptionIntentDataFromListAttachment(listAttachment);

                response.ExpectUserResponse = true;
                response.ExpectedInputs = new ExpectedInput[]
                    {
                        new ExpectedInput()
                        {
                            PossibleIntents = new ISystemIntent[]
                            {
                                new OptionIntent() { InputValueData = optionIntentData }
                            },
                            InputPrompt = new InputPrompt()
                            {
                                RichInitialPrompt = new RichResponse() {
                                    Items = new List<Item>()
                                        {
                                            new SimpleResponse()
                                            {
                                                Content = new SimpleResponseContent
                                                {
                                                    DisplayText = activity.Text,
                                                    Ssml = activity.Speak,
                                                    TextToSpeech = activity.Text
                                                }
                                            }
                                        }.ToArray()
                                }
                            }
                        }
                    };

                return response;
            }

            if (!string.IsNullOrEmpty(activity?.Text))
            {
                var simpleResponse = new SimpleResponse
                {
                    Content = new SimpleResponseContent
                    {
                        DisplayText = activity.Text,
                        Ssml = activity.Speak,
                        TextToSpeech = activity.Text
                    }
                };

                var responseItems = new List<Item> { simpleResponse };

                if (activity.InputHint == null || activity.InputHint == InputHints.AcceptingInput)
                {
                    activity.InputHint =
                        _options.ShouldEndSessionByDefault ? InputHints.IgnoringInput : InputHints.ExpectingInput;
                }

                // check if we should be listening for more input from the user
                switch (activity.InputHint)
                {
                    case InputHints.IgnoringInput:
                        response.ExpectUserResponse = false;
                        response.FinalResponse = new FinalResponse()
                        {
                            RichResponse = new RichResponse() { Items = responseItems.ToArray() }
                        };
                        break;
                    case InputHints.ExpectingInput:
                        response.ExpectUserResponse = true;
                        response.ExpectedInputs = new ExpectedInput[]
                            {
                                new ExpectedInput()
                                {
                                    PossibleIntents = new ISystemIntent[]
                                    {
                                        new TextIntent(),
                                    },
                                    InputPrompt = new InputPrompt()
                                    {
                                        RichInitialPrompt = new RichResponse() {Items = responseItems.ToArray()}
                                    }
                                }
                            };

                        var suggestionChips = GoogleHelper.ConvertSuggestedActivitiesToSuggestionChips(activity.SuggestedActions);
                        if (suggestionChips.Any())
                        {
                            response.ExpectedInputs.First().InputPrompt.RichInitialPrompt.Suggestions =
                                suggestionChips.ToArray();
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                response.ExpectUserResponse = false;
            }

            return response;
        }

        public Activity MergeActivities(IList<Activity> activities)
        {
            var messageActivities = activities?.Where(a => a.Type == ActivityTypes.Message).ToList();

            if (messageActivities == null || messageActivities.Count == 0)
            {
                return null;
            }

            var activity = messageActivities.Last();

            if (messageActivities.Any(a => !string.IsNullOrEmpty(a.Speak)))
            {
                var speakText = string.Join("<break strength=\"strong\"/>", messageActivities
                    .Select(a => !string.IsNullOrEmpty(a.Speak) ? StripSpeakTag(a.Speak) : NormalizeActivityText(a.TextFormat, a.Text))
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Select(s => s));

                activity.Speak = $"<speak>{speakText}</speak>";
            }

            activity.Text = string.Join(". ", messageActivities
                .Select(a => NormalizeActivityText(a.TextFormat, a.Text))
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(s => s.Trim(new char[] { ' ', '.' })));

            activity.Attachments = messageActivities.Where(x => x.Attachments != null).SelectMany(x => x.Attachments).ToList();

            return activity;
        }

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

        private string NormalizeActivityText(string textFormat, string text)
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
                plainText = GoogleMarkdownToPlaintextRenderer.Render(text);
            }
            else // xml format or other unknown and unsupported format.
            {
                plainText = string.Empty;
            }

            if (!SecurityElement.IsValidText(plainText))
            {
                plainText = SecurityElement.Escape(plainText);
            }
            return plainText;
        }

        private void ProcessActivityAttachments(Activity activity, ConversationWebhookResponse response)
        {
            var bfCard = activity.Attachments?.FirstOrDefault(a => a.ContentType == HeroCard.ContentType || a.ContentType == SigninCard.ContentType);

            if (bfCard != null)
            {
                if (bfCard?.ContentType == SigninCard.ContentType)
                {
                    response.ExpectUserResponse = true;
                    response.ResetUserStorage = null;

                    response.ExpectedInputs = new ExpectedInput[]
                    {
                        new ExpectedInput()
                        {
                            PossibleIntents = new ISystemIntent[]
                            {
                                new SigninIntent(),
                            },
                            InputPrompt = new InputPrompt()
                            {
                                RichInitialPrompt = new RichResponse()
                                {
                                    Items = new Item[]
                                    {
                                        new SimpleResponse()
                                        {
                                            Content = new SimpleResponseContent()
                                            {
                                                TextToSpeech = "PLACEHOLDER"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    };
                }

                //if (bfCard?.ContentType == HeroCard.ContentType)
                //{
                //    response.Response.Card = CreateGoogleCardFromHeroCard(bfCard.Content as HeroCard);
                //}
            }
            //else
            //{
            //    var cardAttachment = activity.Attachments?.FirstOrDefault(a => a.ContentType == AlexaAttachmentContentTypes.Card);
            //    if (cardAttachment != null)
            //    {
            //        response.Response.Card = cardAttachment.Content as ICard;
            //    }
            //}

            //var directiveAttachments = activity.Attachments?.Where(a => a.ContentType == AlexaAttachmentContentTypes.Directive).ToList();
            //if (directiveAttachments != null && directiveAttachments.Any())
            //{
            //    response.Response.Directives = directiveAttachments.Select(d => d.Content as IDirective).ToList();
            //}
        }


    }
}
