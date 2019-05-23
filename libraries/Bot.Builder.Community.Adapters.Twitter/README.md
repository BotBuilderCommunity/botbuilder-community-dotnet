# Twitter Adapter for Bot Framework SDK v4

An adapter that integrates Twitter Direct Messages with the Bot Builder. The adapter sets up the required webhooks and responds to CRC requests.
The webhooks code is based on the work by [Tweety](https://github.com/mmgrt/Tweety) with modifications to support the Premium tier of the Account Activity API.

## Installation

Use nuget to add the required dependencies

    dotnet add package Bot.Builder.Community.Twitter.Adapter

## Configuration

Use the provided extensions to setup the adapter, middleware and hosted service in .NET Core

Inside `ConfigureServices(IServiceCollection services)` in your Startup class add the adapter dependencies

```cs
services.AddTwitterAdapter(x => Configuration.Bind("TwitterOptions", x));
```

In your `void Configure(IApplicationBuilder app, IHostingEnvironment env)` method register the adapter middleware

```cs
app.UseTwitterAdapter();
```

In your `appsettings.json` add required configuration entries. See details below for explanation on each entry.

```json
  "TwitterOptions": {
    "WebhookUri": "https://XXXXX.ngrok.io/twitter/adapter",
    "AccessSecret": "",
    "AccessToken": "",
    "ConsumerKey": "",
    "ConsumerSecret": "",
    "Environment": "",
    "Tier": "PremiumFree",
    "BotUsername": "",
    "AllowedUsernames": []
  }
```

That's it.

### Configuration notes

#### WebhookUri

This is public webhook uri that Twitter will integrate with. The adapter sets up a middleware that responds to the configred host in this Uri. The path portion of this address can be anything you like. Use Ngrok for development to test local callbacks.
When deploying to the server, the configured hosted service will automatically update this webhook address if the twitter webhook and the configured webhook are different.
Please note that if using the Free tier, you can only setup one webhook, so after you deploy to the server, running it locally will override the server webhook until you restart the web server.

#### Access tokens and secrets

Once you setup the Twitter developer account and create your app, you will find these at https://developer.twitter.com/en/apps/ under *Keys and tokens* in your app page

#### Environment

The name of the environment the bot will be configured to. Set this in your developer account at https://developer.twitter.com/en/account/environments under *Account Activity API*

#### BotUsername

This is the *App owner* configured in the above URL. This must be set to match the bot screen name - it used to detect and ignore messages that the bot sends to users. *If the username doesn't match, the bot will send it's response back to itself and get stuck in recursive calls.*

#### AllowedUsernames

This is just a convenience configuration that allows you to restrict the twitter screen names that the bot will respond to. If you want the bot to respond to everyone, leave this as is.

### Supported features

Currently, the adapter supports text and quick replies. More features coming soon. Contributions are welcome!

### Future plans

I'd like to integrate [Tweetinvi](https://github.com/linvi/tweetinvi) in place of the current `Webhooks` package as it is more feature complete, however due to a current [bug](https://github.com/linvi/tweetinvi/issues/849) in the library's WebhooksPlugin with namespace conflicts, this cannot be done until v5.0.