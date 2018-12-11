using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Google.Integration;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Google
{
    public class GoogleAdapter : BotAdapter
    {
        private Dictionary<string, List<Activity>> Responses { get; set; }

        private GoogleOptions Options { get; set; }

        public async Task<GoogleResponseBody> ProcessActivity(Payload actionPayload, GoogleOptions googleOptions, BotCallbackHandler callback)
        {
            TurnContext context = null;

            try
            {
                Options = googleOptions;

                var activity = RequestToActivity(actionPayload);
                BotAssert.ActivityNotNull(activity);

                context = new TurnContext(this, activity);

                Responses = new Dictionary<string, List<Activity>>();

                await base.RunPipelineAsync(context, callback, default(CancellationToken)).ConfigureAwait(false);

                var key = $"{activity.Conversation.Id}:{activity.Id}";

                try
                {
                    GoogleResponseBody response = null;
                    var activities = Responses.ContainsKey(key) ? Responses[key] : new List<Activity>();
                    response = CreateResponseFromLastActivity(activities, context);
                    return response;
                }
                finally
                {
                    if (Responses.ContainsKey(key))
                    {
                        Responses.Remove(key);
                    }
                }
            }
            catch (Exception ex)
            {
                await googleOptions.OnTurnError(context, ex);
                throw;
            }
        }

        public override Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken CancellationToken)
        {
            var resourceResponses = new List<ResourceResponse>();

            foreach (var activity in activities)
            {
                switch (activity.Type)
                {
                    case ActivityTypes.Message:
                    case ActivityTypes.EndOfConversation:
                        var conversation = activity.Conversation ?? new ConversationAccount();
                        var key = $"{conversation.Id}:{activity.ReplyToId}";

                        if (Responses.ContainsKey(key))
                        {
                            Responses[key].Add(activity);
                        }
                        else
                        {
                            Responses[key] = new List<Activity> { activity };
                        }

                        break;
                    default:
                        Trace.WriteLine(
                            $"GoogleAdapter.SendActivities(): Activities of type '{activity.Type}' aren't supported.");
                        break;
                }

                resourceResponses.Add(new ResourceResponse(activity.Id));
            }

            return Task.FromResult(resourceResponses.ToArray());
        }

        private static Activity RequestToActivity(Payload actionPayload)
        {
            var activity = new Activity
            {
                ChannelId = "google",
                ServiceUrl = $"",
                Recipient = new ChannelAccount("", "action"),
                From = new ChannelAccount(actionPayload.User.UserId, "user"),

                Conversation = new ConversationAccount(false, "conversation",
                    $"{actionPayload.Conversation.ConversationId}"),

                Type = ActivityTypes.Message,
                Text = actionPayload.Inputs[0]?.RawInputs[0]?.Query,
                Id = new Guid().ToString(),
                Timestamp = DateTime.UtcNow,
                Locale = actionPayload.User.Locale
            };

            return activity;
        }

        private GoogleResponseBody CreateResponseFromLastActivity(IEnumerable<Activity> activities, ITurnContext context)
        {
            var activity = activities.First();

            var response = new GoogleResponseBody()
            {
                Payload = new ResponsePayload()
                {
                    Google = new PayloadContent()
                    {
                        RichResponse = new RichResponse(),
                        ExpectUserResponse = Options.ShouldEndSessionByDefault ? false : true
                    }
                }
            };

            if (!string.IsNullOrEmpty(activity.Text))
            {
                var responseItems = new List<Item>
                {
                    new SimpleResponse
                    {
                        Content = new SimpleResponseContent {
                            DisplayText = activity.Text,
                            Ssml = activity.Speak,
                            TextToSpeech = activity.Text
                        }
                    }
                };

                response.Payload.Google.RichResponse.Items = responseItems.ToArray();

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

        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
