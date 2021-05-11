# Azure Communication Services SMS Adapter for Bot Builder v4 .NET SDK

> **Important!!!** ACS is in preview, and creation of phone numbers is restricted depending on subscriptions and consumption plans. The phone number creation of this readme is first so that you can confirm whether you are able to create phone numbers in order to proceed with the usage of this adapter. If that feature is not available to your subscription and you don’t already have a Communication Services resource with a phone number, you won’t be able to test this path until the Azure Communication Services team enables it.

## Description

This is part of the [Bot Builder Community Extensions](https://github.com/botbuildercommunity) project which contains various pieces of middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

The Azure Communication Services SMS Adapter allows you to add an additional endpoint to your bot for SMS via Azure Communication Services. The adapter can be used in conjunction with other channels meaning, for example, you can have a bot exposed on out of the box channels such as Facebook and Teams, but also via Azure Communication Services SMS.

Incoming Azure Communication Services SMS events are transformed, by the adapter, into Bot Framework Activties and then when your bot sends outgoing activities, the adapter creates an Azure Communication Services SMS send request to send a reply.

The adapter currently supports the following scenarios;

- SMS received
- SMS send
- SMS delivery reports

## Create an Azure Communication Services resource

1. [Create an Azure Communication Services resource](https://docs.microsoft.com/en-us/azure/communication-services/quickstarts/create-communication-resource?tabs=windows&pivots=platform-azp) .

2. [Get a phone number](https://docs.microsoft.com/en-us/azure/communication-services/quickstarts/telephony-sms/get-phone-number).

## Install Bot Framework Composer and set up bot

1. Install [Bot Framework Composer](https://dev.botframework.com/). Once Composer is installed, subscribe to the Early Adopters feed from the Composer application settings. Check for updates to install the latest nightly build in order to access the latest features. If you prefer to build locally, clone [Bot Framework Composer](https://github.com/microsoft/BotFramework-Composer) and build locally from the main branch using the instructions in the repository.

2. In Composer settings, enable the 'New creation experience' feature flag.

![Enable new creation experience](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/component-1-flag.png?raw=true)

3. Create a new bot using the 'Empty' bot template.

![Create new empty bot](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/component-2-new-bot.PNG?raw=true)

4. Go to Package Manager, and select 'Add feed', to add the NuGet feed where the test version of the new community adapters can be found. Use `myget adapters` as name and `https://www.myget.org/F/ms-test-adapters/api/v3/index.json` as url.

![Add preview community adapters feed](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/component-3-add-feed.PNG?raw=true)

5. Install the Azure Communication Services SMS adapter component in package manager by selecting install on the latest version of Bot.Builder.Community.Adapters.ACS.SMS.

## Configure the ACS SMS adapter in Composer

Before you can complete the configuration of your ACS SMS skill, you need to wire up the ACS SMS adapter into your bot in Bot Framework Composer.

1. In Composer, go to your bot settings. Under the `adapters` section, there should be a new entry called `Azure Communication Services connection`. Select `Configure` to wire up your skill.

![Configure adapter](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/alexa-component-1-configure-adapter.PNG?raw=true)

2. A modal will pop up. Fill the **Connection string** and **Phone number** with the values obtained in the Azure portal.

3. Once you close the modal, your adapter should appear as configured in the bot settings.

4. Start your bot in composer.

![Start bot](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/component-4-start-bot.PNG?raw=true)

5. Download [ngrok](https://ngrok.com/) to allow Alexa to access your bot to serve requests. When you publish to Azure this is not needed, but for local testing, ngrok permits hosting the bot locally and provides a public URL.

6. In a command prompt, navigate to where you downloaded ngrok and run `ngrok http 3980 -host-header="localhost:3980"`. This assumes that you started your bot in port 3980 which is Composer's default port for single bots. Adjust the port if you have your bot in other port. Once ran, this command will show the URLs where ngrok is exposing your bot. Copy the url starting with `https://` to wire the bot in the Azure portal in the next section.

![Ngrok url for local bot](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/component-5-ngrok.PNG?raw=true)

## Wire up your bot endpoint to SMS events in Azure Portal

Now that you have created an Azure Communication Services resource and wired up the adapter in your bot project, the final steps are to configure your ACS resource to handle SMS events, which will be triggered when an SMS or SMS delivery report is received, pointing it to the correct endpoint on your bot.

Configure your ACS resource to [handle SMS events](https://docs.microsoft.com/en-us/azure/communication-services/quickstarts/telephony-sms/handle-sms-events). When configuring your ACS event endpoint, specify 'Webhook' as the type and the URL for your endpoint is the URL for your bot, which will be the URL of your deployed application (or ngrok endpoint), plus '/api/acs'. For example, for the ngrok url `https://74747474d.ngrok.io`, in the Alexa portal you should enter `https://74747474d.ngrok.io/api/acs`.

> **Important!!!** Your bot must be running and exposed through ngrok when you configure your endpoint so that the endpoint can be verified by Azure Communication Services.
