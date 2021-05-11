# Google Actions (latest) Adapter Component for Bot Framework Composer / Bot Framework SDK

## Description

This is part of the [Bot Builder Community](https://github.com/botbuildercommunity) project which contains Bot Framework Components and other projects / packages for use with Bot Framework Composer and the Bot Builder .NET SDK v4.

The Actions SDK Adapter allows you to add an additional endpoint to your bot for conversational actions built with the latest Google Actions SDK. The Google endpoint can be used in conjunction with other channels meaning, for example, you can have a bot exposed on out of the box channels such as Facebook and Teams, but also via a Google Action (as well as side by side with other adapters (e.g. the Alexa adapter) also available from the Bot Builder Community Project).

Incoming requests from your Google action are transformed, by the adapter, into Bot Builder Activties and then when your bot responds, the adapter transforms the outgoing Activity into an appropriate Actions SDK response.

The adapter currently supports the following scenarios;

- Support for voice based Google actions
- Support for Actions SDK Card, Table Card, List and Collections
- Automatic conversion of Suggested Actions on outgoing activity into Google Suggestion Chips
- Account Linking - send a Bot Framework Signin Card to trigger the account linking flow
- Full incoming request from Google is added to the incoming activity as ChannelData

*This readme focuses on consuming the Google Actions SDK component in [Bot Framework Composer](https://docs.microsoft.com/en-us/composer/introduction). For more information about the supported scenarios and how to consume the Google Actions SDK adapter in code-first scenarios, visit the [Google Actions SDK adapter readme](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/blob/develop/libraries/Bot.Builder.Community.Adapters.ActionsSDK/README.md).*

## Usage

- [Prerequisites](#Prerequisites)
- [Component Installation](#Component-installation-via-Composer-Package-Manager)
- [Create an Actions SDK action project](#Create-an-Actions-SDK-action-project)
- [Configure your Actions SDK project](#Configure-your-Actions-SDK-project)
- [Configure Composer Actions SDK Connection](#Configure-the-Actions-SDK-Connection-in-Bot-Framework-Composer)
- [Complete configuration for Actions on Google](#Complete-configuration-for-Actions-on-Google)
- [Testing your Action](#testing-your-action)

## Prerequisites

- [Bot Framework Composer](https://dev.botframework.com/)

- Access to the Actions on Google developer console with sufficient permissions to login to create / manage projects at  [https://console.actions.google.com/](https://console.actions.google.com/). If you do not have this you can create an account for free.

- The gactions command-line tool. [Download this here](https://developers.google.com/assistant/conversational/quickstart#install_the_gactions_command-line_tool)

## Component installation via Composer Package Manager

1. Go to Package Manager (in the left hand navigation within Composer).

2. Within in Package Manager, search for an install the latest version of Bot.Builder.Community.Components.Adapters.ActionsSDK.

## Create an Actions SDK action project

1. Log into the [Actions on Google console](https://console.actions.google.com/) and then click the **New project** button. Specify a name for your new project, as well as the language / region. Finally click the **Create project** button.

2. On the next screen, select **Custom** for the type of your new action.

![Select action type](/libraries/Bot.Builder.Community.Adapters.ActionsSDK/media/actions-sdk-select-project-type.PNG?raw=true)

3. On the next screen, select **Blank project** for the project template to use to build your action.

![Select action template](/libraries/Bot.Builder.Community.Adapters.ActionsSDK/media/actions-sdk-select-project-type-2.PNG?raw=true)

4. Your action will now be created. Once the creation process has finished you will be presented with your project dashboard. *Note: you do not need to set a **Display name** yet.*

![Select action template](/libraries/Bot.Builder.Community.Adapters.ActionsSDK/media/actions-sdk-project-created.PNG?raw=true)

## Configure your Actions SDK project

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

## Configure the Actions SDK Connection in Bot Framework Composer

Before you can complete the configuration of your Actiosn SDK project, you need to configure the Actions SDK connection in Bot Framework Composer.

1. In Composer, go to your bot settings. Under the `adapters` section, there should be a new entry called `Google Actions SDK connection`. Select `Configure` to wire up your project.

2. A modal will pop up. Fill the `invocation name` and `project id` with the information from your Actions SDK project.

3. Once you close the modal, your adapter should appear as configured in the bot settings with a green check mark.

## Complete configuration for Actions on Google

1. [Deploy your bot to Azure](https://aka.ms/bot-builder-deploy-az-cli) and make a note of the URL to your deployed bot.

> **NOTE**
> If you are not ready to deploy your bot to Azure, or wish to test / debug your bot locally, you can use a tool such as [ngrok](https://www.ngrok.com) (which you will likely already have installed if you have used the Bot Framework emulator previously) to tunnel through to your bot running locally and provide you with a publicly accessible URL for this. 
> 
> If you wish create an ngrok tunnel and obtain a URL to your bot, use the following command in a terminal window (this assumes your local bot is running on port 3980, alter the port numbers in the command if your bot is not).
> 
> ```
> ngrok.exe http 3980 -host-header="localhost:3980"
> ```

2. Navigate back to the [Google Actions console](https://console.actions.google.com/) and navigate to your action project.

3. Navigate to the **Develop** tab and then to the **Webhook** page on the left hand menu.

4. Enter your Actions SDK endpoint. This will be the URL of your deployed application (or ngrok endpoint - see below), plus '/api/actionssdk' (for example, `https://yourbotapp.azurewebsites.net/api/actionssdk`).

## Testing your action

You can now test interacting with your action using the simulator (if using an ngrok endpoint, ensure both your bot and ngrok endpoint are running). 

1. Navigate to https://console.actions.google.com/ and navigate to your action project.

2. In the action dashboard navigate to the **Test** tab at the top of the page.

3. To perform a basic test enter "ask <ACTION DISPLAY NAME> hello world" into the simulator input box. For example, if your action display name was 'Bot Framework Sample', you would type 'Talk to Bot Framework Sample hello world'. This should return an echo of your message.

![Simulator](/libraries/Bot.Builder.Community.Adapters.Google/media/simulator-test.PNG?raw=true)

Now that you have enabled testing for your action, you can also test your action using a physical Google assistant device or using Google assistant on an Android device. Providing you are logged into the device with the same account used to login to the Actions on Google Console (or an account that you have added as a beta tester for your action within the console).
