# ServiceNow Handoff Component

This is part of the [Bot Builder Community](https://github.com/botbuildercommunity) project which contains open source Bot Framework Composer components, 
along with other extensions, including middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

This component provides integration with the ServiceNow platform, enabling handoff of a conversation to the ServiceNow Virtual Agent or if preferred a human agent using the ServiceNow Bot-to-Bot integration API

## Next Steps

*You can read this information again at any time by visiting the Package Manager in Composer, selecting the package from 
the **Installed** list and clicking **View Readme**.*

Now that you have installed the ServiceNow Handoff Component, you need to.

* Configure your ServiceNow instance, following the [instructions here](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/blob/develop/libraries/Bot.Builder.Community.Components.Handoff.ServiceNow/README.MD#servicenow-handoff-component-for-bot-framework-composer).
* Add the following settings at the root of your settings JSON (navigate to **Project Settings** and toggle the **Advanced Settings View (json)**.), replacing the placeholders as described below. 

```json
{
  "ServiceNowTenant": "{YOUR_TENANT_NAME}.service-now.com",
  "ServiceNowAuthConnectionName": "ServiceNow"
}
```

* Trigger handoff within your bot by adding the **Send a handoff request** action to your design canvas.

For full setup and configuration instructions visit the [ServiceNow Component documentation page on GitHub](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/blob/develop/libraries/Bot.Builder.Community.Components.Handoff.ServiceNow/README.MD#servicenow-handoff-component-for-bot-framework-composer).