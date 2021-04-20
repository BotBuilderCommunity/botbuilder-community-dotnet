# Infobip SMS Adapter for Bot Builder v4 .NET SDK - **_PREVIEW_**

## Build status

| Branch | Status                                                                                                                                                    | Recommended NuGet package version                                                                                                                                                  |
| ------ | --------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | Preview [available via MyGet (version 1.0.0-alpha3)](https://www.myget.org/feed/botbuilder-community-dotnet/package/nuget/Bot.Builder.Community.Adapters.Infobip.Sms/1.0.0-alpha3) |

# Description

This is part of the [Bot Builder Community Extensions](https://github.com/botbuildercommunity) project which contains various pieces of middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

The Infobip SMS adapter enables receiving and sending SMS messages. The Infobip SMS Adapter allows you to add an additional endpoint to your bot for receiving SMS messages. The Infobip endpoint can be used
in conjunction with other channels meaning, for example, you can have a bot exposed on out of the box channels such as Facebook and
Teams, but also via an Infobip (as well as side by side with the Google / Twitter Adapters also available from the Bot Builder Community Project).

Incoming SMS message requests are transformed, by the adapter, into Bot Framework Activites and then when your bot sends outgoing activities, the adapter transforms the outgoing Activity into an Infobip OMNI failover messages.

The adapter currently supports the following scenarios:

- Send/receive text messages
- Verification of incoming Infobip requests
- Receive delivery reports
- Callback data - You can add some data in every message and that data will be returned to bot in delivery report for that message
- Full incoming request from Infobip is added to the incoming activity as ChannelData

## Installation

Available via NuGet package [Bot.Builder.Community.Adapters.Infobip.Sms](https://www.nuget.org/packages/Bot.Builder.Community.Adapters.Infobip.Sms/)

Install into your project using the following command in the package manager;

```
PM> Install-Package Bot.Builder.Community.Adapters.Infobip.Sms
```

## Usage

- [Prerequisites](#prerequisites)
- [Set the Infobip SMS options](#set-the-infobip-sms-options)
- [Wiring up the Infobip SMS adapter in your bot](#wiring-up-the-infobip-sms-adapter-in-your-bot)
- [Incoming SMS message requests to Bot Framework Activity mapping](#incoming-sms-message-requests-to-bot-framework-activity-mapping) - Learn how incoming request types are handled by the adapter and the activities received by your bot.
- [Outgoing Bot Framework Activity to Infobip SMS message mapping](#outgoing-bot-framework-activity-to-infobip-sms-message-mapping) - Learn how outgoing Bot Framework activities are handled by the adapter.
- [Callback data](#calback-data) - The following section will demonstrate how to use callback data
- [Useful links](#useful-links)

### Prerequisites

To receive SMS number you can open your free trial account [here](https://www.infobip.com/signup) or contact [Infobip support](https://www.infobip.com/contact) to help you with the process.

### Set the Infobip SMS options

On the end of process you will get following parameters:

- Api key - will be used for requests authentication and authorization
- Base URL - endpoint on which messages will be sent
- Sms number - will be used as subscriber number from which will be sent SMS outgoing messages
- SMS scenario key - we are using OMNI failover API which can have multiple scenarios. This one will be used for sending SMS messages. For more details you can check [here](https://dev.infobip.com/#programmable-communications/omni-failover).

Also you will need to provide:

- App secret - will be used for incoming request authentication
- Bot URL - Infobip will forward all incoming SMS messages on this endpoint

To authenticate the requests, you'll need to configure the Adapter with the Base URL, API key, App secret, SMS number, SMS scenario key.

You could create in the project an `appsettings.json` file to set the Infobip options as follows:

```json
{
  "InfobipApiBaseUrl": "",
  "InfobipApiKey": "",
  "InfobipAppSecret": "",
  "InfobipSmsNumber": "",
  "InfobipSmsScenarioKey": ""
}
```

### Wiring up the Infobip SMS adapter in your bot

After you completed the configuration of your Infobip SMS adapter, you need to wire up the Infobip SMS adapter into your bot.

#### Install the Infobip SMS adapter NuGet package

Install into your project using the following command in the package manager;

```
PM> Install-Package Bot.Builder.Community.Adapters.Infobip.Sms
```

#### Create an Infobip SMS adapter class

Create a new class that inherits from the **_InfobipSmsAdapter_** class. This class will act as our adapter for the Infobip SMS channel. It includes error handling capabilities (much like the **_BotFrameworkAdapterWithErrorHandler_** class already in the sample, used for handling requests from Azure Bot Service).

```csharp
  public class InfobipSmsAdapterWithErrorHandler: InfobipSmsAdapter
  {
      public InfobipSmsAdapterWithErrorHandler(InfobipSmsAdapterOptions infobipSmsOptions, IInfobipSmsClient infobipSmsClient, ILogger<InfobipSmsAdapterWithErrorHandler> logger)
          : base(infobipSmsOptions, infobipSmsClient, logger)
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
  using Bot.Builder.Community.Adapters.Infobip.Sms;
  using Microsoft.Extensions.Logging;
```

#### Create a new controller for handling Infobip SMS requests

You now need to create a new controller which will handle requests for incoming SMS messages, and delivery reports, on a new endpoint 'api/infobip/sms' instead of the default 'api/messages' used for requests from Azure Bot Service Channels. By adding an additional endpoint to your bot, you can accept requests from Bot Service channels (or additional adapters), as well as from Infobip, using the same bot.

```csharp
  [Route("api/infobip/sms")]
  [ApiController]
  public class InfobipController : ControllerBase
  {
      private readonly InfobipSmsAdapter Adapter;
      private readonly IBot Bot;

      public InfobipController(InfobipSmsAdapter adapter, IBot bot)
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
  using Bot.Builder.Community.Adapters.Infobip.Sms;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Bot.Builder;
  using System.Threading.Tasks;
```

#### Inject Infobip SMS Adapter In Your Bot Startup.cs

Add the following line into the **_ConfigureServices_** method within your Startup.cs file, which will register your Infobip SMS adapter with two dependencies and make it available for your new controller class. The configuration settings, described in the previous step, will be automatically used by the adapter.

```csharp
  //Add dependencies for Infobip SMS Adapter
  services.AddSingleton<InfobipSmsAdapterOptions>();
  services.AddSingleton<IInfobipSmsClient, InfobipSmsClient>();

  // Add Infobip Sms Adapter with error handler
  services.AddSingleton<InfobipSmsAdapter, InfobipSmsAdapterWithErrorHandler>();
```

Once added, your **_ConfigureServices_** method shold look like this.

```csharp
  public void ConfigureServices(IServiceCollection services)
  {
      services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

      // Create the Bot Framework Adapter with error handling enabled.
      services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

      //Add dependencies for Infobip SMS Adapter
      services.AddSingleton<InfobipSmsAdapterOptions>();
      services.AddSingleton<IInfobipSmsClient, InfobipSmsClient>();

      // Add Infobip Sms Adapter with error handler
      services.AddSingleton<InfobipSmsAdapter, InfobipSmsAdapterWithErrorHandler>();

      // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
      services.AddTransient<IBot, EchoBot>();
  }
```

You will also need to add the following using statement, in addition to those already present in the Startup.cs file.

```cs
  using Bot.Builder.Community.Adapters.Infobip.Sms;
```

### Incoming SMS message requests to Bot Framework Activity mapping

All messages sent by end-user Infobip will forward to your endpoint. End-users can send to your bot several different message types. You can read about the different message types [in the Infobip developer documentation](https://dev.infobip.com/#programmable-communications/omni-failover/receive-incoming-messages).

Here are details about common details for all Activity types:

- All activities will have end user subscriber number as ConversationId
- All Infobip request messages will be available in ChannelData
- ActivityId will be Infobip messageId
- All activites will have ChannelId property value equal to **InfobipSmsConstants.ChannelName**

Here are how the adapter handles different message request types.

- **SMS Message Requests -> Message Activity**
  Text property will contain content of message. TextFormat will be always plain.

```cs
  activity.Text = response.Message.CleanText;
  activity.TextFormat = TextFormatTypes.Plain;
```

- **Delivery report -> Event Activity**
  For each message that you send out, we will send you a delivery report on Infobip endpoint as Event Activity. Activity name will be "DELIVERY". That payload will be in ChannelData property. More details about payload and statuses are available [here](https://dev.infobip.com/#programmable-communications/omni-failover/receive-omni-delivery-reports). You can use this to check is message delivered to end-user. You can use this to see is your message seen by user.

### Outgoing Bot Framework Activity to Infobip SMS message mapping

Each Activity will be converted to one SMS message.

Before sending message we need to select SMS as channel by setting ChannelId property of activity to SMS.

```cs
activity.ChannelId = InfobipChannel.Sms;
```

- **Message Activity -> SMS Message**
  If you want send SMS Message add message content to Text property of Activity. If you want set message options, you can do that manualy accrording to this [documentation](https://www.infobip.com/docs/whatsapp/send-whatsapp-over-api#send-free-form-messages).

```cs
  var message = "Hello world!";
  activity.Text = message;
  //or
  activity = MessageFactory.Text(message);

  //SMS message options(NOT MANDATORY)
  var smsOptions = new InfobipOmniSmsMessageOptions();
  smsOptions.ValidityPeriodTimeUnit = InfobipSmsOptions.ValidityPeriodTimeUnitTypes.Hours;
  smsOptions.ValidityPeriod = 12;
  smsOptions.Transliteration = InfobipSmsOptions.TransliterationTypes.All;

  activity.AddInfobipOmniSmsMessageOptions(smsOptions);
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

- [SMS Number Configurations](https://www.infobip.com/docs/sms/configurations)
- [Send and Receive SMS Over API](https://www.infobip.com/docs/sms/send-and-receive-sms-over-api)
- [SMS Language](https://www.infobip.com/docs/sms/sms-language)
