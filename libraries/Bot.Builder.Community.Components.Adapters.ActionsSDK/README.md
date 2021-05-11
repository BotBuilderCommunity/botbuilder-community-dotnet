# Google Actions (latest) Adapter Component for Bot Framework Composer / Bot Framework SDK

> **This adapter is for use with the latest Google Actions SDK for building conversational actions (https://developers.google.com/assistant/conversational/overview).**

## Description

This is part of the [Bot Builder Community Extensions](https://github.com/botbuildercommunity) project which contains various pieces of middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

The Actions SDK Adapter allows you to add an additional endpoint to your bot for conversational actions built with the latest Google Actions SDK. The Google endpoint can be used in conjunction with other channels meaning, for example, you can have a bot exposed on out of the box channels such as Facebook and Teams, but also via a Google Action (as well as side by side with other adapters (e.g. the Alexa adapter) also available from the Bot Builder Community Project).

Incoming requests from your Google action are transformed, by the adapter, into Bot Builder Activties and then when your bot responds, the adapter transforms the outgoing Activity into an appropriate Actions SDK response.

The adapter currently supports the following scenarios;

- Support for voice based Google actions
- Support for Actions SDK Card, Table Card, List and Collections
- Automatic conversion of Suggested Actions on outgoing activity into Google Suggestion Chips
- Account Linking - send a Bot Framework Signin Card to trigger the account linking flow
- Full incoming request from Google is added to the incoming activity as ChannelData

This readme focuses on consuming the Google Actions SDK component in [Bot Framework Composer](https://docs.microsoft.com/en-us/composer/introduction). For more information about the supported scenarios and how to consume the Google Actions SDK adapter in code-first scenarios, visit the [Google Actions SDK adapter readme](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/blob/develop/libraries/Bot.Builder.Community.Adapters.ActionsSDK/README.md).

## Prerequisites

1. Install [Bot Framework Composer](https://dev.botframework.com/). Once Composer is installed, subscribe to the Early Adopters feed from the Composer application settings. Check for updates to install the latest nightly build in order to access the latest features. If you prefer to build locally, clone [Bot Framework Composer](https://github.com/microsoft/BotFramework-Composer) and build locally from the main branch using the instructions in the repository.

2. In Composer settings, enable the 'New creation experience' feature flag.

![Enable new creation experience](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/component-1-flag.png?raw=true)

3. Create a new bot using the 'Empty' bot template.

![Create new empty bot](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/component-2-new-bot.PNG?raw=true)

4. Go to Package Manager, and select 'Add feed', to add the NuGet feed where the test version of the new community adapters can be found. Use `myget adapters` as name and `https://www.myget.org/F/ms-test-adapters/api/v3/index.json` as url.

![Add preview community adapters feed](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/component-3-add-feed.PNG?raw=true)

5. Install the Actions SDK adapter component in package manager by selecting install on the latest version of Bot.Builder.Community.Adapters.ActionsSDK.

6. Log in or create a free account for Actions on [Google developer console](https://console.actions.google.com/).

- The gactions command-line tool. [Download this here](https://developers.google.com/assistant/conversational/quickstart#install_the_gactions_command-line_tool)

## Create an Actions SDK action project

1. Log into the [Actions on Google console](https://console.actions.google.com/) and then click the **New project** button. Specify a name for your new project, as well as the language / region. Finally click the **Create project** button.

2. On the next screen, select **Custom** for the type of your new action.

![Select action type](/libraries/Bot.Builder.Community.Adapters.ActionsSDK/media/actions-sdk-select-project-type.PNG?raw=true)

3. On the next screen, select **Blank project** for the project template to use to build your action.

![Select action template](/libraries/Bot.Builder.Community.Adapters.ActionsSDK/media/actions-sdk-select-project-type-2.PNG?raw=true)

4. Your action will now be created. Once the creation process has finished you will be presented with your project dashboard. _Note: you do not need to set a **Display name** yet._

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

8. Finally, navigate back to your project within the Google Actions Console. You will see that the display name has been updated to 'Bot Framework Sample'. Change this to a display name of your choice. This will also be your invocation name, which is the name used by users when they invoke your action.

## Wiring up the Actions SDK adapter in your bot

Before you can complete the configuration of your Actiosn SDK project, you need to wire up the Actions SDK adapter into your bot in Bot Framework Composer.

1. In Composer, go to your bot settings. Under the `adapters` section, there should be a new entry called `Google Actions SDK connection`. Select `Configure` to wire up your project.

2. A modal will pop up. Fill the invocation name and project id with the information from your Actions SDK project.

3. Once you close the modal, your adapter should appear as configured in the bot settings with a green check mark.

4. Start your bot in composer.

![Start bot](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/component-4-start-bot.PNG?raw=true)

5. Download [ngrok](https://ngrok.com/) to allow Google Actions to access your bot to serve requests. When you publish to Azure this is not needed, but for local testing, ngrok permits hosting the bot locally and provides a public URL.

6. In a command prompt, navigate to where you downloaded ngrok and run `ngrok http 3980 -host-header="localhost:3980"`. This assumes that you started your bot in port 3980 which is Composer's default port for single bots. Adjust the port if you have your bot in other port. Once ran, this command will show the URLs where ngrok is exposing your bot. Copy the url starting with `https://` to complete your project configuration in the Google Actions portal in the next section.

![Ngrok url for local bot](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/component-5-ngrok.PNG?raw=true)

## Complete configuration for Actions on Google

1. Navigate back to the [Google Actions console](https://console.actions.google.com/) and navigate to your action project.

2. Navigate to the **Develop** tab and then to the **Webhook** page on the left hand menu.

3. Enter your Actions SDK endpoint, which is the url that you copied from ngrok, and add `/api/google` at the end. For example, for the ngrok url `https://74747474d.ngrok.io`, in the Alexa portal you should enter `https://74747474d.ngrok.io/api/google`.

### Testing your action

You can now test interacting with your action using the simulator.

1. Navigate to https://console.actions.google.com/ and navigate to your action project.

2. In the action dashboard navigate to the **Test** tab at the top of the page.

3. To perform a basic test enter "ask <ACTION DISPLAY NAME> hello world" into the simulator input box. For example, if your action display name was 'Bot Framework Sample', you would type 'Talk to Bot Framework Sample hello world'. This should return an echo of your message.

![Simulator](/libraries/Bot.Builder.Community.Adapters.Google/media/simulator-test.PNG?raw=true)

Now that you have enabled testing for your action, you can also test your action using a physical Google assistant device or using Google assistant on an Android device. Providing you are logged into the device with the same account used to login to the Actions on Google Console (or an account that you have added as a beta tester for your action within the console).
