# Alexa Adapter Component for Bot Framework Composer / Bot Framework SDK

## Description

This is part of the [Bot Builder Community Extensions](https://github.com/botbuildercommunity) project which contains various pieces of middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

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

## Prerequisites

1. Install [Bot Framework Composer](https://dev.botframework.com/). Once Composer is installed, subscribe to the Early Adopters feed from the Composer application settings. Check for updates to install the latest nightly build in order to access the latest features. If you prefer to build locally, clone [Bot Framework Composer](https://github.com/microsoft/BotFramework-Composer) and build locally from the main branch using the instructions in the repository.

2. If you don't already have one, create a new bot using one of the available templates.

![image](https://user-images.githubusercontent.com/3900649/114547335-3974ab80-9c56-11eb-9abd-9b6938149f42.png)

3. Go to Package Manager (in the left hand navigation within Composer) and select 'Community Packages' from the dropdown filter.

4. Install the Alexa adapter component in Package Manager by selecting install on the latest version of Bot.Builder.Community.Components.Adapters.Alexa.

## Create an Alexa skill

- Access to the Alexa Developer Console with sufficient permissions to login to create / manage skills at [https://developer.amazon.com/alexa/console/ask](https://developer.amazon.com/alexa/console/ask). If you do not have this you can create an account for free.

### Create the Alexa skill in the Alexa developer console

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

## Configure the Alexa adapter in Composer

Before you can complete the configuration of your Alexa skill, you need to wire up the Alexa adapter into your bot in Bot Framework Composer.

1. In Composer, go to your project settings. Within the 'Connections' tab, there should be a new entry called `Amazon Alexa connection`. Select `Configure` to wire up your skill.

![image](https://user-images.githubusercontent.com/3900649/114547215-0e8a5780-9c56-11eb-9add-bfd7c39a4046.png)

2. A modal will pop up. Fill the `Alexa skill id` with the id from the skill you created in the Alexa developer console.

![Configured adapter modal](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/alexa-component-2-adapter-configured.PNG?raw=true)

3. Once you close the modal, your adapter should appear as configured in the bot settings.

![Configured adapter](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/alexa-component-3-success-bot.PNG?raw=true)

4. Start your bot in composer.

![Start bot](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/component-4-start-bot.PNG?raw=true)

5. Download [ngrok](https://ngrok.com/) to allow Alexa to access your bot to serve requests. When you publish to Azure this is not needed, but for local testing, ngrok permits hosting the bot locally and provides a public URL.

6. In a command prompt, navigate to where you downloaded ngrok and run `ngrok http 3980 -host-header="localhost:3980"`. This assumes that you started your bot in port 3980 which is Composer's default port for single bots. Adjust the port if you have your bot in other port. Once ran, this command will show the URLs where ngrok is exposing your bot. Copy the url starting with `https://` to wire the bot in the Alexa portal in the next section.

![Ngrok url for local bot](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/component-5-ngrok.PNG?raw=true)

### Complete configuration of your Alexa skill

Now that you have created an Alexa skill and wired up the adapter in your bot project, the final steps are to configure the endpoint to which requests will be posted to when your Alexa skill is invoked, pointing it to the correct endpoint on your bot.

1. Back within your Alexa skill dashboard, navigate to the **Endpoint** section on the left hand menu. Select **HTTPS** as the **Service Endpoint Type** and set the **Default Region** endpoint to your bot's Alexa endpoint, which is the url that you copied from ngrok, and add `/api/alexa` at the end. For example, for the ngrok url `https://74747474d.ngrok.io`, in the Alexa portal you should enter `https://74747474d.ngrok.io/api/alexa`.

2. In the drop down underneath the text box where you have defined your endpoint, you need to select the type of certificate being used. For development purposes, you can choose **My development endpoint is a sub-domain of a domain that has a wildcard certificate from a certificate authority**, changing this to **My development endpoint has a certificate from a trusted certificate authority** when you publish your skill into Production.

![Skill template](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/alexa-endpoint.PNG?raw=true)

4. Click the **Save Endpoints** button.

### Test your Alexa skill

You can now test interacting with your Alexa skill using the simulator.

1. In the skill dashboard navigate to the **Test** tab at the top of the page.

2. You will see a label **Test is disabled for this skill** with a dropdown list next to it with a value of **Off**. Select this dropdown and select **Development**. This will enable testing for your skill.

3. As a basic test enter "ask <SKILL INVOCATION NAME> hello world" into the simulator input box. For example, if your skill invocation name was 'alexa helper', you would type 'ask alexa helper hello world'. This should return an echo of your message.

![Simulator](/libraries/Bot.Builder.Community.Adapters.Alexa/media/bot-service-adapter-connect-alexa/simulator.PNG?raw=true)

Now that you have enabled testing for your skill, you can also test your skill using a physical Echo device or the Alexa app, providing you are logged into the device / app with the same account used to login to the Alexa Developer Console (or an account that you have added as a beta tester for your skill within the console).

### Customising your conversation

#### Controlling the end of a session

By default, the Alexa adapter is configured to close the session following sending a response. You can explicitly indicate that Alexa should wait for the user to say something else, meaning Alexa should leave the microphone open and listen for further input, by sending an input hint of **_ExpectingInput_** on your outgoing activity.

You can alter the default behavior to leave the session open and listen for further input by default by setting the **_ShouldEndSessionByDefault_** setting on the adapter configuration.
