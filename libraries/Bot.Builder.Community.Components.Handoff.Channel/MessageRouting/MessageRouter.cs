using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Builder.Community.Components.Handoff.Channel.DataStore;
using Bot.Builder.Community.Components.Handoff.Channel.Models;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bot.Builder.Community.Components.Handoff.Channel.MessageRouting
{
    /// <summary>
    /// Provides the main interface for message routing.
    /// </summary>
    public class MessageRouter
    {
        protected MicrosoftAppCredentials MicrosoftAppCredentials;

        /// <summary>
        /// The routing data and all the parties the bot has seen including the instances of itself.
        /// </summary>

        private readonly ILogger _logger;

        private readonly IRoutingDataStore _routingDataStore;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="routingDataStore">The routing data store implementation.</param>
        /// <param name="microsoftAppCredentials">The bot application credentials.
        /// May be required, depending on the setup of your app, for sending messages.</param>
        /// <param name="logger">_logger to use.</param>
        public MessageRouter(IRoutingDataStore routingDataStore, MicrosoftAppCredentials microsoftAppCredentials, ILogger logger = null)
        {
            _routingDataStore = routingDataStore;
            MicrosoftAppCredentials = microsoftAppCredentials;
            _logger = logger ?? NullLogger.Instance;
        }
        
        /// <summary>
        /// Sends the given message to the given recipient.
        /// </summary>
        /// <param name="recipient">The conversation reference of the recipient.</param>
        /// <param name="messageActivity">The message activity to send.</param>
        /// <returns>A valid resource response instance, if successful. Null in case of an error.</returns>
        public virtual async Task<ResourceResponse> SendMessageAsync(ConversationReference recipient, IMessageActivity messageActivity)
        {
            if (recipient == null)
            {
                _logger.LogError("The conversation reference is null");
                return null;
            }

            // We need the bot identity in the SAME CHANNEL/CONVERSATION as the RECIPIENT -
            // Otherwise, the platform (e.g. Slack) will reject the incoming message as it does not
            // recognize the sender
            var botInstance = FindBotInstanceForRecipient(recipient);

            if (botInstance?.Bot == null)
            {
                _logger.LogError("Failed to find the bot instance");
                return null;
            }

            messageActivity.From = botInstance.Bot;
            messageActivity.Recipient = recipient.GetChannelAccount();

            // Make sure the message activity contains a valid conversation ID
            if (messageActivity.Conversation == null)
            {
                messageActivity.Conversation = recipient.Conversation;
            }

            var connectorClient = MicrosoftAppCredentials == null ? new ConnectorClient(new Uri(recipient.ServiceUrl)) : new ConnectorClient(new Uri(recipient.ServiceUrl), MicrosoftAppCredentials);
            ResourceResponse resourceResponse = null;

            try
            {
                resourceResponse = await connectorClient.Conversations.SendToConversationAsync((Activity)messageActivity);
            }
            catch (UnauthorizedAccessException e)
            {
                _logger.LogError($"Failed to send message: {e.Message}");
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to send message: {e.Message}");
            }

            return resourceResponse;
        }

        /// <summary>
        /// Sends the given message to the given recipient.
        /// </summary>
        /// <param name="recipient">The conversation reference of the recipient.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>A valid resource response instance, if successful. Null in case of an error.</returns>
        public virtual async Task<ResourceResponse> SendMessageAsync(ConversationReference recipient, string message)
        {
            var messageActivity = Activity.CreateMessageActivity();
            
            if (recipient != null)
            {
                if (recipient.Conversation != null)
                {
                    messageActivity.Conversation = recipient.Conversation;
                }

                var recipientChannelAccount = recipient.GetChannelAccount();

                if (recipientChannelAccount != null)
                {
                    messageActivity.Recipient = recipientChannelAccount;
                }
            }

            messageActivity.Text = message;

            return await SendMessageAsync(recipient, messageActivity);
        }

        /// <summary>
        /// Stores the conversation reference instances (sender and recipient) in the given activity.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <returns>The list of two results, where the first element is for the sender and the last for the recipient.</returns>
        public IList<ModifyRoutingDataResult> StoreConversationReferences(IActivity activity)
        {
            return new List<ModifyRoutingDataResult>()
            {
                AddConversationReference(activity.CreateSenderConversationReference()),
                AddConversationReference(activity.CreateRecipientConversationReference())
            };
        }

        /// <summary>
        /// Tries to initiate a connection (1:1 conversation) by creating a request on behalf of
        /// the given requestor. This method does nothing, if a request for the same user already exists.
        /// </summary>
        /// <param name="requestor">The requestor conversation reference.</param>
        /// <param name="rejectConnectionRequestIfNoAggregationChannel">
        /// If true, will reject all requests, if there is no aggregation channel.</param>
        /// <returns>The result of the operation:
        /// - ConnectionRequestResultType.Created,
        /// - ConnectionRequestResultType.AlreadyExists,
        /// - ConnectionRequestResultType.NotSetup or
        /// - ConnectionRequestResultType.Error (see the error message for more details).
        /// </returns>
        public virtual ConnectionRequestResult CreateConnectionRequest(ConversationReference requestor, bool rejectConnectionRequestIfNoAggregationChannel = false)
        {
            if (requestor == null)
            {
                throw new ArgumentNullException(nameof(requestor));
            }

            ConnectionRequestResult createConnectionRequestResult;
            AddConversationReference(requestor);
            
            var connectionRequest = new ConnectionRequest(requestor);

            if (IsRegisteredForHandoffRequests(requestor))
            {
                createConnectionRequestResult = new ConnectionRequestResult()
                {
                    Type = ConnectionRequestResultType.Error,
                    ErrorMessage = $"The given ConversationReference ({requestor.GetChannelAccount()?.Name}) is associated with aggregation and cannot request a connection"
                };
            }
            else
            {
                createConnectionRequestResult = AddConnectionRequest(
                    connectionRequest, rejectConnectionRequestIfNoAggregationChannel);
            }

            return createConnectionRequestResult;
        }

        /// <summary>
        /// Tries to reject the connection request of the associated with the given conversation reference.
        /// </summary>
        /// <param name="requestorToReject">The conversation reference of the party whose request to reject.</param>
        /// <param name="rejecter">The conversation reference of the party  rejecting the request (optional).</param>
        /// <returns>The result of the operation:
        /// - ConnectionRequestResultType.Rejected or
        /// - ConnectionRequestResultType.Error (see the error message for more details).
        /// </returns>
        public virtual ConnectionRequestResult RejectConnectionRequest(ConversationReference requestorToReject, ConversationReference rejecter = null)
        {
            if (requestorToReject == null)
            {
                throw new ArgumentNullException("The conversation reference instance of the party whose request to reject cannot be null");
            }

            ConnectionRequestResult rejectConnectionRequestResult = null;
            var connectionRequest = FindConnectionRequest(requestorToReject);

            if (connectionRequest != null)
            {
                rejectConnectionRequestResult = RemoveConnectionRequest(connectionRequest);
                rejectConnectionRequestResult.Rejecter = rejecter;
            }

            return rejectConnectionRequestResult;
        }

        /// <summary>
        /// Tries to establish a connection (1:1 chat) between the two given parties.
        ///
        /// Note that the conversation owner will have a new separate conversation reference in the created
        /// conversation, if a new direct conversation is created.
        /// </summary>
        /// <param name="conversationReference1">The conversation reference who owns the conversation (e.g. customer service agent).</param>
        /// <param name="conversationReference2">The other conversation reference in the conversation.</param>
        /// <param name="createNewDirectConversation">
        /// If true, will try to create a new direct conversation between the bot and the
        /// conversation owner (e.g. agent) where the messages from the other (client) conversation
        /// reference are routed.
        ///
        /// Note that this will result in the conversation owner having a new separate conversation
        /// reference in the created connection (for the new direct conversation).
        /// </param>
        /// <returns>
        /// The result of the operation:
        /// - ConnectionResultType.Connected,
        /// - ConnectionResultType.Error (see the error message for more details).
        /// </returns>
        public virtual async Task<ConnectionResult> ConnectAsync(ConversationReference conversationReference1, ConversationReference conversationReference2, bool createNewDirectConversation)
        {
            if (conversationReference1 == null || conversationReference2 == null)
            {
                throw new ArgumentNullException(
                    $"Neither of the arguments ({nameof(conversationReference1)}, {nameof(conversationReference2)}) can be null");
            }

            var botInstance = FindConversationReference(conversationReference1.ChannelId, conversationReference1.Conversation.Id, null, true);

            if (botInstance == null)
            {
                return new ConnectionResult()
                {
                    Type = ConnectionResultType.Error,
                    ErrorMessage = "Failed to find the bot instance"
                };
            }

            ConversationResourceResponse conversationResourceResponse = null;

            if (createNewDirectConversation)
            {
                var conversationReference1ChannelAccount = conversationReference1.GetChannelAccount();

                var connectorClient = new ConnectorClient(
                    new Uri(conversationReference1.ServiceUrl), 
                    MicrosoftAppCredentials);

                try
                {
                    conversationResourceResponse =
                        await connectorClient.Conversations.CreateDirectConversationAsync(
                            botInstance.Bot, conversationReference1ChannelAccount);
                }
                catch (Exception e)
                {
                    _logger.LogError($"Failed to create a direct conversation: {e.Message}");
                    // Do nothing here as we fallback (continue without creating a direct conversation)
                }

                if (conversationResourceResponse != null
                    && !string.IsNullOrEmpty(conversationResourceResponse.Id))
                {
                    // The conversation account of the conversation owner for this 1:1 chat is different -
                    // thus, we need to re-create the conversation owner instance
                    var directConversationAccount = new ConversationAccount(id: conversationResourceResponse.Id);
                    var conversationReference1IsBot = conversationReference1.IsBot();

                    conversationReference1 = new ConversationReference(
                        null,
                        conversationReference1IsBot ? null : conversationReference1ChannelAccount,
                        conversationReference1IsBot ? conversationReference1ChannelAccount : null,
                        directConversationAccount,
                        conversationReference1.ChannelId,
                        conversationReference1.ServiceUrl);

                    AddConversationReference(conversationReference1);

                    AddConversationReference(new ConversationReference(
                        null,
                        null,
                        botInstance.Bot,
                        directConversationAccount,
                        botInstance.ChannelId,
                        botInstance.ServiceUrl));
                }
            }

            var connection = new Connection(conversationReference1, conversationReference2);
            var connectResult = ConnectAndRemoveConnectionRequest(connection, conversationReference2);
            connectResult.ConversationResourceResponse = conversationResourceResponse;

            return connectResult;
        }

        /// <summary>
        /// Disconnects all connections associated with the given conversation reference.
        /// </summary>
        /// <param name="conversationReference">The conversation reference connected in a conversation.</param>
        /// <returns>The results:
        /// - ConnectionResultType.Disconnected,
        /// - ConnectionResultType.Error (see the error message for more details).
        /// </returns>
        public virtual IList<ConnectionResult> Disconnect(ConversationReference conversationReference)
        {
            var disconnectResults = new List<ConnectionResult>();
            var wasDisconnected = true;

            while (wasDisconnected)
            {
                wasDisconnected = false;
                var connection = FindConnection(conversationReference);

                if (connection != null)
                {
                    var disconnectResult = Disconnect(connection);
                    disconnectResults.Add(disconnectResult);

                    if (disconnectResult.Type == ConnectionResultType.Disconnected)
                    {
                        wasDisconnected = true;
                    }
                }
            }

            return disconnectResults;
        }

        /// <summary>
        /// Routes the message in the given activity, if the sender is connected in a conversation.
        /// </summary>
        /// <param name="activity">The activity to handle.</param>
        /// <param name="addNameToMessage">If true, will add the name of the sender to the beginning of the message.</param>
        /// <returns>The result of the operation:
        /// - MessageRouterResultType.NoActionTaken, if no routing rule for the sender is found OR
        /// - MessageRouterResultType.OK, if the message was routed successfully OR
        /// - MessageRouterResultType.FailedToForwardMessage in case of an error (see the error message).
        /// </returns>
        public virtual async Task<MessageRoutingResult> RouteMessageIfSenderIsConnectedAsync(IMessageActivity activity, bool addNameToMessage = true)
        {
            var sender = activity.CreateSenderConversationReference();
            var connection = FindConnection(sender);

            var messageRoutingResult = new MessageRoutingResult()
            {
                Type = MessageRoutingResultType.NoActionTaken,
                Connection = connection
            };

            if (connection != null)
            {
                var recipient = sender.Match(connection.ConversationReference1)
                        ? connection.ConversationReference2 
                        : connection.ConversationReference1;

                if (recipient != null)
                {
                    var message = activity.Text;

                    if (addNameToMessage)
                    {
                        var senderName = sender.GetChannelAccount().Name;

                        if (!string.IsNullOrWhiteSpace(senderName))
                        {
                            message = $"{senderName}: {message}";
                        }
                    }

                    var resourceResponse = await SendMessageAsync(recipient, message);

                    if (resourceResponse != null)
                    {
                        messageRoutingResult.Type = MessageRoutingResultType.MessageRouted;

                        if (!UpdateTimeSinceLastActivity(connection))
                        {
                            _logger.LogError("Failed to update the time since the last activity property of the connection");
                        }
                    }
                    else
                    {
                        messageRoutingResult.Type = MessageRoutingResultType.FailedToRouteMessage;
                        messageRoutingResult.ErrorMessage = $"Failed to forward the message to the recipient";
                    }
                }
                else
                {
                    messageRoutingResult.Type = MessageRoutingResultType.Error;
                    messageRoutingResult.ErrorMessage = "Failed to find the recipient to forward the message to";
                }
            }

            return messageRoutingResult;
        }

        /// <summary>
        /// Checks if the given conversation reference instance is associated with aggregation.
        /// In human tongue this means that the given conversation reference is, for instance,
        /// a customer service agent who deals with the customer connection requests.
        /// </summary>
        /// <param name="conversationReference">The conversation reference to check.</param>
        /// <returns>True, if is associated. False otherwise.</returns>
        public virtual bool IsRegisteredForHandoffRequests(ConversationReference conversationReference)
        {
            var handoffRegisteredUsers = GetUsersRegisteredForHandoffRequests();

            return (conversationReference != null
                    && handoffRegisteredUsers != null
                    && handoffRegisteredUsers.Any()
                    && handoffRegisteredUsers.Any(c => c.Conversation.Id == conversationReference.Conversation.Id
                                                       && c.ServiceUrl == conversationReference.ServiceUrl
                                                       && c.ChannelId == conversationReference.ChannelId));
        }

        /// <summary>
        /// Tries to find a connection request by the given conversation reference
        /// (associated with the requestor).
        /// </summary>
        /// <param name="conversationReference">The conversation reference associated with the requestor.</param>
        /// <returns>The connection request or null, if not found.</returns>
        public ConnectionRequest FindConnectionRequest(ConversationReference conversationReference)
        {
            foreach (var connectionRequest in GetConnectionRequests())
            {
                if (conversationReference.Match(connectionRequest.Requestor))
                {
                    return connectionRequest;
                }
            }

            return null;
        }

        /// <returns>The current global time.</returns>
        public virtual DateTime GetCurrentTime()
        {
            return DateTime.UtcNow;
        }

        /// <summary>
        /// Adds the given ConversationReference.
        /// </summary>
        /// <param name="conversationReferenceToAdd">The new ConversationReference to add.</param>
        /// <returns>The result of the operation with type:
        /// - ModifyRoutingDataResultType.Added,
        /// - ModifyRoutingDataResultType.AlreadyExists or
        /// - ModifyRoutingDataResultType.Error (see the error message for more details).
        /// </returns>
        public virtual ModifyRoutingDataResult AddConversationReference(ConversationReference conversationReferenceToAdd)
        {
            if (conversationReferenceToAdd == null)
            {
                throw new ArgumentNullException(nameof(conversationReferenceToAdd));
            }

            if (conversationReferenceToAdd.IsBot()
                    ? _routingDataStore.GetBotInstances().Contains(conversationReferenceToAdd)
                    : _routingDataStore.GetUsers().Contains(conversationReferenceToAdd))
            {
                return new ModifyRoutingDataResult()
                {
                    Type = ModifyRoutingDataResultType.AlreadyExists
                };
            }

            if (_routingDataStore.AddConversationReference(conversationReferenceToAdd))
            {
                return new ModifyRoutingDataResult()
                {
                    Type = ModifyRoutingDataResultType.Added
                };
            }

            return new ModifyRoutingDataResult()
            {
                Type = ModifyRoutingDataResultType.Error,
                ErrorMessage = "Failed to add the conversation reference"
            };
        }
        
        /// <returns>The aggregation channels as a readonly list.</returns>
        public IList<ConversationReference> GetUsersRegisteredForHandoffRequests()
        {
            return _routingDataStore.GetHandoffChannels();
        }

        /// <summary>
        /// Adds the given aggregation channel.
        /// </summary>
        /// <param name="conversationReferenceToAdd">The aggregation channel to add.</param>
        /// <returns>The result of the operation with type:
        /// - ModifyRoutingDataResultType.Added,
        /// - ModifyRoutingDataResultType.AlreadyExists or
        /// - ModifyRoutingDataResultType.Error (see the error message for more details).
        /// </returns>
        public virtual ModifyRoutingDataResult RegisterUserForHandoffRequests(ConversationReference conversationReferenceToAdd)
        {
            if (conversationReferenceToAdd == null)
            {
                throw new ArgumentNullException(nameof(conversationReferenceToAdd));
            }

            if (conversationReferenceToAdd.GetChannelAccount() != null)
            {
                throw new ArgumentException("The conversation reference instance for an aggregation channel cannot contain a channel account");
            }

            if (string.IsNullOrWhiteSpace(conversationReferenceToAdd.ChannelId)
                || conversationReferenceToAdd.Conversation == null
                || string.IsNullOrWhiteSpace(conversationReferenceToAdd.Conversation.Id))
            {
                return new ModifyRoutingDataResult()
                {
                    Type = ModifyRoutingDataResultType.Error,
                    ErrorMessage = "Aggregation channel must contain a valid channel and conversation ID"
                };
            }

            var registeredUsers = GetUsersRegisteredForHandoffRequests();

            if (registeredUsers.Contains(conversationReferenceToAdd))
            {
                return new ModifyRoutingDataResult()
                {
                    Type = ModifyRoutingDataResultType.AlreadyExists
                };
            }

            if (_routingDataStore.MakeConversationRefAvailableForHandoff(conversationReferenceToAdd))
            {
                return new ModifyRoutingDataResult()
                {
                    Type = ModifyRoutingDataResultType.Added
                };
            }

            return new ModifyRoutingDataResult()
            {
                Type = ModifyRoutingDataResultType.Error,
                ErrorMessage = "Failed to add the aggregation channel"
            };
        }

        /// <summary>
        /// Removes the given aggregation channel.
        /// </summary>
        /// <param name="aggregationChannelToRemove">The aggregation channel to remove.</param>
        /// <returns>True, if removed successfully. False otherwise.</returns>
        public virtual bool DeregisterUserForHandoffRequests(ConversationReference aggregationChannelToRemove)
        {
            return _routingDataStore.MakeConversationRefUnavailableForHandoff(aggregationChannelToRemove);
        }

        /// <returns>The connection requests as a readonly list.</returns>
        public IList<ConnectionRequest> GetConnectionRequests()
        {
            return _routingDataStore.GetConnectionRequests();
        }

        /// <summary>
        /// Adds the given connection request.
        /// </summary>
        /// <param name="connectionRequestToAdd">The connection request to add.</param>
        /// <param name="rejectConnectionRequestIfNoHandoffChannel">
        /// If true, will reject all requests, if there is no aggregation channel.</param>
        /// <returns>The result of the operation:
        /// - ConnectionRequestResultType.Created,
        /// - ConnectionRequestResultType.AlreadyExists,
        /// - ConnectionRequestResultType.NotSetup or
        /// - ConnectionRequestResultType.Error (see the error message for more details).
        /// </returns>
        public virtual ConnectionRequestResult AddConnectionRequest(ConnectionRequest connectionRequestToAdd, bool rejectConnectionRequestIfNoHandoffChannel = false)
        {
            if (connectionRequestToAdd == null)
            {
                throw new ArgumentNullException(nameof(connectionRequestToAdd));
            }

            var addConnectionRequestResult = new ConnectionRequestResult()
            {
                ConnectionRequest = connectionRequestToAdd
            };

            if (GetConnectionRequests().Contains(connectionRequestToAdd))
            {
                addConnectionRequestResult.Type = ConnectionRequestResultType.AlreadyExists;
            }
            else
            {
                if (!GetUsersRegisteredForHandoffRequests().Any() && rejectConnectionRequestIfNoHandoffChannel)
                {
                    addConnectionRequestResult.Type = ConnectionRequestResultType.NotSetup;
                }
                else
                {
                    connectionRequestToAdd.ConnectionRequestTime = GetCurrentTime();

                    if (_routingDataStore.AddConnectionRequest(connectionRequestToAdd))
                    {
                        addConnectionRequestResult.Type = ConnectionRequestResultType.Created;
                    }
                    else
                    {
                        addConnectionRequestResult.Type = ConnectionRequestResultType.Error;
                        addConnectionRequestResult.ErrorMessage = "Failed to add the connection request - this is likely an error caused by the storage implementation";
                    }
                }
            }

            return addConnectionRequestResult;
        }

        /// <summary>
        /// Removes the connection request of the user with the given conversation reference.
        /// </summary>
        /// <param name="connectionRequestToRemove">The connection request to remove.</param>
        /// <returns>The result of the operation:
        /// - ConnectionRequestResultType.Rejected or
        /// - ConnectionRequestResultType.Error (see the error message for more details).
        /// </returns>
        public virtual ConnectionRequestResult RemoveConnectionRequest(ConnectionRequest connectionRequestToRemove)
        {
            if (connectionRequestToRemove == null)
            {
                throw new ArgumentNullException(nameof(connectionRequestToRemove));
            }

            var removeConnectionRequestResult = new ConnectionRequestResult
            {
                ConnectionRequest = connectionRequestToRemove
            };

            if (GetConnectionRequests().Contains(connectionRequestToRemove))
            {
                if (_routingDataStore.RemoveConnectionRequest(connectionRequestToRemove))
                {
                    removeConnectionRequestResult.Type = ConnectionRequestResultType.Rejected;
                }
                else
                {
                    removeConnectionRequestResult.Type = ConnectionRequestResultType.Error;
                    removeConnectionRequestResult.ErrorMessage = "Failed to remove the connection request associated with the given user";
                }
            }
            else
            {
                removeConnectionRequestResult.Type = ConnectionRequestResultType.Error;
                removeConnectionRequestResult.ErrorMessage = "Could not find a connection request associated with the given user";
            }

            return removeConnectionRequestResult;
        }

        /// <summary>
        /// Tries to find a connection associated with the given conversation reference.
        /// </summary>
        /// <param name="conversationReference">The conversation reference associated with the connection to find.</param>
        /// <returns>The connection or null, if not found.</returns>
        public virtual Connection FindConnection(ConversationReference conversationReference)
        {
            foreach (var connection in _routingDataStore.GetConnections())
            {
                if (conversationReference.Match(connection.ConversationReference1)
                    || conversationReference.Match(connection.ConversationReference2))
                {
                    return connection;
                }
            }

            return null;
        }
        
        /// <summary>
        /// Adds the given connection and clears the connection request associated with the given
        /// conversation reference instance, if one exists.
        /// </summary>
        /// <param name="connectionToAdd">The connection to add.</param>
        /// <param name="requestor">The requestor.</param>
        /// <returns>The result of the operation:
        /// - ConnectionResultType.Connected,
        /// - ConnectionResultType.Error (see the error message for more details).
        /// </returns>
        public virtual ConnectionResult ConnectAndRemoveConnectionRequest(Connection connectionToAdd, ConversationReference requestor)
        {
            var connectResult = new ConnectionResult()
            {
                Connection = connectionToAdd
            };

            connectionToAdd.TimeSinceLastActivity = GetCurrentTime();
            bool wasConnectionAdded = _routingDataStore.AddConnection(connectionToAdd);

            if (wasConnectionAdded)
            {
                var acceptedConnectionRequest = FindConnectionRequest(requestor);

                if (acceptedConnectionRequest == null)
                {
                    _logger.LogError("Failed to find the connection request to remove");
                }
                else
                {
                    RemoveConnectionRequest(acceptedConnectionRequest);
                }

                connectResult.Type = ConnectionResultType.Connected;
                connectResult.ConnectionRequest = acceptedConnectionRequest;
            }
            else
            {
                connectResult.Type = ConnectionResultType.Error;
                connectResult.ErrorMessage = $"Failed to add the connection {connectionToAdd}";
            }

            return connectResult;
        }

        /// <summary>
        /// Updates the time since last activity property of the given connection instance.
        /// </summary>
        /// <param name="connection">The connection to update.</param>
        /// <returns>True, if the connection was updated successfully. False otherwise.</returns>
        public virtual bool UpdateTimeSinceLastActivity(Connection connection)
        {
            if (_routingDataStore.RemoveConnection(connection))
            {
                connection.TimeSinceLastActivity = GetCurrentTime();
                return _routingDataStore.AddConnection(connection);
            }

            return false;
        }

        /// <summary>
        /// Disconnects the given connection.
        /// </summary>
        /// <param name="connectionToDisconnect">The connection to disconnect.</param>
        /// <returns>The result of the operation:
        /// - ConnectionResultType.Disconnected,
        /// - ConnectionResultType.Error (see the error message for more details).
        /// </returns>
        public virtual ConnectionResult Disconnect(Connection connectionToDisconnect)
        {
            var disconnectResult = new ConnectionResult()
            {
                Connection = connectionToDisconnect
            };

            foreach (var connection in _routingDataStore.GetConnections())
            {
                if (connectionToDisconnect.Equals(connection))
                {
                    if (_routingDataStore.RemoveConnection(connectionToDisconnect))
                    {
                        disconnectResult.Type = ConnectionResultType.Disconnected;
                    }
                    else
                    {
                        disconnectResult.Type = ConnectionResultType.Error;
                        disconnectResult.ErrorMessage = "Failed to remove the connection";
                    }

                    break;
                }
            }

            return disconnectResult;
        }

        /// <summary>
        /// Tries to find the conversation references in the given list matching the given criteria.
        /// You can define one or more criteria, but you must define at least one.
        /// </summary>
        /// <param name="conversationReferencesToSearch">The list of conversation references to search.</param>
        /// <param name="channelId">The channel ID to match (optional).</param>
        /// <param name="conversationAccountId">The conversation account ID to match (optional).</param>
        /// <param name="channelAccountId">The channel account ID to match (optional).</param>
        /// <param name="onlyBotInstances">If true, will only look for the conversation reference instances belonging to a bot.</param>
        /// <returns>The list of matching conversation references or null, if none found.</returns>
        public virtual IList<ConversationReference> FindConversationReferences(
            IList<ConversationReference> conversationReferencesToSearch,
            string channelId = null,
            string conversationAccountId = null,
            string channelAccountId = null,
            bool onlyBotInstances = false)
        {
            if (string.IsNullOrWhiteSpace(channelId)
                && string.IsNullOrWhiteSpace(conversationAccountId)
                && string.IsNullOrWhiteSpace(channelAccountId))
            {
                throw new ArgumentNullException("At least one search criteria must be defined");
            }

            IEnumerable<ConversationReference> conversationReferencesFound = null;

            try
            {
                conversationReferencesFound = conversationReferencesToSearch.Where(conversationReference =>
                {
                    if (onlyBotInstances && !conversationReference.IsBot())
                    {
                        return false;
                    }

                    if (!string.IsNullOrWhiteSpace(channelId))
                    {
                        if (string.IsNullOrWhiteSpace(conversationReference.ChannelId)
                            || !conversationReference.ChannelId.Equals(channelId))
                        {
                            return false;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(conversationAccountId))
                    {
                        if (conversationReference.Conversation == null
                            || string.IsNullOrWhiteSpace(conversationReference.Conversation.Id)
                            || !conversationReference.Conversation.Id.Equals(conversationAccountId))
                        {
                            return false;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(channelAccountId))
                    {
                        var channelAccount = conversationReference.GetChannelAccount();

                        if (channelAccount == null
                            || string.IsNullOrWhiteSpace(channelAccount.Id)
                            || !channelAccount.Id.Equals(channelAccountId))
                        {
                            return false;
                        }
                    }

                    return true;
                });
            }
            catch (ArgumentNullException e)
            {
                _logger.LogError($"Failed to search conversation references: {e.Message}");
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError($"Failed to search conversation references: {e.Message}");
            }

            return conversationReferencesFound?.ToArray();
        }

        /// <summary>
        /// Tries to find the conversation references in all the collections including connection
        /// requests and connections.
        ///
        /// You can define one or more criteria, but you must define at least one.
        /// </summary>
        /// <param name="channelId">The channel ID to match (optional).</param>
        /// <param name="conversationAccountId">The conversation account ID to match (optional).</param>
        /// <param name="channelAccountId">The channel account ID to match (optional).</param>
        /// <param name="onlyBotInstances">If true, will only look for the conversation reference instances belonging to a bot.</param>
        /// <returns>The conversation reference instance matching the given search criteria or null, if not found.</returns>
        public virtual ConversationReference FindConversationReference(
            string channelId = null,
            string conversationAccountId = null,
            string channelAccountId = null,
            bool onlyBotInstances = false)
        {
            var conversationReferencesToSearch = new List<ConversationReference>();

            if (!onlyBotInstances)
            {
                // Users
                conversationReferencesToSearch.AddRange(_routingDataStore.GetUsers());
            }

            conversationReferencesToSearch.AddRange(_routingDataStore.GetBotInstances());

            var conversationReferencesFound = FindConversationReferences(
                    conversationReferencesToSearch,
                    channelId,
                    conversationAccountId,
                    channelAccountId,
                    onlyBotInstances);

            if (conversationReferencesFound == null || conversationReferencesFound.Count == 0)
            {
                conversationReferencesToSearch.Clear();

                // Connection requests
                foreach (var connectionRequest in GetConnectionRequests())
                {
                    conversationReferencesToSearch.Add(connectionRequest.Requestor);
                }

                conversationReferencesFound = FindConversationReferences(
                    conversationReferencesToSearch,
                    channelId, conversationAccountId, channelAccountId, onlyBotInstances);
            }

            if (conversationReferencesFound == null || conversationReferencesFound.Count == 0)
            {
                conversationReferencesToSearch.Clear();

                // Connections
                foreach (var connection in _routingDataStore.GetConnections())
                {
                    conversationReferencesToSearch.Add(connection.ConversationReference1);
                    conversationReferencesToSearch.Add(connection.ConversationReference2);
                }

                conversationReferencesFound = FindConversationReferences(
                    conversationReferencesToSearch,
                    channelId, conversationAccountId, channelAccountId, onlyBotInstances);
            }

            if (conversationReferencesFound != null && conversationReferencesFound.Count > 0)
            {
                return conversationReferencesFound[0];
            }

            return null;
        }

        /// <summary>
        /// Tries to find the bot instance that can reach the recipient.
        /// This means that the bot instance, if found, is in the same channel and the conversation
        /// as the recipient.
        /// </summary>
        /// <param name="recipient">The recipient.</param>
        /// <returns>The bot instance or null, if not found.</returns>
        public ConversationReference FindBotInstanceForRecipient(ConversationReference recipient)
        {
            if (recipient == null)
            {
                throw new ArgumentNullException("The given conversation reference is null");
            }

            return FindConversationReference(
                recipient.ChannelId, recipient.Conversation?.Id, null, true);
        }
    }
}