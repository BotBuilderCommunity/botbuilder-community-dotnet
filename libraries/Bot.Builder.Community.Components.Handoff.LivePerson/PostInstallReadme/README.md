# LivePerson Handoff Component

This is part of the [Bot Builder Community](https://github.com/botbuildercommunity) project which contains open source Bot Framework Composer components, 
along with other extensions, including middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

This component provides integration with the LivePerson platform, enabling handoff of a conversation to a LivePerson automated or human agent.

## Next Steps

*You can read this information again at any time by visiting the Package Manager in Composer, selecting the package from 
the **Installed** list and clicking **View Readme**.*

Now that you have installed the LivePerson Handoff Component, you need to.

* Configure your LivePerson application, within the LivePerson Developer Portal, to point to your bot, following the [instructions here](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/tree/develop/libraries/Bot.Builder.Community.Components.Handoff.LivePerson#liveperson-handoff-component-for-bot-framework-composer).
* Add the following settings at the root of your settings JSON (navigate to **Project Settings** and toggle the **Advanced Settings View (json)**.), replacing the placeholders as described below.

    ```json
    "Bot.Builder.Community.Components.Handoff.LivePerson": {
        "LivePersonAccount": "<YOUR LIVEPERSON ACCOUNT ID>",
        "LivePersonClientId": "<YOUR LIVEPERSON APP ID>",
        "LivePersonClientSecret": "<YOUR LIVEPERSON APP SECRET>",
        "MicrosoftAppId": "<YOUR BOT'S MICROSOFT APP ID>"
    }
    ``` 

* Trigger handoff within your bot by adding the **Send a handoff request** action to your design canvas.

For full setup and configuration instructions visit the [LivePerson Component documentation page on GitHub](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/tree/develop/libraries/Bot.Builder.Community.Components.Handoff.LivePerson#liveperson-handoff-component-for-bot-framework-composer).