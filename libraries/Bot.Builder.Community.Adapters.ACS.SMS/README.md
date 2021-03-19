# Azure Communication Services SMS Adapter for Bot Builder v4 .NET SDK

## Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [Available via NuGet](https://www.nuget.org/packages/Bot.Builder.Community.Adapters.ACS.SMS/) |

## Description

This is part of the [Bot Builder Community Extensions](https://github.com/botbuildercommunity) project which contains various pieces of middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

The Azure Communication Services SMS Adapter allows you to add an additional endpoint to your bot for SMS via Azure Communication Services. The adapter can be used
in conjunction with other channels meaning, for example, you can have a bot exposed on out of the box channels such as Facebook and 
Teams, but also via Azure Communication Services SMS.

Incoming Azure Communication Services SMS events are transformed, by the adapter, into Bot Framework Activties and then when your bot sends outgoing activities, the adapter creates an Azure Communication Services SMS send request to send a reply.

The adapter currently supports the following scenarios;

* SMS received
* SMS send
* SMS delivery reports

## Installation

Available via NuGet package [Bot.Builder.Community.Adapters.ACS.SMS](https://www.nuget.org/packages/Bot.Builder.Community.Adapters.ACS.SMS/).

Install into your project use the following command in the package manager. 

```
    PM> Install-Package Bot.Builder.Community.Adapters.ACS.SMS
```

## Sample

Sample bot is available [here](../../samples/ACS%20SMS%20Adapter%20Sample).

## Usage

### Create an Azure Communication Services resource

1. [Create an Azure Communication Services resource](https://docs.microsoft.com/en-us/azure/communication-services/quickstarts/create-communication-resource?tabs=windows&pivots=platform-azp) .

2. [Get a phone number](https://docs.microsoft.com/en-us/azure/communication-services/quickstarts/telephony-sms/get-phone-number).

### Wiring up the ACS SMS adapter in your bot

Before you can complete the configuration of your ACS resource, you need to wire up the ACS adapter into your bot.

#### Install the ACS SMS adapter NuGet packages

Available via NuGet package [Bot.Builder.Community.Adapters.ACS.SMS](https://www.nuget.org/packages/Bot.Builder.Community.Adapters.ACS.SMS/).

Install into your project use the following command in the package manager. 

```
    PM> Install-Package Bot.Builder.Community.Adapters.ACS.SMS
```

#### Create an ACS SMS adapter class

Create a new class that inherits from the ***AcsSmsAdatper*** class. This class will act as our adapter for the Azure Communication Services. It includes error handling capabilities (much like the ***BotFrameworkAdapterWithErrorHandler*** class already in the sample, used for handling requests from Azure Bot Service).  

```csharp
    public class AcsSmsAdapterWithErrorHandler : AcsSmsAdapter
    {
        public AcsSmsAdapterWithErrorHandler(ILogger<AcsSmsAdapter> logger)
            : base(new AcsSmsAdapterOptions(), logger)
    {
        OnTurnError = async (turnContext, exception) =>
        {
            // Log any leaked exception from the application.
            logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");

            // Send a message to the user
            await turnContext.SendActivityAsync("The bot encountered an error or bug.");
            await turnContext.SendActivityAsync("To continue to run this bot, please fix the bot source code.");
        };
    }
}
```

You will also need to add the following using statements.

```cs
using Bot.Builder.Community.Adapters.ACS.SMS;
using Microsoft.Extensions.Logging;
```

#### Create a new controller for handling Azure Communication Services events

You now need to create a new controller which will handle requests from Azure Communication Services, on a new endpoing 'api/acssms' instead of the default 'api/messages' used for requests from Azure Bot Service Channels.  By adding an additional endpoint to your bot, you can accept requests from Bot Service channels (or additional adapters), as well as from Azure Communication Services, using the same bot.

```csharp
[Route("api/acssms")]
[ApiController]
public class AcsSmsController : ControllerBase
{
    private readonly AcsSmsAdapter _adapter;
    private readonly IBot _bot;

    public AlexaController(AcsSmsAdapter adapter, IBot bot)
    {
        _adapter = adapter;
        _bot = bot;
    }

    [HttpPost]
    public async Task PostAsync()
    {
        // Delegate the processing of the HTTP POST to the adapter.
        // The adapter will invoke the bot.
        await _adapter.ProcessAsync(Request, Response, _bot);
    }
}
```

You will also need to add the following using statements.

```cs
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.ACS.SMS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
```

#### Inject Azure Communication Services SMS Adapter In Your Bot Startup.cs

Add the following into the ***ConfigureServices*** method within your Startup.cs file, which will register your ACS SMS adapter and make it available for your new controller class.

Here you should also set the ACS adapter options, using the Phone Number you created within your Azure Communication Services resource and also the ACS Connection String, which you can get from the *Keys* section within your ACS resource in the Azure portal.

You can also specify here if your bot should receive delivery reports from ACS, which the adapter will transform into Event Activities for your to handle within your bot.

```csharp
services.AddSingleton<AcsSmsAdapter, AcsSmsAdapterWithErrorHandler>();

services.AddSingleton(sp =>
        {
            return new AcsSmsAdapterOptions()
            {
                AcsPhoneNumber = "", // e.g. +15552622396
                AcsConnectionString = "", // From the Azure portal and should start with 'endpoint=https://'
                EnableDeliveryReports = true
            };
        });
```

Once added, your ***ConfigureServices*** method shold look like this.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

    // Create the default Bot Framework Adapter (used for Azure Bot Service channels and emulator).
    services.AddSingleton<IBotFrameworkHttpAdapter, BotFrameworkAdapterWithErrorHandler>();

    // Create the Azure Communication Services SMS Adapter
    services.AddSingleton<AcsSmsAdapter, AcsSmsAdapterWithErrorHandler>();

    // Set our ACS SMS adapter options
    services.AddSingleton(sp =>
            {
                return new AcsSmsAdapterOptions()
                {
                    AcsPhoneNumber = "", // e.g. +15552622396
                    AcsConnectionString = "", // From the Azure portal and should start with 'endpoint=https://'
                    EnableDeliveryReports = true
                };
            });

    // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
    services.AddTransient<IBot, EchoBot>();
}
```

You will also need to add the following using statement, in addition to those already present in the startup.cs file.

```cs
using Bot.Builder.Community.Adapters.ACS.CMS;
```

### Complete configuration of your Azure Communication Services Resources for SMS events

Now that you have created an Azure Communication Services resource and wired up the adapter in your bot project, the final steps are to configure your ACS resource to handle SMS events, which will be posted to when an SMS or SMS delivery report is received, pointing it to the correct endpoint on your bot.

1. [Deploy your bot to Azure](https://aka.ms/bot-builder-deploy-az-cli) and make a note of the URL to your deployed bot.

2. Configure your ACS resource to [handle SMS events](https://docs.microsoft.com/en-us/azure/communication-services/quickstarts/telephony-sms/handle-sms-events).  When configuring your ACS event endpoint, specify 'Webhook' as the type and the URL for your endpoint is the URL for your bot, which will be the URL of your deployed application (or ngrok endpoint), plus '/api/acssms' (for example, `https://yourbotapp.azurewebsites.net/api/acssms`). Your bot must be running when you configure your endpoint so that the endpoint can be verified by Azure Communication Services.

> [!NOTE]
> If you are not ready to deploy your bot to Azure, or wish to debug your bot when using the ACS SMS adapter, you can use a tool such as [ngrok](https://www.ngrok.com) (which you will likely already have installed if you have used the Bot Framework emulator previously) to tunnel through to your bot running locally and provide you with a publicly accessible URL for this. 
> 
> If you wish create an ngrok tunnel and obtain a URL to your bot, use the following command in a terminal window (this assumes your local bot is running on port 3978, alter the port numbers in the command if your bot is not).
> 
> ```
> ngrok.exe http 3978 -host-header="localhost:3978"
> ```
