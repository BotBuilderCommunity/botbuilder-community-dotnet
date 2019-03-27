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
    * [.NET Core MVC](#.net-core-mvc)
	* [.NET Core](#.net-core)
* [Default Google Request to Activity mapping](#Default-Google-Request-to-Activity-mapping)
* [Default Activity to Google Response mapping](#Default-Activity-to-Google-Response-mapping)
* [Google TurnContext Extension Methods](#TurnContext-Extension-Methods)
	* [Adding Suggestion Chips to a response](#Adding-Suggestion-Chips-to-a-response)
	* [Adding an Google Card to a response](#Adding-an-Google-Card-to-a-response)
	* [Adding audio as part of a response](#Send-an-audio-repsonse-to-the-user)
	* [Getting a list of supported capabilities for a user's device (e.g. Screen / Audio)](#Get-a-list-of-capabilities-supported-by-the-user's-device)
	* [Get the original Google Request sent by an action](#Get-entire-Google-Request-Body)

### Configuring your Google action

The Google Adapter is designed to integrate with a Google Action that has been configured with particular settings
(to allow for all of the text entered by the user to be sent through to your bot).

Please use the following configuration for your Google Action;

1. From https://console.actions.google.com/, add a single intent for your action by clicking - choosing the 'Custom' intent type.  This will create your 'actions.intent.MAIN' intent in DialogFlow. This should have a fulfilment type of 'Conversational' and a fulfilment tool 'DialogFlow'.  Your main intent within your action should look like the below;

![Google Action Main Intent](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/blob/master/samples/Google%20Adapter%20Sample/mainintent.png)

2. When you click on your main intent you will enter into DialogFlow to configure it.  Set the following;
	* You will begin with two intents within DialogFlow - 'Welcome' and 'Default Fallback Intent'.
	* Delete the Welcome intent
	* Create a new intent called 'Default' and specify text phrases by entering some of the queries that are likely to be made to your bot

![DialogFlow Intent](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/blob/master/samples/Google%20Adapter%20Sample/intent-dialogflow.png)

3. To configure your fulfilment (within DialogFlow) enable the 'webhook' setting and specify your bots Google Action endpoint. e.g. https://yourbot.azurewebsites.net/api/actionrequests 

![Google Action Main Intent](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/blob/master/samples/Google%20Adapter%20Sample/fulfilment.png)

4. You should now be able to test your action with the simulator (https://console.actions.google.com/) or on a device.


### Adding the adapter and skills endpoint to your bot

Currently integration is available for .NET Core applications, with or without MVC.
When using the non-MVC approach, a new endpoint for your Google skill is created at '/api/actionrequests'. 
e.g. http://www.yourbot.com/api/actionrequests.  This is the endpoint that you should configure within the Amazon Alexa
Skills Developer portal as the endpoint for your skill.

When using MVC, you configure your chosen endpoint when creating your controller which will handle your skill requests. An example of this can be seen [in the .NET Core MVC secton below](#.net-core-mvc).

#### .NET Core MVC

You can use the Google adapter with your bot within a .NET Core MVC project by registering the GoogleHttpAdapter.
When registering the GoogleHttpAdapter, you can add middleware and also set settings, such as if a session should 
end by default or what happens when an error occurs during a bot's turn.

```cs
	services.AddSingleton<IGoogleHttpAdapter>((sp) =>
	{
	    var telemetryClient = sp.GetService<IBotTelemetryClient>();
	
	    var googleHttpAdapter = new GoogleHttpAdapter(validateRequests: true)
	    {
	        OnTurnError = async (context, exception) =>
	        {
	            telemetryClient.TrackException(exception);
	            await context.SendActivityAsync("Sorry, something went wrong");
	        },
	        ShouldEndSessionByDefault = true,
	    };
	
	    var appInsightsLogger = new TelemetryLoggerMiddleware(telemetryClient);
	
		// register middleware
	    googleHttpAdapter.Use(appInsightsLogger);
	    googleHttpAdapter.Use(new AutoSaveStateMiddleware(userState, conversationState));
	
	    return googleHttpAdapter;
	});
``` 

Once you have registered your the GoogleHttpAdapter in Startup.cs, you can then create a normal
MVC controller to provide an endpoint for your bot and process requests to them using the Google adapter.

```cs 

    [Route("api/actionrequests")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IGoogleHttpAdapter _adapter;
        private readonly IBot _bot;

        public BotController(IGoogleHttpAdapter adapter, IBot bot)
        {
            _adapter = adapter;
            _bot = bot;
        }

        [HttpPost]
        public async Task PostAsync()
        {
            await _adapter.ProcessAsync(Request, Response, _bot);
        }
    }

```

#### .NET Core (non-MVC)

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
* **Text** : Raw text input from the user's query to the Google Assistant
* **Value** : Google intent matched / triggered
* **Type** : Message Activity
* **Id** : Random generated Guid
* **Timestamp** : Timestamp (UTC)
* **Locale** : User locale from the Google request

The entire body of the Google request is placed into the Activity as Channel Data, of type **Payload**.


### Default Activity to Google Response mapping

The Google adapter will send a response to the Google action request if the outgoing activity is of type MessageActivity or EndOfConversation activity.

If the activity you send from your bot is of type EndOfConversation then a response is sent indicating that the session should be ended.

If the activity type you send from your bot is of type MessageActivity the following values are mapped to a Google response object;

* **SimpleResponse DisplayText** : Populated using the value of activity.Text if it is not null.
* **OutputSpeech SSML** : Populated using the value of activity.Speak if it is not null.
* **OutputSpeech TextToSpeech** : Populated using the value of activity.Text if it is not null.
* **ExpectUserResponse** : Defaults to true. However, setting the InputHint property on the activity to InputHint.IgnoringInput will set this value to true and end the session.


### TurnContext Extension Methods

#### Adding Suggestion Chips to a response

The Google adapter will automatically convert any Suggestion Actions on your outgoing activity to Google Suggestion Chips.
You can also explicitly add exception chips to the response as shown below.

```cs

turnContext.GoogleAddSuggestionChipsToResponse(new List<Suggestion>()
	{
	    new Suggestion() { Title = "Suggestion Chip 1" },
	    new Suggestion() { Title = "Suggestion Chip 2" },
	});

```

#### Adding an Google Card to a response

You can attach a card to your response to be shown in the Google app or a Google device with a display.

The following types of card are supported;

* Basic Card (populate the title / content properties)

```cs

turnContext.GoogleSetCard(
	"This is the card title",
	"This is the card subtitle",
	new Image() {
		AccessibilityText = "This is the accessibility text",
		Url = "https://dev.botframework.com/Client/Images/ChatBot-BotFramework.png",
	},
	ImageDisplayOptions.DEFAULT,
	"This is **some text** to *go into the card*.");

```

#### Send an audio repsonse to the user

Google Actions support playing back audio files as part of a response. You can add audio to 
you response using the below extension method.

```cs 

turnContext.GoogleSetAudioResponse(
	"http://www.hochmuth.com/mp3/Haydn_Cello_Concerto_D-1.mp3",
	"Audio Name",
	"This is a description of the audio",
	new Image()
	{
	    AccessibilityText = "This is the accessibility text",
	    Url = "https://dev.botframework.com/Client/Images/ChatBot-BotFramework.png",
	},
	new Image()
	{
	    AccessibilityText = "This is the accessibility text",
	    Url = "https://dev.botframework.com/Client/Images/ChatBot-BotFramework.png",
	});

```

#### Get a list of capabilities supported by the user's device

You can find out which capabilities are supported by the user device.  Using the below
extension method you will receive a list of surface capabilities. e.g. actions.capability.SCREEN_OUTPUT or actions.capability.AUDIO_OUTPUT.
This can be useful if you want to tailor your response depending on which device the user is using.

```cs

turnContext.GoogleGetSurfaceCapabilities();

```

#### Get entire Google Request Body

We have provided an extension method to allow you to get the original Google request body, which we store on the ChannelData property of the Activity sent to your bot, as a strongly typed object of type GoogleRequestBody.  To get the request just call the extension method as below;

```cs
    Payload payload = context.GetGoogleRequestPayload();
```

***Note: If you call this extension method when the incoming Activity is not from an Google skill then the extension method will simply return null.*** 
