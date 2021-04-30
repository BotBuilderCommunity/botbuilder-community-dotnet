# Infobip WhatsApp for Bot Builder v4 .NET SDK - **_PREVIEW_**

## Build status

| Branch | Status                                                                                                                                                    | Recommended NuGet package version                                                                                                                                                       |
| ------ | --------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | Preview [available via MyGet (version 1.0.0-alpha3)](https://www.myget.org/feed/botbuilder-community-dotnet/package/nuget/Bot.Builder.Community.Adapters.Infobip.WhatsApp/1.0.0-alpha3) |

# Description

This is part of the [Bot Builder Community Extensions](https://github.com/botbuildercommunity) project which contains various pieces of middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

The Infobip Whatsapp adapter enables receiving and sending Whatsapp messages. The Infobip WhatsApp Adapter allows you to add an additional endpoint to your bot for receiving WhatsApp messages. The Infobip endpoint can be used
in conjunction with other channels meaning, for example, you can have a bot exposed on out of the box channels such as Facebook and
Teams, but also via an Infobip (as well as side by side with the Google / Twitter Adapters also available from the Bot Builder Community Project).

Incoming WhatsApp message requests are transformed, by the adapter, into Bot Framework Activites and then when your bot sends outgoing activities, the adapter transforms the outgoing Activity into an Infobip OMNI failover messages.

The adapter currently supports the following scenarios:

- Send/receive text messages
- Send/receive media messages (document, image, video, audio) - Supported formats for media message types available [here](https://developers.facebook.com/docs/whatsapp/api/media/#supported-files)
- Send/receive location messages
- Send template messages
- Verification of incoming Infobip requests
- Receive delivery reports
- Receive seen reports
- Callback data - You can add some data in every message and that data will be returned to bot in delivery report for that message
- Full incoming request from Infobip is added to the incoming activity as ChannelData

## Installation

Available via NuGet package [Bot.Builder.Community.Adapters.Infobip.WhatsApp](https://www.nuget.org/packages/Bot.Builder.Community.Adapters.Infobip.WhatsApp/)

Install into your project using the following command in the package manager;

```
PM> Install-Package Bot.Builder.Community.Adapters.Infobip.WhatsApp
```

## Usage

- [Prerequisites](#prerequisites)
- [Set Infobip WhatsApp credentials](#set-the-infobip-whatsapp-credentials)
- [Wiring up the Infobip WhatsApp adapter in your bot](#wiring-up-the-infobip-whatsapp-adapter-in-your-bot)
- [Incoming Whatsapp message requests to Bot Framework Activity mapping](#incoming-whatsapp-message-requests-to-bot-framework-activity-mapping) - Learn how incoming request types are handled by the adapter and the activities received by your bot.
- [Outgoing Bot Framework Activity to Infobip Whatsapp message mapping](#outgoing-bot-framework-activity-to-infobip-whatsapp-message-mapping) - Learn how outgoing Bot Framework activities are handled by the adapter.
- [Callback data](#calback-data) - The following section will demonstrate how to use callback data
- [Useful links](#useful-links)

### Prerequisites

You need to contact [Infobip support](https://www.infobip.com/contact) regarding WhatsApp business verification and obtaining Infobip credentials. More details about that are available [here](https://www.infobip.com/docs/whatsapp/client-onboarding).

### Set the Infobip WhatsApp options

On the end of process you will get following parameters:

- Api key - will be used for requests authentication and authorization
- Base URL - endpoint on which messages will be sent
- WhatsApp number - will be used as subscriber number from which will be sent WhatsApp outgoing messages
- WhatsApp scenario key - we are using OMNI failover API which can have multiple scenarios. This one will be used for sending WhatsApp messages. For more details you can check [here](https://dev.infobip.com/#programmable-communications/omni-failover).

Also you will need to provide:

- App secret - will be used for incoming request authentication
- Bot URL - Infobip will forward all incoming WhatsApp messages on this endpoint

To authenticate the requests, you'll need to configure the Adapter with the Base URL, API key, App secret, WhatsApp number, WhatsApp scenario key.

You could create in the project an `appsettings.json` file to set the Infobip options as follows:

```json
{
  "InfobipApiBaseUrl": "",
  "InfobipApiKey": "",
  "InfobipAppSecret": "",
  "InfobipWhatsAppNumber": "",
  "InfobipWhatsAppScenarioKey": ""
}
```

### Wiring up the Infobip WhatsApp adapter in your bot

After you completed the configuration of your Infobip adapter, you need to wire up the Infobip adapter into your bot.

#### Install the Infobip WhatsApp adapter NuGet package

Install into your project using the following command in the package manager;

```
PM> Install-Package Bot.Builder.Community.Adapters.Infobip.WhatsApp
```

#### Create an Infobip WhatsApp adapter class

Create a new class that inherits from the **_InfobipWhatsAppAdapter_** class. This class will act as our adapter for the Infobip WhatsApp channel. It includes error handling capabilities (much like the **_BotFrameworkAdapterWithErrorHandler_** class already in the sample, used for handling requests from Azure Bot Service).

```csharp
  public class InfobipWhatsAppAdapterWithErrorHandler: InfobipWhatsAppAdapter
  {
      public InfobipWhatsAppAdapterWithErrorHandler(InfobipWhatsAppAdapterOptions infobipWhatsAppOptions, IInfobipWhatsAppClient infobipWhatsAppClient, ILogger<InfobipAdapterWithErrorHandler> logger)
          : base(infobipWhatsAppOptions, infobipWhatsAppClient, logger)
      {
          OnTurnError = async (turnContext, exception) =>
          {
              OnTurnError = async (turnContext, exception) =>
              {
                  // Log any leaked exception from the application.
                  logger.LogError($"Exception caught : {exception.Message}");

                  // Send a catch-all apology to the user.
                  await turnContext.SendActivityAsync("Sorry, it looks like something went wrong.");
              };
          };
      }
  }
```

You will also need to add the following using statements.

```cs
  using Bot.Builder.Community.Adapters.Infobip.WhatsApp;
  using Microsoft.Extensions.Logging;
```

#### Create a new controller for handling Infobip WhatsApp requests

You now need to create a new controller which will handle requests for incoming WhatsAppMessages, delivery and seen reports, on a new endpoint 'api/infobip/whatsapp' instead of the default 'api/messages' used for requests from Azure Bot Service Channels. By adding an additional endpoint to your bot, you can accept requests from Bot Service channels (or additional adapters), as well as from Infobip, using the same bot.

```csharp
  [Route("api/infobip/whatsapp")]
  [ApiController]
  public class InfobipController : ControllerBase
  {
      private readonly InfobipWhatsAppAdapter Adapter;
      private readonly IBot Bot;

      public InfobipController(InfobipWhatsAppAdapter adapter, IBot bot)
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

```cs
  using Bot.Builder.Community.Adapters.Infobip.WhatsApp;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Bot.Builder;
  using System.Threading.Tasks;
```

#### Inject Infobip WhatsApp Adapter In Your Bot Startup.cs

Add the following line into the **_ConfigureServices_** method within your Startup.cs file, which will register your Infobip adapter with two dependencies and make it available for your new controller class. The configuration settings, described in the previous step, will be automatically used by the adapter.

```csharp
  //Add dependencies for Infobip Adapter
  services.AddSingleton<InfobipWhatsAppAdapterOptions>();
  services.AddSingleton<IInfobipWhatsAppClient, InfobipWhatsAppClient>();

  // Add Infobip Adapter with error handler
  services.AddSingleton<InfobipWhatsAppAdapter, InfobipWhatsAppAdapterWithErrorHandler>();
```

Once added, your **_ConfigureServices_** method shold look like this.

```csharp
  public void ConfigureServices(IServiceCollection services)
  {
      services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

      // Create the Bot Framework Adapter with error handling enabled.
      services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

      //Add dependencies for Infobip WhatsApp Adapter
      services.AddSingleton<InfobipWhatsAppAdapterOptions>();
      services.AddSingleton<IInfobipWhatsAppClient, InfobipWhatsAppClient>();

      // Add Infobip WhatsApp Adapter with error handler
      services.AddSingleton<InfobipWhatsAppAdapter, InfobipWhatsAppAdapterWithErrorHandler>();

      // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
      services.AddTransient<IBot, EchoBot>();
  }
```

You will also need to add the following using statement, in addition to those already present in the Startup.cs file.

```cs
  using Bot.Builder.Community.Adapters.Infobip.WhatsApp;
```

### Incoming Whatsapp message requests to Bot Framework Activity mapping

All messages sent by end-user Infobip will forward to your endpoint. End-users can send to your bot several different message types. You can read about the different message types [in the Infobip developer documentation](https://dev.infobip.com/#programmable-communications/omni-failover/receive-incoming-messages).

Here are details about common details for all Activity types:

- All activities will have end user subscriber number as ConversationId
- All Infobip request messages will be available in ChannelData
- ActivityId will be Infobip messageId
- All activites will have ChannelId property value equal to **InfobipWhatsAppConstants.ChannelName**

Here are how the adapter handles different message request types.

- **Text Message Requests -> Message Activity**
  Text property will contain content of message. TextFormat will be always plain.

```cs
  activity.Text = response.Message.Text;
  activity.TextFormat = TextFormatTypes.Plain;
```

- **Location Message Requests -> Message Activity**
  Message content will be in Entities. It will be mapped to _GeoCoordinates_.

```cs
  activity.Entities.Add(new GeoCoordinates
  {
      Latitude = response.Message.Latitude,
      Longitude = response.Message.Longitude
  });
```

- **Media Message Requests -> Message Activity**
  Message content will be stored as Attachment. Content type will be valid [MIME type](https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types).

```cs
  activity.Attachments = new List<Attachment>
  {
      new Attachment
      {
          ContentType = contentType,
          ContentUrl = response.Message.Url.AbsoluteUri,
          Name = response.Message.Caption
      }
  };
```

You will recieve just URL of media so you can use extension method _Download_ on Attachment class which is provided by Infobip Adapter if you want to download content.

```cs
  var media = attachment.Download();
```

- **Delivery report -> Event Activity**
  For each message that you send out, we will send you a delivery report on Infobip endpoint as Event Activity. Activity name will be "DELIVERY". That payload will be in ChannelData property. More details about payload and statuses are available [here](https://dev.infobip.com/#programmable-communications/omni-failover/receive-omni-delivery-reports). You can use this to check is message delivered to end-user. You can use this to see is your message seen by user.

- **Seen report -> Event Activity**
  Seen report also will be forwarded to Infobip endpoint as Event Activity. Activity name will be "SEEN". Payload is also available in ChannelData property. More details about payload are available [here](https://dev.infobip.com/#programmable-communications/omni-failover/receive-omni-seen-reports).

### Outgoing Bot Framework Activity to Infobip Whatsapp message mapping

Each Activity can have multiple attachments or entities and it is converted to WhatsApp messages. In one WhatsApp message we can send just one media item or location so one Activity can produce multiple WhatsApp messages.

- **Message Activity -> WhatsApp text Message**
  If you want send text WhatsApp Message add message content to Text property of Activity. If you want format message, you can do that manualy accrording to this [documentation](https://www.infobip.com/docs/whatsapp/send-whatsapp-over-api#send-free-form-messages). Adapter will just pass that message without modification.

````cs
  var message = "Some text with *bold*, _italic_, ~strike through~ and ```code``` formatting";
  activity.Text = message;
  //or
  activity = MessageFactory.Text(message);
````

- **Message Activity -> WhatsApp location Message**
  For location message you should create entity and it can be one of this two types: _GeoCoordinates_ or _Place_.

```cs
  activity.Entities.Add(new GeoCoordinates
  {
      Latitude = 12.3456789,
      Longitude = 23.456789,
      Name = "caption"
  });
  //or
  activity.Entities.Add(new Place
  {
      Address = "Address",
      Geo = new GeoCoordinates {Latitude = 12.3456789, Longitude = 23.456789, Name = "caption"}
  });

```

- **Message Activity -> WhatsApp media Message**
  If you want send media message you need to insert media URL which should be available for Infobip. Content type should be valid [MIME type](https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types) and type of MIME type will be used for detecting type of WhatsApp media message. If ContentType is not valid MIME type message will be sent as document. Content property of Attachment will be ignored because it is not supported by Infobip. If you want to add caption you should add it to Text property. If you have multiple attachments only first attachment will have caption. Caption is available for all WhatsApp media message types except for audio.

```cs
  var attachment = new Attachment
  {
      ContentType = "image/png",
      ContentUrl = "https://cdn-www.infobip.com/wp-content/uploads/2019/09/05100710/rebranding-blog-banner-v3.png"
  };
  activity = MessageFactory.Attachment(attachment);
```

- **Message Activity -> WhatsApp template Message**
  Adapter supports WhatsApp template messages. More details about WhatsApp template messages are availeble [here](https://www.infobip.com/docs/whatsapp/message-templates-guidelines).

```cs
  var templateMessage = new InfobipWhatsAppTemplateMessage
  {
      TemplateNamespace = "template_namespace",
      TemplateData = new[] { "one", "two" },
      TemplateName = "template_name",
      Language = "en",
      MediaTemplateData = new InfobipWhatsAppMediaTemplateData
      {
          MediaTemplateHeader = new InfobipWhatsAppMediaTemplateHeader
          {
              DocumentFilename = "Test file name"
          },
          MediaTemplateBody = new InfobipWhatsAppMediaTemplateBody
          {
              Placeholders = new[] { "three", "four" }
          }
      }
  };
  activity.AddInfobipWhatsAppTemplateMessage(templateMessage);
```

### Callback data

For each message you can add some custom data and that data will be returned to bot in delivery report for that message.

Send callback data:

```cs
  var callbackData = new Dictionary<string, string>
  {
      {"data1", "true"},
      {"data2", "12"}
  };

  activity.AddInfobipCallbackData(callbackData);
```

Get callback data from delivery report:

```cs
  var callbackData = activity.Entities
                    .First(x => x.Type == InfobipEntityType.CallbackData);
  var sentData = callbackData.GetAs<Dictionary<string, string>>();
```

### Useful links

- [WhatsApp Client Onboarding](https://www.infobip.com/docs/whatsapp/client-onboarding)
- [Technical Specifications](https://www.infobip.com/docs/whatsapp/technical-specifications)
- [Type of Accounts](https://www.infobip.com/docs/whatsapp/type-of-accounts)
- [Type of Messages](https://www.infobip.com/docs/whatsapp/types-of-messages)
- [WhatsApp Message Templates Guidlines](https://www.infobip.com/docs/whatsapp/message-templates-guidelines)
- [WhatsApp Opt-in](https://www.infobip.com/docs/whatsapp/opt-in)
- [Send WhatsApp over API](https://www.infobip.com/docs/whatsapp/send-whatsapp-over-api)
