## Google Adapter for Bot Builder v4 .NET SDK

### Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [![NuGet version](https://img.shields.io/badge/NuGet-1.0.100-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Adapters.Google/) |

### Description

This is part of the [Bot Builder Community Extensions](https://github.com/botbuildercommunity) project which contains various pieces of middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

The Google Adapter allows you to add an additional endpoint to your bot for custom Google Actions. The Google endpoint can be used
in conjunction with other channels meaning, for example, you can have a bot exposed on out of the box channels such as Facebook and 
Teams, but also via an Google Skill (as well as side by side with the Alexa Adapter also available from the Bot Builder Community Project).

Incoming Google Action requests are transformed, by the adapter, into Bot Builder Activties and then when your bot responds, the adapter transforms the outgoing Activity into an Google response.

A basic sample bot is available [here](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/tree/master/samples/Google%20Adapter%20Sample).

At the moment the adapter supports building voice / text based Google Actions, with the addition of some features, such as card support. 
**We will be adding many new features (similar to those found within the Alexa adapter) soon!**

* Support for voice based Google bots
* Support for Google Cards
* Integration libraries (similar to those for the Bot Framework Adapter), allowing for the bot to also have its own middleware pipeline for Google Actions
* TurnContext extensions allowing the developer to;
    * Attach an Google Card to the response
    * Retrieve the raw Google request
	* Set choice list to use native choice prompt for Google Actions
* Automatic conversion of Suggested Actions into Google Suggestion Chips

### Installation

Available via NuGet package [Bot.Builder.Community.Adapters.Google](https://www.nuget.org/packages/Bot.Builder.Community.Adapters.Google/)

Install into your project using the following command in the package manager;
```
    PM> Install-Package Bot.Builder.Community.Adapters.Google
```

### Sample

Basic sample bot available [here](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/tree/master/samples/Google%20Adapter%20Sample).

### Usage

* [Google Action Configuration](#configuring-your-google-action)
* [Adding the adapter and skills endpoint to your bot](#adding-the-adapter-and-skills-endpoint-to-your-bot)
    * [WebApi](#webapi)
	* [.NET Core](#.net-core)
* [Default Google Request to Activity mapping](#Default-Google-Request-to-Activity-mapping)
* [Default Activity to Google Response mapping](#Default-Activity-to-Google-Response-mapping)
* [Google TurnContext Extension Methods](#TurnContext-Extension-Methods)
	* [Adding an Google Card to a response](#Adding-an-Google-Card-to-a-response)

### Configuring your Google action

The Google Adapter is designed to integrate with a Google Action that has been configured with particular settings
(to allow for all of the text entered by the user to be sent through to your bot).

Please use the following configuration for your Google Action;

1. Create a single intent for your action - choosing the 'Custom' intent type.  This will create your 'actions.intent.MAIN' intent in DialogFlow. 
The should have a fulfilment type of 'Conversational' and a fulfilment tool 'DialogFlow'.  Your main intent within your action should look like the below;

![Google Action Main Intent](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/blob/feature/google-adapter/samples/Google%20Adapter%20Sample/mainintent.png)

2. When you create your main intent you will enter DialogFlow to configure it.  Set the following;
	* You will begin with two intents within DialogFlow - 'Welcome' and 'Default Fallback Intent'.
	* Delete the Welcome intent
	* Create a new intent called 'Default' and specify text phrases by entering some of the queries that are likely to be made to your bot

![DialogFlow Intent](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/blob/feature/google-adapter/samples/Google%20Adapter%20Sample/intent-dialogflow.png)

3. To configure your fulfilment (within DialogFlow) enable the 'webhook' setting and specify your bots Google Action endpoint. e.g. https://yourbot.azurewebsites.net/api/actionrequests 

![Google Action Main Intent](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/blob/feature/google-adapter/samples/Google%20Adapter%20Sample/fulfilment.png)

4. You should now be able to test your action with the simulator or on a device.


### Adding the adapter and skills endpoint to your bot

Currently there are integration libraries available for WebApi and .NET Core available for the adapter.
When using the integration libraries a new endpoint for your Google skill is created at '/api/actionrequests'. 
e.g. http://www.yourbot.com/api/actionrequests.  This is the endpoint that you should configure within the Google
Actions Developer Console as the fulfilment webhook endpoint for your action.

#### WebApi

When implementing your bot using WebApi, the integration layer for Google works the same as the default for Bot Framework.  The only difference being in your BotConfig file under the App_Start folder you call MapGoogleBotFramework instead;

```cs
    public class BotConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapGoogleBotFramework(botConfig => { 
				botConfig.GoogleBotOptions.GoogleOptions.ShouldEndSessionByDefault = true;
                botConfig.GoogleBotOptions.GoogleOptions.ActionInvocationName = "Google Action Invocation Name";
			});
        }
    }
``` 

#### .NET Core

An example of using the Google adapter with a bot built on Asp.Net Core. Again, The implementation of the 
integration layer is based upon the same patterns used for the Bot Framework integration layer. 
In Startup.cs you can configure your bot to use the Google adapter using the following;

```cs
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGoogleBot<EchoBot>(options =>
            {
                options.GoogleOptions.ActionInvocationName = "Google Action Invocation Name";
                options.GoogleOptions.ShouldEndSessionByDefault = false;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseGoogle();
        }
```


### Default Google Request to Activity mapping

When an incoming request is receieved, the activity sent to your bot is comprised of the following values;

* **Channel ID** : "google"
* **Recipient Channel Account** : Id = "", Name = "action"
* **From Channel Account** : Id = User Id from the Google request, Name = "user"
* **Conversation Account** : Id = "{Google payload conversation Id}"
* **Type** : Message Activity
* **Id** : Random generated Guid
* **Timestamp** : Timestamp (UTC)
* **Locale** : User locale from the Google request

The entire body of the Google request is placed into the Activity as Channel Data, of type Payload.


### Default Activity to Google Response mapping

The Google adapter will send a response to the Google action request if the outgoing activity is of type MessageActivity or EndOfConversation activity.

If the actvity you send from your bot is of type EndOfConversation then a response is sent indicating that the session should be ended.

If the activity type you send from your bot is of type MessageActivity the following values are mapped to a Google response object;

* **SimpleResponse DisplayText** : Populated using the value of activity.Text if it is not null.
* **OutputSpeech SSML** : Populated using the value of activity.Speak if it is not null.
* **OutputSpeech TextToSpeech** : Populated using the value of activity.Text if it is not null.

* **ExpectUserResponse** : Defaults to true. However, setting the InputHint property on the activity to InputHint.IgnoringInput will set this value to true and end the session.


### TurnContext Extension Methods

#### Adding an Google Card to a response

You can attach a card to your response to be shown in the Google app or on a Fire tablet.

```cs

turnContext.GoogleSetCard(new GoogleBasicCard()
	{
	    Content = new GoogleBasicCardContent()
	    {
	        Title = "This is the card title",
	        Subtitle = "This is the card subtitle",
	        FormattedText = "This is some text to go into the card." +
	                    "**This text should be bold** and " +
	                    "*this text should be italic*.",
	        Display = ImageDisplayOptions.DEFAULT,
	        Image = new Image()
	        {
	            AccessibilityText = "This is the accessibility text",
	            Url = "https://dev.botframework.com/Client/Images/ChatBot-BotFramework.png"
	        },
	    },
	});

```

The following types of card are supported;

* Basic Card (populate the title / content properties)

#### Get entire Google Request Body

We have provided an extension method to allow you to get the original Google request body, which we store on the ChannelData property of the Activity sent to your bot, as a strongly typed object of type GoogleRequestBody.  To get the request just call the extension method as below;

```cs
    Payload payload = context.GetGoogleRequestPayload();
```

***Note: If you call this extension method when the incoming Activity is not from an Google skill then the extension method will simply return null.*** 
