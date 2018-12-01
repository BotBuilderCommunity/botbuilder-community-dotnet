# Location Dialog Sample

This sample demonstrates using the Location Dialog.

Before running this sample you need to replace "<BING MAPS OR AZURE MAPS API KEY>" in LocationDialogSampleBot.cs
with your Bing / Azure maps API key.  

The dialog in this case is configured to use Azure Maps, but this
can be changed when adding the dialog to the DialogSet.

In this example there is a main dialog, which when the user sends a message, starts a location
dialog to ask the user for their location. In this example the Street and Postal/Zip Codes are set
as required and the dialog is configured to ask for a final confirmation from the user and to offer
the user the ability to save a found location in their favorites.

This sample uses Memory Storage for state, so user favorites will be lost everytime you restart the sample.

Below is how the sample adds the dialog to the DialogSet and this is where all options can be configured.

```cs

Dialogs.Add(new LocationDialog("<BING MAPS OR AZURE MAPS API KEY>",
        "Please enter a location",
        conversationState,
        useAzureMaps: true,
        requiredFields: LocationRequiredFields.StreetAddress | LocationRequiredFields.PostalCode,
        options: LocationOptions.None
        ));

```


