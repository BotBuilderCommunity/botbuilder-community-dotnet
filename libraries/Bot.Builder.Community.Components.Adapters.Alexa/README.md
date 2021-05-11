# AlexaAdapter Component for Bot Framework Composer / Bot Framework SDK

## Description

This is part of the [Bot Builder Community](https://github.com/botbuildercommunity) project which contains Bot Framework Components and other projects / packages for use with Bot Framework Composer and the Bot Builder .NET SDK v4.

The Alexa Adapter allows you to add an additional endpoint to your bot for Alexa Skills. The Alexa endpoint can be used
in conjunction with other channels meaning, for example, you can have a bot exposed on out of the box channels such as Facebook and
Teams, but also via an Alexa Skill (as well as side by side with the Google / Twitter Adapters also available from the Bot Builder Community Project).

Incoming Alexa Skill requests are transformed, by the adapter, into Bot Framework Activties and then when your bot sends outgoing activities, the adapter transforms the outgoing Activity into an Alexa Skill response.

The adapter currently supports the following scenarios;

- Support for voice based Alexa Skills
- Ability to send Alexa Cards (via automatic translation of Bot Framework Hero Cards or using the included Alexa specific attachments)
- Support for the available display directives for Echo Show / Spot devices
- Support for audio / video directives
- Full incoming request from Alexa is added to the incoming activity as ChannelData
- Validation of incoming Alexa requests (required for certification), including validation the request has come from a specific skill (validated against the ID)

This readme focuses on consuming the Alexa component in [Bot Framework Composer](https://docs.microsoft.com/en-us/composer/introduction). For more information about the supported scenarios and how to consume the Alexa adapter in code-first scenarios, visit the [Alexa adapter readme](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/blob/develop/libraries/Bot.Builder.Community.Adapters.Alexa/README.md).

## Usage

- [Prerequisites](#Prerequisites)
- [Component Installation](#Component-installation-via-Composer-Package-Manager)
- [Create an Alexa skill](#Create-an-Alexa-skill)
- [Configure the Alexa connection in Composer](#Configure-the-Alexa-connection-in-Composer)
- [Complete configuration of your Alexa skill](#Complete-configuration-of-your-Alexa-skill)
- [Test your Alexa skill](#Test-your-Alexa-skill)
- [Customising your conversation](#Customising-your-conversation)

## Prerequisites

- [Bot Framework Composer](https://dev.botframework.com/)

- Access to the Alexa Developer Console with sufficient permissions to login to create / manage skills at  [https://developer.amazon.com/alexa/console/ask](https://developer.amazon.com/alexa/console/ask). If you do not have this you can create an account for free.

## Component installation via Composer Package Manager

1. Go to Package Manager (in the left hand navigation within Composer).

2. Within in Package Manager, search for an install the latest version of Bot.Builder.Community.Components.Adapters.Alexa.

## Create an Alexa skill

1. Log into the [Alexa Developer Console](https://developer.amazon.com/alexa/console/ask) and then click the 'Create Skill' button.

2. On the next screen enter a name for your new skill. On this page you can **Choose a model to add to your skill** (**Custom** selected by default) and **Choose a method to host your skill's backend resources** (**Provision your own** selected by default). Leave the default options selected and click the **Create Skill** button.

![Skill model and hosting](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/create-skill-options.PNG?raw=true)

3. On the next screen you will be asked to **Choose a template**. **Start from scratch** will be selected by default. Leave **Start from scratch** selected and click the **Choose** button.

![Skill template](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/create-skill-options2.PNG?raw=true)

4. You will now be presented with your skill dashboard. Navigate to **JSON Editor** within the **Interaction Model** section of the left hand menu.

5. Paste the JSON below into the **JSON Editor**, replacing the following values;

- **YOUR SKILL INVOCATION NAME** - This is the name that users will use to invoke your skill on Alexa. For example, if your skill invocation name was 'adapter helper', then a user would could say "Alexa, launch adapter helper" to launch the skill.

- **EXAMPLE PHRASES** - You should provide 3 example phases that users could use to interact with your skill. For example, if a user might say "Alexa, ask adapter helper to give me details of the alexa adapter", your example phrase would be "give me details of the alexa adapter".

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
          "samples": ["{phrase}"]
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

## Configure the Alexa connection in Composer

Before you can complete the configuration of your Alexa skill, you need to wire up the Alexa adapter into your bot in Bot Framework Composer.

1. In Composer, go to your project settings. Within the 'Connections' tab, there should be a new entry called `Amazon Alexa connection`. Select `Configure` to wire up your skill.

![image](https://user-images.githubusercontent.com/3900649/114547215-0e8a5780-9c56-11eb-9add-bfd7c39a4046.png)

2. A modal will pop up. Fill the `Alexa skill id` with the id from the skill you created in the Alexa developer console.

![Configured adapter modal](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/alexa-component-2-adapter-configured.PNG?raw=true)

3. Once you close the modal, your adapter should appear as configured in the bot settings.

![Configured adapter](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/alexa-component-3-success-bot.PNG?raw=true)

## Complete configuration of your Alexa skill

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

## Test your Alexa skill

You can now test interacting with your Alexa skill using the simulator. 

1. In the skill dashboard navigate to the **Test** tab at the top of the page.

2. You will see a label **Test is disabled for this skill** with a dropdown list next to it with a value of **Off**. Select this dropdown and select **Development**. This will enable testing for your skill.

3. As a basic test enter "ask <SKILL INVOCATION NAME> hello world" into the simulator input box. For example, if your skill invocation name was 'alexa helper', you would type 'ask alexa helper hello world'. This should return an echo of your message.

![Simulator](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/simulator.PNG?raw=true)

Now that you have enabled testing for your skill, you can also test your skill using a physical Echo device or the Alexa app, providing you are logged into the device / app with the same account used to login to the Alexa Developer Console (or an account that you have added as a beta tester for your skill within the console).

## Customising your conversation

### Controlling the end of a session

By default, the Alexa adapter is configured to close the session following sending a response. You can explicitly indicate that Alexa should wait for the user to say something else, meaning Alexa should leave the microphone open and listen for further input, by sending an input hint of **Expecting** on your outgoing activity.

To set an input hint on an individual outgoing activity within Composer, add a speech tab to your response and then use the **Input Hint** dropdown to select the appropriate value.

You can alter the default behavior to leave the session open and listen for further input by default by setting the **End session by default** setting on the connection configuration within your project settings.

### Incoming Alexa request to Bot Framework activity mapping

The Alexa service can send your bot a number of different request types.  You can read about the different request types [in the Alexa developer documentation](https://developer.amazon.com/en-US/docs/alexa/custom-skills/request-types-reference.html).

Here is how the adapter handles different types.

* **Intent Requests -> Message Activity**
Most incoming requests when a user talks to your skill will be sent as an IntentRequest. In this case the adapter transforms this into a Bot Framework Message activity.
Key exceptions here include if a user triggers a built in Amazon intent. The only built in intent available in the default configuration described within this document is the AMAZON.StopIntent - if this is receieved then this is converted to an EndOfConversationActivity.
Any other intent requests receieved, that are not using the default intent described within this article, your bot will receieve an Event Activity, where the type is set to 'IntentRequest' and the incoming request payload set as the value property on the activity.

* **Launch Request -> ConversationUpdate Activity**
If a user explicitly launches your skill without any other intent / input (e.g. "Alexa open ***your skill name***"), then your bot will recieve a ConversationUpdate Activity.  This is mirrors the default functionality on Azure bot Service channels when a user starts a conversation.

* **SessionEnded Request -> EndOfCOnversation Activity**
In the case that your bot receieves a SessionEnded request, your bot will recieve an EndOfConversation activity. You can use this opportunity to clean up any data related to the conversation / user that you wish, but any outgoing activities you send in response will be ignored by the adapter, due to Alexa not allowing responses to SessionEnded requests.

* **All other request types -> Event Activity**
All request types, not explicity mentioned above, will be sent to your bot as an Event activity. The name property on the Event activity will be set to the type of the Alexa request, with the value set to the full request payload.
