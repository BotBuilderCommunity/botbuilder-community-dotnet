# Infobip Messages API Sample Bot

The Infobip Messages API adapter includes a comprehensive sample bot that demonstrates **real Infobip API integration** using the actual `Bot.Builder.Community.Adapters.Infobip.Messages` library.

## 📍 Sample Location

The sample bot is located at:

```
samples/Infobip Messages Adapter Sample
```

## 🏗️ Architecture

```
┌─────────────────────────────────────────┐
│           Sample Bot Project            │
│   (samples/Infobip Messages...)         │
│                                         │
│  ┌─────────────────────────────────┐    │
│  │         EchoBot.cs              │    │
│  │  • Uses extension methods       │    │
│  │  • Tests all channels          │    │
│  │  • Demonstrates features       │    │
│  └─────────────────────────────────┘    │
│                   │                     │
│                   ▼                     │
│  ┌─────────────────────────────────┐    │
│  │      Startup.cs                 │    │
│  │  • References actual library    │    │
│  │  • Configures DI               │    │
│  │  • Sets up adapters            │    │
│  └─────────────────────────────────┘    │
└─────────────────────────────────────────┘
                   │
                   │ ProjectReference
                   ▼
┌─────────────────────────────────────────┐
│        Infobip Messages Library         │
│  (libraries/Bot.Builder.Community       │
│   .Adapters.Infobip.Messages)          │
│                                         │
│  • InfobipMessagesAdapter               │
│  • InfobipMessagesClient                │
│  • Extension Methods                    │
│  • Channel Configuration               │
│  • Payload Logging                     │
└─────────────────────────────────────────┘
                   │
                   │ HTTP API Calls
                   ▼
┌─────────────────────────────────────────┐
│           Infobip Portal                │
│    (Messages API Endpoints)             │
│                                         │
│  • https://api.infobip.com       │
│  • /messages-api/1/messages             │
│  • Real message delivery               │
└─────────────────────────────────────────┘
```

## 🚀 Key Features Demonstrated

- ✅ **Real API Integration** - Uses actual library to send messages to Infobip
- ✅ **Multiple Channel Support** (WhatsApp, SMS, Viber, etc.)
- ✅ **Payload Logging** with console output and structured logging
- ✅ **Channel-specific messaging** with proper ALL CAPS formatting
- ✅ **Interactive buttons and lists**
- ✅ **Media message handling**
- ✅ **Callback data tracking**
- ✅ **Template messages**
- ✅ **Error handling and debugging**
- ✅ **Webhook endpoint configuration**
- ✅ **Regional endpoint support** (e.g., `https://api.infobip.com`)

## 📋 Prerequisites

1. **Infobip Account** with Messages API access
2. **API Credentials** (API Key, Base URL, App Secret)
3. **Configured Channels** (WhatsApp, SMS, etc.) in Infobip portal
4. **.NET Core 3.1 or .NET 5+**

## ⚙️ Configuration

Navigate to the sample directory and update `appsettings.json` with your **real** Infobip credentials:

```bash
cd samples/Infobip\ Messages\ Adapter\ Sample
```

Update `appsettings.json`:

```json
{
  "InfobipApiKey": "your-actual-infobip-api-key",
  "InfobipMessagesApiBaseUrl": "https://api.infobip.com",
  "InfobipAppSecret": "your-webhook-secret",
  "DefaultSender": "sender-id",
  "DefaultChannel": "WHATSAPP",
  "EnablePayloadLogging": true,
  "EnableAutomaticChannelDetection": true,
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Bot.Builder.Community.Adapters.Infobip": "Debug"
    }
  }
}
```

## 🏃‍♂️ Running the Sample

1. **Navigate to sample directory:**

   ```bash
   cd samples/Infobip\ Messages\ Adapter\ Sample
   ```

2. **Install dependencies:**

   ```bash
   dotnet restore
   ```

3. **Build the project:**

   ```bash
   dotnet build
   ```

4. **Run the bot:**

   ```bash
   dotnet run
   ```

5. **Test with Bot Framework Emulator:**

   - Connect to: `http://localhost:5000/api/messages`
   - Try the various test commands

6. **Test with Infobip webhook:**
   - Use ngrok: `ngrok http 5000`
   - Configure Infobip webhook: `https://your-ngrok-url.ngrok-free.app/api/infobip`

## 💬 Test Commands That Send Real Messages

The sample bot responds to these commands and **actually sends messages to Infobip**:

### Channel Testing (Real API Calls)

- `"test whatsapp"` - Send via WhatsApp with explicit channel setting
- `"test sms"` - Send via SMS with explicit channel setting
- `"test viber"` - Send via Viber Business Messages
- `"test channels"` - Show all supported channels

### Message Types (Real API Integration)

- `"test buttons"` - Interactive buttons (works best on WhatsApp)
- `"test list"` - Interactive list (WhatsApp specific)
- `"test media"` - Media message with image
- `"test location"` - Location sharing
- `"test template"` - WhatsApp template message

### Advanced Features (Real Tracking)

- `"test callback"` - Message with callback data for delivery tracking
- `"test tracking"` - Message with entity/app ID tracking
- `"test fallback"` - Multi-channel fallback configuration
- `"debug payload"` - Show expected payload format

### Utility

- `"help"` - Show all available commands
- `"status"` - Show current configuration

## 📋 Expected Payload Format (Actual API Calls)

The sample bot generates real payloads sent to Infobip in the correct format with ALL CAPS:

```json
{
  "messages": [
    {
      "sender": "sender-id",
      "channel": "WHATSAPP",
      "destinations": [
        {
          "to": "user-conversation-id"
        }
      ],
      "content": {
        "body": {
          "type": "TEXT",
          "text": "Hello World"
        }
      }
    }
  ]
}
```

### Key Format Requirements (Verified with Real API):

- ✅ Channel names are in **ALL CAPS** (e.g., `"WHATSAPP"`, `"SMS"`, `"VIBER_BM"`)
- ✅ Content types are in **ALL CAPS** (e.g., `"TEXT"`, `"DOCUMENT"`, `"LOCATION"`)
- ✅ Structure matches Infobip Messages API requirements exactly
- ✅ All requests are logged to console for verification

## 🐛 Debugging Features

### Console Output (Real API Logging)

The sample bot prints detailed payload information for **actual API calls**:

```
=== INFOBIP MESSAGES API REQUEST ===
Endpoint: https://api.infobip.com/messages-api/1/messages
Payload: {"messages":[{"sender":"sender-id","channel":"WHATSAPP",...}]}
===================================

🚀 Sending message to Infobip Messages API
🎯 Channel detected from channel data: WHATSAPP
📤 Sender: sender-id
📍 Destination: user-conversation-id
✅ Infobip Messages API response successful
📥 Response Payload: {"messageId":"msg-12345","status":{"groupId":1,"name":"PENDING_ACCEPTED"}}
```

### Logger Output

Structured logging with emojis for easy identification:

```
🚀 Sending message to Infobip Messages API
🎯 Channel detected from channel data: WHATSAPP
📤 Sender: sender-id
📍 Destination: user-conversation-id
✅ Infobip Messages API response successful
```

## 🔧 Library Integration Examples

The sample demonstrates real usage of the library:

### Channel Configuration (Real Implementation)

```csharp
// Method 1: Simple channel setting
var activity = MessageFactory.Text("Hello from WhatsApp!");
activity.SetInfobipChannel(InfobipChannels.WhatsApp);
await turnContext.SendActivityAsync(activity);

// Method 2: Channel with fallback
activity.SetInfobipChannelConfiguration(
    primaryChannel: InfobipChannels.WhatsApp,
    fallbackChannels: new[] { InfobipChannels.SMS, InfobipChannels.ViberBM }
);

// Method 3: Using channel data
activity.ChannelData = new Dictionary<string, object>
{
    ["infobipChannel"] = InfobipChannels.SMS
};
```

### Interactive Features (Real API Integration)

```csharp
// Interactive buttons that send real API calls
var reply = MessageFactory.SuggestedActions(
    new[] { "✅ Yes", "❌ No", "🤔 Maybe" },
    "Choose an option:");
reply.SetInfobipChannel(InfobipChannels.WhatsApp);

// Interactive lists that generate real API payloads
var sections = new[]
{
    new InfobipSection
    {
        SectionTitle = "Popular Items",
        Items = new[]
        {
            new InfobipListItem { Id = "product1", Text = "iPhone 15 Pro", Description = "Latest Apple smartphone" }
        }
    }
};
activity.AddInfobipInteractiveList("Product Catalog", sections);
```

### Tracking and Analytics (Real Data)

```csharp
// Add callback data for real delivery tracking
var callbackData = new Dictionary<string, string>
{
    ["messageType"] = "test",
    ["userId"] = turnContext.Activity.From.Id,
    ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
};
activity.AddInfobipCallbackData(callbackData);

// Add entity/application tracking for real analytics
activity.AddInfobipEntityId($"entity-{DateTime.UtcNow:yyyyMMdd}");
activity.AddInfobipApplicationId("sample-bot-app");
```

## 📞 Troubleshooting

### Common Issues

1. **"Channel field missing"**

   - Solution: The sample automatically sets default channel if missing
   - Check console logs for channel detection messages

2. **"Invalid API credentials"**

   - Solution: Verify credentials in `appsettings.json`
   - Test with the library's health check functionality

3. **"Messages not delivered"**

   - Solution: Check console logs for detailed API responses
   - Verify channel configuration in Infobip portal
   - Ensure real API credentials are configured

4. **"Payload format incorrect"**
   - Solution: The sample ensures ALL CAPS formatting automatically
   - Check console output for actual payload sent to API

### Debug Mode

The sample includes comprehensive logging. Enable detailed logging:

```json
{
  "Logging": {
    "LogLevel": {
      "Bot.Builder.Community.Adapters.Infobip": "Debug"
    }
  }
}
```

## 🧪 Testing Workflow

1. **Start the sample bot** from the samples directory
2. **Connect Bot Framework Emulator** to test basic functionality
3. **Try test commands** to see different message types and **real API calls**
4. **Check console output** for payload logging and API responses
5. **Use ngrok** for webhook testing with real Infobip channels
6. **Monitor delivery reports** for callback data tracking
7. **Verify messages** are actually received on your devices

## 📚 Additional Resources

- [Library Implementation](../../libraries/Bot.Builder.Community.Adapters.Infobip.Messages/)
- [Channel Configuration Guide](../../libraries/Bot.Builder.Community.Adapters.Infobip.Messages/CHANNEL_CONFIGURATION.md)
- [Testing Guide](../../libraries/Bot.Builder.Community.Adapters.Infobip.Messages/TESTING.md)
- [Infobip Messages API Documentation](https://www.infobip.com/docs/api/platform/messages-api)
- [Bot Framework Documentation](https://docs.microsoft.com/en-us/azure/bot-service/)

## 🎯 Key Benefits

1. **Real Integration** - Uses actual library, not stubs
2. **Complete Implementation** - All features working with real API calls
3. **Comprehensive Logging** - See exactly what's sent to Infobip
4. **Production Ready** - Proper error handling and configuration
5. **Multi-Channel** - Test all supported channels with real messages
6. **Easy Testing** - Bot Framework Emulator + ngrok for full testing

The sample provides a complete working example of real Infobip Messages API integration with the actual library, proper payload formatting, and comprehensive debugging capabilities. Use it as a reference for integrating the library into your own bots!
