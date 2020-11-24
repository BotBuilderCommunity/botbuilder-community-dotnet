# Google Actions SDK (latest) Adapter for Bot Builder v4 .NET SDK

> **This adapter is for use with the latest Google Actions SDK for building conversational actions (https://developers.google.com/assistant/conversational/overview). The adapter supporting the Legacy Actions SDK and DialogFlow can be found [here](../Bot.Builder.Community.Adapters.Google).**

## Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [Available via NuGet](https://www.nuget.org/packages/Bot.Builder.Community.Adapters.ActionsSDK/) |

## Description

This is part of the [Bot Builder Community Extensions](https://github.com/botbuildercommunity) project which contains various pieces of middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

The Actions SDK Adapter allows you to add an additional endpoint to your bot for conversational actions built with the latest Google Actions SDK. The Google endpoint can be used in conjunction with other channels meaning, for example, you can have a bot exposed on out of the box channels such as Facebook and Teams, but also via a Google Action (as well as side by side with other adapters (e.g. the Alexa adapter) also available from the Bot Builder Community Project).

Incoming requests from your Google action are transformed, by the adapter, into Bot Builder Activties and then when your bot responds, the adapter transforms the outgoing Activity into an appropriate Actions SDK response.

The adapter currently supports the following scenarios;

* Support for voice based Google actions
* Support for Actions SDK Card, Table Card, List and Collections
* Automatic conversion of Suggested Actions on outgoing activity into Google Suggestion Chips
* Account Linking - send a Bot Framework Signin Card to trigger the account linking flow
* Full incoming request from Google is added to the incoming activity as ChannelData

## Installation

Available via NuGet package [Bot.Builder.Community.Adapters.ActionsSDK](https://www.nuget.org/packages/Bot.Builder.Community.Adapters.ActionsSDK/).

Install into your project use the following command in the package manager. 

```
    PM> Install-Package Bot.Builder.Community.Adapters.ActionsSDK
```

## Sample

A Sample bot with examples of Actions SDK specific functionality (such as sending cards, lists etc.) is available [here](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/samples/Actions%20SDK%20Adapter%20Sample).

## Usage

* [Prerequisites](#prerequisites)
* [Create an Actions SDK action project](#create-an-Actions-SDK-action-project)
* [Configure your Actions SDK project](#configure-your-Actions-SDK-project)
* [Wiring up the Actions SDK adapter in your bot](#wiring-up-the-actions-sdk-adapter-in-your-bot)
* [Complete configuration of your Actions SDK project](#complete-configuration-of-your-actions-sdk-project)
* [Testing your Action](#testing-your-action)
* [Incoming action requests to Bot Framework activity mapping](#Incoming-action-requests-to-Bot-Framework-activity-mapping)
* [Customising your conversation](#customising-your-conversation) - Learn about controlling end of session, use of cards, lists and collections.
* [Account Linking](#account-linking) - Learn about to use the Account Linking feature and how the trigger the sign in flow and handle the resulting events.

This article will walk you through modifying the EchoBot sample to connect it to an Actions SDK action.

### Prerequisites

* The [EchoBot sample code](https://github.com/microsoft/BotBuilder-Samples/tree/master/samples/csharp_dotnetcore/02.echo-bot)

* Access to the Actions on Google developer console with sufficient permissions to login to create / manage projects at  [https://console.actions.google.com/](https://console.actions.google.com/). If you do not have this you can create an account for free.

* The gactions command-line tool. [Download this here](https://developers.google.com/assistant/conversational/quickstart#install_the_gactions_command-line_tool)

### Create an Actions SDK action project

1. Log into the [Actions on Google console](https://console.actions.google.com/) and then click the **New project** button. Specify a name for your new project, as well as the language / region. Finally click the **Create project** button.

2. On the next screen, select **Custom** for the type of your new action.

![Select action type](/libraries/Bot.Builder.Community.Adapters.ActionsSDK/media/actions-sdk-select-project-type.PNG?raw=true)

3. On the next screen, select **Blank project** for the project template to use to build your action.

![Select action template](/libraries/Bot.Builder.Community.Adapters.ActionsSDK/media/actions-sdk-select-project-type-2.PNG?raw=true)

4. Your action will now be created. Once the creation process has finished you will be presented with your project dashboard. *Note: you do not need to set a **Display name** yet.*

![Select action template](/libraries/Bot.Builder.Community.Adapters.ActionsSDK/media/actions-sdk-project-created.PNG?raw=true)

### Configure your Actions SDK project

You now need to configure your new Actions SDK project, with appropriate settings, scenes, intents, types etc. You will do this using the gactions CLI tool and the provided action template files included in the 'action' folder in this repository.

1. If you haven't already, [install the gactions CLI tool](https://developers.google.com/assistant/conversational/quickstart#install_the_gactions_command-line_tool).

2. If you haven't already, Download a copy of this repository.

3. Navigate to botbuilder-community-dotnet/libraries/Bot.Builder.Community.Adapters.ActionsSDK/action/settings/settings.yaml, and replace <YOUR PROJECT ID> with your project ID. You can find your project id by navigating to **Project settings** from the menu in the top right hand of the Google Actions Console.

![Project settings menu](/libraries/Bot.Builder.Community.Adapters.ActionsSDK/media/actions-sdk-select-project-settings.PNG?raw=true)

4. Open a command prompt and change the directory to the botbuilder-community-dotnet/libraries/Bot.Builder.Community.Adapters.ActionsSDK/action/ directory.

5. Run 'gactions login' to login and follow the instructions to login to your account.

6. Run 'gactions push' to push local settings to your project.

7. Run 'gactions deploy preview' to deploy your project (including the locally defined scenes etc).

8. Finally, navigate back to your project within the Google Actions Console. You will see that the display name has been updated to 'Bot Framework Sample'. Change this to a display name of your choice.  This will also be your invocation name, which is the name used by users when they invoke your action. 

### Wiring up the Actions SDK adapter in your bot

#### Install the Actions SDK adapter NuGet package

Add  the [Bot.Builder.Community.Adapters.ActionsSDK](https://www.nuget.org/packages/Bot.Builder.Community.Adapters.ActionsSDK/) NuGet package. For more information on using NuGet, see [Install and manage packages in Visual Studio](https://aka.ms/install-manage-packages-vs)

#### Create a Google adapter class

Create a new class that inherits from the ***ActionsSdkAdapter*** class. This class will act as our adapter for our action. It includes error handling capabilities (much like the ***BotFrameworkAdapterWithErrorHandler*** class already in the sample, used for handling requests from Azure Bot Service).  

```csharp
    public class ActionsSdkAdapterWithErrorHandler : ActionsSdkAdapter
    {
        public ActionsSdkAdapterWithErrorHandler(ILogger<ActionsSdkAdapter> logger, ActionsSdkAdapterOptions adapterOptions)
            : base(adapterOptions, logger)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError($"Exception caught : {exception.Message}");

                // Send a catch-all apology to the user.
                await turnContext.SendActivityAsync("Sorry, it looks like something went wrong.");
            };
        }
    }
```

You will also need to add the following using statements.

```cs
using Bot.Builder.Community.Adapters.ActionsSDK;
using Microsoft.Extensions.Logging;
```

#### Create a new controller for handling requests from your Actions SDK action

You now need to create a new controller which will handle requests from your action, on a new endpoing 'api/actionssdk' instead of the default 'api/messages' used for requests from Azure Bot Service Channels.  By adding an additional endpoint to your bot, you can accept requests from Bot Service channels (or additional adapters), as well as from the Actions SDK, using the same bot.

```csharp
[Route("api/actionssdk")]
[ApiController]
public class ActionsSdkController : ControllerBase
{
    private readonly ActionsSdkAdapter Adapter;
    private readonly IBot Bot;

    public ActionsSdkController(ActionsSdkAdapter adapter, IBot bot)
    { 
        Adapter = adapter;
        Bot = bot;
    }

    [HttpPost]
    public async Task PostAsync()
    {
        // Delegate the processing of the HTTP POST to the adapter.
        // The adapter will invoke the bot.
        await Adapter.ProcessAsync(Request, Response, Bot);
    }
}
```

You will also need to add the following using statements.

```cs
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.ActionsSDK;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
```

#### Inject Actions SDK Adapter and Actions SDK Adapter Options In Your Bot Startup.cs

1. Add the following code into the ***ConfigureServices*** method within your Startup.cs file, which will register your Google adapter and make it available for your new controller class. We will also create and register a ***ActionsSdkAdapterOptions*** class, which will contain necessary information for your adapter to function correctly.  

You need to set the following properties 

* **ActionInvocationName** - replace "YOUR-ACTION-DISPLAY-NAME" with the display name you gave to your action. 

* **ActionProjectId** - The ID of your **Actions SDK** project.

```csharp
    // Create the Actions Sdk Adapter
    services.AddSingleton<ActionsSdkAdapter, ActionsSdkAdapterWithErrorHandler>();

    // Create ActionsSdkAdapterOptions
    services.AddSingleton(sp =>
    {
        return new ActionsSdkAdapterOptions()
        {
            ActionInvocationName = "YOUR-ACTION-DISPLAY-NAME",
            ActionProjectId = "",
            ShouldEndSessionByDefault = false
        };
    });
```

2. Once added, your ***ConfigureServices*** method shold look like this.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

    // Create the default Bot Framework Adapter (used for Azure Bot Service channels and emulator).
    services.AddSingleton<IBotFrameworkHttpAdapter, BotFrameworkAdapterWithErrorHandler>();

    // Create the Actions Sdk Adapter
    services.AddSingleton<ActionsSdkAdapter, ActionsSdkAdapterWithErrorHandler>();

    // Create ActionsSdkAdapterOptions
    services.AddSingleton(sp =>
    {
        return new ActionsSdkAdapterOptions()
        {
            ActionInvocationName = "YOUR-ACTION-DISPLAY-NAME",
            ActionProjectId = "",
        };
    });

    // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
    services.AddTransient<IBot, EchoBot>();
}
```

3. You will also need to add the following using statement, in addition to those already present in the startup.cs file.

```cs
using Bot.Builder.Community.Adapters.ActionsSdk;
```
### Complete configuration of your Actions SDK project

Now that you have wired up the adapter in your bot project, the final steps are to configure the endpoint, for your action, to which requests will be posted to when your action is invoked, pointing it to the correct endpoint on your bot.

To complete this step, [deploy your bot to Azure](https://aka.ms/bot-builder-deploy-az-cli) and make a note of the URL to your deployed bot. Your Actions SDK messaging endpoint is the URL for your bot, which will be the URL of your deployed application (or ngrok endpoint), plus '/api/actionssdk' (for example, `https://yourbotapp.azurewebsites.net/api/actionssdk`).

> [!NOTE]
> If you are not ready to deploy your bot to Azure, or wish to debug your bot when using the Actions SDK adapter, you can use a tool such as [ngrok](https://www.ngrok.com) (which you will likely already have installed if you have used the Bot Framework emulator previously) to tunnel through to your bot running locally and provide you with a publicly accessible URL for this. 
> 
> If you wish create an ngrok tunnel and obtain a URL to your bot, use the following command in a terminal window (this assumes your local bot is running on port 3978, alter the port numbers in the command if your bot is not).
> 
> ```
> ngrok.exe http 3978 -host-header="localhost:3978"
> ```

#### Complete configuration for Actions on Google

1. Navigate back to the [Google Actions console](https://console.actions.google.com/) and navigate to your action project.

2. Navigate to the **Develop** tab and then to the **Webhook** page on the left hand menu.

3. Enter your Actions SDK endpoint, described above, as the HTTPS endpoint for your webhook (the default value set by the template files is 'https://www.urltoyourbot.com/api/actionssdk/').

![Set your webhook endpoint](/libraries/Bot.Builder.Community.Adapters.ActionsSDK/media/actions-sdk-set-webhook.PNG?raw=true)

### Testing your action

You can now test interacting with your action using the simulator. 

1. Navigate to https://console.actions.google.com/ and navigate to your action project.

2. In the action dashboard navigate to the **Test** tab at the top of the page.

3. To perform a basic test enter "ask <ACTION DISPLAY NAME> hello world" into the simulator input box. For example, if your action display name was 'Bot Framework Sample', you would type 'Talk to Bot Framework Sample hello world'. This should return an echo of your message.

![Simulator](/libraries/Bot.Builder.Community.Adapters.Google/media/simulator-test.PNG?raw=true)

Now that you have enabled testing for your action, you can also test your action using a physical Google assistant device or using Google assistant on an Android device. Providing you are logged into the device with the same account used to login to the Actions on Google Console (or an account that you have added as a beta tester for your action within the console).

### Incoming action requests to Bot Framework activity mapping

The Actions SDK service can send your bot a number of different request types.

* By default, incoming requests take the query text (the text / typed spoken by the user) and convert them to a Bot Framework Message Activity.

* **Explicit invocation -> ConversationUpdateActivity**
If a user invokes your action without specifying an intent / further query (e.g. OK Google, talk to Bot Framework Sample - ***where 'Bot Framework Sample' is your action display / invocation name***), this will be to your bot as a ConversationUpdateActivity. This is mirrors the default functionality on Azure bot Service channels when a user starts a conversation.

* **actions.intent.cancel -> Event Activity**
If the user explicitly cancels their conversation, then your action will send a cancel intent, which is transformed into an EndOfConversation activity.  You do not need to explicitly handle this activity type, as the adapter will automatically end the conversation correctly. However, should you wish to send a closing message to the user when they cancel, you can respond to this activity type.

* **Account Linking Events**
Please see the [account linking](#account-linking) details below for more details on how to manage Account Linking and respond to specific Account Linking events. 

### Customising your conversation

#### Controlling the end of a session

By default, the Actions SDK adapter is configured to close the session following sending a response. You can explicitly indicate that your action should wait for the user to say something else, meaning the microphone should be left open and listen for further input, by sending an input hint of ***ExpectingInput*** on your outgoing activity.

```cs
await turnContext.SendActivityAsync("Your message text", inputHint: InputHints.ExpectingInput);
```

You can alter the default behavior to leave the session open and listen for further input by default by setting the ***ShouldEndSessionByDefault*** setting on the ***ActionsSdkAdapterOptions*** class within your startup.cs class.

```csharp
    // Create ActionsSdkAdapterOptions
    services.AddSingleton(sp =>
    {
        return new ActionsSdkAdapterOptions()
        {
            ShouldEndSessionByDefault = false,
            ActionInvocationName = "YOUR-ACTION-DISPLAY-NAME",
            ActionProjectId = "YOUR-PROJECT-ID"
        };
    });
```

If you do set ***ShouldEndSessionByDefault*** to false, then you need to explicity end the conversation when you are ready, by sending an input hint of ***IgnoringInput*** on your last outgoing activity.

```cs
await turnContext.SendActivityAsync("Your message text", inputHint: InputHints.IgnoringInput);
```

#### Handling multiple outgoing activities

Your action expects a single response to each request that is sent to your bot. However, it is not uncommon for a bot to send multiple activities back in response to a request.  This can cause issues, especially if you are using Google alongside other channels or adapters.

To combat this issue the adapter will automatically concatenate multiple activities into a single activity, combining the Speak and Text properties of the activities.

#### Automatic conversion of native Bot Framework card attachments

* A Bot Framework HeroCard attached to an outgoing activity, will be automatically transformed into an Actions SDK Card.

* If you have enabled Account Linking for your action, then sending a Bot Framework Signin card on your outgoing activity will trigger the Account Linking flow.

#### Explicitly sending a Google card as part of your response

You can include a Google card in your response, which is shown on devices that have a screen.  To do this you include an attachment on your outgoing activity, using the ***ContentItemFactory*** class.  For more information about cards see https://developers.google.com/assistant/conversational/prompts-rich

```cs
    var activityWithCard = MessageFactory.Text($"Ok, I included a card.");
    var card = ContentItemFactory.CreateCard("card title", "card subtitle", new Link()
    {
        Name = "Microsoft",
        Open = new OpenUrl() { Url = "https://www.microsoft.com" }
    });
    activityWithCard.Attachments.Add(card.ToAttachment());
    await turnContext.SendActivityAsync(activityWithCard, cancellationToken);
```

#### Providing a user an interactive list or collection

The following code shows how to provide a user with a list or collection (More information about Lists can be found [here](https://developers.google.com/assistant/conversational/prompts-selection). 

You can use the ***ContentItemFactory*** class, the output of which can then be converted into an attachment and attached to your outgoing activity. 

Below shows an example of sending a list. The steps to send a collection are very similar, but instead of using the **CreateList** method, you would use **CreateCollection**.

```cs

    var items = new List<ListItem>()
    {
        new ListItem()
        {
            Key = "ITEM_1",
            Synonyms = new List<string>() { "Item 1", "First item" },
            Item = new EntryDisplay()
            {
                Title = "Item #1",
                Description = "Description of Item #1",
                Image = new Image()
                {
                    Url = "https://developers.google.com/assistant/assistant_96.png",
                    Height = 0,
                    Width = 0,
                    Alt = "Google Assistant logo"
                }
            }
        },
        new ListItem()
        {
            Key = "ITEM_2",
            Synonyms = new List<string>() { "Item 2", "Second item" },
            Item = new EntryDisplay()
            {
                Title = "Item #2",
                Description = "Description of Item #2",
                Image = new Image()
                {
                    Url = "https://developers.google.com/assistant/assistant_96.png",
                    Height = 0,
                    Width = 0,
                    Alt = "Google Assistant logo"
                }
            }
        }
    }

    var activityWithListAttachment = MessageFactory.Text($"This is a list.");
    var list = ContentItemFactory.CreateList(
                items,
                title: "InternalList title",
                subtitle: "InternalList subtitle");
    activityWithListAttachment.Attachments.Add(list.ToAttachment());
    await turnContext.SendActivityAsync(activityWithListAttachment, cancellationToken);
```

#### Sending a table card

You can include information formatted into a table using the Table Card. See https://developers.google.com/assistant/conversational/prompts-rich for more information about Table Cards.

```cs
var activityWithTableCardAttachment = MessageFactory.Text($"Ok, I included a table card.");

var table = ContentItemFactory.CreateTable(
    new List<TableColumn>()
    {
        new TableColumn() { Header = "Column 1" },
        new TableColumn() { Header = "Column 2" }
    },
    new List<TableRow>()
    {
        new TableRow() 
        {
            Cells = new List<TableCell>
            {
                new TableCell { Text = "Row 1, Item 1" },
                new TableCell { Text = "Row 1, Item 2" }
            }
        },
        new TableRow() {
            Cells = new List<TableCell>
            {
                new TableCell { Text = "Row 2, Item 1" },
                new TableCell { Text = "Row 2, Item 2" }
            }
        }
    },
    "Table Card Title",
    "Table card subtitle",
    new Link { Name = "Microsoft", Open = new OpenUrl() { Url = "https://www.microsoft.com" } });
    
activityWithTableCardAttachment.Attachments.Add(table.ToAttachment());
await turnContext.SendActivityAsync(activityWithTableCardAttachment, cancellationToken);
```

### Account Linking

The action template files, included within this repo, already have appropriate scenes for account linking and account linking will be enabled by default.  If you do not wish to use account linking, simply navigate to the **Account Linking** page within your project dashbaord in the Google Actions console and disable the toggle in order to disable account linking.  On this page you can also set appropriate settings to determine if account linking uses only Google Sign-In or also enable an additional OAuth endpoint.

More details about configuring account linking can be found at https://developers.google.com/assistant/identity.

#### Triggering the Account Linking sign-in flow

In order to prompt a user to sign in to your action, you should first check to ensure they have not already linked their account (using the channel data on the incoming activity).  If they have not linked their account, then you can send a Bot Framework Sign-in card on your outgoing activity, which will be transformed into the appropriate response to the Actions SDK to trigger the flow.

```cs
    var channelData = (ActionsSdkRequest)turnContext.Activity.ChannelData;

    if (channelData.User.AccountLinkingStatus == "LINKED")
    {
        await turnContext.SendActivityAsync("You're already signed in!", cancellationToken: cancellationToken);
    }
    else
    {
        var activityWithSigninCard = MessageFactory.Text($"Ok, I included a signin card.");
        var signinCard = new SigninCard();
        activityWithSigninCard.Attachments.Add(signinCard.ToAttachment());
        await turnContext.SendActivityAsync(activityWithSigninCard, cancellationToken);
    }
```

Once a user completes the account linking flow, either sucessfully or unsucessfully, your bot will receieve an EventActivity with the name 'AccountLinking', along with a value of either 'Completed', 'Cancelled' or 'Error', to altert you to the status.




















