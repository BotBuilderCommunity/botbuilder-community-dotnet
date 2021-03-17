# Zoom Adapter for Bot Builder v4 .NET SDK

## Description

This is part of the [Bot Builder Community Extensions](https://github.com/botbuildercommunity) project which contains various pieces of middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

The Zoom Adapter allows you to add an additional endpoint to your bot for Zoom apps. The Zoom endpoint can be used
in conjunction with other channels meaning, for example, you can have a bot exposed on out of the box channels such as Facebook and
Teams, but also via a Zoom app, such as a chatbot (as well as side by side with the Google / Twitter Adapters also available from the Bot Builder Community Project).

Incoming Zoom app requests are transformed, by the adapter, into Bot Framework Activties and then when your bot sends outgoing activities, the adapter transforms the outgoing Activity into an appropriate Zoom response.

The adapter currently supports the following scenarios;

- Automatically transform incoming chatbot app messages into Bot Framework Message activities
- Supports sending of all common Zoom chatbot message templates
- Transforms incoming interactive message events (such as use interacting with dropdown / editable fields) into Event activites with strongly typed payload objects
- Handles all incoming events from Zoom (that the app has been subscribed to) and transforms into Bot Framework Event activities

This readme focuses on consuming the Zoom component in [Bot Framework Composer](https://docs.microsoft.com/en-us/composer/introduction). For more information about the supported scenarios and how to consume the Zoom adapter in code-first scenarios, visit the [Zoom adapter readme](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/blob/develop/libraries/Bot.Builder.Community.Adapters.Zoom/README.md).

## Prerequisites

1. Install [Bot Framework Composer](https://dev.botframework.com/). Once Composer is installed, subscribe to the Early Adopters feed from the Composer application settings. Check for updates to install the latest nightly build in order to access the latest features. If you prefer to build locally, clone [Bot Framework Composer](https://github.com/microsoft/BotFramework-Composer) and build locally from the main branch using the instructions in the repository.

2. In Composer settings, enable the 'New creation experience' feature flag.

![Enable new creation experience](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/component-1-flag.png?raw=true)

3. Create a new bot using the 'Empty' bot template.

![Create new empty bot](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/component-2-new-bot.PNG?raw=true)

4. Go to Package Manager, and select 'Add feed', to add the NuGet feed where the test version of the new community adapters can be found. Use `myget adapters` as name and `https://www.myget.org/F/ms-test-adapters/api/v3/index.json` as url.

![Add preview community adapters feed](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/component-3-add-feed.PNG?raw=true)

5. Install the Zoom adapter component in package manager by selecting install on the latest version of Bot.Builder.Community.Adapters.Zoom.

6. Download [ngrok](https://ngrok.com/) to allow Alexa to access your bot to serve requests. When you publish to Azure this is not needed, but for local testing, ngrok permits hosting the bot locally and provides a public URL.

7. In a command prompt, navigate to where you downloaded ngrok and run `ngrok http 3980 -host-header="localhost:3980"`. This assumes that you will use port 3980 which is Composer's default port for single bots. If you plan to use another bot, adjust the command above. Once ran, this command will show the URLs where ngrok is exposing your bot. Copy the url starting with `https://` to wire the bot endpoint in the Zoom marketplace in the next section.

![Ngrok url for local bot](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/component-5-ngrok.PNG?raw=true)

## Create a Zoom app

1. Log into the [Zoom Developer Console](https://marketplace.zoom.us/develop) or create ana ccount, and then click the **Create** button next to the Chatbot app type. In the popup presented, choose a name for your chatbot and then click **Create**.

2. On the next screen you will be presented with the **Client ID** and **Client Secret** for your new app. Copy and make a note of these, as you will need them later when configuring your bot application. We'll fill the **Redirect URL for OAuth** and **Whitelist URL** later in the process.

3. Click the **Scopes** section in the left hand menu. Click the **+Add Scopes** button and search for 'Chat'. Select the scope **Enable Chatbot within Zoom Chat Client** (**imchat:bot**) and click **Done**.

4. Click the **Feature** section in the left hand menu. Set the endpoint URL to where your bot's Zoom endpoint will be, which is the url that you copied from ngrok, and add `/api/zoom` at the end. For example, for the ngrok url `https://74747474d.ngrok.io`, in the Alexa portal you should enter `https://74747474d.ngrok.io/api/zoom`. Click **Save**

5. One the URLs are saved, two pieces of information should appear above: the **Verification token** and **Bot JID**. Copy and make a note of these values since along with the **Client ID** and **Client secret** of step 1 will be used to configure the adapter.

6. Click back into the **App credentials** section in the left hand menu. Update the **Redirect URL for OAuth** and **Whitelist URL** to the value `https://zoom.us/launch/chat?jid=<Your Bot JID from step 5>`, replacing the last part with your bot JID. Example: `https://zoom.us/launch/chat?jid=robot_v3ynierrdntw-yfsdfsm1a9g@xmpp.zoom.us`.

## Wiring up the Zoom adapter in your bot

Now it is time to configure the adapter for your bot in Bot Framework Composer, using the values we got from the Zoom marketplace.

1. In Composer, go to your bot settings. Under the `adapters` section, there should be a new entry called `Zoom connection`. Select `Configure` to wire up your app.

2. A modal will pop up. Fill the fields with the values from steps 2 and 5 from the preiovus section.

3. Once you close the modal, your adapter should appear as configured in the bot settings.

4. Start your bot in composer.

### Install and Test your Zoom app

You can now test interacting with your Zoom app using the Zoom client.

1. In the Zoom developer dashboard, click the **Local Test** section link on the left hand menu.

2. Click the **Install** button to install yur Zoom app locally.

3. You can now login to the Zoom client and interact with your bot via chat.
