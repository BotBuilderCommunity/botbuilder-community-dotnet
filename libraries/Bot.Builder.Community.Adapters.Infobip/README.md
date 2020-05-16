# Infobip WhatsApp Adapter for Bot Builder v4 .NET SDK

## Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [Available via NuGet](https://www.nuget.org/packages/Bot.Builder.Community.Adapters.Infobip/) |

# Description

This is part of the [Bot Builder Community Extensions](https://github.com/botbuildercommunity) project which contains various pieces of middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

The Infobip WhatsApp adapter enables receiving and sending WhatsApp messages. The Infobip adapter allows you to add an additional endpoint to your bot for receiving WhatsApp messages. The Infobip endpoint can be used in conjunction with other channels meaning, for example, you can have a bot exposed on out of the box channels such as Facebook and Teams, but also via an Infobip (as well as side by side with the Google/Twitter adapters also available from the Bot Builder Community Project).

Incoming WhatsApp message requests are transformed, by the adapter, into Bot Framework Activites and then when your bot sends outgoing activities, the adapter transforms the outgoing Activity into an Infobip OMNI failover messages.

The adapter currently supports the following scenarios:

* Send/receive text messages
* Send/receive media messages (document, image, video, audio) - Supported formats for media message types available [here](https://developers.facebook.com/docs/whatsapp/api/media/#supported-files)
* Send/receive location messages
* Send template messages
* Verification of incoming Infobip requests
* Receive delivery reports
* Receive seen reports
* Callback data - You can add some data in every message and that data will be returned to bot in the delivery report for that message
* Full incoming request from Infobip is added to the incoming activity as ChannelData

## Installation

Available via NuGet package [Bot.Builder.Community.Adapters.Infobip](https://www.nuget.org/packages/Bot.Builder.Community.Adapters.Infobip/)

Install into your project using the following command in the package manager;

````
PM> Install-Package Bot.Builder.Community.Adapters.Infobip
````
## Sample

Sample bot, showing examples of Infobip specific functionality is available [here](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/samples/Infobip%20Adapter%20Sample).


## Usage

* [Prerequisites](#prerequisites)
* [Set Infobip credentials](#set-the-infobip-credentials)
* [Wiring up the Infobip adapter in your bot](#wiring-up-the-infobip-adapter-in-your-bot)
* [Incoming Whatsapp message requests to Bot Framework Activity mapping](#incoming-whatsapp-message-requests-to-bot-framework-activity-mapping) - Learn how incoming request types are handled by the adapter and the activities received by your bot.
* [Outgoing Bot Framework Activity to Infobip Whatsapp message mapping](#outgoing-bot-framework-activity-to-infobip-whatsapp-message-mapping) - Learn how outgoing Bot Framework activities are handled by the adapter.
* [Callback data](#calback-data) - The following sections will demonstrate how to send and receive Whatsapp messages using the Infobip adapter, by walking you through modifying the EchoBot sample.
* [Useful links](#useful-links)

### Prerequisites

Contact the [Infobip](https://www.infobip.com/contact) regarding the WhatsApp business verification and obtaining Infobip credentials. More details about that are available [here](https://www.infobip.com/docs/whatsapp/client-onboarding).

### Set the Infobip options

At the end of process you will get the following parameters:
* API key - will be used for requests authentication and authorization 
* Base URL - endpoint on which messages will be sent
* WhatsApp number - will be used as subscriber number from which will be sent outgoing messages
* Scenario key - we are using OMNI failover API which can have multiple scenarios. For more details refer to the [Infobip API Reference](https://dev.infobip.com/#programmable-communications/omni-failover).

Also, you will need to provide:
* App secret - will be used for incoming request authentication
* Bot URL - Infobip will forward all incoming WhatsApp messages on this endpoint

To authenticate the requests, you will need to configure the Adapter with the Base URL, API key, App secret, WhatsApp number and Scenario key.

You could create in the project an `appsettings.json` file to set the Infobip options as follows:

```json
{
  "InfobipApiBaseUrl": "",
  "InfobipApiKey": "",
  "InfobipAppSecret": "",
  "InfobipWhatsAppNumber": "",
  "InfobipScenarioKey": "",
}
```

### Wiring up the Infobip adapter in your bot

After you completed the configuration of your Infobip adapter, you need to wire up the Infobip adapter into your bot.

#### Install the Infobip adapter NuGet package

Install it into your project using the following command in the package manager;

````
PM> Install-Package Bot.Builder.Community.Adapters.Infobip
````

#### Create an Infobip adapter class

Create a new class that inherits from the ***InfobipAdapter*** class. This class will act as our adapter for the Infobip WhatsApp channel. It includes error handling capabilities (much like the ***BotFrameworkAdapterWithErrorHandler*** class already in the sample, used for handling requests from Azure Bot Service).

```csharp
  public class InfobipAdapterWithErrorHandler: InfobipAdapter
  {
      public InfobipAdapterWithErrorHandler(InfobipAdapterOptions infobipOptions, IInfobipClient infobipClient, ILogger<InfobipAdapterWithErrorHandler> logger)
          : base(infobipOptions, infobipClient, logger)
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
  using Bot.Builder.Community.Adapters.Infobip;
  using Microsoft.Extensions.Logging;
```

#### Create a new controller for handling Infobip requests

You now need to create a new controller which will handle requests for incoming WhatsApp messages, delivery and seen reports, on a new endpoint 'api/infobip' instead of the default 'api/messages' used for requests from Azure Bot Service Channels. By adding an additional endpoint to your bot, you can accept requests from Bot Service channels (or additional adapters), as well as from Infobip, using the same bot.

```csharp
  [Route("api/infobip")]
  [ApiController]
  public class InfobipController : ControllerBase
  {
      private readonly InfobipAdapter Adapter;
      private readonly IBot Bot;

      public InfobipController(InfobipAdapter adapter, IBot bot)
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
  using Bot.Builder.Community.Adapters.Infobip;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Bot.Builder;
  using System.Threading.Tasks;
```

#### Inject Infobip Adapter In Your Bot Startup.cs

Add the following line into the ***ConfigureServices*** method within your Startup.cs file, which will register your Infobip adapter with two dependencies and make it available for your new controller class. The configuration settings, described in the previous step, will be automatically used by the adapter.

```csharp
  //Add dependencies for Infobip Adapter
  services.AddSingleton<InfobipAdapterOptions>();
  services.AddSingleton<IInfobipClient, InfobipClient>();

  // Add Infobip Adapter with the error handler
  services.AddSingleton<InfobipAdapter, InfobipAdapterWithErrorHandler>();
```

Once added, your ***ConfigureServices*** method should look like this.

```csharp
  public void ConfigureServices(IServiceCollection services)
  {
      services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

      // Create the Bot Framework Adapter with error handling enabled.
      services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

      //Add dependencies for Infobip Adapter
      services.AddSingleton<InfobipAdapterOptions>();
      services.AddSingleton<IInfobipClient, InfobipClient>();

      // Add Infobip Adapter with error handler
      services.AddSingleton<InfobipAdapter, InfobipAdapterWithErrorHandler>();

      // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
      services.AddTransient<IBot, EchoBot>();
  }
```

You will also need to add the following using statement, in addition to those already present in the Startup.cs file.

```cs
  using Bot.Builder.Community.Adapters.Infobip;
```

### Incoming WhatsApp message requests to Bot Framework Activity mapping

All messages sent by the end user, Infobip will forward to your endpoint. End users can send to your bot several different message types. You can read about the different message types [in the Infobip developer documentation](https://dev.infobip.com/#programmable-communications/omni-failover/receive-incoming-messages).

Here are details about common details for all Activity types:

* All activities will have end user subscriber number as ConversationId
* All Infobip request messages will be available in ChannelData
* ActivityId will be Infobip messageId

Here is how the adapter handles different message request types.

* **Text Message Requests -> Message Activity**
Text property will contain the message content. TextFormat will be always plain.

```cs
  activity.Text = response.Message.Text;
  activity.TextFormat = TextFormatTypes.Plain;
```

* **Location Message Requests -> Message Activity**
Message content will be in Entities. It will be mapped to *GeoCoordinates*.

```cs
  activity.Entities.Add(new GeoCoordinates
  {
      Latitude = response.Message.Latitude,
      Longitude = response.Message.Longitude
  });
```

* **Media Message Requests -> Message Activity**
The message content will be stored as an Attachment. Content type will be valid [MIME type](https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types).

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

You will receive just the media URL so you can use the extension method *Download* on Attachment class which is provided by the Infobip Adapter if you want to download the content.

```cs
  var media = attachment.Download();
```

* **Delivery report -> Event Activity**
For each message that you send out, we will send you a delivery report on the Infobip endpoint as Event Activity. Activity name will be "DELIVERY". That payload will be in ChannelData property. More details about payload and statuses are available [here](https://dev.infobip.com/#programmable-communications/omni-failover/receive-omni-delivery-reports). You can use this to check whether the message was delivered to the end user.

* **Seen report -> Event Activity**
Seen report will also be forwarded to the Infobip endpoint as Event Activity. Activity name will be "SEEN". Payload is also available in ChannelData property. More details about payload are available [here](https://dev.infobip.com/#programmable-communications/omni-failover/receive-omni-seen-reports). You can use this to see whether the message was seen by end user.

### Outgoing Bot Framework Activity to Infobip WhatsApp message mapping
Each Activity can have multiple attachments or entities and it is converted to WhatsApp messages. In one WhatsApp message we can send just one media item or location so one Activity can produce multiple WhatsApp messages.

* **Message Activity -> WhatsApp text Message**
If you want send text WhatsApp Message add message content to Text property of Activity. If you want to format the message, you can do that manually according to this [documentation](https://www.infobip.com/docs/whatsapp/send-whatsapp-over-api#send-free-form-messages). Adapter will just pass that message without modification.

```cs
  var message = "Some text with *bold*, _italic_, ~strike through~ and ```code``` formatting";
  activity.Text = message;
  //or
  activity = MessageFactory.Text(message);
```

* **Message Activity -> WhatsApp location Message**
For location message you should create an entity and it can be one of these two types: *GeoCoordinates* or *Place*.

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

* **Message Activity -> WhatsApp media Message**
If you want send a media message, you need to insert the media URL which should be available for Infobip. Content type should be valid [MIME type](https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types) and type of MIME type will be used for detecting type of WhatsApp media message. If ContentType is not valid MIME type, message will be sent as a document. Content property of Attachment will be ignored because it is not supported by Infobip. If you want to add a caption, you should add it to Text property. If you have multiple attachments, only first attachment will have caption. Caption is available for all WhatsApp media message types except for audio. 

```cs
  var attachment = new Attachment
  {
      ContentType = "image/png",
      ContentUrl = "https://cdn-www.infobip.com/wp-content/uploads/2019/09/05100710/rebranding-blog-banner-v3.png"
  };
  activity = MessageFactory.Attachment(attachment);
```

* **Message Activity -> WhatsApp template Message**
Adapter supports WhatsApp template messages. More details about WhatsApp template messages are available [here](https://www.infobip.com/docs/whatsapp/message-templates-guidelines).

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
  activity.Attachments.Add(new InfobipAttachment(templateMessage));
```

### Callback data
For each message, you can add some custom data and that data will be returned to bot in delivery report for that message. 

Send callback data:

```cs
  var callbackData = new Dictionary<string, string>
  {
      {"data1", "true"},
      {"data2", "12"}
  };

  _activity.Entities = new List<Entity>
  {
      new InfobipCallbackData(callbackData)
  };
```
Get callback data from delivery report:

```cs
  var callbackData = activity.Entities
                    .First(x => x.Type == InfobipConstants.InfobipCallbackDataEntityType);
  var sentData = callbackData.GetAs<Dictionary<string, string>>();
```

### Useful links
* [WhatsApp Client Onboarding](https://www.infobip.com/docs/whatsapp/client-onboarding)
* [Technical Specifications](https://www.infobip.com/docs/whatsapp/technical-specifications)
* [Type of Accounts](https://www.infobip.com/docs/whatsapp/type-of-accounts)
* [Type of Messages](https://www.infobip.com/docs/whatsapp/types-of-messages)
* [WhatsApp Message Templates Guidlines](https://www.infobip.com/docs/whatsapp/message-templates-guidelines)
* [WhatsApp Opt-in](https://www.infobip.com/docs/whatsapp/opt-in)
* [Send WhatsApp over API](https://www.infobip.com/docs/whatsapp/send-whatsapp-over-api)
