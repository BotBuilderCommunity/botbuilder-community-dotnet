﻿# Unofficial MessageBird WhatsApp Adapter for Bot Builder v4 .NET SDK

This project is created by Ahmet Kocadoğan to help Bot Framework community for WhatsApp channel and not related with MessageBird officially.

# Description

This is part of the [Bot Builder Community](https://github.com/botbuildercommunity) project which contains Bot Framework Components and other projects / packages for use with Bot Framework Composer and the Bot Builder .NET SDK v4.

The MessageBird WhatsApp adapter enables receiving and sending WhatsApp messages. The MessageBird adapter allows you to add an additional endpoint to your bot for receiving WhatsApp messages. The MessageBird endpoint can be used in conjunction with other channels meaning, for example, you can have a bot exposed on out of the box channels such as Facebook and Teams, but also via an MessageBird (as well as side by side with the Google/Twitter adapters also available from the Bot Builder Community Project).

Incoming MessageBird message requests are transformed, by the adapter, into Bot Framework Activites and then when your bot sends outgoing activities, the adapter transforms the outgoing Activity into an MessageBird messages.

The adapter currently supports the following scenarios:

* Send/receive messages with WhatsApp Sandbox
* Send/receive text messages
* Send/receive media messages (document, image, video, audio) - Supported formats for media message types available [here](https://developers.facebook.com/docs/whatsapp/api/media/#supported-files)
* Send/receive stickers - This feature will be supported as soon as MessageBird nuget package add this support. You can check the status of my PR about WhatsApp Sticker messages [here](https://github.com/messagebird/csharp-rest-api/pull/111)
* Send/receive location messages
* Verification of incoming MessageBird requests (New request verification way of MessageBird API via Messagebird-Signature-Jwt header is supported)
* Receive delivery reports
* Full incoming request from MessageBird is added to the incoming activity as ChannelData

### Sample

Basic sample bot available [here](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/tree/develop/samples/MessageBird%20Adapter%20Sample).

## Usage

* [Prerequisites](#prerequisites)
* [Set MessageBird credentials](#set-the-messagebird-credentials)
* [Wiring up the MessageBird adapter in your bot](#wiring-up-the-messagebird-adapter-in-your-bot)
* [Incoming Whatsapp message requests to Bot Framework Activity mapping](#incoming-whatsapp-message-requests-to-bot-framework-activity-mapping) - Learn how incoming request types are handled by the adapter and the activities received by your bot.
* [Outgoing Bot Framework Activity to MessageBird Whatsapp message mapping](#outgoing-bot-framework-activity-to-messagebird-whatsapp-message-mapping) - Learn how outgoing Bot Framework activities are handled by the adapter.
* [Useful links](#useful-links)

### Prerequisites

Create a [MessageBird](https://messagebird.com/en/whatsapp/) account and activate WhatsApp Sandbox or Production.
Get your [SigningKey](https://dashboard.messagebird.com/en/developers/settings)
Get your [AccessKey](https://dashboard.messagebird.com/en/developers/access)

### Set the MessageBird options

At the end of process you will get the following parameters:
* SigningKey - will be used for requests authentication and authorization 
* AccessKey - will be used for MessageBird Conversations API
* MessageBirdWebhookEndpointUrl - will be used for request verification


To authenticate the requests, you will need to configure the Adapter with the Signing Key, Access Key and your endpoint for MessageBird webhooks.

You could create in the project an `appsettings.json` file to set the MessageBird options as follows:

```json
{
  "MessageBirdAccessKey": "access_key_that_you_obtained_from_messagebird",
  "MessageBirdSigningKey": "signing_key_that_you_obtained_from_messagebird",
  "MessageBirdWebhookEndpointUrl": "your_bot_endpoint_url_for_incoming_messagebird_requests"
}
```

### Wiring up the MessageBird adapter in your bot

After you completed the configuration of your MessageBird adapter, you need to wire up the MessageBird adapter into your bot.



#### Create an MessageBird adapter class

Create a new class that inherits from the ***MessageBirdAdapter*** class. This class will act as our adapter for the MessageBird WhatsApp channel. It includes error handling capabilities (much like the ***BotFrameworkAdapterWithErrorHandler*** class already in the sample, used for handling requests from Azure Bot Service).

```csharp
    public class MessageBirdAdapterWithErrorHandler : MessageBirdAdapter
    {
        public MessageBirdAdapterWithErrorHandler(ILogger<MessageBirdAdapter> logger, MessageBirdAdapterOptions adapterOptions)
            : base(adapterOptions, logger)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError($"Exception caught : {exception.Message}");

                // Send a catch-all apology to the user.
                await turnContext.SendActivityAsync("Sorry, it looks like something went wrong.");
            };
        }
    }
```

You will also need to add the following using statements.

```cs
  using Bot.Builder.Community.Adapters.MessageBird;
  using Microsoft.Extensions.Logging;
```

#### Create a new controller for handling MessageBird requests

You now need to create a new controller which will handle requests for incoming WhatsApp messages, delivery and seen reports, on a new endpoint 'api/messagebird' instead of the default 'api/messages' used for requests from Azure Bot Service Channels. By adding an additional endpoint to your bot, you can accept requests from Bot Service channels (or additional adapters), as well as from MessageBird, using the same bot.


```csharp
  [Route("api/messagebird")]
  [ApiController]
  public class MessageBirdController : ControllerBase
  {
      private readonly MessageBirdAdapter Adapter;
      private readonly IBot Bot;

      public MessageBirdController(MessageBirdAdapter adapter, IBot bot)
      {
          Adapter = adapter;
          Bot = bot;
      }

      [HttpPost]
      public async Task PostAsync()
      {
          // Delegate the processing of the HTTP POST to the adapter.
          // The adapter will invoke the bot.
          await Adapter.ProcessAsync(Request, Response, Bot);
      }
  }
```

You will also need to add the following using statements.



#### Inject MessageBird Adapter In Your Bot Startup.cs

Add the following line into the ***ConfigureServices*** method within your Startup.cs file, which will register your MessageBird adapter with two dependencies and make it available for your new controller class. The configuration settings, described in the previous step, will be automatically used by the adapter.

```csharp
  //Add dependencies for MessageBird Adapter
  services.AddSingleton<MessageBirdAdapterOptions>();

  // Add MessageBird Adapter with the error handler
  services.AddSingleton<MessageBirdAdapter, MessageBirdAdapterWithErrorHandler>();

```

### Sample code for sending text, image, audio, video, location, file and WhatsApp Sticker (WhatsApp Sticker will be added soon)

## Sending Text Sample Code
```csharp
    var reply = MessageFactory.Text("your text here");
    await turnContext.SendActivityAsync(reply);

```

## Sending Image Sample Code
```csharp
    var reply = MessageFactory.Text("Image");
    Attachment attachment = new Attachment();
    attachment.Name = "Bot Framework Arcitecture";
    attachment.ContentType = "image";
    attachment.ContentUrl = "https://docs.microsoft.com/en-us/bot-framework/media/how-it-works/architecture-resize.png";
    reply.Attachments = new List<Attachment>() { attachment };
    await turnContext.SendActivityAsync(reply);

```

## Sending Audio Sample Code
```csharp
    var reply = MessageFactory.Text("Audio");
    Attachment attachment = new Attachment();
    attachment.Name = "";
    attachment.ContentType = "audio";
    attachment.ContentUrl = "url_of_your_audio";
    reply.Attachments = new List<Attachment>() { attachment };
    await turnContext.SendActivityAsync(reply);

```

## Sending Video Sample Code
```csharp
    var reply = MessageFactory.Text("Video");
    Attachment attachment = new Attachment();
    attachment.Name = "";
    attachment.ContentType = "video";
    attachment.ContentUrl = "url_of_your_video";
    reply.Attachments = new List<Attachment>() { attachment };
    await turnContext.SendActivityAsync(reply);

```

## Sending Location Sample Code
```csharp
    var reply = MessageFactory.Text("Location");
    reply.Entities = new List<Entity>() { new GeoCoordinates() { Latitude = 41.0572, Longitude = 29.0433 } };
    await turnContext.SendActivityAsync(reply);

```

## Sending File Sample Code
```csharp
    var reply = MessageFactory.Text("File");
    Attachment attachment = new Attachment();
    attachment.ContentType = "file";
    attachment.ContentUrl = "https://qconlondon.com/london-2017/system/files/presentation-slides/microsoft_bot_framework_best_practices.pdf";
    attachment.Name = "Microsoft Bot Framework Best Practices";
    reply.Attachments = new List<Attachment>() { attachment };
    await turnContext.SendActivityAsync(reply);

```

## Sending WhatsApp Sticker Sample Code (This will be added soon)
```csharp
    var reply = MessageFactory.Text("WhatsApp Sticker");
    Attachment attachment = new Attachment();
    attachment.Name = "";
    attachment.ContentType = "whatsappsticker";
    attachment.ContentUrl = "url_of_your_sticker";
    reply.Attachments = new List<Attachment>() { attachment };
    await turnContext.SendActivityAsync(reply);

```


### Useful links
* [WhatsApp Sandbox](https://dashboard.messagebird.com/en/whatsapp/sandbox)
* [Signing Key](https://dashboard.messagebird.com/en/developers/settings)
* [Access Key](https://dashboard.messagebird.com/en/developers/access)
* [WhatsApp Quickstarts](https://developers.messagebird.com/quickstarts/whatsapp-overview/)
* [Conversations Documentation](https://developers.messagebird.com/quickstarts/conversations-overview/)
* [MessageBird Website](https://messagebird.com)
* [New Request Verification Method](https://developers.messagebird.com/api/#verifying-http-requests)
