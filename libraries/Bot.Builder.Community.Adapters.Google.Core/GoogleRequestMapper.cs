using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Authentication;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapters.Google.Core
{
    public class GoogleRequestMapper
    {
        private readonly GoogleRequestMapperOptions _options;
        private ILogger _logger;

        public GoogleRequestMapper(GoogleRequestMapperOptions options = null, ILogger logger = null)
        {
            _options = options ?? new GoogleRequestMapperOptions();
            _logger = logger ?? NullLogger.Instance;
        }

        public Activity RequestToActivity(string requestBody)
        {
            Activity activity;

            if (_options.WebhookType == GoogleWebhookType.DialogFlow)
            {
                var dialogFlowRequest = JsonConvert.DeserializeObject<DialogFlowRequest>(requestBody);
                activity = DialogFlowRequestToActivity(dialogFlowRequest);
            }
            else
            {
                var actionPayload = JsonConvert.DeserializeObject<ActionsPayload>(requestBody);
                activity = PayloadToActivity(actionPayload);
            }

            return activity;
        }
        
        public ConversationWebhookResponse CreateConversationResponseFromLastActivity(Activity activity)
        {
            if (!SecurityElement.IsValidText(activity.Text))
            {
                activity.Text = SecurityElement.Escape(activity.Text);
            }

            var response = new ConversationWebhookResponse();

            // TODO!
            //var userStorage = new JObject
            //{
            //    { "UserId", context.TurnState["GoogleUserId"]?.ToString() }
            //};
            //response.UserStorage = userStorage?.ToString();

            if (activity?.Attachments != null
                && activity.Attachments.FirstOrDefault(a => a.ContentType == SigninCard.ContentType) != null)
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

                return response;
            }

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

                if (activity.Attachments != null && activity.Attachments.Any(a => a.ContentType == "google/card-attachment"))
                {
                    var cardAttachment = activity.Attachments.First(a => a.ContentType == "google/card-attachment") as BasicCardAttachment;
                    responseItems.Add(cardAttachment.Card);
                }

                if (activity.Attachments != null && activity.Attachments.Any(a => a.ContentType == "google/table-card-attachment"))
                {
                    var cardAttachment = activity.Attachments.First(a => a.ContentType == "google/table-card-attachment") as TableCardAttachment;
                    responseItems.Add(cardAttachment.Card);
                }

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

        public DialogFlowResponse CreateDialogFlowResponseFromActivity(Activity activity)
        {
            var response = new DialogFlowResponse()
            {
                Payload = new ResponsePayload()
                {
                    Google = new PayloadContent()
                    {
                        RichResponse = new RichResponse(),
                        ExpectUserResponse = !_options.ShouldEndSessionByDefault
                    }
                }
            };

            if (activity.Attachments.Any(a => a.GetType() == typeof(ListAttachment)))
            {
                var listAttachment = activity.Attachments?.FirstOrDefault(a => a.GetType() == typeof(ListAttachment)) as ListAttachment;
                var optionIntentData = GoogleHelper.GetOptionIntentDataFromListAttachment(listAttachment);
                response.Payload.Google.SystemIntent = new DialogFlowOptionSystemIntent() { Data = optionIntentData };
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

                response.Payload.Google.RichResponse.Items = responseItems.ToArray();

                // If suggested actions have been added to outgoing activity
                // add these to the response as Google Suggestion Chips
                var suggestionChips = GoogleHelper.ConvertSuggestedActivitiesToSuggestionChips(activity.SuggestedActions);
                if (suggestionChips.Any())
                {
                    response.Payload.Google.RichResponse.Suggestions = suggestionChips.ToArray();
                }

                // check if we should be listening for more input from the user
                switch (activity.InputHint)
                {
                    case InputHints.IgnoringInput:
                        response.Payload.Google.ExpectUserResponse = false;
                        break;
                    case InputHints.ExpectingInput:
                        response.Payload.Google.ExpectUserResponse = true;
                        break;
                    case InputHints.AcceptingInput:
                    default:
                        break;
                }
            }
            else
            {
                response.Payload.Google.ExpectUserResponse = false;
            }

            return response;
        }

        private Activity DialogFlowRequestToActivity(DialogFlowRequest request)
        {
            var activity = PayloadToActivity(request.OriginalDetectIntentRequest.Payload);
            activity.ChannelData = request;
            return activity;
        }

        private Activity PayloadToActivity(ActionsPayload payload)
        {
            var activity = new Activity()
            {
                ChannelId = _options.ChannelId,
                ServiceUrl = _options.ServiceUrl,
                Recipient = new ChannelAccount("", "action"),
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
        
    }
}
