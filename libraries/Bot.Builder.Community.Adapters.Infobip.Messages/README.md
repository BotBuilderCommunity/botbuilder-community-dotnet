# Infobip Messages Adapter for Bot Builder v4 .NET SDK

## Requirements
- .NET Standard 2.0 or later

## Build status

| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ---------------------------------- |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | Preview [available via MyGet (version 1.0.0-alpha3)](https://www.myget.org/feed/botbuilder-community-dotnet/package/nuget/Bot.Builder.Community.Adapters.Infobip.Messages/1.0.0-alpha3) |

# Description

This is part of the [Bot Builder Community](https://github.com/botbuildercommunity) project which contains Bot Framework Components and other projects / packages for use with Bot Framework Composer and the Bot Builder .NET SDK v4.

The Infobip Messages adapter enables receiving and sending messages over multiple channels (WhatsApp, SMS, Viber, etc.).

## Installation

Available via NuGet package [Bot.Builder.Community.Adapters.Infobip.Messages](https://www.nuget.org/packages/Bot.Builder.Community.Adapters.Infobip.Messages/)

Install into your project using the following command in the package manager:

```
PM> Install-Package Bot.Builder.Community.Adapters.Infobip.Messages
```

## Usage

- [Prerequisites](#prerequisites)
- [Set the Infobip Messages options](#set-the-infobip-messages-options)
- [Wiring up the Infobip Messages adapter in your bot](#wiring-up-the-infobip-messages-adapter-in-your-bot)

## Prerequisites
- .NET Standard 2.0 or later
- Infobip account and credentials

## Set the Infobip Messages options

Configure default options in `appsettings.json`:

```json
{
  "InfobipApiKey": "your-api-key",
  "InfobipMessagesApiBaseUrl": "https://api.infobip.com",
  "InfobipAppSecret": "your-app-secret",
  "DefaultEntityId": "your-default-entity-id",
  "DefaultApplicationId": "your-default-app-id",
  "DefaultValidityPeriod": 24,
  "DefaultValidityPeriodTimeUnit": "HOURS",
  "EnableUrlShortening": true,
  "EnableUrlTracking": true,
  "EnableInteractiveMessaging": true,
  "EnableWhatsAppTemplates": true,
  "EnableCarouselSupport": true,
  "EnableLocationSharing": true,
  "EnableWhatsAppFlows": true,
  "AutoConvertToInteractiveList": true,
  "InteractiveListThreshold": 3,
  "MaxCarouselCards": 10,
  "AdaptationMode": "FLEXIBLE"
}
```

Or configure programmatically:

```csharp
services.AddSingleton(sp =>
{
    var options = new InfobipMessagesAdapterOptions("api-key", "https://api.infobip.com")
    {
        DefaultEntityId = "entity-123",
        DefaultApplicationId = "app-456",
        DefaultValidityPeriod = 24,
        DefaultValidityPeriodTimeUnit = InfobipValidityPeriodTimeUnit.Hours,
        EnableUrlShortening = true,
        EnableUrlTracking = true,
        EnableInteractiveMessaging = true,
        EnableWhatsAppTemplates = true,
        EnableCarouselSupport = true,
        EnableLocationSharing = true,
        EnableWhatsAppFlows = true,
        AdaptationMode = InfobipAdaptationMode.Flexible,
        AutoConvertToInteractiveList = true,
        InteractiveListThreshold = 3,
        MaxCarouselCards = 10,
        DefaultUrlOptions = new InfobipUrlOptions
        {
            CustomDomain = "short.yourdomain.com",
            RemoveProtocol = true
        }
    };
    return options;
});
```

## Wiring up the Infobip Messages adapter in your bot

### 1. Create an Adapter Class

```csharp
public class InfobipMessagesAdapterWithErrorHandler : InfobipMessagesAdapter
{
    public InfobipMessagesAdapterWithErrorHandler(
        InfobipMessagesAdapterOptions options,
        IInfobipMessagesClient client,
        ILogger<InfobipMessagesAdapterWithErrorHandler> logger)
        : base(options, client, logger)
    {
        OnTurnError = async (turnContext, exception) =>
        {
            // Log any leaked exception from the application
            logger.LogError($"Exception caught: {exception.Message}");

            // Send a catch-all apology to the user
            await turnContext.SendActivityAsync("Sorry, it looks like something went wrong.");
        };
    }
}
```

### 2. Create a Controller

```csharp
[Route("api/infobip/messages")]
[ApiController]
public class InfobipMessagesController : ControllerBase
{
    private readonly InfobipMessagesAdapter Adapter;
    private readonly IBot Bot;

    public InfobipMessagesController(InfobipMessagesAdapter adapter, IBot bot)
    {
        Adapter = adapter;
        Bot = bot;
    }

    [HttpPost]
    public async Task PostAsync()
    {
        // Delegate the processing of the HTTP POST to the adapter
        await Adapter.ProcessAsync(Request, Response, Bot);
    }
}
```

### 3. Register Dependencies in Startup.cs

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

    // Create the Bot Framework Adapter with error handling enabled
    services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

    // Add dependencies for Infobip Messages Adapter
    services.AddSingleton<InfobipMessagesAdapterOptions>();
    services.AddSingleton<IInfobipMessagesClient, InfobipMessagesClient>();

    // Add Infobip Messages Adapter with error handler
    services.AddSingleton<InfobipMessagesAdapter, InfobipMessagesAdapterWithErrorHandler>();

    // Create the bot as a transient
    services.AddTransient<IBot, YourBot>();
}
```

### Message Handling

#### Incoming Messages

The adapter automatically converts incoming messages to Bot Framework activities:

- **Text messages** ? `Message` activity with `Text` property
- **Media messages** ? `Message` activity with `Attachments`
- **Location messages** ? `Message` activity with `GeoCoordinates` entity
- **Contact messages** ? `Message` activity with contact entities
- **Interactive messages** ? `Message` activity with `Value` containing user selection
- **Button/List replies** ? `Message` activity with structured `Value` object
- **Flow responses** ? `Message` activity with flow data in `Value`
- **Delivery reports** ? `Event` activity with name "DELIVERY"
- **Seen reports** ? `Event` activity with name "SEEN"

#### Outgoing Messages

Send messages using standard Bot Framework activities:

```csharp
// Text message
await turnContext.SendActivityAsync("Hello, world!");

// Message with quick reply buttons
var reply = MessageFactory.SuggestedActions(
    new[] { "Option 1", "Option 2", "Option 3" },
    "Choose an option:");
await turnContext.SendActivityAsync(reply);

// Media message
var attachment = new Attachment
{
    ContentType = "image/jpeg",
    ContentUrl = "https://docs.microsoft.com/en-us/bot-framework/media/how-it-works/architecture-resize.png",
    Name = "Bot Framework Architecture"
};
var imageMessage = MessageFactory.Attachment(attachment);
await turnContext.SendActivityAsync(imageMessage);

// Hero card (converted to appropriate format per channel)
var heroCard = new HeroCard
{
    Title = "Card Title",
    Subtitle = "Card Subtitle",
    Text = "Card description text",
    Images = new List<CardImage> { new CardImage("https://example.com/image.jpg") },
    Buttons = new List<CardAction>
    {
        new CardAction(ActionTypes.OpenUrl, "Open URL", value: "https://example.com"),
        new CardAction(ActionTypes.PostBack, "Click Me", value: "button_clicked")
    }
};
var heroCardMessage = MessageFactory.Attachment(heroCard.ToAttachment());
await turnContext.SendActivityAsync(heroCardMessage);
```

### Callback Data

Add custom data to track messages:

```csharp
var callbackData = new Dictionary<string, string>
{
    {"orderId", "12345"},
    {"userId", "user123"}
};

var activity = MessageFactory.Text("Your order has been processed!");
activity.AddInfobipCallbackData(callbackData);
await turnContext.SendActivityAsync(activity);
```

Retrieve callback data from delivery reports:

```csharp
if (turnContext.Activity.Type == ActivityTypes.Event &&
    turnContext.Activity.Name == "DELIVERY")
{
    var callbackData = turnContext.Activity.GetInfobipCallbackData();
    if (callbackData != null)
    {
        var orderId = callbackData.GetValueOrDefault("orderId");
        var userId = callbackData.GetValueOrDefault("userId");
        // Process delivery report...
    }
}
```

## Testing

For comprehensive testing instructions including Bot Framework Emulator integration, see the [Testing Guide](TESTING.md).

### Sample Bot

A comprehensive sample bot demonstrating **real Infobip Messages API integration** is available at:

```
samples/Infobip Messages Adapter Sample
```

**Key Features:**

- **Uses actual library** - `Bot.Builder.Community.Adapters.Infobip.Messages` (not stubs)
- **Real API calls** - Sends actual messages to Infobip Messages API
- **Complete payload logging** - See exact requests/responses in console
- **All channel configurations** with proper payload formatting
- **Interactive message examples** (buttons, lists, carousels)
- **Template message support** for WhatsApp Business
- **Callback data tracking** for delivery reports
- **Regional endpoint support** (e.g., `https://api.infobip.com`)

**Sample Architecture:**

```
Sample Bot Project ? Uses Library ? Calls Infobip API ? Real Message Delivery
```

See the [Sample Bot Guide](SAMPLE_BOT.md) for detailed usage instructions.

### Payload Format Validation

The adapter ensures all messages use the correct format with **ALL CAPS** for channels and content types:

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

### Quick Start with Sample Bot

1. **Navigate to the sample directory:**

   ```bash
   cd samples/Infobip\ Messages\ Adapter\ Sample
   ```

2. **Update `appsettings.json`** with your **real** Infobip credentials:

   ```json
   {
     "InfobipApiKey": "your-actual-api-key",
     "InfobipMessagesApiBaseUrl": "https://api.infobip.com",
     "DefaultSender": "sender-id",
     "EnablePayloadLogging": true
   }
   ```

3. **Run the sample bot:**

   ```bash
   dotnet run
   ```

4. **Connect Bot Framework Emulator** to `http://localhost:5000/api/messages`

5. **Test commands that make real API calls:**

   - `"help"` - Show all available test commands
   - `"test whatsapp"` - Send via WhatsApp with real API call and logging
   - `"test sms"` - Send via SMS with real API call and logging
   - `"test buttons"` - Interactive buttons with real API integration
   - `"debug payload"` - Show expected payload format
   - `"status"` - Show current configuration

6. **Check console output** for real API request/response logging:
   ```
   === INFOBIP MESSAGES API REQUEST ===
   Endpoint: https://api.infobip.com/messages-api/1/messages
   Payload: {"messages":[{"channel":"WHATSAPP",...}]}
   ===================================
   ```

### Example Test Bot Usage

```csharp
// The sample bot demonstrates real library usage:

// Test different channels with real API calls
await turnContext.SendActivityAsync("Type 'test whatsapp' to send via WhatsApp");
await turnContext.SendActivityAsync("Type 'test sms' to send via SMS");
await turnContext.SendActivityAsync("Type 'debug payload' to see format");

// Check console output for payload logging:
// === INFOBIP MESSAGES API REQUEST ===
// Endpoint: https://api.infobip.com/messages-api/1/messages
// Payload: {"messages":[{"channel":"WHATSAPP",...}]}
```

## Channel-Specific Features

### WhatsApp Business API

- Template messages with dynamic parameters
- Interactive buttons and lists
- Media messages with captions
- Location sharing and requests
- Contact sharing
- WhatsApp Flows integration
- Reply context (message threading)
- Sticker support
- Carousel messages
- Calendar event integration

### SMS/MMS

- Long message support
- Media attachments (MMS)
- Delivery reports
- URL shortening and tracking

### Viber Business Messages

- Rich media messages
- Keyboards and buttons
- Stickers
- Location sharing

### RCS (Rich Communication Services)

- Rich cards and carousels
- Suggested actions
- Media messages
- Location sharing

### Other Channels

The adapter automatically adapts Bot Framework features to the capabilities of each channel using the configured adaptation mode.

## Documentation Links

- [Infobip Messages API Documentation](https://www.infobip.com/docs/api/platform/messages-api)
- [WhatsApp Business API Documentation](https://www.infobip.com/docs/whatsapp)
- [Microsoft Bot Framework Documentation](https://docs.microsoft.com/en-us/azure/bot-service/)
- [Bot Builder Community Project](https://github.com/botbuildercommunity)

## What's New

This enhanced version includes:

- **WhatsApp Templates** - Full support for Business API templates
- **Interactive Lists** - Convert choice prompts to native lists
- **Carousel Messages** - Multi-card horizontal scrolling
- **WhatsApp Flows** - Interactive form integration
- **Location Requests** - Ask users to share location
- **Calendar Events** - Add-to-calendar functionality
- **Contact Sharing** - Share business contact information
- **Reply Context** - Message threading support
- **Sticker Support** - Send and receive stickers
- **Enhanced Adaptive Cards** - Better card conversion
- **Automatic Fallback** - Graceful degradation for unsupported features
- **Comprehensive Configuration** - Fine-grained control over all features

## ? Acceptance Criteria Met

This implementation fully meets all specified acceptance criteria:

- **All current and future channels** supported via unified Messages API
- **Free-form messaging** across all channels
- **WhatsApp template sending** with dynamic parameters
- **All message types**: Text, Image, Document, Video, Audio, Buttons, Reply, Open URL, Request location, Add calendar event, Contact, Stickers, Location, Carousel, WhatsApp flows
- **Advanced features**: Entity/Application ID, Validity period, Adaptation mode, Regional options, Callback data, URL shortening & tracking
- **Future-proof architecture** automatically supports new Messages API channels
- **Bot Framework integration** with standard activities and cards
- **Comprehensive testing** and documentation
