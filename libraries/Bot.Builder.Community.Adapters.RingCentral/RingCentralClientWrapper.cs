using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Bot.Builder.Community.Adapters.RingCentral.Handoff;
using Bot.Builder.Community.Adapters.RingCentral.Helpers;
using Bot.Builder.Community.Adapters.RingCentral.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RingCentral.EngageDigital.Api;
using RingCentral.EngageDigital.Client;
using RingCentral.EngageDigital.SourceSdk;
using RingCentral.EngageDigital.SourceSdk.Models;
using static Bot.Builder.Community.Adapters.RingCentral.RingCentralConstants;
using rcModels = RingCentral.EngageDigital.Model;
using rcSourceSdk = RingCentral.EngageDigital.SourceSdk;

namespace Bot.Builder.Community.Adapters.RingCentral
{
    public class RingCentralClientWrapper
    {
        private readonly IOptionsMonitor<RingCentralOptions> _options;
        private readonly IHandoffRequestRecognizer _handoffRequestRecognizer;
        private readonly DimeloClient _ringCentralClient;
        private readonly ILogger _logger;

        public RingCentralClientWrapper(IOptionsMonitor<RingCentralOptions> options, IHandoffRequestRecognizer handoffRequestRecognizer, ILogger logger = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _handoffRequestRecognizer = handoffRequestRecognizer ?? throw new ArgumentNullException(nameof(handoffRequestRecognizer));
            _logger = logger ?? NullLogger.Instance;

            if (string.IsNullOrWhiteSpace(_options.CurrentValue.RingCentralEngageApiAccessToken))
            {
                throw new ArgumentException(nameof(options.CurrentValue.RingCentralEngageApiAccessToken));
            }

            if (_options.CurrentValue.RingCentralEngageApiUrl == null)
            {
                throw new ArgumentException(nameof(options.CurrentValue.RingCentralEngageApiUrl));
            }

            // DimeloClient is used to send messages out to RingCentral platform in order to log messages
            // only do this if this feature has been enabled.
            if (_options.CurrentValue.LogMessagesToRingCentral)
            {
                if (string.IsNullOrWhiteSpace(_options.CurrentValue.RingCentralEngageCustomSourceRealtimeEndpointUrl))
                {
                    throw new ArgumentException(nameof(_options.CurrentValue.RingCentralEngageCustomSourceRealtimeEndpointUrl));
                }

                if (_options.CurrentValue.RingCentralEngageCustomSourceApiAccessToken == null)
                {
                    throw new ArgumentException(nameof(_options.CurrentValue.RingCentralEngageCustomSourceApiAccessToken));
                }
                _ringCentralClient = new DimeloClient(_options.CurrentValue.RingCentralEngageCustomSourceRealtimeEndpointUrl, _options.CurrentValue.RingCentralEngageCustomSourceApiAccessToken);
            }
        }

        public RingCentralClientWrapper(DimeloClient ringCentralClient, ILogger logger = null)
        {
            _ringCentralClient = ringCentralClient ?? throw new ArgumentNullException(nameof(ringCentralClient));
            _logger = logger ?? NullLogger.Instance;
        }

        public RingCentralClientWrapper(IOptionsMonitor<RingCentralOptions> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _handoffRequestRecognizer = new StaticHandoffRequestRecognizer();
            _logger = NullLogger.Instance;
        }

        /// <summary>
        /// This is used to process the result that comes from the act of setting up a new webhook in the RingCentral 
        /// platform. This immediately fires a webhook event to validate the webhook setup. Here we check that the 
        /// verify_token is that from the settings within RingCentral Webhook setup and reply with the challenge response
        /// if successful.
        /// </summary>
        /// <remarks>
        /// See https://developers.ringcentral.com/engage/guide/webhooks/create.
        /// </remarks>
        /// <param name="httpRequest"><see cref="HttpRequest"/> having the details to do the verification.</param>
        /// <param name="httpResponse">Altered response, based on the success of the verification process.</param>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task VerifyWebhookAsync(HttpRequest httpRequest, HttpResponse httpResponse, CancellationToken cancellationToken)
        {
            _ = httpRequest ?? throw new ArgumentNullException(nameof(httpRequest));
            _ = httpResponse ?? throw new ArgumentNullException(nameof(httpResponse));

            var queryStringData = HttpUtility.ParseQueryString(httpRequest.QueryString.Value);

            string mode = queryStringData["hub.mode"];
            string challenge = queryStringData["hub.challenge"];
            string token = queryStringData["hub.verify_token"];

            // All parameters must have a value, and mode must be "subscribe"
            if (string.IsNullOrEmpty(mode) ||
                string.IsNullOrEmpty(challenge) ||
                string.IsNullOrEmpty(token) ||
                !mode.Equals("subscribe", StringComparison.OrdinalIgnoreCase))
            {
                httpResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            // Validate token with that from RingCentral webhook configuration
            if (token == _options.CurrentValue.RingCentralEngageWebhookValidationToken)
            {
                // Return the challenge
                httpResponse.StatusCode = (int)HttpStatusCode.OK;
                await httpResponse.WriteAsync(challenge, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                httpResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
        }

        /// <summary>
        /// Resolves the IActivity from the RingCentral payload, will send appropriate response back to RingCentral and return the type of RingCentral event to handle
        /// A single webhook endpoint can handle multiple events from RingCentral - therefore need to resolve what type of event this is from the payload
        /// Messages can also be posted to the adapter from a RingCentral Custom Source - which can be configured to be the bot endpoint.  This is used in the scenario
        /// of human handoff when a Intervention.Opened is when a human operator takes over a conversation (eg. "Engages").
        /// </summary>
        /// <param name="adapter">RingCentral adapter.</param>
        /// <param name="botAdapter">Bot adapter.</param>
        /// <param name="request">HttpRequest from caller.</param>
        /// <param name="response">HttpResponse for caller.</param>
        /// <returns>Task.</returns>
        public async Task<Tuple<RingCentralHandledEvent, Activity>> GetActivityFromRingCentralRequestAsync(RingCentralAdapter adapter, IBotFrameworkHttpAdapter botAdapter, HttpRequest request, HttpResponse response)
        {
            _ = adapter ?? throw new ArgumentNullException(nameof(adapter));
            _ = botAdapter ?? throw new ArgumentNullException(nameof(botAdapter));
            _ = request ?? throw new ArgumentNullException(nameof(request));
            _ = response ?? throw new ArgumentNullException(nameof(response));

            var payloadType = await GetTypedRingCentralPayloadAsync(request.Body).ConfigureAwait(false);

            switch (payloadType)
            {
                case RingCentralEngageEvent ringCentralEngageEvent:
                    {
                        var metadata = ringCentralEngageEvent.Events.FirstOrDefault()?.Resource?.Metadata;
                        if (ringCentralEngageEvent.Events.FirstOrDefault().Type.Equals(RingCentralEventDescription.ContentImported, StringComparison.InvariantCultureIgnoreCase))
                        {
                            var newMessageActivity = await GetActivityFromRingCentralEventAsync(ringCentralEngageEvent, response).ConfigureAwait(false);

                            if (newMessageActivity == null)
                            {
                                break;
                            }

                            var handoffRequestStatus = await _handoffRequestRecognizer.RecognizeHandoffRequestAsync(newMessageActivity).ConfigureAwait(false);

                            // Bot requsted or bot in charge (not agent), return an activity             
                            if (handoffRequestStatus == HandoffTarget.Bot
                                || metadata.CategoryIds.Contains(_options.CurrentValue.RingCentralEngageBotControlledThreadCategoryId, StringComparer.OrdinalIgnoreCase))
                            {
                                return new Tuple<RingCentralHandledEvent, Activity>(RingCentralHandledEvent.ContentImported, newMessageActivity);
                            }
                        }

                        break;
                    }
                case RingCentralEngageAction ringCentralAction:
                    {
                        switch (ringCentralAction.Action)
                        {
                            case RingCentralEventDescription.MessageCreate:
                                {
                                    var conversationRef = RingCentralSdkHelper.ConversationReferenceFromForeignThread(ringCentralAction.Params?.ThreadId, _options.CurrentValue.BotId);
                                    var humanActivity = RingCentralSdkHelper.RingCentralAgentResponseActivity(ringCentralAction.Params?.Body);
                                    _logger.LogTrace($"GetActivityFromRingCentralRequestAsync: ForeignThreadId: {ringCentralAction.Params?.ThreadId}, ConversationId: {conversationRef.Conversation.Id}, ChannelId: {conversationRef.ChannelId}, ServiceUrl: {conversationRef.ServiceUrl}");

                                    // Use the botAdapter to send this agent (proactive) message through to the end user
                                    await ((IAdapterIntegration)botAdapter).ContinueConversationAsync(
                                        _options.CurrentValue.MicrosoftAppId,
                                        conversationRef,
                                        async (ITurnContext turnContext, CancellationToken cancellationToken) =>
                                        {
                                            MicrosoftAppCredentials.TrustServiceUrl(conversationRef.ServiceUrl);
                                            await turnContext.SendActivityAsync(humanActivity).ConfigureAwait(false);
                                        }, default).ConfigureAwait(false);

                                    object res = new
                                    {
                                        id = ringCentralAction.Params.InReplyToId,
                                        body = ringCentralAction.Params.Body
                                    };
                                    var rbody = JsonSerializer.Serialize(res);

                                    response.StatusCode = (int)HttpStatusCode.OK;
                                    await response.WriteAsync(rbody).ConfigureAwait(false);

                                    return new Tuple<RingCentralHandledEvent, Activity>(RingCentralHandledEvent.Action, humanActivity);
                                }
                            case RingCentralEventDescription.ImplementationInfo:
                                {
                                    // Return implementation info response
                                    // https://github.com/ringcentral/engage-digital-source-sdk/wiki/Request-Response
                                    // https://github.com/ringcentral/engage-digital-source-sdk/wiki/Actions-details
                                    response.StatusCode = (int)HttpStatusCode.OK;

                                    var implementationResponse = RingCentralSdkHelper.ImplementationInfoResponse();
                                    var rbody = JsonSerializer.SerializeToUtf8Bytes(implementationResponse);
                                    response.ContentType = "application/json";
                                    await response.Body.WriteAsync(rbody);
                                    return new Tuple<RingCentralHandledEvent, Activity>(RingCentralHandledEvent.Action, null);
                                }
                            case RingCentralEventDescription.MessageList:
                            case RingCentralEventDescription.PrivateMessagesList:
                            case RingCentralEventDescription.ThreadsList:
                                {
                                    response.StatusCode = (int)HttpStatusCode.OK;
                                    return new Tuple<RingCentralHandledEvent, Activity>(RingCentralHandledEvent.Action, null);
                                }
                            case RingCentralEventDescription.PrivateMessagesShow:
                            case RingCentralEventDescription.ThreadsShow:
                                {
                                    response.StatusCode = (int)HttpStatusCode.OK;
                                    return new Tuple<RingCentralHandledEvent, Activity>(RingCentralHandledEvent.Action, null);
                                }
                            default:
                                break;
                        }
                    }
                    break;
                default:
                    break;
            }
            return new Tuple<RingCentralHandledEvent, Activity>(RingCentralHandledEvent.Unknown, null);
        }

        /// <summary>
        /// Forward bot message message to RingCentral API, ensure a full transcript log for both the user & bot messages 
        /// are persisted in the RingCentral platform.
        /// </summary>
        /// <param name="activity">Activity to publish to RingCentral.</param>
        /// <remarks>
        /// Here we only deal with Activity.Text - additional DownRenderingMiddleware is applied to the pipleline before this 
        /// method should be invoke, therefore converting any adaptive cards, herocards, buttons etc. to clear text in the text field.
        /// </remarks>
        /// <returns>Task.</returns>
        public async Task SendActivityToRingCentralAsync(Activity activity)
        {
            _ = activity ?? throw new ArgumentNullException(nameof(activity));

            _logger.LogTrace($"SendActivityToRingCentralAsync: ForeignThreadId: {RingCentralSdkHelper.BuildForeignThreadIdFromActivity(activity)}, ConversationId: {activity.Conversation.Id}, ChannelId: {activity.ChannelId}, ServiceUrl: {activity.ServiceUrl}");

            // Skip publishing messages to RingCentral when it's opted out
            if (!_options.CurrentValue.LogMessagesToRingCentral)
            {
                return;
            }

            // Avoid publishing messages coming from an agent (they're already on RingCentral)
            if (RingCentralSdkHelper.IsActivityFromRingCentralOperator(activity))
            {
                return;
            }

            // Avoid publishing messages without text
            if (string.IsNullOrWhiteSpace(activity.Text))
            {
                return;
            }

            DimeloRequest request = new DimeloRequest();
            request.Action = CreateAction.Message;
            request.Params = new Message()
            {
                Id = activity.Id,
                ThreadId = RingCentralSdkHelper.BuildForeignThreadIdFromActivity(activity),
                Body = activity.Text,
                Author = new User()
                {
                    Id = activity.From.Id,
                    CreatedAt = DateTime.Now,
                    Screenname = string.IsNullOrEmpty(activity.From?.Name)
                        ? $"A {activity.ChannelId} user"
                        : activity.From.Name,
                    Puppetizable = true
                },
                InReplyToId = activity.Recipient.Id,

                // CONSIDER: Which actions required Human Handoff capability
                Actions = new List<rcSourceSdk.Models.Action>()
                {
                    rcSourceSdk.Models.Action.Create,
                    rcSourceSdk.Models.Action.Reply,
                    rcSourceSdk.Models.Action.List
                }
            };

            // Use RingCentral Client Source SDK as we need to pass the specific actions above
            // to allow an operator to take over the conversations
            await _ringCentralClient.SendAsync(request).ConfigureAwait(false);
        }

        /// <summary>
        /// Converts a Bot Framework activity to a RingCentral message.   
        /// </summary>
        /// <param name="activity">Activity to send to RingCentral.</param>
        /// <param name="sourceId">Id of the RingCentral source/channel where to send the activity to.</param>
        /// <returns>Id of the content generated.</returns>
        public virtual async Task<string> SendContentToRingCentralAsync(Activity activity, string sourceId)
        {
            _ = activity ?? throw new ArgumentNullException(nameof(activity));

            var config = RingCentralSdkHelper.InitializeRingCentralConfiguration(
                _options.CurrentValue.RingCentralEngageApiUrl,
                _options.CurrentValue.RingCentralEngageApiAccessToken);

            ContentsApi contentsApi = new ContentsApi(config);

            string messageBody = activity.Text;

            if (!string.IsNullOrEmpty(messageBody))
            {
                try
                {
                    var content = await contentsApi.CreateContentAsync(
                        body: activity.Text,
                        sourceId: sourceId,
                        _private: 1,
                        inReplyToId: activity.From.Id).ConfigureAwait(false);

                    return content.Id;
                }
                catch (ApiException exp)
                {
                    // {"message":"Can't reply to closed thread","error":"cant_reply_to_closed_thread","status":403}
                    if (exp.ErrorCode == 403)
                    {
                        _logger.LogWarning($"SendContentToRingCentralAsync: {exp.Message}");
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Categorize a thread to either handoff the control of replies to an aget or the bot.
        /// </summary>
        /// <param name="target">Target, where to handoff the control.</param>
        /// <param name="thread">Thread to change.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task HandoffConversationControlToAsync(HandoffTarget target, rcModels.Thread thread)
        {
            _ = thread ?? throw new ArgumentNullException(nameof(thread));

            string categoryToAdd;
            string categoryToRemove;

            switch (target)
            {
                case HandoffTarget.Agent:
                    categoryToAdd = _options.CurrentValue.RingCentralEngageAgentControlledThreadCategoryId; // Agent
                    categoryToRemove = _options.CurrentValue.RingCentralEngageBotControlledThreadCategoryId; // Bot
                    break;
                case HandoffTarget.Bot:
                    categoryToAdd = _options.CurrentValue.RingCentralEngageBotControlledThreadCategoryId; // Bot
                    categoryToRemove = _options.CurrentValue.RingCentralEngageAgentControlledThreadCategoryId; // Agent
                    break;
                default:
                    _logger.LogWarning("Handoff target \"{HandoffTarget}\" is not supported", target.ToString());
                    return;
            }

            var existingCategoryIds = thread.ThreadCategoryIds?.ToList() ?? new List<string>();

            // Add categories to existing categories
            var mergedCategoryIds = existingCategoryIds
                .Except(new string[] { categoryToRemove })
                .Union(new string[] { categoryToAdd })
                .Distinct()
                .ToList();

            // Categorize as bot or agent controleld thread
            var threadsApi = GetThreadsApi();
            await threadsApi.CategorizeThreadAsyncWithHttpInfo(
                thread.Id,
                new Collection<string>(mergedCategoryIds)).ConfigureAwait(false);
        }

        public async Task<rcModels.Thread> GetThreadByIdAsync(string threadId)
        {
            _ = threadId ?? throw new ArgumentNullException(nameof(threadId));

            var threadsApi = GetThreadsApi();
            
            var thread = await threadsApi.GetThreadAsync(threadId).ConfigureAwait(false);

            if (thread == null)
            {
                _logger.LogWarning(
                    $"GetThreadByIdAsync: No thread could be found using thread id: {threadId}");
            }

            return thread;
        }

        public async Task<rcModels.Thread> GetThreadByForeignThreadIdAsync(string foreignThreadId)
        {
            var threadsApi = GetThreadsApi();

            var matchingThreads = await threadsApi.GetAllThreadsAsync($"foreign_id:'{foreignThreadId}'").ConfigureAwait(false);
            var thread = matchingThreads.Records.FirstOrDefault();

            if (thread == null)
            {
                _logger.LogWarning(
                    $"GetThreadByForeignThreadIdAsync: No thread could be found using foreign thread id: {foreignThreadId}",
                    foreignThreadId);
            }

            return thread;
        }

        private ThreadsApi GetThreadsApi()
        {
            var config = RingCentralSdkHelper.InitializeRingCentralConfiguration(
                _options.CurrentValue.RingCentralEngageApiUrl,
                _options.CurrentValue.RingCentralEngageApiAccessToken);

            return new ThreadsApi(config);
        }

        /// <summary>
        /// Create an BF Activity from a RingCentral action event.  Mapping the values from the RingCentral payload
        /// to the Activity properties in order to send through the bot logic for handling.  Here we check the source id of the RingCentral
        /// message, we only want to handle known sources and not repeat any messages from for example Custom Sources (ie. Bot Framework).
        /// </summary>
        /// <param name="ringCentralEngageRequest">RingCentralEngageEvent object.</param>
        /// <param name="response">HttpResponse for caller.</param>
        /// <returns>Task.</returns>
        public async Task<Activity> GetActivityFromRingCentralEventAsync(RingCentralEngageEvent ringCentralEngageRequest, HttpResponse response)
        {
            _ = ringCentralEngageRequest ?? throw new ArgumentNullException(nameof(ringCentralEngageRequest));
            _ = response ?? throw new ArgumentNullException(nameof(response));

            Resource ringCentralEventResource = ringCentralEngageRequest.Events.FirstOrDefault().Resource ?? throw new ArgumentNullException(nameof(Resource));

            var sourceId = ringCentralEventResource.Metadata.SourceId;
            var threadId = ringCentralEventResource.Metadata.ThreadId;
            var channelId = RingCentralChannels.GetFromResourceType(ringCentralEventResource.Type);

            // Ensure we have some body text from WhatsApp channel
            if (string.IsNullOrEmpty(ringCentralEventResource.Metadata.Body))
            {
                _logger.LogWarning($"GetActivityFromRingCentralEventAsync: RingCentral Message Received from Source Id: '{sourceId}' but missing body text");
                response.StatusCode = (int)HttpStatusCode.NoContent;
                return null;
            }

            var ringCentralActivity = new Activity()
            {
                Id = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                ChannelId = channelId,
                Conversation = new ConversationAccount()
                {
                    Id = ringCentralEventResource.Metadata.UserId ?? ringCentralEventResource.Metadata.AuthorId
                },
                From = new ChannelAccount()
                {
                    Id = ringCentralEventResource.Metadata.UserId ?? ringCentralEventResource.Metadata.AuthorId
                },
                Recipient = new ChannelAccount()
                {
                    Id = ringCentralEventResource.Id
                },
                Text = ringCentralEventResource.Metadata?.Body,
                ChannelData = new RingCentralChannelData()
                {
                    SourceId = sourceId,
                    ThreadId = threadId
                },
                Type = ActivityTypes.Message
            };

            object rcResponse = new
            {
                id = ringCentralEngageRequest.Events.FirstOrDefault().Resource.Metadata.ForeignId
            };
            var rcResponseBody = JsonSerializer.Serialize(rcResponse);

            response.StatusCode = (int)HttpStatusCode.OK;
            await response.WriteAsync(rcResponseBody).ConfigureAwait(false);
            return ringCentralActivity;
        }

        /// <summary>
        /// Resolve a dynamic payload from RingCentral webhook event or action to
        /// ensure we have a payload that we can act upon.
        /// </summary>
        /// <param name="payload">Stream that contains the RingCentral payload.</param>
        /// <returns><see cref="RingCentralEngageEvent"/> or <see cref="RingCentralEngageAction"/> payload.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="payload"/> is null or not recognized as valid payload.</exception>
        private async Task<RingCentralPayload> GetTypedRingCentralPayloadAsync(Stream payload)
        {
            _ = payload ?? throw new ArgumentNullException(nameof(payload));

            using var ms = new MemoryStream();
            await payload.CopyToAsync(ms).ConfigureAwait(false);
            ms.Position = 0;
            if (ms.Length == 0)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            var bytes = ms.ToArray();

            // Probe if its an event
            var rcEvent = GetRingCentralPayloadDataAs<RingCentralEngageEvent>(bytes);
            if (rcEvent?.Events != null)
            {
                var metadata = rcEvent.Events.FirstOrDefault()?.Resource?.Metadata;
                if (metadata == null)
                {
                    throw new ArgumentNullException(nameof(payload), $"{nameof(RingCentralEngageEvent)} metadata must not be null.");
                }

                return rcEvent;
            }

            // Probe if its an action
            var rcAction = GetRingCentralPayloadDataAs<RingCentralEngageAction>(bytes);
            if (rcAction?.Action != null)
            {
                return rcAction;
            }

            throw new ArgumentNullException(nameof(payload));
        }

        private T GetRingCentralPayloadDataAs<T>(byte[] payload) 
            where T : RingCentralPayload
        {
            _ = payload ?? throw new ArgumentNullException(nameof(payload));

            var typedPayload = JsonSerializer.Deserialize<T>(payload, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            });

            return typedPayload;
        }
    }
}
