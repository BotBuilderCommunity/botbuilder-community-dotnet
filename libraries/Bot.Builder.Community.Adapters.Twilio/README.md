# Microsoft Bot Framework TwilioAdapter for .NET

This package contains an adapter that communicates directly with the Twilio API, and translates messages to and from a standard format used by your bot.

## How to Install

````
PM> Install-Package Microsoft.Bot.Builder.Adapters.Twilio
````
## How to Use

### Set the Twilio Credentials

To authenticate the requests, you'll need to configure the Adapter with the Twilio Number, the Account Sid, an Auth Token and a Validation Url.

You could create in the project an `appsettings.json` file to set the Twilio credentials as follows:

```json
{
    "TwilioNumber": "",
    "TwilioAccountSid": "",
    "TwilioAuthToken": "",
    "TwilioValidationUrl": ""
}
```

### Use TwilioAdapter in your App

TwilioAdapter provides a translation layer for BotBuilder so that bot developers can connect to Twilio SMS and have access to the Twilio API.

To add the Twilio Adapter to a bot, for example, an `EchoBot`, in the `Startup` class you should add:

```C#
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

    // Create the Bot Framework Twilio Adapter.
    services.AddSingleton<IBotFrameworkHttpAdapter, TwilioAdapter>();

    // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
    services.AddTransient<IBot, EchoBot>();
}
```