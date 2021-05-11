# Zoom Adapter Component for Bot Framework Composer / Bot Framework SDK

## Description

This is part of the [Bot Builder Community](https://github.com/botbuildercommunity) project which contains Bot Framework Components and other projects / packages for use with Bot Framework Composer and the Bot Builder .NET SDK v4.

Incoming Zoom app requests are transformed, by the adapter, into Bot Framework Activties and then when your bot sends outgoing activities, the adapter transforms the outgoing Activity into an appropriate Zoom response.

The adapter currently supports the following scenarios;

- Automatically transform incoming chatbot app messages into Bot Framework Message activities
- Supports sending of all common Zoom chatbot message templates
- Transforms incoming interactive message events (such as use interacting with dropdown / editable fields) into Event activites with strongly typed payload objects
- Handles all incoming events from Zoom (that the app has been subscribed to) and transforms into Bot Framework Event activities

*This readme focuses on consuming the Zoom component in [Bot Framework Composer](https://docs.microsoft.com/en-us/composer/introduction). For more information about the supported scenarios and how to consume the Zoom adapter in code-first scenarios, visit the [Zoom adapter readme](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/blob/develop/libraries/Bot.Builder.Community.Adapters.Zoom/README.md).*

## Usage

- [Prerequisites](#Prerequisites)
- [Component Installation](#Component-installation-via-Composer-Package-Manager)
- [Get your bots Zoom endpoint](#Get-your-bots-Zoom-endpoint)
- [Create a Zoom app](#Create-a-Zoom-app)
- [Configure the Zoom connection in Composer](#Configuring-the-Zoom-connection-in-Bot-Framework-Composer)
- [Install and Test your Zoom app](#Install-and-Test-your-Zoom-app)

## Prerequisites

- [Bot Framework Composer](https://dev.botframework.com/)

- Access to the Zoom Developer Console with sufficient permissions to login to create / manage apps at  [https://marketplace.zoom.us/develop](https://marketplace.zoom.us/develop). If you do not have this you can create an account for free.


## Component installation via Composer Package Manager

1. Go to Package Manager (in the left hand navigation within Composer).

2. Within in Package Manager, search for an install the latest version of Bot.Builder.Community.Components.Adapters.Zoom.

## Get your bots Zoom endpoint

In order to sucessfully configure Zoom to send requests to your bot, you are required to provide it with you bot's Zoom endpoint. To do this deploy your bot to Azure and make a note of the URL to your deployed bot. Your Zoom messaging endpoint is the URL for your bot, which will be the URL of your deployed application (or ngrok endpoint), plus '/api/zoom' (for example, `https://yourbotapp.azurewebsites.net/api/zoom`).

> [!NOTE]
> If you are not ready to deploy your bot to Azure, or wish to debug your bot when using the Alexa adapter, you can use a tool such as [ngrok](https://www.ngrok.com) (which you will likely already have installed if you have used the Bot Framework emulator previously) to tunnel through to your bot running locally and provide you with a publicly accessible URL for this. 
> 
> If you wish create an ngrok tunnel and obtain a URL to your bot, use the following command in a terminal window (this assumes your local bot is running on port 3978, alter the port numbers in the command if your bot is not).
> 
> ```
> ngrok.exe http 3978 -host-header="localhost:3978"
> ```
>

## Create a Zoom app

1. Log into the [Zoom Developer Console](https://marketplace.zoom.us/develop) or create ana ccount, and then click the **Create** button next to the Chatbot app type. In the popup presented, choose a name for your chatbot and then click **Create**.

2. On the next screen you will be presented with the **Client ID** and **Client Secret** for your new app. Copy and make a note of these, as you will need them later when configuring your bot application. We'll fill the **Redirect URL for OAuth** and **Whitelist URL** later in the process.

3. Click the **Scopes** section in the left hand menu. Click the **+Add Scopes** button and search for 'Chat'. Select the scope **Enable Chatbot within Zoom Chat Client** (**imchat:bot**) and click **Done**.

4. Click the **Feature** section in the left hand menu. Set the endpoint URL to where your bot's Zoom endpoint (see [Get your bots Zoom endpoint](#Get-your-bots-Zoom-endpoint)). Click **Save**

5. Once the URLs are saved, two pieces of information should appear above: the **Verification token** and **Bot JID**. Copy and make a note of these values since along with the **Client ID** and **Client secret** of step 1 will be used to configure the new Zoom connection in Bot Framework Composer.

6. Click back into the **App credentials** section in the left hand menu. Update the **Redirect URL for OAuth** and **Whitelist URL** to the value `https://zoom.us/launch/chat?jid=<Your Bot JID from step 5>`, replacing the last part with your bot JID. Example: `https://zoom.us/launch/chat?jid=robot_v3ynierrdntw-yfsdfsm1a9g@xmpp.zoom.us`.

## Configuring the Zoom connection in Bot Framework Composer

1. In Composer, go to your project settings. Within the 'Connections' tab, there should be a new entry called `Zoom connection`. Select `Configure` to configure the new connection.

![image](https://user-images.githubusercontent.com/3900649/114547215-0e8a5780-9c56-11eb-9add-bfd7c39a4046.png)

2. A modal will pop up. Fill the fields with the values from steps 2 and 5 from the preiovus section.

![Configured adapter modal](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/alexa-component-2-adapter-configured.PNG?raw=true)

3. Once you close the modal, your connection should appear as configured in the bot settings.

![Configured adapter](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/alexa-component-3-success-bot.PNG?raw=true)

## Install and Test your Zoom app

You can now test interacting with your Zoom app using the Zoom client.

1. In the Zoom developer dashboard, click the **Local Test** section link on the left hand menu.

2. Click the **Install** button to install yur Zoom app locally.

3. You can now login to the Zoom client and interact with your bot via chat.
