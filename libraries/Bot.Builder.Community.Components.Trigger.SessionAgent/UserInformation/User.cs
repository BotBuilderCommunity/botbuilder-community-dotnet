using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Components.Trigger.SessionAgent.UserInformation
{
    public class User : IUser
    {
        public string UserId { get; }

        public DateTime LastAccessTime { get; set; }

        public ConversationReference ConversationReference { get; }

        public IConnectorClient ConnectorClient { get; }

        public IBot Bot { get; }

        public IBotFrameworkHttpAdapter BotAdapter { get; }

        public bool IsReminder { get; set; }
        
        public User(IBot bot, IBotFrameworkHttpAdapter botAdapter, ITurnContext turnContext)
        {
            if (turnContext == null)
                throw new ArgumentNullException(nameof(turnContext));

            Bot = bot ?? throw new ArgumentNullException(nameof(bot));

            BotAdapter = botAdapter ?? throw new ArgumentNullException(nameof(botAdapter));

            ConversationReference = turnContext.Activity.GetConversationReference();

            var connectorClient = turnContext.TurnState.Get<IConnectorClient>();

            if (connectorClient == null)
                throw new ArgumentNullException(nameof(connectorClient));

            UserId = ConversationReference.User.Id;

            LastAccessTime = DateTime.UtcNow;
            ConnectorClient = new ConnectorClient(connectorClient.BaseUri, connectorClient.Credentials);

            IsReminder = true;
        }

        public async Task SendTrigger(string triggerName)
        {
            if (ConversationReference != null && ConnectorClient != null)
            {
                var activity = PrepareActivity(triggerName, ConversationReference);

                if (activity != null)
                {
                    activity.Timestamp = DateTimeOffset.UtcNow;
                    activity.LocalTimestamp = DateTimeOffset.UtcNow;
                    activity.MembersAdded = null;
                    
                    var turnContext = new TurnContext((BotAdapter)BotAdapter, activity);
                    turnContext.TurnState.Add(ConnectorClient);
                    
                    await Bot.OnTurnAsync(turnContext);
                }
            }
        }

        private static Activity PrepareActivity(string activityName,ConversationReference conversationReference)
        {

            if (conversationReference == null)
                return null;

            var createActivity = conversationReference.GetContinuationActivity();

            createActivity.Type = activityName;
            createActivity.ReplyToId = string.Empty;

            createActivity.Entities = new List<Entity>();

            var entity = new Entity
            {
                Type = activityName
            };
            entity.SetAs(new Mention()
            {
                Mentioned = new ChannelAccount()
                {
                    Id = conversationReference.User.Id,
                    Name = conversationReference.User.Name,
                    Role = conversationReference.User.Role,
                    AadObjectId = conversationReference.User.AadObjectId

                },
                Type = activityName
            });

            createActivity.Entities.Add(entity);

            return createActivity;
        }

    }

}
