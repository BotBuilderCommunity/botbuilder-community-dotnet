# Alexa Adapter Sample

This sample demonstrates using the Alexa Adapter (updated to use the current preview package [available on MyGet](https://www.myget.org/feed/botbuilder-community-dotnet/package/nuget/Bot.Builder.Community.Adapters.Alexa/4.6.4-beta0016) to allow a bot built using the Bot Builder SDK (v4) to build an Alexa skill.

## Instructions

This sample shows a bot which will work through the Emulator (or other text based channels, such as Facebook, if you connect them using the Azure Bot Service), but
that can also be used via an Alexa device (or simulator). 

You can then try talking to your Alexa skill.  By default anything you say will repeated back to you and the skill will ask you to say something else, simulating a multi-turn conversation.

* "finish" - by default the sample leaves requests open and waits for more speech from the user (multi turn conversation), but saying "finish" will force the bot to send a final message and use the IgnoringInput InputHint on the outgoing activity to end the conversation.

* "card" (e.g. "alexa ask <INVOCATION NAME> card") - sends a response back to the user with a simple Alexa card attached.

* "display" or "hint" (e.g. "alexa ask <INVOCATION NAME> display") - if you are using a device with a display (such as an Echo Spot / Show) or the Alexa simulator, then saying "display" / "hint" will show a response which includes a display or hint directive depending on the command given.

### Congfiguring your Alexa skill

Your Alexa skill endpoint should be set to **https://<YOURBOTURL.COM>/api/alexa**.  You can use a tool like Ngrok (which you will already have installed if you use the Bot Framework Emulator), 
which you can use to point Alexa to your local bot. If your local bot is running on port 3978, then using the command **.\ngrok.exe http 3978 -host-header="localhost:3978"** will provide you with 
a public URL which will tunnel through to your bot.  In this case you can set your Alexa skill endpoint to **<NGROK PROVIDED HTTPS URL>/api/alexa**

To use this sample, use the JSON sample below when configuring your Alexa skill within the JSON Editor in the Alexa Skills Developer portal.

Once you have configured your skill with the JSON (**Note: Be sure to 
replace your Skill invocation name at the top of the JSON with the correct one 
for your skill)** and set your endpoint, you should then be able to 
communicate with your bot via your Alexa device.

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
                    "samples": [
                        "{phrase}"
                    ]
                },
                {
                    "name": "AMAZON.FallbackIntent",
                    "samples": []
                },
                {
                    "name": "AMAZON.NavigateHomeIntent",
                    "samples": []
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
                                "value": "hi there Alexa"
                            }
                        },
                        {
                            "name": {
                                "value": "you are just going to repeat what I said aren't you"
                            }
                        },
                        {
                            "name": {
                                "value": "what colour is the sky?"
                            }
                        }
                    ]
                }
            ]
        }
    }
}

```

**Note:** TO be able to use the full range of commands provided by this bot, you should ensure that you have enabled 'Display Interface' 
under the Interfaces section within the Alexa Skills Console.