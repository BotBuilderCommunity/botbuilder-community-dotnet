# Alexa Adapter for Bot Builder v4 .NET SDK - ***PREVIEW***

## Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | Preview [available via MyGet (version 4.6.4-beta0036)](https://www.myget.org/feed/botbuilder-community-dotnet/package/nuget/Bot.Builder.Community.Adapters.Alexa/4.6.4-beta0036) |

## Description

This is part of the [Bot Builder Community Extensions](https://github.com/botbuildercommunity) project which contains various pieces of middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

The Alexa Adapter allows you to add an additional endpoint to your bot for Alexa Skills. The Alexa endpoint can be used
in conjunction with other channels meaning, for example, you can have a bot exposed on out of the box channels such as Facebook and 
Teams, but also via an Alexa Skill (as well as side by side with the Google / Twitter Adapters also available from the Bot Builder Community Project).

Incoming Alexa Skill requests are transformed, by the adapter, into Bot Builder Activties and then when your bot responds, the adapter transforms the outgoing Activity into an Alexa Skill response.

The adapter currently supports the following scenarios;

* Support for voice based Alexa Skills
* Support for the available display directives for Echo Show / Spot devices, with support for the new Fire Tablets coming very soon
* Support for Alexa Cards
* Support for Audio / Video directives
* TurnContext extensions allowing the developer to;
    * Send Alexa Progressive Updates
    * Specify Alexa RePrompt speech / text
    * Add to / access Alexa Session Attributes (similar to TurnState in Bot Builder SDK)
    * Check if a device supports audio or audio and display
* Full incoming request from Alexa is added to the incoming activity as ChannelData
* Validation of incoming Alexa requests (required for certification)

## Installation

The preview of the next version of the Alexa Adapter is [available via MyGet (version 4.6.4-beta0036)](https://www.myget.org/feed/botbuilder-community-dotnet/package/nuget/Bot.Builder.Community.Adapters.Alexa/4.6.4-beta0036).

To install into your project use the following command in the package manager.  If you wish to use the Visual Studio package manager, then add https://www.myget.org/F/botbuilder-community-dotnet/api/v3/index.json as an additional package source.
```
    PM> Install-Package Bot.Builder.Community.Adapters.Alexa -Version 4.6.4-beta0036 -Source https://www.myget.org/F/botbuilder-community-dotnet/api/v3/index.json
```

## Sample

Sample bot, showing examples of Alexa specific functionality using the current preview is available [here](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/tree/feature/adopt-alexadotnet/samples/Alexa%20Adapter%20Sample).

## Usage

* [Prerequisites](#prerequisites)
* [Create an Alexa skill](#create-an-alexa-skill)
* [Wiring up the Alexa adapter in your bot](#wiring-up-the-alexa-adapter-in-your-bot)
* [Complete configuration of your Alexa skill](#complete-configuration-of-your-alexa-skill)
* [Test your Alexa skill](#test-your-alexa-skill) - Test your bot in the Alexa skill simulator and Alexa devices
* [Customising your conversation](#customising-your-conversation) - Learn about controlling end of session and use of cards / display directives etc.

In this article you will learn how to connect a bot to an Alexa skill using the Alexa adapter.  This article will walk you through modifying the EchoBot sample to connect it to a skill.

### Prerequisites

* The [EchoBot sample code](https://github.com/microsoft/BotBuilder-Samples/tree/master/samples/csharp_dotnetcore/02.echo-bot)

* Access to the Alexa Developer Console with sufficient permissions to login to create / manage skills at  [https://developer.amazon.com/alexa/console/ask](https://developer.amazon.com/alexa/console/ask). If you do not have this you can create an account for free.

### Create an Alexa skill

1. Log into the [Alexa Developer Console](https://developer.amazon.com/alexa/console/ask) and then click the 'Create Skill' button.

2. On the next screen enter a name for your new skill.  On this page you can **Choose a model to add to your skill** (**Custom** selected by default) and **Choose a method to host your skill's backend resources** (**Provision your own** selected by default).  Leave the default options selected and click the **Create Skill** button.

![Skill model and hosting](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/create-skill-options.PNG?raw=true)

3. On the next screen you will be asked to **Choose a template**.  **Start from scratch** will be selected by default. Leave **Start from scratch** selected and click the **Choose** button.

![Skill template](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/create-skill-options2.PNG?raw=true)

4. You will now be presented with your skill dashboard. Navigate to **JSON Editor** within the **Interaction Model** section of the left hand menu.

5. Paste the JSON below into the **JSON Editor**, replacing the following values;

* **YOUR SKILL INVOCATION NAME** - This is the name that users will use to invoke your skill on Alexa. For example, if your skill invocation name was 'adapter helper', then a user would could say "Alexa, launch adapter helper" to launch the skill.

* **EXAMPLE PHRASES** - You should provide 3 example phases that users could use to interact with your skill.  For example, if a user might say "Alexa, ask adapter helper to give me details of the alexa adapter", your example phrase would be "give me details of the alexa adapter".

```json
{
    "interactionModel": {
        "languageModel": {
            "invocationName": "<YOUR SKILL INVOCATION NAME>",
            "intents": [
                {
                    "name": "GetUserIntent",
                    "slots": [
                        {
                            "name": "phrase",
                            "type": "phrase"
                        }
                    ],
                    "samples": [
                        "{phrase}"
                    ]
                },
                {
                    "name": "AMAZON.CancelIntent",
                    "samples": []
                },
                {
                    "name": "AMAZON.HelpIntent",
                    "samples": []
                },
                {
                    "name": "AMAZON.StopIntent",
                    "samples": []
                },
                {
                    "name": "AMAZON.NavigateHomeIntent",
                    "samples": []
                }
            ],
            "types": [
                {
                    "name": "phrase",
                    "values": [
                        {
                            "name": {
                                "value": "<EXAMPLE PHRASE>"
                            }
                        },
                        {
                            "name": {
                                "value": "<EXAMPLE PHRASE>"
                            }
                        },
                        {
                            "name": {
                                "value": "<EXAMPLE PHRASE>"
                            }
                        }
                    ]
                }
            ]
        }
    }
}
```

6. Click the **Save Model** button and then click **Build Model**, which will update the configuration for your skill.

### Wiring up the Alexa adapter in your bot

Before you can complete the configuration of your Alexa skill, you need to wire up the Alexa adapter into your bot.

#### Install the Alexa adapter NuGet package

Add  the [Bot.Builder.Community.Adapters.Alexa](https://www.nuget.org/packages/Bot.Builder.Community.Adapters.Alexa/) NuGet package. For more information on using NuGet, see [Install and manage packages in Visual Studio](https://aka.ms/install-manage-packages-vs)

#### Create an Alexa adapter class

Create a new class that inherits from the ***AlexaAdapter*** class. This class will act as our adapter for the Alexa channel. It includes error handling capabilities (much like the ***BotFrameworkAdapterWithErrorHandler*** class already in the sample, used for handling requests from Azure Bot Service).  We are also adding the ***AlexaRequestToMessageEventActivitiesMiddleware*** class to the middleware pipeline, which will transform incoming Alexa requests into more familiar activity types. 

```csharp
    public class AlexaAdapterWithErrorHandler : AlexaAdapter
    {
        public AlexaAdapterWithErrorHandler(ILogger<AlexaAdapter> logger)
            : base(new AlexaAdapterOptions(), logger)
    {
        OnTurnError = async (turnContext, exception) =>
        {
            // Log any leaked exception from the application.
            logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");

            // Send a message to the user
            await turnContext.SendActivityAsync("The bot encountered an error or bug.");
            await turnContext.SendActivityAsync("To continue to run this bot, please fix the bot source code.");
        };

        Use(new AlexaRequestToMessageEventActivitiesMiddleware());
    }
}
```

You will also need to add the following using statements.

```cs
using Bot.Builder.Community.Adapters.Alexa;
using Bot.Builder.Community.Adapters.Alexa.Middleware;
using Microsoft.Extensions.Logging;
```

#### Create a new controller for handling Alexa requests

You now need to create a new controller which will handle requests from your Alexa skill, on a new endpoing 'api/alexa' instead of the default 'api/messages' used for requests from Azure Bot Service Channels.  By adding an additional endpoint to your bot, you can accept requests from Bot Service channels (or additional adapters), as well as from Alexa, using the same bot.

```csharp
[Route("api/alexa")]
[ApiController]
public class AlexaController : ControllerBase
{
    private readonly AlexaAdapter _adapter;
    private readonly IBot _bot;

    public AlexaController(AlexaAdapter adapter, IBot bot)
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
using Bot.Builder.Community.Adapters.Alexa;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
```

#### Inject Alexa Adapter In Your Bot Startup.cs

Add the following line into the ***ConfigureServices*** method within your Startup.cs file, which will register your Alexa adapter and make it available for your new controller class.  The configuration settings, described in the next step, will be automatically used by the adapter.

```csharp
services.AddSingleton<AlexaAdapter, AlexaAdapterWithErrorHandler>();
```

Once added, your ***ConfigureServices*** method shold look like this.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

    // Create the default Bot Framework Adapter (used for Azure Bot Service channels and emulator).
    services.AddSingleton<IBotFrameworkHttpAdapter, BotFrameworkAdapterWithErrorHandler>();

    // Create the Slack Adapter
    services.AddSingleton<AlexaAdapter, AlexaAdapterWithErrorHandler>();

    // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
    services.AddTransient<IBot, EchoBot>();
}
```

You will also need to add the following using statement, in addition to those already present in the startup.cs file.

```cs
using Bot.Builder.Community.Adapters.Alexa;
```

### Complete configuration of your Alexa skill

Now that you have created an Alexa skill and wired up the adapter in your bot project, the final steps are to configure the endpoint to which requests will be posted to when your Alexa skill is invoked, pointing it to the correct endpoint on your bot.

1. To complete this step, [deploy your bot to Azure](https://aka.ms/bot-builder-deploy-az-cli) and make a note of the URL to your deployed bot. Your Alexa messaging endpoint is the URL for your bot, which will be the URL of your deployed application (or ngrok endpoint), plus '/api/alexa' (for example, `https://yourbotapp.azurewebsites.net/api/alexa`).

> [!NOTE]
> If you are not ready to deploy your bot to Azure, or wish to debug your bot when using the Alexa adapter, you can use a tool such as [ngrok](https://www.ngrok.com) (which you will likely already have installed if you have used the Bot Framework emulator previously) to tunnel through to your bot running locally and provide you with a publicly accessible URL for this. 
> 
> If you wish create an ngrok tunnel and obtain a URL to your bot, use the following command in a terminal window (this assumes your local bot is running on port 3978, alter the port numbers in the command if your bot is not).
> 
> ```
> ngrok.exe http 3978 -host-header="localhost:3978"
> ```

2. Back within your Alexa skill dashboard, navigate to the **Endpoint** section on the left hand menu.  Select **HTTPS** as the **Service Endpoint Type** and set the **Default Region** endpoint to your bot's Alexa endpoint, such as https://yourbotapp.azurewebsites.net/api/alexa.

3. In the drop down underneath the text box where you have defined your endpoint, you need to select the type of certificate being used.  For development purposes, you can choose **My development endpoint is a sub-domain of a domain that has a wildcard certificate from a certificate authority**, changing this to **My development endpoint has a certificate from a trusted certificate authority** when you publish your skill into Production.

![Skill template](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/alexa-endpoint.PNG?raw=true)

4. Click the **Save Endpoints** button.

### Test your Alexa skill

You can now test interacting with your Alexa skill using the simulator. 

1. In the skill dashboard navigate to the **Test** tab at the top of the page.

2. You will see a label **Test is disabled for this skill** with a dropdown list next to it with a value of **Off**. Select this dropdown and select **Development**. This will enable testing for your skill.

3. As a basic test enter "ask <SKILL INVOCATION NAME> hello world" into the simulator input box. For example, if your skill invocation name was 'alexa helper', you would type 'ask alexa helper hello world'. This should return an echo of your message.

![Simulator](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/simulator.PNG?raw=true)

Now that you have enabled testing for your skill, you can also test your skill using a physical Echo device or the Alexa app, providing you are logged into the device / app with the same account used to login to the Alexa Developer Console (or an account that you have added as a beta tester for your skill within the console).

### Customising your conversation

#### Controlling the end of a session

By default, the Alexa adapter is configured to close the session following sending a response. You can explicitly indicate that Alexa should wait for the user to say something else, meaning Alexa should leave the microphone open and listen for further input, by sending an input hint of ***ExpectingInput*** on your outgoing activity.

```cs
await turnContext.SendActivityAsync("Your message text", inputHint: InputHints.ExpectingInput);
```

You can alter the default behavior to leave the session open and listen for further input by default by setting the ***ShouldEndSessionByDefault*** setting on the ***AlexaAdapterOptions*** class when creating your adapter, as shown below.

```csharp
    public class AlexaAdapterWithErrorHandler : AlexaAdapter
    {
        public AlexaAdapterWithErrorHandler(ILogger<AlexaAdapter> logger)
            : base(new AlexaAdapterOptions() { ShouldEndSessionByDefault = false }, logger)
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

If you do set ***ShouldEndSessionByDefault*** to true, then you need to explicity end the conversation when you are ready, by sending an input hint of ***IgnoringInput*** on your last outgoing activity.

```cs
await turnContext.SendActivityAsync("Your message text", inputHint: InputHints.IgnoringInput);
```

#### Handling multiple outgoing activities

By default, Alexa expects a single response to each request that is sent to your bot. However, it is not uncommon for a bot to send multiple activities back in response to a request.  This can cause issues, especially if you are using Alexa alongside other channels or adapters.

To combat this issue the adapter will automatically concatenate multiple activities into a single activity, combining the Speak and Text properties of the activities.

***Note: by default, the previous version of the adapter took the last version of the activity. If you have previously deployed a bot using the adapter, you may need to consider extending the adapter and overriding the activity processing behavior shown below.***

```cs
    public class AlexaAdapterEx : AlexaAdapter
    {
        public override Activity ProcessOutgoingActivities(List<Activity> activities)
        {
            return activities.Last();
        }
    }
```

#### Sending an Alexa card as part of your response

You can include an Alexa card in your response, which is shown on devices that have a screen and / or in the activity feed in the Alexa app.  To do this you include an attachment on your outgoing activity.

```cs
var activityWithCard = MessageFactory.Text($"Ok, I included a simple card.");
                    activityWithCard.Attachments.Add(
                        new CardAttachment(
                            new SimpleCard()
                            {
                                Title = "This is a simple card",
                                Content = "This is the simple card content"
                            }));
                    await turnContext.SendActivityAsync(activityWithCard, cancellationToken);
```

#### Sending display directives for devices that support screens

You can send Alexa display directives, which will show structured information on devices with a screen, such as the Echo Show or Echo Spot. If you wish to send display directives, you need to enable the **Display Interface** setting within the **Interfaces** section within the Alexa Skills Console.

To send a display directive, you send a ***DirectiveAttachment*** on your outgoing activity. On the attachment you set the template that you would like to use and populate the required fields.

You can find information about the various display templates available and their required properties at [https://developer.amazon.com/en-US/docs/alexa/custom-skills/display-template-reference.html](https://developer.amazon.com/en-US/docs/alexa/custom-skills/display-template-reference.html).

```cs
var activityWithDisplayDirective = MessageFactory.Text($"Ok, I included a display directive`.");
                    activityWithDisplayDirective.Attachments.Add(
                        new DirectiveAttachment(
                            new DisplayRenderTemplateDirective()
                            {
                                Template = new BodyTemplate1()
                                {
                                    BackgroundImage = new TemplateImage()
                                    {
                                        ContentDescription = "Test",
                                        Sources = new List<ImageSource>()
                                        {
                                            new ImageSource()
                                            {
                                                Url = "https://via.placeholder.com/576.PNG?raw=true/09f/fff",
                                            }
                                        }
                                    },
                                    Content = new TemplateContent()
                                    {
                                        Primary = new TemplateText() { Text = "Test", Type = "PlainText" }
                                    },
                                    Title = "Test title",
                                }
                            }));
```