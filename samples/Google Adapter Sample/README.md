# Alexa Adapter Sample

This sample demonstrates using the Alexa Adapter to allow a bot built using the Bot Builder SDK (v4) to build an Alexa skill.

## Instructions

This sample shows a bot which will work through the Emulator (or other text based channels, such as Facebook, if you connect them using the Azure Bot Service), but
that can also be used via an Alexa device. 

### Congfiguring your Alexa skill

Your Alexa skill endpoint should be set to https://<YOURBOTURL.COM>/api/skillrequests, 

To use this sample, use the JSON sample below when configuring your Alexa skill within the JSON Editor in the Alexa Skills Developer portal.

Once you have configured your skill with the JSON (**Note: Be sure to 
replace your Skill invocation name at the top of the JSON with the correct one 
for your skill)** and set your endpoint, you should then be able to 
communicate with your bot via your Alexa device.

The JSON below does the following;

* Configures a single Intent, 'GetUserIntent', with a single slot called 'Phrase'.  This configuration
will allow you to capture everything that the user says and this sample then uses the 
**AlexaIntentRequestToMessageActivityMiddleware** middleware to automatically transform values receieved into MessageActivities

* Adds various out of the box Amazon Alexa built-in intents, such as Help or Cancel to your model.

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