# Google Adapter Sample

This sample demonstrates using the Google Adapter (updated to use the current preview package [available on MyGet](https://www.myget.org/feed/botbuilder-community-dotnet/package/nuget/Bot.Builder.Community.Adapters.Google/4.6.4-beta0033) to allow a bot built using the Bot Builder SDK (v4) to connect to Google Assistant.

## Instructions

This sample shows a bot which will work through the Emulator (or other text based channels, such as Facebook, if you connect them using the Azure Bot Service), but
that can also be used via a Google Assistant device (or simulator). 

You can then try talking to your Google action.  By default anything you say will repeated back to you and the skill will ask you to say something else, simulating a multi-turn conversation.

* "finish" - by default the sample leaves requests open and waits for more speech from the user (multi turn conversation), but saying "finish" will force the bot to send a final message and use the IgnoringInput InputHint on the outgoing activity to end the conversation.

* "card" (e.g. "ask <INVOCATION NAME> card") - sends a response back to the user with a basic Google card attached.

* "table" (e.g. "ask <INVOCATION NAME> table") - sends a response back to the user with a Google Table Card attached.

* "list" / "carousel" (e.g. "ask <INVOCATION NAME> list" or "ask <INVOCATION NAME> carousel") - This will show a list or carousel from which the user can select an item.
