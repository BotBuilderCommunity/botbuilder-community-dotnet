# Zoom Adapter for Bot Builder v4 .NET SDK

## Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [Available via NuGet](https://www.nuget.org/packages/Bot.Builder.Community.Adapters.Zoom/) |

## Description

This is part of the [Bot Builder Community Extensions](https://github.com/botbuildercommunity) project which contains various pieces of middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

The Zoom Adapter allows you to add an additional endpoint to your bot for Zoom apps. The Zoom endpoint can be used
in conjunction with other channels meaning, for example, you can have a bot exposed on out of the box channels such as Facebook and 
Teams, but also via a Zoom app, such as a chatbot (as well as side by side with the Google / Twitter Adapters also available from the Bot Builder Community Project).

Incoming Zoom app requests are transformed, by the adapter, into Bot Framework Activties and then when your bot sends outgoing activities, the adapter transforms the outgoing Activity into an appropriate Zoom response.

The adapter currently supports the following scenarios;

* Automatically transform incoming chatbot app messages into Bot Framework Message activities
* Supports sending of all common Zoom chatbot message templates
* Transforms incoming interactive message events (such as use interacting with dropdown / editable fields) into Event activites with strongly typed payload objects
* Handles all incoming events from Zoom (that the app has been subscribed to) and transforms into Bot Framework Event activities

## Installation

Available via NuGet package [Bot.Builder.Community.Adapters.Zoom](https://www.nuget.org/packages/Bot.Builder.Community.Adapters.Zoom/).

Install into your project use the following command in the package manager. 

```
    PM> Install-Package Bot.Builder.Community.Adapters.Zoom
```

## Sample

Sample bots showing examples of Zoom specific functionality are available [here](../../samples/Zoom%20Adapter%20Sample) and [here](../../samples/Teams%20Zoom%20Sample).

## Usage

* [Prerequisites](#prerequisites)
* [Create a Zoom chatbot app](#create-a-zoom-app)
* [Wiring up the Zoom adapter in your bot](#wiring-up-the-zoom-adapter-in-your-bot)
* [Complete configuration of your Zoom app](#complete-configuration-of-your-zoom-app)
* [Populate your Application Settings](#configure-your-bot-application-settings-appsettingsjson)
* [Install and Test your Zoom app](#install-and-test-your-zoom-app)
* [Incoming Zoom request to Bot Framework activity mapping](#Incoming-Zoom-request-to-Bot-Framework-activity-mapping) - Learn how incoming request types are handled by the adapter and the activities receieved by your bot.
* [Sending Zoom Message Templates](#sending-zoom-message-templates) - Sending Zoom chat messages using Zoom message templates

The following sections will demonstrate how to connect a bot to an Alexa skill using the Alexa adapter, by walking you through modifying the EchoBot sample to connect it to an Alexa skill.

### Prerequisites

* The [EchoBot sample code](https://github.com/microsoft/BotBuilder-Samples/tree/master/samples/csharp_dotnetcore/02.echo-bot)

* Access to the Zoom Developer Console with sufficient permissions to login to create / manage apps at  [https://marketplace.zoom.us/develop](https://marketplace.zoom.us/develop). If you do not have this you can create an account for free.

### Create a Zoom app

1. Log into the [Zoom Developer Console](https://marketplace.zoom.us/develop) and then click the **Create** button next to the Chatbot app type.  In the popup presented, choose a name for your chatbot and then click **Create**.

2. On the next screen you will be presented with the **Client ID** and **Client Secret** for your new app.  Copy and make a note of these, as you will need them later when configuring your bot application. 

3. Click the **Scopes** section in the left hand menu. Click the **+Add Scopes** button and search for 'Chat'. Select the scope **Enable Chatbot within Zoom Chat Client** (**imchat:bot**) and click **Done**.

### Wiring up the Zoom adapter in your bot

Before you can complete the configuration of your Zoom app, you need to wire up the Zoom adapter into your bot.

#### Install the Zoom adapter NuGet packages

Available via NuGet package [Bot.Builder.Community.Adapters.Zoom](https://www.nuget.org/packages/Bot.Builder.Community.Adapters.Zoom/).

Install into your project use the following command in the package manager. 

```
    PM> Install-Package Bot.Builder.Community.Adapters.Zoom
```

#### Create a Zoom adapter class

Create a new class that inherits from the ***ZoomAdapter*** class. This class will act as our adapter for Zoom. It includes error handling capabilities (much like the ***BotFrameworkAdapterWithErrorHandler*** class already in the sample, used for handling requests from Azure Bot Service).  

The adapter also reads Zoom congifuration values from our appSettings.json file and supplies them to the adapter via the ***ZoomAdapterOptions*** class.

```csharp
    public class ZoomAdapterWithErrorHandler : ZoomAdapter
    {
        public ZoomAdapterWithErrorHandler(IConfiguration configuration, ILogger<ZoomAdapter> logger)
                : base(new ZoomAdapterOptions()
                {
                    ValidateIncomingZoomRequests = false,
                    ClientId = configuration["ZoomClientId"],
                    ClientSecret = configuration["ZoomClientSecret"],
                    BotJid = configuration["ZoomBotJid"],
                    VerificationToken = configuration["ZoomVerificationToken"]
                }, logger)
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
using Bot.Builder.Community.Adapters.Zoom;
using Microsoft.Extensions.Logging;
```

#### Create a new controller for handling Zoom requests

You now need to create a new controller which will handle requests from your Zoom app, on a new endpoing 'api/zoom' instead of the default 'api/messages' used for requests from Azure Bot Service Channels.  By adding an additional endpoint to your bot, you can accept requests from Bot Service channels (or additional adapters), as well as from Zoom, using the same bot.

```csharp
[Route("api/zoom")]
[ApiController]
public class ZoomController : ControllerBase
{
    private readonly ZoomAdapter _adapter;
    private readonly IBot _bot;

    public ZoomController(ZoomAdapter adapter, IBot bot)
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
using Bot.Builder.Community.Adapters.Zoom;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
```

#### Inject Zoom Adapter In Your Bot Startup.cs

Add the following line into the ***ConfigureServices*** method within your Startup.cs file, which will register your Zoom adapter and make it available for your new controller class.  The configuration settings, described in the next step, will be automatically used by the adapter.

```csharp
services.AddSingleton<ZoomAdapter, ZoomAdapterWithErrorHandler>();
```

Once added, your ***ConfigureServices*** method shold look like this.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

    // Create the default Bot Framework Adapter (used for Azure Bot Service channels and emulator).
    services.AddSingleton<IBotFrameworkHttpAdapter, BotFrameworkAdapterWithErrorHandler>();

    // Create the Zoom Adapter
    services.AddSingleton<ZoomAdapter, ZoomAdapterWithErrorHandler>();

    // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
    services.AddTransient<IBot, EchoBot>();
}
```

You will also need to add the following using statement, in addition to those already present in the startup.cs file.

```cs
using Bot.Builder.Community.Adapters.Zoom;
```

### Complete configuration of your Zoom app

Now that you have created a Zoom app and wired up the adapter in your bot project, the final steps are to configure the endpoint to which requests will be posted to when your Zoom app is invoked, pointing it to the correct endpoint on your bot.

1. To complete this step, [deploy your bot to Azure](https://aka.ms/bot-builder-deploy-az-cli) and make a note of the URL to your deployed bot. Your Zoom endpoint is the URL for your bot, which will be the URL of your deployed application (or ngrok endpoint), plus '/api/zoom' (for example, `https://yourbotapp.azurewebsites.net/api/zoom`).

> [!NOTE]
> If you are not ready to deploy your bot to Azure, or wish to debug your bot when using the Zoom adapter, you can use a tool such as [ngrok](https://www.ngrok.com) (which you will likely already have installed if you have used the Bot Framework emulator previously) to tunnel through to your bot running locally and provide you with a publicly accessible URL for this. 
> 
> If you wish create an ngrok tunnel and obtain a URL to your bot, use the following command in a terminal window (this assumes your local bot is running on port 3978, alter the port numbers in the command if your bot is not).
> 
> ```
> ngrok.exe http 3978 -host-header="localhost:3978"
> ```

2. Back within your Zoom app developer dashboard, navigate to the **Features** section on the left hand menu.  On this page you are required to provide endpoints for both development and production. At this stage you can use your bot's Zoom endpoint, such as https://yourbotapp.azurewebsites.net/api/zoom. You will be required to change the Production endpoint later before you publish your app. Once you have completed your endpoints, click **Save**.

3. Once you endpoints have been saved, the Zoom developer dashboard will generate and display a **Verification Token** and two **Bot JID** values (one for development and one for production). Copy these values as you will need them to complete the configuration of your bot application.

### Configure your bot application settings (appSettings.json)

Using the values you saved earlier for Client ID, Client Secret, Verification Token and Bot JID (use the development Bot JID for development / testing), populate these values into the appSettings.json file within your bot application.

```json
{
  "MicrosoftAppId": "",
  "MicrosoftAppPassword": "",
  "ZoomClientId": "",
  "ZoomClientSecret": "",
  "ZoomBotJid": "",
  "ZoomVerificationToken": ""
}
```

### Install and Test your Zoom app

You can now test interacting with your Zoom app using the Zoom client. 

1. In the Zoom developer dashboard, click the **Local Test** section link on the left hand menu.

2. Click the **Install** button to install yur Zoom app locally.

3. You can now login to the Zoom client and interact with your bot via chat.


### Incoming Zoom request to Bot Framework activity mapping

A Zoom app can send a large number of events. The adapter has been developed to handle some specific event types and transform them into the Bot Framework activity types (including some Zoom specific strongly typed objects), but it will also handle any event sent by Zoom so that you can access any event payload within your bot application.

Here is how the adapter handles different event types.

* **Bot Notification Events -> Message Activity**
When a user sends a chat message to your Zoom app, Zoom sends an event of type "bot_notification".  In this case the adapter will transform this into a Bot Framework Message activity, using the Text property on the incoming event to set the Text property on the activity.

* **Bot Action (Buttons) Events -> Message Activity**
If you send a message from your bot application that contains one or more Suggested Actions, the adapter use the Zoom Actions message template to display buttons to a user.  If a user clicks one of these buttons, then the incoming event (***interactive_message_actions***) is transformed into a Message activity, using the ***DisplayText*** property of the button to set the Text value on the Message Activity.

An example of sending the suggested actions and how this is displayed.

```cs
var message = MessageFactory.SuggestedActions(
    new List<CardAction>() {
        new CardAction(text: "This is the text", displayText: "display text"),
        new CardAction(text: "This is the text for action 2", displayText: "action 2 display text")
    },
    "Here are some actions.");
    
await turnContext.SendActivityAsync(message, cancellationToken);
```

![Actions](/libraries/Bot.Builder.Community.Adapters.Zoom/media/message-actions.PNG?raw=true)

* **Dropdown / Editable Field Interactive Message Events -> Event Activities with strongly typed objects**
You use the Zoom specific attachments made available with the adapter to send interactive messages to a user containing a dropdown list or one or more editable fields. If a user changes the value of editable fields, or changes the selected value in a dropdown list, the adapter will send your bot an Event activity. The event activity will have a name of ***interactive_message_fields_editable*** and a value of type ***InteractiveMessageFieldsEditablePayload*** if a user changes the value of an editable field. If a user changes the selected value in a dropdown, the resulting event activity will have a name of ***interactive_message_select*** and a value of type ***InteractiveMessageSelectPayload***.

* **All other event types -> Event Activity**
All event types, not explicity mentioned above, will be sent to your bot as an Event activity. The name property on the Event activity will be set to the type of the Zoom event type, with the value set to the full event payload as a JObject.

### Sending Zoom Message Templates

Zoom chatbot apps can use a number of different message templates to send rich messages to a user in chat.  You can take advantage of these message templates and send rich messages to a user using Zoom specific attachments made available with the adapter.

Examples of sending rich messages using each of these can be seen below.

* **Fields (editable or non-editable)**

![Fields message](/libraries/Bot.Builder.Community.Adapters.Zoom/media/fields-message.PNG?raw=true)

```cs
var fieldsMessage = MessageFactory.Text("This is the text");

fieldsMessage.Attachments.Add(new FieldsBodyItem()
{
    Fields = new List<ZoomField>()
    {
        new ZoomField() { Key="item1", Value = "value 1", Editable = true},
        new ZoomField() { Key="item2", Value = "value 2", Editable = true},
    }
}.ToAttachment());

await turnContext.SendActivityAsync(fieldsMessage, cancellationToken);
```

* **Attachment**

![Attachment message](/libraries/Bot.Builder.Community.Adapters.Zoom/media/attachment-message.PNG?raw=true)

```cs
var messageWithAttachment = MessageFactory.Text("Here is an attachment.");
                    
messageWithAttachment.Attachments.Add(new AttachmentBodyItem()
{
    ImageUrl = new Uri("https://s3.amazonaws.com/user-content.stoplight.io/19808/1560465782721"),
    Size = 250,
    Information = new ZoomAttachmentInfo()
    {
        Description = new ZoomAttachmentInfoContent() { Text = "This is a description" },
        Title = new ZoomAttachmentInfoContent() { Text = "This is a title" }
    },
    Ext = FileExtensions.jpeg,
    ResourceUrl = new Uri("https://www.microsoft.com")
}.ToAttachment());
                    
await turnContext.SendActivityAsync(messageWithAttachment, cancellationToken);
```

* **Dropdown**

![Dropdown message](/libraries/Bot.Builder.Community.Adapters.Zoom/media/dropdown-message.PNG?raw=true)

```cs
var messageWithDropdown = MessageFactory.Text("Here is a dropdown.");

messageWithDropdown.Attachments.Add(new DropdownBodyItem()
{
    Text = "Visit the Microsoft web site",
    SelectItems = new List<ZoomSelectItem>()
    {
        new ZoomSelectItem() { Text = "Item 1", Value = "item1" },
        new ZoomSelectItem() { Text = "Item 2", Value = "item2" }
    }
}.ToAttachment());

await turnContext.SendActivityAsync(messageWithDropdown, cancellationToken);
```

* **Message with Link**

![Link message](/libraries/Bot.Builder.Community.Adapters.Zoom/media/link-message.PNG?raw=true)

```cs
var messageWithLink = MessageFactory.Text("Here is a link.");

messageWithLink.Attachments.Add(new MessageBodyItemWithLink()
{
    Link = "https://www.microsoft.com",
    Text = "Visit the Microsoft web site"
}.ToAttachment());

await turnContext.SendActivityAsync(messageWithLink, cancellationToken);
```

