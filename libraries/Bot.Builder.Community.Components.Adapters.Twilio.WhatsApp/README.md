# Twilio WhatsApp Adapter for Bot Builder v4 .NET SDK

## Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [Available via NuGet](https://www.nuget.org/packages/Bot.Builder.Community.Adapters.Twilio.WhatsApp/) |

## Description

This is part of the [Bot Builder Community](https://github.com/botbuildercommunity) project which contains Bot Framework Components and other projects / packages for use with Bot Framework Composer and the Bot Builder .NET SDK v4.

The Twilio WhatsApp adapter for the Microsoft Bot Framework allows you to add an additional endpoint to your bot for use with Twilio WhatsApp. The goal of this adapter is to support WhatsApp via Twilio as seamlessly as possible. All supported features on WhatsApp are mapped to the default Bot Framework SDK.

>To send messages with WhatsApp in production, you have to wait for WhatsApp to formally approve your account. But, that doesn't mean you have to wait to start building. [Twilio Sandbox for WhatsApp](https://www.twilio.com/console/messaging/whatsapp) lets you test your app in a developer environment.

This adapter supports the limited capabilities of Twilio WhatsApp, including;

* Send and receive text messages
* Send and receive text messages with attachments (`image`, `audio`, `video`, `document`, `location`)
* Send proactive notifications
* Track message deliveries (`sent`, `delivered` and `read` receipts)

## Status
__Currently the Twilio WhatsApp channel is in beta.__
>Products in Beta may occasionally fall short of speed or performance benchmarks, have gaps in functionality, and contain bugs.

>APIs are stable and unlikely to change as the product moves from Beta to Generally Available (GA). We will do our best to minimize any changes and their impact on your applications. Products in Beta are not covered by Twilio's SLA's and are not recommended for production applications.


## Installation

Available via NuGet package [Bot.Builder.Community.Adapters.Twilio.WhatsApp](https://www.nuget.org/packages/Bot.Builder.Community.Adapters.Twilio.WhatsApp/).

Install into your project use the following command in the package manager. 

```
    PM> Install-Package Bot.Builder.Community.Adapters.Twilio.WhatsApp
```


## Usage

1. Use your existing Twilio account or create a new Twilio account.
2. Go to the [Twilio Sandbox for WhatsApp](https://www.twilio.com/console/messaging/whatsapp) and follow the first steps.
3. At the `Configure your Sandbox` step, add your endpoint URLs. Those URLs will be defined by the snippet below, by default the URL will be `[your-bot-url]/api/whatsapp/messages`.
_The `status callback url` is optional and should only be used if you want to track deliveries of your messages._
4. Go to your [Dashboard](https://www.twilio.com/console/sms/dashboard) and click on `Show API Credentials`.
5. Implement the snippet below and add your Account SID, Auth Token, your phone number and the endpoint URL you configured in the sandbox.
6. Give it a try! Your existing bot should be able to operate on the WhatsApp channel via Twilio.


## Component installation via Composer Package Manager

1. Go to Package Manager (in the left hand navigation within Composer).

2. Within in Package Manager, search for an install the latest version of Bot.Builder.Community.Adapters.Twilio.WhatsApp.

## Configure the Twilio whatsapp connection in Composer

Before you can complete the configuration of your Twilio WhatsApp , you need to wire up the Twilio whatsapp adapter into your bot in Bot Framework Composer.

1. In Composer, go to your project settings. Within the 'Connections' tab, there should be a new entry called `Twilio WhatsApp Adapter Connection`. Select `Configure` to wire up your skill.

2. A modal will pop up. Fill the all the required fields information

3. Once you close the modal, your adapter should appear as configured in the bot settings.

## Complete configuration of your Twilio whatsapp

Now that you have created an Twilio whatsapp and wired up the adapter in your bot project, the final steps are to configure the endpoint to which requests will be posted to when your Twilio whatsapp is invoked, pointing it to the correct endpoint on your bot.

1. To complete this step, [deploy your bot to Azure](https://aka.ms/bot-builder-deploy-az-cli) and make a note of the URL to your deployed bot. Your Alexa messaging endpoint is the URL for your bot, which will be the URL of your deployed application (or ngrok endpoint), plus '/api/alexa' (for example, `https://yourbotapp.azurewebsites.net/api/whatsapp`).

> [!NOTE]
> If you are not ready to deploy your bot to Azure, or wish to debug your bot when using the Alexa adapter, you can use a tool such as [ngrok](https://www.ngrok.com) (which you will likely already have installed if you have used the Bot Framework emulator previously) to tunnel through to your bot running locally and provide you with a publicly accessible URL for this. 
> 
> If you wish create an ngrok tunnel and obtain a URL to your bot, use the following command in a terminal window (this assumes your local bot is running on port 3978, alter the port numbers in the command if your bot is not).
> 
> ```
> ngrok.exe http 3978 -host-header="localhost:3978"
> ```

2. Back within your Alexa skill dashboard, navigate to the **Endpoint** section on the left hand menu.  Select **HTTPS** as the **Service Endpoint Type** and set the **Default Region** endpoint to your bot's Alexa endpoint, such as https://yourbotapp.azurewebsites.net/api/whatsapp.

## Test your Twilio WhatsApp





