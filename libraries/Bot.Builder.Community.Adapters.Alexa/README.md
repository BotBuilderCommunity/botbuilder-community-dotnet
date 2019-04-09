## Alexa Adapter for Bot Builder v4 .NET SDK

### Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [![NuGet version](https://img.shields.io/badge/NuGet-1.0.100-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Adapters.Alexa/) |

### Description

This is part of the [Bot Builder Community Extensions](https://github.com/garypretty/botbuilder-community) project which contains various pieces of middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

The Alexa Adapter allows you to add an additional endpoint to your bot for Customer Alexa Skills. The Alexa endpoint can be used
in conjunction with other channels meaning, for example, you can have a bot exposed on out of the box channels such as Facebook and 
Teams, but also via an Alexa Skill.

Incoming Alexa Skill requests are transformed, by the adapter, into Bot Builder Activties and then when your bot responds, the adapter transforms the outgoing Activity into an Alexa response.

A basic sample bot is available [here](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/tree/master/samples/Alexa%20Adapter%20Sample).

The adapter supports a broad range of capabilities for Alexa Skills, including;

* Support for voice based Alexa bots
* Support for the available display directives for Echo Show / Spot devices, with support for the new Fire Tablets coming very soon
* Support for Alexa Cards
* Support for Audio / Video directives
* Integration libraries (similar to those for the Bot Framework Adapter), allowing for the bot to also have its own middleware pipeline for Alexa
* TurnContext extensions allowing the developer to;
    * Send Alexa Progressive Updates
    * Explicitly define and attach an Alexa Card to the response (alternatively Bot Builder Hero / Signin cards can be automatically converted if attached to the outgoing activity)
    * Specify Alexa RePrompt speech / text
    * Add to / access Alexa Session Attributes (similar to TurnState in Bot Builder SDK)
    * Check if a device supports audio or audio and display
    * Retrieve the raw Alexa request
    * A new method to allow the user to call the relevant API and access user details (with skill permission), such as address
* Validation of incoming Alexa requests (required for certification)
* Middleware to automatically translate incoming Alexa requests into different types of activities


### Installation

Available via NuGet package [Bot.Builder.Community.Adapters.Alexa](https://www.nuget.org/packages/Bot.Builder.Community.Adapters.Alexa/)

Install into your project using the following command in the package manager;
```
    PM> Install-Package Bot.Builder.Community.Adapters.Alexa
```

### Sample

Basic sample bot available [here](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/tree/master/samples/Alexa%20Adapter%20Sample).

### Usage

* [Adding the adapter and skills endpoint to your bot](#adding-the-adapter-and-skills-endpoint-to-your-bot)
    * [.NET Core MVC](#net-core-mvc)
	* [.NET Core](#net-core-non-mvc)
* [Default Alexa Request to Activity mapping](#Default-Alexa-Request-to-Activity-mapping)
* [Default Activity to Alexa Response mapping](#Default-Activity-to-Alexa-Response-mapping)
* [Alexa TurnContext Extension Methods](#TurnContext-Extension-Methods)
    * [Session Attributes](#Session-Attributes)
	* [Adding an Alexa Card to a response](#Adding-an-Alexa-Card-to-a-response)
	* [Progressive Responses](#Progressive-Responses)
	* [Get entire Alexa Request Body](#Get-entire-Alexa-Request-Body)
	* [Get the user's device address](#Get-the-alexa-device-address)
	* [Add Directives to response](#Add-Directives-to-response)
	* [Check if device has Display or Audio Player support](#Check-if-device-has-Display-or-Audio-Player-support)
* [Alexa Middleware for transforming incoming Intent requests to Message activities](#Alexa-IntentRequest-to-MessageActivity-Middleware)
* [Alexa Show / Spot Display Support](#Alexa-Show-/-Spot-Display-Support)
* [Automated Translation of Bot Framework Cards into Alexa Cards](#Automated-Translation-of-Bot-Framework-Cards-into-Alexa-Cards)


### Adding the adapter and skills endpoint to your bot

Currently integration is available for .NET Core applications, with or without MVC.
When using the non-MVC approach, a new endpoint for your Alexa skill is created at '/api/skillrequests'. 
e.g. http://www.yourbot.com/api/skillrequests.  This is the endpoint that you should configure within the Amazon Alexa
Skills Developer portal as the endpoint for your skill.

When using MVC, you configure your chosen endpoint when creating your controller which will handle your skill requests. An example of this can be seen [in the .NET Core MVC secton below](#.net-core-mvc).

#### .NET Core MVC

You can use the Alexa adapter with your bot within a .NET Core MVC project by registering the AlexaHttpAdapter.
When registering the AlexaHttpAdapter, you can add middleware and also set settings, such as if a session should 
end by default or what happens when an error occurs during a bot's turn.

```cs
	services.AddSingleton<IAlexaHttpAdapter>((sp) =>
	{
	    var alexaHttpAdapter = new AlexaHttpAdapter(validateRequests: true)
	    {
	        OnTurnError = async (context, exception) =>
	        {
	            await context.SendActivityAsync("Sorry, something went wrong");
	        },
	        ShouldEndSessionByDefault = true,
	        ConvertBotBuilderCardsToAlexaCards = false
	    };
	
	    return alexaHttpAdapter;
	});
``` 

Once you have registered your the AlexaHttpAdapter in Startup.cs, you can then create a normal
MVC controller to provide an endpoint for your bot and process requests to them using the Alexa adapter.

```cs 

    [Route("api/skillrequests")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IAlexaHttpAdapter _adapter;
        private readonly IBot _bot;

        public BotController(IAlexaHttpAdapter adapter, IBot bot)
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

An example of using the Alexa adapter with a bot built on Asp.Net Core (without MVC). The implementation of the 
integration layer is based upon the same patterns used for the Bot Framework integration layer. 
In Startup.cs you can configure your bot to use the Alexa adapter using the following;

```cs

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAlexaBot<EchoBot>(options =>
        {
            options.AlexaOptions.ValidateIncomingAlexaRequests = true;
            options.AlexaOptions.ShouldEndSessionByDefault = false;
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
        app.UseDefaultFiles()
            .UseStaticFiles()
            .UseAlexa();
    }

```


### Default Alexa Request to Activity mapping

When an incoming request is receieved, the activity sent to your bot is comprised of the following values;

* **Channel ID** : "alexa"
* **Recipient Channel Account** : Id = Application Id from the Alexa request, Name = "skill"
* **From Channel Account** : Id = User Id from the Alexa request, Name = "user"
* **Conversation Account** : Id = "{Alexa request Application Id}:{Alexa Request User Id}"
* **Type** : Request Type from the Alexa request. e.g. IntentRequest, LaunchRequest or SessionEndedRequest
* **Id** : Request Id from the Alexa request
* **Timestamp** : Timestamp from the Alexa request
* **Locale** : Locale from the Alexa request

For incoming requests of type IntentRequest we also set the following properties on the Activity

* **Value** : DialogState value from the Alexa request

For incoming requests of type SessionEndedRequest we also set the following properties on the Activity

* **Code** : Reason value from the Alexa request
* **Value** : Error value from the Alexa request

The entire body of the Alexa request is placed into the Activity as Channel Data, of type skillRequest.


### Default Activity to Alexa Response mapping

The Alexa adapter will send a response to the Alexa skill request if the outgoing activity is of type MessageActivity or EndOfConversation activity.

If the actvity you send from your bot is of type EndOfConversation then a response is sent indicating that the session should be ended, by setting the the ShouldEndSession flag on the ALexa response to true.

If the activity type you send from your bot is of type MessageActivity the following values are mapped to an Alexa response object;

* **OutputSpeech Type** : Set to 'SSML' if activity.Speak is not null. Set to 'PlainText' if the activity.Text property is populated but the activity.Speak property is not.
* **OutputSpeech SSML** : Populated using the value of activity.Speak if it is not null.
* **OutputSpeech Text** : Populated using the value of activity.Text if it is not null.

* **ShouldEndSession** : Defaults to false. However, setting the InputHint property on the activity to InputHint.IgnoringInput will set this value to true and end the session.


### TurnContext Extension Methods


#### Session Attributes

Alexa Skills use Session Attributes on the request / response objects to allow for values to be persisted accross turns of a conversation.  When an incoming Alexa request is receieved we place the Session Attributes on the request into the Services collection on the TurnContext.  We then provide an extension method on the context to allow you to add / update / remove items on the Session Attributes list. Calling the extension method AlexaSessionAttributes returns an object of type Dictionary<string, string>. If you wanted to add an item to the Session Attributes collection you could do the following;

```cs 
    context.AlexaSessionAttributes.Add("NewItemKey","New Item Value");
```

#### Adding an Alexa Card to a response

You can attach a card to your response to be shown in the Alexa app or on a Fire tablet.

```cs

var card = new AlexaCard
{
    Type = AlexaCardType.Simple,
    Title = "Card title",
    Content = "This is the content to be shown on the card"
};

context.AlexaSetCard(

```

The following types of card are supported;

* Simple (populate the title / content properties)
* Standard (populate the title / text / image properties)
* LinkAccount
* AskForPermissionConsent (populate the Permissions property with a list of permission types)


#### Progressive Responses

Alexa Skills allow only a single primary response for each request.  However, if your bot will be running some form of long running activity (such as a lookup to a 3rd party API) you are able to send the user a holding response using the Alexa Progressive Responses API, before sending your final response.

To send a Progressive Response we have provided an extension method on the TurnContext called AlexaSendProgressiveResponse, which takes a string parameter which is the text you wish to be spoken back to the user. e.g.

```cs
    context.AlexaSendProgressiveResponse("Hold on, I will just check that for you.");
```

The extension method will get the right values from the incoming request to determine the correct API endpoint / access token and send your Progressive response for you.  The extension method will also return a HttpResponseMessage which will provide information as to if the Progressive Response was send successfully or if there was any kind of error.

***Note: Alexa Skills allow you to send up to 5 progressive responses on each turn.  You should manage and check the number of Progressive Responses you are sending as the Bot Builder SDK does not check this.*** 



#### Get entire Alexa Request Body

We have provided an extension method to allow you to get the original Alexa request body, which we store on the ChannelData property of the Activity sent to your bot, as a strongly typed object of type skillRequest.  To get the request just call the extension method as below;

```cs
    skillRequest request = context.GetskillRequest();
```

***Note: If you call this extension method when the incoming Activity is not from an Alexa skill then the extension method will simply return null.*** 



#### Get the Alexa device address

You can access the address the user has set against their device (or in some cases the address stored against their Amazon account).

You can call the AlexaGetUserAddress TurnContext extension to retrieve an AlexaAddress object.

If the skill doesn't have the permission to access the address, a UnauthorizedAccessException will be thrown.
In this case you can send a card to the user's Alexa app to prompt them to grant the permissions, as shown below.

```cs

AlexaAddress alexaAddress = null;

try
{
    alexaAddress = await context.AlexaGetUserAddress();
}
catch (Exception ex)
{
    if (ex is UnauthorizedAccessException)
    {
        await context.SendActivityAsync("Sorry, Looks like I dont have permission to see your address. "
		+ " I have sent a card to your Alexa app to ask for the permission");

        context.AlexaSetCard(new AlexaCard()
        {
            Type = AlexaCardType.AskForPermissionsConsent,
            Permissions = new string[] { "read::alexa:device:all:address" }
        });
    }

    alexaAddress = null;
}

```


#### Add Directives to response

Add objects of type IAlexaDirective to a collection used when sending outgoing requests to add directives to the response.  This allows you to do things like 
controlling the display on Echo Show / Spot devices.  Classes are included for Display and Hint Directives.

```cs
	dialogContext.Context.AlexaResponseDirectives().Add(displayDirective);
```


#### Check if device has Display or Audio Player support

```cs 
    dialogContext.Context.AlexaDeviceHasDisplay()

	dialogContext.Context.AlexaDeviceHasAudioPlayer()
```


### Alexa IntentRequest to MessageActivity Middleware

By default, incomign requests from Alexa are transformed into Activity objects, where the type of Activity is the same as the incoming Alexa request type. e.g. IntentRequest or LaunchRequest.
You can use the AlexaIntentRequestToMessageActivityMiddleware middleware to automatically transform Alexa Intent requests into MessageActivities, so that you can more easily build bots that work accross multiple channels, including Alexa.

You can add the middleware to your bot with the following line;

```cs 

options.Middleware.Add(
	new AlexaIntentRequestToMessageActivityMiddleware(transformPattern: RequestTransformPatterns.MessageActivityTextFromSinglePhraseSlotValue));

```

You can specify one of two transform patterns;

* **MessageActivityTextFromSinglePhraseSlotValue** - This option will look for a single Alexa intent slot called 'Phrase' and set the contents of this slot as the Text property for the generated Activity.

* **MessageActivityTextFromIntentAndAllSlotValues** - This slot will generate MessageActivity text from the name of the Intent and all identified slots. e.g. "Intent='YourIntentName' SlotName1='SlotValue' SlotName2='SlotValue'"

Optionally you can also provide your own function to transform the incoming Alexa request and determine how your generate the MessageActivity yourself.

Note: Any incoming requests with built in Amazon Intents will simply have the name of the Intent set as the Message activity text. e.g.

```cs

if (activity.Text == "AMAZON.CancelIntent")
{
	...
}

```

This middleare will only transform incoming custom IntentRequests, for other types of request such as LaunchRequest or SessionEndedRequest will geneate activities of that type, which you can then check within your code. e.g.

```cs

if (activity.Type == AlexaRequestTypes.LaunchRequest)
{
	...
}

```


### Alexa Show / Spot Display Support

Display Directives to support devices with screens, such as the Echo Show and Spot, can be added to your bots response.  A class for each of the currently supported templates 
exists within the Alexa.Directives namespace.  A context extension method allows you to add directives to the services collection which will then be used by the Alexa Adapter 
when processing outgoing activities and will add the appropriate JSON to the response.

***Note: You should also use the AlexaDeviceHasDisplay() extension method on the ITurnContext object to check if the Alexa device that has sent the incoming 
request has a display - this is because if you send a display directive to a device without a display it will cause an error and not simply be ignored.***

``` cs

            var displayDirective = new DisplayDirective()
            {
                Template = new DisplayRenderBodyTemplate1()
                {
                    BackButton = BackButtonVisibility.HIDDEN,
                    Title = "Claim Update",
                    TextContent = new TextContent()
                    {
                        PrimaryText = new InnerTextContent()
                        {
                            Text = "<font size=\"7\"><b>Good news!</b></font>",
                            Type = TextContentType.RichText
                        },
                        SecondaryText = new InnerTextContent()
                        {
                            Text = "<br/><font size=\"3\">This is your Secondary Text",
                            Type = TextContentType.RichText
                        },
                        TertiaryText = new InnerTextContent()
                        {
                            Text = "This is tertiary text - this time it is plain text",
                            Type = TextContentType.PlainText
                        }
                    },
                    Token = "",
                    BackgroundImage = new Image()
                    {
                        ContentDescription = "test",
                        Sources = new ImageSource[]
                                {
                                    new ImageSource()
                                    {
                                        Url = "https://www.yourimageurl.com/background.jpg",
                                        WidthPixels = 1025,
                                        HeightPixels = 595
                                    }
                                }
                    }
                }
            };

            if (dialogContext.Context.AlexaDeviceHasDisplay())
            {
                dialogContext.Context.AlexaResponseDirectives().Add(displayDirective);
            }

```


### Automated Translation of Bot Framework Cards into Alexa Cards

The Alexa Adapter supports sending Bot Framework cards of type HeroCard, ThumbnailCard and SigninCard as part of your replies to the Alexa skill request.

The Alexa adapter will use the first card attachment by default, unless you have disabled this using the AlexaBotOptions property ConvertFirstActivityAttachmentToAlexaCard.

* **HeroCard and ThumbnailCard** : 

 * Alexa Card Small Image URL = The first image in the Images collection on the Hero / Thumbnail card
 * Alexa Card Large Image URL = If a second image exists in the Images collection on the Hero / Thumbnail card this will be used. If no second image exists then this is null.
 * Alexa Card Title = Title property of the Hero / Thumbnail card
 * Alexa Card Content = Text property on the Hero / Thumbnail card

***Note: You should ensure that the images you use on your HeroCard / Thumbnail cards are the correct expected size for Alexa Skills responses.***

* **SigninCard** : If a SignInCard is attached to your outgoing activity, this will be mapped as a LinkAccount card in the Alexa response.
