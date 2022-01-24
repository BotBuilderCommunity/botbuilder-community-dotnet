# Microsoft Bot Framework SlackAdapter for .NET

This package contains an adapter that communicates directly with the Slack API, and translates messages to and from a standard format used by your bot.

## How to Install

````
PM> Install-Package Microsoft.Bot.Builder.Adapters.Slack
````
## How to Use

### Set the Slack Credentials

To authenticate the requests, you'll need to configure the Adapter with the Verification Token, the Bot Token and a Client Signing Secret.

You could create in the project an `appsettings.json` file to set the Slack credentials as follows:

```json
{
  "SlackVerificationToken": "",
  "SlackBotToken": "",
  "SlackClientSigningSecret": ""
}
```

### Use SlackAdapter in your App

SlackAdapter provides a translation layer for BotBuilder so that bot developers can connect to Slack and have access to the Slack API.

To add the Slack Adapter to a bot, for example, an `EchoBot`, in the `Startup` class you should add:

```C#
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

    // Create the Bot Framework Slack Adapter.
    services.AddSingleton<IBotFrameworkHttpAdapter, SlackAdapter>();

    // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
    services.AddTransient<IBot, EchoBot>();
}
```