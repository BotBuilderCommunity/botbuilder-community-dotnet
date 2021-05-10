# Infobip Viber Adapter for Bot Builder v4 .NET SDK - **_PREVIEW_**

## Build status

| Branch | Status                                                                                                                                                    | Recommended NuGet package version                                                                                                                                                    |
| ------ | --------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | Preview [available via MyGet (version 1.0.0-alpha3)](https://www.myget.org/feed/botbuilder-community-dotnet/package/nuget/Bot.Builder.Community.Adapters.Infobip.Viber/1.0.0-alpha3) |

# Description

This is part of the [Bot Builder Community Extensions](https://github.com/botbuildercommunity) project which contains various pieces of middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

The Infobip Viber adapter enables receiving and sending Viber messages. The Infobip Viber adapter allows you to add an additional endpoint to your bot for receiving Viber messages. The Infobip endpoint can be used in conjunction with other channels meaning, for example, you can have a bot exposed on out of the box channels such as Facebook and Teams, but also via an Infobip (as well as side by side with the Google / Twitter Adapters also available from the Bot Builder Community Project).

Incoming Viber message requests are transformed, by the adapter, into Bot Framework Activites and then when your bot sends outgoing activities, the adapter transforms the outgoing Activity into an Infobip OMNI failover messages.

The adapter currently supports the following scenarios:

- Send/receive text messages
- Send rich messages (text, image and button) - More details about rich messages over Viber can be found [here](https://www.infobip.com/docs/viber/send-viber-messages-over-api#send-rich-messages-over-viber)
- Verification of incoming Infobip requests
- Receive delivery reports
- Callback data - You can add some data in every message and that data will be returned to bot in delivery report for that message
- Full incoming request from Infobip is added to the incoming activity as ChannelData

## Installation

Available via NuGet package [Bot.Builder.Community.Adapters.Infobip.Viber](https://www.nuget.org/packages/Bot.Builder.Community.Adapters.Infobip.Viber/)

Install into your project using the following command in the package manager;

```
PM> Install-Package Bot.Builder.Community.Adapters.Infobip.Viber
```

## Usage

- [Prerequisites](#prerequisites)
- [Set the Infobip Viber options](#set-the-infobip-viber-options)
- [Wiring up the Infobip Viber adapter in your bot](#wiring-up-the-infobip-viber-adapter-in-your-bot)
- [Incoming Viber message requests to Bot Framework Activity mapping](#incoming-viber-message-requests-to-bot-framework-activity-mapping) - Learn how incoming request types are handled by the adapter and the activities received by your bot.
- [Outgoing Bot Framework Activity to Infobip Viber message mapping](#outgoing-bot-framework-activity-to-infobip-viber-message-mapping) - Learn how outgoing Bot Framework activities are handled by the adapter.
- [Callback data](#calback-data) - The following section will demonstrate how to use callback data
- [Useful links](#useful-links)

### Prerequisites

You need to contact [Infobip support](https://www.infobip.com/contact) which will you help with Viber approvall procedure. More details available [here](https://www.infobip.com/docs/viber/onboarding-procedure).

### Set the Infobip Viber options

On the end of process you will get following parameters:

- Api key - will be used for requests authentication and authorization
- Base URL - endpoint on which messages will be sent
- Viber sender - will be used as sender name from which will be sent Viber outgoing messages
- Viber scenario key - we are using OMNI failover API which can have multiple scenarios. This one will be used for sending Viber messages. For more details you can check [here](https://dev.infobip.com/#programmable-communications/omni-failover).

Also you will need to provide:

- App secret - will be used for incoming request authentication
- Bot URL - Infobip will forward all incoming Viber messages on this endpoint

To authenticate the requests, you'll need to configure the Adapter with the Base URL, API key, App secret, Viber sender, Viber scenario key.

You could create in the project an `appsettings.json` file to set the Infobip options as follows:

```json
{
  "InfobipApiBaseUrl": "",
  "InfobipApiKey": "",
  "InfobipAppSecret": "",
  "InfobipViberSender": "",
  "InfobipViberScenarioKey": ""
}
```

### Wiring up the Infobip Viber adapter in your bot

After you completed the configuration of your Infobip Viber adapter, you need to wire up the Infobip Viber adapter into your bot.

#### Install the Infobip Viber adapter NuGet package

Install into your project using the following command in the package manager;

```
PM> Install-Package Bot.Builder.Community.Adapters.Infobip.Viber
```

#### Create an Infobip Viber adapter class

Create a new class that inherits from the **_InfobipViberAdapter_** class. This class will act as our adapter for the Infobip Viber channel. It includes error handling capabilities (much like the **_BotFrameworkAdapterWithErrorHandler_** class already in the sample, used for handling requests from Azure Bot Service).

```csharp
  public class InfobipViberAdapterWithErrorHandler: InfobipViberAdapter
  {
      public InfobipViberAdapterWithErrorHandler(InfobipViberAdapterOptions infobipOptions, IInfobipViberClient infobipViberClient, ILogger<InfobipViberAdapterWithErrorHandler> logger)
          : base(infobipViberOptions, infobipViberClient, logger)
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
  using Bot.Builder.Community.Adapters.Infobip.Viber;
  using Microsoft.Extensions.Logging;
```

#### Create a new controller for handling Infobip requests

You now need to create a new controller which will handle requests for incoming Viber messages and delivery reports, on a new endpoint 'api/infobip/viber' instead of the default 'api/messages' used for requests from Azure Bot Service Channels. By adding an additional endpoint to your bot, you can accept requests from Bot Service channels (or additional adapters), as well as from Infobip, using the same bot.

```csharp
  [Route("api/infobip/viber")]
  [ApiController]
  public class InfobipController : ControllerBase
  {
      private readonly InfobipViberAdapter Adapter;
      private readonly IBot Bot;

      public InfobipController(InfobipViberAdapter adapter, IBot bot)
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
  using Bot.Builder.Community.Adapters.Infobip.Viber;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Bot.Builder;
  using System.Threading.Tasks;
```

#### Inject Infobip Viber Adapter In Your Bot Startup.cs

Add the following line into the **_ConfigureServices_** method within your Startup.cs file, which will register your Infobip adapter with two dependencies and make it available for your new controller class. The configuration settings, described in the previous step, will be automatically used by the adapter.

```csharp
  //Add dependencies for Infobip Viber Adapter
  services.AddSingleton<InfobipViberAdapterOptions>();
  services.AddSingleton<IInfobipViberClient, InfobipViberClient>();

  // Add Infobip Viber Adapter with error handler
  services.AddSingleton<InfobipViberAdapter, InfobipViberAdapterWithErrorHandler>();
```

Once added, your **_ConfigureServices_** method shold look like this.

```csharp
  public void ConfigureServices(IServiceCollection services)
  {
      services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

      // Create the Bot Framework Adapter with error handling enabled.
      services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

      //Add dependencies for Infobip Viber Adapter
      services.AddSingleton<InfobipViberAdapterOptions>();
      services.AddSingleton<IInfobipViberClient, InfobipViberClient>();

      // Add Infobip Viber Adapter with error handler
      services.AddSingleton<InfobipViberAdapter, InfobipViberAdapterWithErrorHandler>();

      // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
      services.AddTransient<IBot, EchoBot>();
  }
```

You will also need to add the following using statement, in addition to those already present in the Startup.cs file.

```cs
  using Bot.Builder.Community.Adapters.Infobip.Viber;
```

### Incoming Viber message requests to Bot Framework Activity mapping

All messages sent by end-user Infobip will forward to your endpoint. End-users can send to your bot several different message types. You can read about the different message types [in the Infobip developer documentation](https://dev.infobip.com/#programmable-communications/omni-failover/receive-incoming-messages).

Here are details about common details for all Activity types:

- All activities will have end user subscriber number as ConversationId
- All Infobip request messages will be available in ChannelData
- ActivityId will be Infobip messageId
- All activites will have ChannelId property value equal to **InfobipViberConstants.ChannelName**

Only incoming supported messages are text messages. Here are how the adapter handles Viber incoming message.

- **Viber Message Requests -> Message Activity**
  Text property will contain content of message. TextFormat will be always plain.

```cs
  activity.Text = response.Message.Text;
  activity.TextFormat = TextFormatTypes.Plain;
```

- **Delivery report -> Event Activity**
  For each message that you send out, we will send you a delivery report on Infobip endpoint as Event Activity. Activity name will be "DELIVERY". That payload will be in ChannelData property. More details about payload and statuses are available [here](https://dev.infobip.com/#programmable-communications/omni-failover/receive-omni-delivery-reports). You can use this to check is message delivered to end-user. You can use this to see is your message seen by user.

### Outgoing Bot Framework Activity to Infobip Viber message mapping

Each Activity can have multiple attachments and it is converted to Viber messages.

- **Message Activity -> Viber text Message**
  If you want send Viber text message add message content to Text property of Activity. Infobip adapter will just pass that message without modification.

```cs
  var message = "Some dummy text";
  activity.Text = message;
  //or
  activity = MessageFactory.Text(message);
```

- **Message Activity -> Viber rich Message**
  If you want send rich message you should add InfobipOmniFailoverMessage to your activity. For Viber rich messages any combination of text, image or buttons is allowed. The only constraint when sending the button that the buttonUrl and buttonText are mandatory! Image URL should be available for Infobip.

```cs
  var message = new InfobipOmniViberMessage
  {
      Text = "Hi John Doe!",
      ImageUrl = "some-valid-image-url",
      ButtonText = "Button text",
      ButtonUrl = "Button url"
  };
  activity.AddInfobipViberMessage(message);
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

- [Viber](https://www.infobip.com/docs/viber)
- [Technical Specifications](https://www.infobip.com/docs/viber/viber-technical-specifications)
- [Types of Business Messages](https://www.infobip.com/docs/viber/types-of-business-messages)
- [Onboarding Procedure](https://www.infobip.com/docs/viber/onboarding-procedure)
- [Send Viber over API](https://www.infobip.com/docs/viber/send-viber-messages-over-api)
