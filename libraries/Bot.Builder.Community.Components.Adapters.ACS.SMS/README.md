# Azure Communication Services SMS Adapter Component for Bot Framework Composer / Bot Framework SDK

> **Important!** ACS is in preview, and creation of phone numbers is restricted depending on subscriptions and consumption plans. The phone number creation of this readme is first so that you can confirm whether you are able to create phone numbers in order to proceed with the usage of this adapter. If that feature is not available to your subscription and you don’t already have a Communication Services resource with a phone number, you won’t be able to test this path until the Azure Communication Services team enables it.

## Description

This is part of the [Bot Builder Community Extensions](https://github.com/botbuildercommunity) project which contains various pieces of middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

The Azure Communication Services SMS Adapter allows you to add an additional endpoint to your bot for SMS via Azure Communication Services. The adapter can be used in conjunction with other channels meaning, for example, you can have a bot exposed on out of the box channels such as Facebook and Teams, but also via Azure Communication Services SMS.

Incoming Azure Communication Services SMS events are transformed, by the adapter, into Bot Framework Activties and then when your bot sends outgoing activities, the adapter creates an Azure Communication Services SMS send request to send a reply.

The adapter currently supports the following scenarios;

- SMS received
- SMS send
- SMS delivery reports

This readme focuses on consuming the Azure Communication Services SMS adapter component in [Bot Framework Composer](https://docs.microsoft.com/en-us/composer/introduction). For more information about the supported scenarios and how to consume the adapter in code-first scenarios, visit the [Azure Communication Services adapter readme](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/blob/develop/libraries/Bot.Builder.Community.Adapters.ACS.SMS/README.md).

## Prerequisites

1. Install [Bot Framework Composer](https://dev.botframework.com/). Once Composer is installed, subscribe to the Early Adopters feed from the Composer application settings. Check for updates to install the latest nightly build in order to access the latest features. If you prefer to build locally, clone [Bot Framework Composer](https://github.com/microsoft/BotFramework-Composer) and build locally from the main branch using the instructions in the repository.

2. If you don't already have one, create a new bot using one of the available templates.

3. Go to Package Manager (in the left hand navigation within Composer) and select 'Community Packages' from the dropdown filter.

4. Install the Alexa adapter component in Package Manager by selecting install on the latest version of Bot.Builder.Community.Components.Adapters.ACS.SMS.

5. [Create an Azure Communication Services resource](https://docs.microsoft.com/en-us/azure/communication-services/quickstarts/create-communication-resource?tabs=windows&pivots=platform-azp) and [get a phone number](https://docs.microsoft.com/en-us/azure/communication-services/quickstarts/telephony-sms/get-phone-number).

## Configure the ACS SMS adapter in Composer

Before you can complete the configuration of your ACS SMS skill, you need to wire up the ACS SMS adapter into your bot in Bot Framework Composer.

1. In Composer, go to your bot settings. Under the `adapters` section, there should be a new entry called `Azure Communication Services connection`. Select `Configure`.

![image](https://user-images.githubusercontent.com/3900649/114547215-0e8a5780-9c56-11eb-9add-bfd7c39a4046.png)

2. A modal will pop up. Fill the **Connection string** and **Phone number** with the values obtained in the Azure portal.

3. Once you close the modal, your adapter should appear as configured in the bot settings.

## Complete configuration of your Azure Communication Services resource

Now that you have created an Azure Communication Services resource and configured the connection in your Composer bot, the final steps are to configure the endpoint to which requests from will be posted to when an SMS is sent to your number, pointing it to the correct endpoint on your bot.

1. [Deploy your bot to Azure](https://aka.ms/bot-builder-deploy-az-cli) and make a note of the URL to your deployed bot.

2. Configure your ACS resource to [handle SMS events](https://docs.microsoft.com/en-us/azure/communication-services/quickstarts/telephony-sms/handle-sms-events).  When configuring your ACS event endpoint, specify 'Webhook' as the type and the URL for your endpoint is the URL for your bot, which will be the URL of your deployed application (or ngrok endpoint), plus '/api/acssms' (for example, `https://yourbotapp.azurewebsites.net/api/acssms`). Your bot must be running when you configure your endpoint so that the endpoint can be verified by Azure Communication Services.

> [!NOTE]
> If you are not ready to deploy your bot to Azure, or wish to debug your bot when using the ACS SMS adapter, you can use a tool such as [ngrok](https://www.ngrok.com) (which you will likely already have installed if you have used the Bot Framework emulator previously) to tunnel through to your bot running locally and provide you with a publicly accessible URL for this. 
> 
> If you wish create an ngrok tunnel and obtain a URL to your bot, use the following command in a terminal window (this assumes your local bot is running on port 3978, alter the port numbers in the command if your bot is not).
> 
> ```
> ngrok.exe http 3978 -host-header="localhost:3978"
> ```
