# LUIS Dialog Sample

This sample demonstrates using the Luis Dialog, which is a migration of the v3 LuisDialog to work with the v4 SDK.

# Create the LuisDialog class
To create a dialog that uses LUIS, first create a class that derives from LuisDialog and specify the LuisModel attribute. To populate the modelID, subscriptionKey and domain parameters for the LuisModel attribute.

The domain parameter is determined by the Azure region to which your LUIS app is published. If not supplied, it defaults to westus.api.cognitive.microsoft.com. See Regions and keys for more information.

# Create methods to handle intents
Within the class, create the methods that execute when your LUIS model matches a user's utterance to intent. To designate the method that runs when a specific intent is matched, specify the LuisIntent attribute.

# Try the bot
You can run the bot using the Bot Framework Emulator and tell it to create a note.