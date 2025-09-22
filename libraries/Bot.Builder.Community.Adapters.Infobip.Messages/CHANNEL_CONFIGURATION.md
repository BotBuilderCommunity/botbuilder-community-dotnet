# Enhanced Infobip Messages API Adapter - Channel Configuration & Debugging

## New Features Added

### 1. **Payload Logging & Debugging**

The adapter now automatically logs all request and response payloads to help with debugging:

- Logs to both ILogger and Console output
- Shows complete request/response JSON
- Includes endpoint URLs and HTTP status codes
- Color-coded console output with emojis for easy identification

### 2. **Enhanced Channel Detection**

Multiple ways to specify which Infobip channel to use:

#### Priority Order (highest to lowest):

1. **Channel Specification Entity** (most explicit)
2. **Channel Data** (multiple key variations)
3. **Bot Framework ChannelId** (automatic mapping)
4. **Default Channel** (from configuration)

#### Supported Channels:

- `WHATSAPP` (WhatsApp Business API)
- `SMS` (Text messaging)
- `MMS` (Multimedia messaging)
- `VIBER_BM` (Viber Business Messages)
- `VIBER_BOT` (Viber Public Accounts)
- `RCS` (Rich Communication Services)
- `APPLE_MB` (Apple Messages for Business)
- `INSTAGRAM_DM` (Instagram Direct Messages)
- `LINE_ON` (LINE Official Accounts)
- `MESSENGER` (Facebook Messenger)
- `GOOGLE_BM` (Google Business Messages)
- `TELEGRAM` (Telegram)
- `EMAIL` (Email messaging)
- `VOICE` (Voice calls)
- `PUSH` (Push notifications)

## Usage Examples

### Method 1: Using Extension Methods (Recommended)

```csharp
// Simple channel setting
var activity = MessageFactory.Text("Hello from WhatsApp!");
activity.SetInfobipChannel(InfobipChannels.WhatsApp);
await turnContext.SendActivityAsync(activity);

// Channel with fallback options
var activity2 = MessageFactory.Text("Hello with fallback!");
activity2.SetInfobipChannelConfiguration(
    primaryChannel: InfobipChannels.WhatsApp,
    fallbackChannels: new[] { InfobipChannels.SMS, InfobipChannels.Viber },
    sender: "MyBot"
);
await turnContext.SendActivityAsync(activity2);

// Using Messages API specific method
var activity3 = MessageFactory.Text("Using Messages API channel key");
activity3.SetInfobipMessagesChannel(InfobipChannels.RCS);
await turnContext.SendActivityAsync(activity3);
```

### Method 2: Using Channel Specification (Advanced)

```csharp
var activity = MessageFactory.Text("Advanced channel specification");
activity.AddInfobipChannelPreference(
    InfobipChannels.WhatsApp,
    InfobipChannels.SMS,
    InfobipChannels.Viber
);
await turnContext.SendActivityAsync(activity);
```

### Method 3: Using Channel Data

```csharp
var activity = MessageFactory.Text("Using channel data");
activity.ChannelData = new Dictionary<string, object>
{
    ["infobipChannel"] = InfobipChannels.SMS,
    ["sender"] = "MyCompany",
    ["customOptions"] = new { priority = "high" }
};
await turnContext.SendActivityAsync(activity);
```

### Method 4: Configuration-Based Default

```json
// appsettings.json
{
  "InfobipApiKey": "your-api-key",
  "InfobipMessagesApiBaseUrl": "https://api.infobip.com",
  "InfobipAppSecret": "your-app-secret",
  "DefaultChannel": "SMS",
  "DefaultSender": "YourCompany",
  "EnableAutomaticChannelDetection": true
}
```

```csharp
// No explicit channel - uses DefaultChannel from config
var activity = MessageFactory.Text("Using default channel from config");
await turnContext.SendActivityAsync(activity);
```

## Debug Output Examples

### Console Output (Request):

```
=== INFOBIP MESSAGES API REQUEST ===
Endpoint: https://api.infobip.com/messages-api/1/messages
Payload: {
  "messages": [
    {
      "channel": "WHATSAPP",
      "sender": "YourBot",
      "destinations": [
        {
          "to": "+1234567890"
        }
      ],
      "content": {
        "body": {
          "text": "Hello from WhatsApp!",
          "type": "TEXT"
        }
      },
      "messageId": "msg-12345"
    }
  ]
}
===================================
```

### Logger Output:

```
Sending message to Infobip Messages API
Endpoint: https://api.infobip.com/messages-api/1/messages
Request Payload: {"messages":[...]}
Channel detected from channel data: WHATSAPP
Sender: YourBot
Destination: +1234567890
Infobip Messages API response successful
Response Payload: {"messageId":"msg-12345","status":{"groupId":1,"name":"PENDING_ACCEPTED"}}
```

## Advanced Configuration

### Regional Endpoints

```csharp
// For different regional endpoints
services.Configure<InfobipMessagesAdapterOptions>(options =>
{
    options.InfobipMessagesApiBaseUrl = "https://api.infobip.com"; // Europe
    // options.InfobipMessagesApiBaseUrl = "https://abc123.api.infobip.com"; // US
    // options.InfobipMessagesApiBaseUrl = "https://api.infobip.com"; // Global
});
```

### Channel-Specific Features

```csharp
// WhatsApp template message
var activity = MessageFactory.Text("Template message");
activity.SetInfobipChannel(InfobipChannels.WhatsApp);
activity.AddInfobipWhatsAppTemplate("order_confirmation", new
{
    customer_name = "John Doe",
    order_number = "12345",
    total_amount = "$99.99"
});
await turnContext.SendActivityAsync(activity);

// SMS with callback data
var smsActivity = MessageFactory.Text("SMS with tracking");
smsActivity.SetInfobipChannel(InfobipChannels.SMS);
smsActivity.AddInfobipCallbackData(new Dictionary<string, string>
{
    ["orderId"] = "12345",
    ["campaignId"] = "summer2023"
});
await turnContext.SendActivityAsync(smsActivity);
```

## Architecture Changes

### Request Flow:

1. **Activity Created** ? Bot creates activity with content
2. **Channel Detection** ? Multiple methods checked in priority order
3. **Content Conversion** ? Activity converted to Infobip format
4. **Payload Logging** ? Request logged to console/logger
5. **API Call** ? HTTP POST to `/messages-api/1/messages`
6. **Response Logging** ? Response logged to console/logger

### Channel Field Guarantee:

- Channel field is **always** present in requests
- Defaults to WhatsApp if no channel specified
- Supports all current and future Infobip channels
- Automatic mapping from Bot Framework channel IDs

## Breaking Changes: None

All changes are backward compatible - existing code continues to work unchanged.

## Support & Troubleshooting

If you see payload logs but messages aren't being delivered:

1. Check API credentials are correct
2. Verify channel is configured in Infobip portal
3. Ensure sender number/ID is approved for the channel
4. Check Infobip account has sufficient credits
5. Verify webhook URLs if expecting delivery reports
