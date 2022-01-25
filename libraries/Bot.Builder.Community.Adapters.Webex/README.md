# Microsoft Bot Framework WebexAdapter for .NET

This package contains an adapter that communicates directly with the Webex Teams API, and translates messages to and from a standard format used by your bot.

## How to Install

````
PM> Install-Package Bot.Builder.Community.Adapters.Webex
````
## How to Use

### Set the Webex Credentials
When your bot sends a request to Webex API, it must include information that Webex can use to verify its identity.

To authenticate the requests, you'll need to configure the Adapter with the Public Address, the Access Token, a Secret and an optional Webhook Name.

You could create in the project an `appsettings.json` file to set the Webex credentials as follows:

```json
{
  "WebexPublicAddress": "",
  "WebexAccessToken": "",
  "WebexSecret": "",
  "WebexWebhookName": ""
}
```

### Use WebexAdapter in your App

WebexAdapter provides a translation layer for BotBuilder so that bot developers can connect to Webex Teams and have access to Webex's API.

To add the WebexAdapter to a bot, for example, an `EchoBot`, in the `Startup` class you should add:

```C#
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

    // Create the Bot Framework Webex Adapter.
    services.AddSingleton<IBotFrameworkHttpAdapter, WebexAdapter>();

    // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
    services.AddTransient<IBot, EchoBot>();
}
```