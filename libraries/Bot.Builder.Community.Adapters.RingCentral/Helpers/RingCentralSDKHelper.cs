using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Bot.Builder.Community.Adapters.RingCentral.Schema;
using Microsoft.Bot.Schema;
using RingCentral.EngageDigital.Client;

namespace Bot.Builder.Community.Adapters.RingCentral.Helpers
{
    public static class RingCentralSdkHelper
    {
        /// <summary>
        /// Using the RingCentral Source SDK really hinges on correct configuration.  For us we have found that the
        /// Source SDK URL and and the Source SDK Bearer token are the most important pieces.  This method returns an
        /// new instance of the RingCentral.Engage.Configuartion that will passed to all APIs found in API folder.  These APIs
        /// can all created with an arugument passing in the Configuration.
        /// </summary>
        /// <param name="sourceSDKURL">Url for the source SDK.</param>
        /// <param name="sourceSDKBearer">Bearer token for Source API - this token is in raw text.</param>
        /// <returns>Configuration instance with auth headers set.</returns>
        public static Configuration InitializeRingCentralConfiguration(string sourceSDKURL, string sourceSDKBearer)
        {
            // Setup Source SDK Configuration - the default header needs the bearer token for auth
            var defaultHeader = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + sourceSDKBearer }
            };

            // Create the configuration with proper configuation to allow access to Source SDK
            var sourceSDKConfiguration = new Configuration()
            {
                BasePath = sourceSDKURL,
                DefaultHeader = defaultHeader
            };

            return sourceSDKConfiguration;
        }

        public static string BuildForeignThreadIdFromActivity(Activity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            //return $"{activity.Conversation.Id}_{activity.ChannelId}_{activity.ServiceUrl}_{activity.From.Id}";
            return $"{activity.Conversation.Id}_{activity.ChannelId}_{activity.ServiceUrl}";
        }

        public static ConversationReference ConversationReferenceFromForeignThread(string foreignThreadId, string botId)
        {
            if (string.IsNullOrEmpty(foreignThreadId))
            {
                throw new ArgumentNullException(nameof(foreignThreadId));
            }

            if (string.IsNullOrEmpty(botId))
            {
                throw new ArgumentNullException(nameof(botId));
            }

            var threadSplitter = foreignThreadId.Split("_");
            if (threadSplitter.Length != 3)
            {
                throw new IndexOutOfRangeException(nameof(foreignThreadId));
            }

            var conversationRef = new ConversationReference();
            conversationRef.Conversation = new ConversationAccount() { Id = threadSplitter[0] };
            conversationRef.ChannelId = threadSplitter[1];
            conversationRef.ServiceUrl = threadSplitter[2];
            conversationRef.Bot = new ChannelAccount() { Id = botId, Name = botId };
            conversationRef.User = new ChannelAccount() { Id = "User", Name = "User" };
            return conversationRef;
        }

        /// <summary>
        /// Creates an IActivity from the given text message from RingCentral platform
        /// Adds the IsHuman response metadata to the activity, so we can check this
        /// with the bot logic.
        /// </summary>
        /// <returns>Activity instance with message text.</returns>
        public static Activity RingCentralAgentResponseActivity(string message)
        {
            var entity = new Entity(nameof(RingCentralMetadata));
            entity.SetAs<RingCentralMetadata>(new RingCentralMetadata() { IsHumanResponse = true });
            entity.Type = nameof(RingCentralMetadata);

            Activity activity = new Activity()
            {
                Id = Guid.NewGuid().ToString(),
                Type = ActivityTypes.Message,
                InputHint = InputHints.AcceptingInput,
                Text = StripHTML(message),
                Entities = new List<Entity>() { entity }
            };

            return activity;
        }

        /// <summary>
        /// Determine if the activity is from a RingCentral operator/agent (human).  When an agent intervenes
        /// into a bot conversation (takes over control) - RingCentral fires a webhook messages with the 
        /// content into the RingCentral adapter - we don't want to echo these back out to RingCentral.
        public static bool IsActivityFromRingCentralOperator(Activity activity)
        {
            _ = activity ?? throw new ArgumentNullException(nameof(activity));

            bool retVal = false;

            if (activity.Entities == null)
            {
                return retVal;
            }

            foreach (var a in activity?.Entities)
            {
                try
                {
                    if (a.Type == nameof(RingCentralMetadata))
                    {
                        var meta = a.GetAs<RingCentralMetadata>();
                        retVal = meta.IsHumanResponse;
                    }
                }
                catch { }
            }

            return retVal;
        }

        /// <summary>
        /// Creates the response required by the RingCentral custom source enabling the required capabilities.
        /// </summary>
        /// <returns>ImplementationInfo instance.</returns>
        public static ImplementationInfo ImplementationInfoResponse()
        {
            return new ImplementationInfo()
            {
                Objects = new Objects()
                {
                    Messages = new List<string>() { "create", "reply", "list", "show", "delete" },
                    PrivateMessages = new List<string>() { "create", "reply", "list", "show", "delete" },
                    Threads = new List<string>() { "create", "reply", "list", "show", "delete" }
                },
                Options = new List<string>() { "view.messaging" }
            };
        }

        /// <summary>
        /// Remove HTML tags from RingCentral agent responses.
        /// </summary>
        /// <param name="input">String input text containing html tags</param>
        /// <returns>String raw text output.</returns>
        public static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", string.Empty);
        }
    }
}
