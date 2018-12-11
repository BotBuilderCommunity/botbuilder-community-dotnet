## google Adapter for Bot Builder v4 .NET SDK

### Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [![NuGet version](https://img.shields.io/badge/NuGet-1.0.100-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Adapters.google/) |

### Description

This is part of the [Bot Builder Community Extensions](https://github.com/garypretty/botbuilder-community) project which contains various pieces of middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

The google Adapter allows you to add an additional endpoint to your bot for Customer google Skills. The google endpoint can be used
in conjunction with other channels meaning, for example, you can have a bot exposed on out of the box channels such as Facebook and 
Teams, but also via an google Skill.

Incoming google Skill requests are transformed, by the adapter, into Bot Builder Activties and then when your bot responds, the adapter transforms the outgoing Activity into an google response.

A basic sample bot is available [here](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/tree/master/samples/google%20Adapter%20Sample).

The adapter supports a broad range of capabilities for google Skills, including;

* Support for voice based google bots
* Support for the available display directives for Echo Show / Spot devices, with support for the new Fire Tablets coming very soon
* Support for google Cards
* Support for Audio / Video directives
* Integration libraries (similar to those for the Bot Framework Adapter), allowing for the bot to also have its own middleware pipeline for google
* TurnContext extensions allowing the developer to;
    * Send google Progressive Updates
    * Explicitly define and attach an google Card to the response (alternatively Bot Builder Hero / Signin cards can be automatically converted if attached to the outgoing activity)
    * Specify google RePrompt speech / text
    * Add to / access google Session Attributes (similar to TurnState in Bot Builder SDK)
    * Check if a device supports audio or audio and display
    * Retrieve the raw google request
    * A new method to allow the user to call the relevant API and access user details (with skill permission), such as address
* Validation of incoming google requests (required for certification)
* Middleware to automatically translate incoming google requests into different types of activities


### Installation

Available via NuGet package [Bot.Builder.Community.Adapters.google](https://www.nuget.org/packages/Bot.Builder.Community.Adapters.google/)

Install into your project using the following command in the package manager;
```
    PM> Install-Package Bot.Builder.Community.Adapters.google
```

### Sample

Basic sample bot available [here](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/tree/master/samples/google%20Adapter%20Sample).

### Usage

* [Adding the adapter and skills endpoint to your bot](#adding-the-adapter-and-skills-endpoint-to-your-bot)
    * [WebApi](#webapi)
	* [.NET Core](#.net-core)
* [Default google Request to Activity mapping](#Default-google-Request-to-Activity-mapping)
* [Default Activity to google Response mapping](#Default-Activity-to-google-Response-mapping)
* [google TurnContext Extension Methods](#TurnContext-Extension-Methods)
    * [Session Attributes](#Session-Attributes)
	* [Adding an google Card to a response](#Adding-an-google-Card-to-a-response)
	* [Progressive Responses](#Progressive-Responses)
	* [Get entire google Request Body](#Get-entire-google-Request-Body)
	* [Get the user's device address](#Get-the-google-device-address)
	* [Add Directives to response](#Add-Directives-to-response)
	* [Check if device has Display or Audio Player support](#Check-if-device-has-Display-or-Audio-Player-support)
* [google Middleware for transforming incoming Intent requests to Message activities](#google-IntentRequest-to-MessageActivity-Middleware)
* [google Show / Spot Display Support](#google-Show-/-Spot-Display-Support)
* [Automated Translation of Bot Framework Cards into google Cards](#Automated-Translation-of-Bot-Framework-Cards-into-google-Cards)


### Adding the adapter and skills endpoint to your bot

Currently there are integration libraries available for WebApi and .NET Core available for the adapter.
When using the integration libraries a new endpoint for your google skill is created at '/api/skillrequests'. 
e.g. http://www.yourbot.com/api/skillrequests.  This is the endpoint that you should configure within the Amazon google
Skills Developer portal as the endpoint for your skill.

#### WebApi

When implementing your bot using WebApi, the integration layer for google works the same as the default for Bot Framework.  The only difference being in your BotConfig file under the App_Start folder you call MapgoogleBotFramework instead;

```cs
    public class BotConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapgoogleBotFramework(botConfig => { 
				botConfig.googleBotOptions.googleOptions.ShouldEndSessionByDefault = true;
                botConfig.googleBotOptions.googleOptions.ValidateIncominggoogleRequests = false;
			});
        }
    }
``` 

#### .NET Core

An example of using the google adapter with a bot built on Asp.Net Core. Again, The implementation of the 
integration layer is based upon the same patterns used for the Bot Framework integration layer. 
In Startup.cs you can configure your bot to use the google adapter using the following;

```cs
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddgoogleBot<EchoBot>(options =>
            {
                options.googleOptions.ValidateIncominggoogleRequests = true;
                options.googleOptions.ShouldEndSessionByDefault = false;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseDefaultFiles()
                .UseStaticFiles()
                .Usegoogle();
        }
```


### Default google Request to Activity mapping

When an incoming request is receieved, the activity sent to your bot is comprised of the following values;

* **Channel ID** : "google"
* **Recipient Channel Account** : Id = Application Id from the google request, Name = "skill"
* **From Channel Account** : Id = User Id from the google request, Name = "user"
* **Conversation Account** : Id = "{google request Application Id}:{google Request User Id}"
* **Type** : Request Type from the google request. e.g. IntentRequest, LaunchRequest or SessionEndedRequest
* **Id** : Request Id from the google request
* **Timestamp** : Timestamp from the google request
* **Locale** : Locale from the google request

For incoming requests of type IntentRequest we also set the following properties on the Activity

* **Value** : DialogState value from the google request

For incoming requests of type SessionEndedRequest we also set the following properties on the Activity

* **Code** : Reason value from the google request
* **Value** : Error value from the google request

The entire body of the google request is placed into the Activity as Channel Data, of type googleRequestBody.


### Default Activity to google Response mapping

The google adapter will send a response to the google skill request if the outgoing activity is of type MessageActivity or EndOfConversation activity.

If the actvity you send from your bot is of type EndOfConversation then a response is sent indicating that the session should be ended, by setting the the ShouldEndSession flag on the google response to true.

If the activity type you send from your bot is of type MessageActivity the following values are mapped to an google response object;

* **OutputSpeech Type** : Set to 'SSML' if activity.Speak is not null. Set to 'PlainText' if the activity.Text property is populated but the activity.Speak property is not.
* **OutputSpeech SSML** : Populated using the value of activity.Speak if it is not null.
* **OutputSpeech Text** : Populated using the value of activity.Text if it is not null.

* **ShouldEndSession** : Defaults to false. However, setting the InputHint property on the activity to InputHint.IgnoringInput will set this value to true and end the session.


### TurnContext Extension Methods


#### Session Attributes

google Skills use Session Attributes on the request / response objects to allow for values to be persisted accross turns of a conversation.  When an incoming google request is receieved we place the Session Attributes on the request into the Services collection on the TurnContext.  We then provide an extension method on the context to allow you to add / update / remove items on the Session Attributes list. Calling the extension method googleSessionAttributes returns an object of type Dictionary<string, string>. If you wanted to add an item to the Session Attributes collection you could do the following;

```cs 
    context.googleSessionAttributes.Add("NewItemKey","New Item Value");
```

#### Adding an google Card to a response

You can attach a card to your response to be shown in the google app or on a Fire tablet.

```cs

var card = new googleCard
{
    Type = googleCardType.Simple,
    Title = "Card title",
    Content = "This is the content to be shown on the card"
};

context.googleSetCard(

```

The following types of card are supported;

* Simple (populate the title / content properties)
* Standard (populate the title / text / image properties)
* LinkAccount
* AskForPermissionConsent (populate the Permissions property with a list of permission types)


#### Progressive Responses

google Skills allow only a single primary response for each request.  However, if your bot will be running some form of long running activity (such as a lookup to a 3rd party API) you are able to send the user a holding response using the google Progressive Responses API, before sending your final response.

To send a Progressive Response we have provided an extension method on the TurnContext called googleSendProgressiveResponse, which takes a string parameter which is the text you wish to be spoken back to the user. e.g.

```cs
    context.googleSendProgressiveResponse("Hold on, I will just check that for you.");
```

The extension method will get the right values from the incoming request to determine the correct API endpoint / access token and send your Progressive response for you.  The extension method will also return a HttpResponseMessage which will provide information as to if the Progressive Response was send successfully or if there was any kind of error.

***Note: google Skills allow you to send up to 5 progressive responses on each turn.  You should manage and check the number of Progressive Responses you are sending as the Bot Builder SDK does not check this.*** 



#### Get entire google Request Body

We have provided an extension method to allow you to get the original google request body, which we store on the ChannelData property of the Activity sent to your bot, as a strongly typed object of type googleRequestBody.  To get the request just call the extension method as below;

```cs
    googleRequestBody request = context.GetgoogleRequestBody();
```

***Note: If you call this extension method when the incoming Activity is not from an google skill then the extension method will simply return null.*** 



#### Get the google device address

You can access the address the user has set against their device (or in some cases the address stored against their Amazon account).

You can call the googleGetUserAddress TurnContext extension to retrieve an googleAddress object.

If the skill doesn't have the permission to access the address, a UnauthorizedAccessException will be thrown.
In this case you can send a card to the user's google app to prompt them to grant the permissions, as shown below.

```cs

googleAddress googleAddress = null;

try
{
    googleAddress = await context.googleGetUserAddress();
}
catch (Exception ex)
{
    if (ex is UnauthorizedAccessException)
    {
        await context.SendActivityAsync("Sorry, Looks like I dont have permission to see your address. "
		+ " I have sent a card to your google app to ask for the permission");

        context.googleSetCard(new googleCard()
        {
            Type = googleCardType.AskForPermissionsConsent,
            Permissions = new string[] { "read::google:device:all:address" }
        });
    }

    googleAddress = null;
}

```


#### Add Directives to response

Add objects of type IgoogleDirective to a collection used when sending outgoing requests to add directives to the response.  This allows you to do things like 
controlling the display on Echo Show / Spot devices.  Classes are included for Display and Hint Directives.

```cs
	dialogContext.Context.GoogleResponseDirectives().Add(displayDirective);
```


#### Check if device has Display or Audio Player support

```cs 
    dialogContext.Context.googleDeviceHasDisplay()

	dialogContext.Context.googleDeviceHasAudioPlayer()
```


### google IntentRequest to MessageActivity Middleware

By default, incomign requests from google are transformed into Activity objects, where the type of Activity is the same as the incoming google request type. e.g. IntentRequest or LaunchRequest.
You can use the googleIntentRequestToMessageActivityMiddleware middleware to automatically transform google Intent requests into MessageActivities, so that you can more easily build bots that work accross multiple channels, including google.

You can add the middleware to your bot with the following line;

```cs 

options.Middleware.Add(
	new googleIntentRequestToMessageActivityMiddleware(transformPattern: RequestTransformPatterns.MessageActivityTextFromSinglePhraseSlotValue));

```

You can specify one of two transform patterns;

* **MessageActivityTextFromSinglePhraseSlotValue** - This option will look for a single google intent slot called 'Phrase' and set the contents of this slot as the Text property for the generated Activity.

* **MessageActivityTextFromIntentAndAllSlotValues** - This slot will generate MessageActivity text from the name of the Intent and all identified slots. e.g. "Intent='YourIntentName' SlotName1='SlotValue' SlotName2='SlotValue'"

Optionally you can also provide your own function to transform the incoming google request and determine how your generate the MessageActivity yourself.

Note: Any incoming requests with built in Amazon Intents will simply have the name of the Intent set as the Message activity text. e.g.

```cs

if (activity.Text == "AMAZON.CancelIntent")
{
	...
}

```

This middleare will only transform incoming custom IntentRequests, for other types of request such as LaunchRequest or SessionEndedRequest will geneate activities of that type, which you can then check within your code. e.g.

```cs

if (activity.Type == googleRequestTypes.LaunchRequest)
{
	...
}

```


### google Show / Spot Display Support

Display Directives to support devices with screens, such as the Echo Show and Spot, can be added to your bots response.  A class for each of the currently supported templates 
exists within the google.Directives namespace.  A context extension method allows you to add directives to the services collection which will then be used by the google Adapter 
when processing outgoing activities and will add the appropriate JSON to the response.

***Note: You should also use the googleDeviceHasDisplay() extension method on the ITurnContext object to check if the google device that has sent the incoming 
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

            if (dialogContext.Context.googleDeviceHasDisplay())
            {
                dialogContext.Context.GoogleResponseDirectives().Add(displayDirective);
            }

```


### Automated Translation of Bot Framework Cards into google Cards

The google Adapter supports sending Bot Framework cards of type HeroCard, ThumbnailCard and SigninCard as part of your replies to the google skill request.

The google adapter will use the first card attachment by default, unless you have disabled this using the googleBotOptions property ConvertFirstActivityAttachmentTogoogleCard.

* **HeroCard and ThumbnailCard** : 

 * google Card Small Image URL = The first image in the Images collection on the Hero / Thumbnail card
 * google Card Large Image URL = If a second image exists in the Images collection on the Hero / Thumbnail card this will be used. If no second image exists then this is null.
 * google Card Title = Title property of the Hero / Thumbnail card
 * google Card Content = Text property on the Hero / Thumbnail card

***Note: You should ensure that the images you use on your HeroCard / Thumbnail cards are the correct expected size for google Skills responses.***

* **SigninCard** : If a SignInCard is attached to your outgoing activity, this will be mapped as a LinkAccount card in the google response.
