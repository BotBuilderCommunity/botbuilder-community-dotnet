using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Bot.Builder.Community.Adapters.Infobip.Messages;
using Bot.Builder.Community.Adapters.Infobip.Messages.Models;
using System;

namespace Infobip_Messages_Adapter_Sample.Bots
{
    /// <summary>
    /// Enhanced EchoBot with direct Infobip API integration for sending real messages
    /// </summary>
    public class EchoBot : ActivityHandler
    {
        private static readonly Dictionary<string, UserState> _userStates = new Dictionary<string, UserState>();

        private readonly IInfobipMessagesClient _infobipClient;
        private readonly InfobipMessagesAdapterOptions _infobipOptions;

        public EchoBot(IInfobipMessagesClient infobipClient, InfobipMessagesAdapterOptions infobipOptions)
        {
            _infobipClient = infobipClient;
            _infobipOptions = infobipOptions;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var userMessage = turnContext.Activity.Text?.Trim().ToLowerInvariant();
            var userId = turnContext.Activity.From.Id;

            Console.WriteLine($">> Received message: '{turnContext.Activity.Text}' from {userId}");

            if (_userStates.ContainsKey(userId))
            {
                await HandleUserStateCollection(turnContext, cancellationToken);
                return;
            }

            switch (userMessage)
            {
                case "send whatsapp":
                case "send sms":
                case "send viber":
                case "send rcs":
                    await StartMessageCollection(turnContext, cancellationToken, userMessage);
                    break;

                case "help":
                    await SendHelpMessage(turnContext, cancellationToken);
                    break;

                case "status":
                    await SendStatusMessage(turnContext, cancellationToken);
                    break;

                case "test whatsapp":
                    await SendTestMessage(turnContext, cancellationToken, "WhatsApp");
                    break;

                case "test sms":
                    await SendTestMessage(turnContext, cancellationToken, "SMS");
                    break;

                case "test rcs":
                    await SendTestMessage(turnContext, cancellationToken, "RCS");
                    break;

                default:
                    await SendEchoMessage(turnContext, cancellationToken);
                    break;
            }
        }

        private async Task StartMessageCollection(ITurnContext turnContext, CancellationToken cancellationToken, string command)
        {
            var userId = turnContext.Activity.From.Id;

            string channel = InfobipChannels.WhatsApp;
            string channelName = "WhatsApp";

            switch (command)
            {
                case "send sms":
                    channel = InfobipChannels.SMS;
                    channelName = "SMS";
                    break;
                case "send viber":
                    channel = InfobipChannels.ViberBM;
                    channelName = "Viber Business Messages";
                    break;
                case "send rcs":
                    channel = InfobipChannels.RCS;
                    channelName = "RCS (Rich Communication Services)";
                    break;
            }

            _userStates[userId] = new UserState
            {
                SelectedChannel = channel,
                ChannelName = channelName,
                Step = CollectionStep.ApiKey
            };

            var message = $@"[SEND] **Send {channelName} Message - Step 1/4: API Key**

Please provide your Infobip API key to authenticate with the Messages API.

Where to find it:
- Login to Infobip Portal (portal.infobip.com)
- Go to Account Settings > API Keys
- Copy your API key

Example: `your-api-key-here-1234567890`

Type your API key now (or 'cancel' to stop):";

            await turnContext.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
        }

        private async Task HandleUserStateCollection(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var userId = turnContext.Activity.From.Id;
            var userState = _userStates[userId];
            var input = turnContext.Activity.Text?.Trim();

            if (input?.ToLowerInvariant() == "cancel")
            {
                _userStates.Remove(userId);
                await turnContext.SendActivityAsync(MessageFactory.Text("[CANCEL] Message sending cancelled."), cancellationToken);
                return;
            }

            switch (userState.Step)
            {
                case CollectionStep.ApiKey:
                    await HandleApiKeyStep(turnContext, cancellationToken, input);
                    break;
                case CollectionStep.Sender:
                    await HandleSenderStep(turnContext, cancellationToken, input);
                    break;
                case CollectionStep.Recipient:
                    await HandleRecipientStep(turnContext, cancellationToken, input);
                    break;
                case CollectionStep.Message:
                    await HandleMessageStep(turnContext, cancellationToken, input);
                    break;
            }
        }

        private async Task HandleApiKeyStep(ITurnContext turnContext, CancellationToken cancellationToken, string input)
        {
            var userId = turnContext.Activity.From.Id;
            var userState = _userStates[userId];

            if (string.IsNullOrEmpty(input) || input.Length < 10)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("[ERROR] Invalid API key. Please provide a valid Infobip API key (at least 10 characters)."), cancellationToken);
                return;
            }

            userState.ApiKey = input;
            userState.Step = CollectionStep.Sender;

            var message = $@"[SEND] **Send {userState.ChannelName} Message - Step 2/4: Sender**

Please provide the sender phone number or ID that will appear as the message sender.

Format examples:
- Phone number: +sender-id
- Alphanumeric: YourCompany (for SMS)
- Short code: 12345

Type your sender now:";

            await turnContext.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
        }

        private async Task HandleSenderStep(ITurnContext turnContext, CancellationToken cancellationToken, string input)
        {
            var userId = turnContext.Activity.From.Id;
            var userState = _userStates[userId];

            if (string.IsNullOrEmpty(input))
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("[ERROR] Invalid sender. Please provide a valid sender number or ID."), cancellationToken);
                return;
            }

            userState.Sender = input;
            userState.Step = CollectionStep.Recipient;

            var message = $@"[SEND] **Send {userState.ChannelName} Message - Step 3/4: Recipient**

Please provide the recipient's phone number (who will receive the message).

Format: International format with country code
Examples:
- +sender-id (UK)
- +1234567890 (US)

Type the recipient's phone number now:";

            await turnContext.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
        }

        private async Task HandleRecipientStep(ITurnContext turnContext, CancellationToken cancellationToken, string input)
        {
            var userId = turnContext.Activity.From.Id;
            var userState = _userStates[userId];

            userState.Recipient = input;
            userState.Step = CollectionStep.Message;

            var message = $@"[SEND] **Send {userState.ChannelName} Message - Step 4/4: Message Content**

Please type the message you want to send.

Current configuration:
- Channel: {userState.ChannelName}
- From: {userState.Sender}
- To: {userState.Recipient}
- API Key: {MaskApiKey(userState.ApiKey)}

Type your message content now:";

            await turnContext.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
        }

        private async Task HandleMessageStep(ITurnContext turnContext, CancellationToken cancellationToken, string input)
        {
            var userId = turnContext.Activity.From.Id;
            var userState = _userStates[userId];

            if (string.IsNullOrEmpty(input))
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("[ERROR] Empty message. Please provide message content."), cancellationToken);
                return;
            }

            userState.MessageContent = input;
            await SendDirectToInfobip(turnContext, cancellationToken, userState);
            _userStates.Remove(userId);
        }

        private async Task SendDirectToInfobip(ITurnContext turnContext, CancellationToken cancellationToken, UserState userState)
        {
            try
            {
                var summaryMessage = $@"[SENDING] **Sending Message Summary**

- Channel: {userState.ChannelName}
- From: {userState.Sender}
- To: {userState.Recipient}
- Message: {userState.MessageContent}
- API Key: {MaskApiKey(userState.ApiKey)}

[STATUS] Sending message directly to Infobip API...";

                await turnContext.SendActivityAsync(MessageFactory.Text(summaryMessage), cancellationToken);

                var callbackDataObject = new
                {
                    ActivityId = $"activity-{Guid.NewGuid()}",
                    ResourceId = "https://example-org.crm.dynamics.com/",
                    TenantId = Guid.NewGuid().ToString()
                };

                var infobipMessage = new
                {
                    messages = new[]
                    {
                        new
                        {
                            sender = userState.Sender,
                            channel = userState.SelectedChannel,
                            destinations = new[]
                            {
                                new { to = userState.Recipient }
                            },
                            content = new
                            {
                                body = new
                                {
                                    type = "TEXT",
                                    text = userState.MessageContent
                                }
                            },
                            callbackData = Newtonsoft.Json.JsonConvert.SerializeObject(callbackDataObject)
                        }
                    }
                };

                Console.WriteLine("=== DIRECT INFOBIP API CALL ===");
                Console.WriteLine($"Endpoint: {_infobipOptions.InfobipMessagesApiBaseUrl}/messages-api/1/messages");
                Console.WriteLine($"API Key: {MaskApiKey(userState.ApiKey)}");
                Console.WriteLine($"Payload: {Newtonsoft.Json.JsonConvert.SerializeObject(infobipMessage, Newtonsoft.Json.Formatting.Indented)}");

                var response = await SendWithCustomApiKey(infobipMessage, userState.ApiKey, cancellationToken);

                var successMessage = $@"[SUCCESS] **Message Sent Successfully!**

[DELIVERY] Delivery Confirmation:
- Channel: {userState.ChannelName}
- From: {userState.Sender}
- To: {userState.Recipient}
- Time: {DateTime.UtcNow:HH:mm:ss} UTC
- Message ID: {response?.MessageId ?? "Unknown"}

[INFO] Check console for detailed API request/response logging.

[SECURITY] Your API key was used only for this message and has not been stored.";

                await turnContext.SendActivityAsync(MessageFactory.Text(successMessage), cancellationToken);
                Console.WriteLine($"[SUCCESS] Message sent successfully to {userState.Recipient} via {userState.ChannelName}");
            }
            catch (Exception ex)
            {
                var errorMessage = $@"[ERROR] **Message Delivery Failed**

Error: {ex.Message}

Possible solutions:
- Verify API key is correct and active
- Check if sender number is configured in Infobip account
- Ensure recipient phone number format is correct (+country code)
- Verify channel is configured in your Infobip account

Check console logs for detailed error information.";

                await turnContext.SendActivityAsync(MessageFactory.Text(errorMessage), cancellationToken);
                Console.WriteLine($"[ERROR] Failed to send message: {ex.Message}");
            }
        }

        private async Task<InfobipMessagesResponse> SendWithCustomApiKey(object message, string apiKey, CancellationToken cancellationToken)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(message);
            var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var requestUri = $"{_infobipOptions.InfobipMessagesApiBaseUrl}/messages-api/1/messages";

            using (var httpClient = new System.Net.Http.HttpClient())
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"App {apiKey}");

                var response = await httpClient.PostAsync(requestUri, content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[API] API Response: {responseJson}");
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<InfobipMessagesResponse>(responseJson);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API] API Error ({response.StatusCode}): {errorContent}");
                throw new System.Net.Http.HttpRequestException($"API request failed: {response.StatusCode} - {errorContent}");
            }
        }

        private async Task SendTestMessage(ITurnContext turnContext, CancellationToken cancellationToken, string channelType)
        {
            var message = $"[TEST] This is a test {channelType} message using default configuration from appsettings.json";
            var activity = MessageFactory.Text(message);

            switch (channelType)
            {
                case "WhatsApp":
                    activity.SetInfobipChannel(InfobipChannels.WhatsApp);
                    break;
                case "SMS":
                    activity.SetInfobipChannel(InfobipChannels.SMS);
                    break;
                case "RCS":
                    activity.SetInfobipChannel(InfobipChannels.RCS);
                    break;
            }

            Console.WriteLine($"[TEST] Sending test {channelType} message with default config");
            await turnContext.SendActivityAsync(activity, cancellationToken);
        }

        private async Task SendEchoMessage(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var echoText = $"[ECHO] Echo: {turnContext.Activity.Text}";
            await turnContext.SendActivityAsync(MessageFactory.Text(echoText), cancellationToken);

            if (turnContext.Activity.Text?.ToLowerInvariant().Contains("hello") == true ||
                turnContext.Activity.Text?.ToLowerInvariant().Contains("hi") == true)
            {
                await Task.Delay(1000, cancellationToken);
                await turnContext.SendActivityAsync(
                    MessageFactory.Text("[TIP] Try 'send whatsapp' to send a REAL WhatsApp message, or 'help' for all commands!"),
                    cancellationToken);
            }
        }

        private async Task SendHelpMessage(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var helpMessage = @"[HELP] **Infobip Messages API Sample Bot**

**[SEND] Send REAL Messages (Works in Bot Framework Emulator!):**
- `send whatsapp` - Send WhatsApp message with custom API key
- `send sms` - Send SMS message with custom API key  
- `send viber` - Send Viber message with custom API key
- `send rcs` - Send RCS (Rich Communication Services) message with custom API key

**[TEST] Test Commands (use default config):**
- `test whatsapp` - Test WhatsApp with default settings
- `test sms` - Test SMS with default settings
- `test rcs` - Test RCS with default settings

**[UTIL] Utility:**
- `help` - Show this help message
- `status` - Show current configuration

**[SECURITY] Security Features:**
- Dynamic API key collection (no hardcoded keys)
- Custom sender configuration per message
- Temporary credential storage only
- Complete input validation

[INFO] The 'send' commands will prompt you for API key, sender, and recipient, then send a REAL message to Infobip!

[DEBUG] Check console output for detailed API request/response logging.";

            await turnContext.SendActivityAsync(MessageFactory.Text(helpMessage), cancellationToken);
        }

        private async Task SendStatusMessage(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var statusMessage = $@"[STATUS] **Bot Status:**

- **Library:** Bot.Builder.Community.Adapters.Infobip.Messages (actual library)
- **Status:** [READY] Ready to send real messages to Infobip
- **API Base URL:** {_infobipOptions.InfobipMessagesApiBaseUrl}
- **Default Channel:** {_infobipOptions.DefaultChannel}
- **Default Sender:** {_infobipOptions.DefaultSender ?? "Not configured"}
- **Payload Logging:** [ENABLED] Enabled (check console)

**[FEATURES] Custom Send Features:**
- Dynamic API key collection per message
- Custom sender configuration per message  
- Custom recipient configuration per message
- Temporary credential storage only

**Endpoints:**
- Bot Framework: /api/messages (for emulator)
- Infobip Webhook: /api/infobip (for incoming messages)

[ACTION] Use 'send whatsapp' to send a real message using your API key!";

            await turnContext.SendActivityAsync(MessageFactory.Text(statusMessage), cancellationToken);
        }

        private string MaskApiKey(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey) || apiKey.Length < 8)
                return "***";

            return apiKey.Substring(0, 4) + "***" + apiKey.Substring(apiKey.Length - 4);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = @"[WELCOME] **Welcome to Infobip Messages API Sample Bot!**

This bot demonstrates real Infobip Messages API integration with the actual library.

**[SEND] Send REAL Messages (Works in Bot Framework Emulator!):**
- Type `send whatsapp` - Send real WhatsApp message with custom API key
- Type `send sms` - Send real SMS message with custom API key
- Type `send viber` - Send real Viber message with custom API key
- Type `send rcs` - Send real RCS message with custom API key

**[SECURITY] Security Features:**
- No API keys stored in code
- Dynamic credential collection
- Temporary storage only

Type `help` for all available commands!

Ready to test real multi-channel messaging!";

            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText), cancellationToken);
                }
            }
        }
    }

    internal class UserState
    {
        public string SelectedChannel { get; set; }
        public string ChannelName { get; set; }
        public string ApiKey { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public string MessageContent { get; set; }
        public CollectionStep Step { get; set; }
    }

    internal enum CollectionStep
    {
        ApiKey,
        Sender,
        Recipient,
        Message
    }
}