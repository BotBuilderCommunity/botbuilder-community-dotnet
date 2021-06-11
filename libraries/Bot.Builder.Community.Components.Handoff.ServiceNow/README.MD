# ServiceNow Handoff Component for Bot Framework Composer

## Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [Available via NuGet](https://www.nuget.org/packages/Bot.Builder.Community.Components.Handoff.ServiceNow/) |

## Description

This is part of the [Bot Builder Community](https://github.com/botbuildercommunity) project which contains open source Bot Framework Composer components, along with other extensions, including middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

This component provides integration with the ServiceNow platform, enabling handoff of a conversation to the ServiceNow Virtual Agent or if preferred a human agent using the ServiceNow Bot-to-Bot integration API [v](https://docs.servicenow.com/bundle/paris-now-intelligence/page/administer/virtual-agent/concept/bot2bot.html)

## Usage

* [Prerequisites](#prerequisites)
* [Composer component installation](#composer-component-installation)
* [ServiceNow configuration](#servicenow-configuration)
* [Bot Channel Registration oAuth Connection Configuration](#bot-channel-registrator-oauth-connection-configuration)
* [Composer component configuration](#composer-component-configuration)
* [Triggering handoff from your bot](#triggering-handoff-from-your-bot)

### Prerequisites

* A ServiceNow account with sufficient permissions to login to create / manage apps within your ServiceNow instance. If you do not have this you can create a trial account for free at https://developer.servicenow.com/

### Composer component installation

1. Navigate to the Bot Framework Composer **Package Manager**.
2. Change the filter to **Community packages**.
3. Search for 'servicenow' and install **Bot.Builder.Community.Components.Handoff.ServiceNow**

![image](https://user-images.githubusercontent.com/3900649/115234449-f3fe2580-a110-11eb-95f4-e549cd14ceea.png)

### ServiceNow Configuration

1. Ensure the ServiceNow Virtual Agent is configured and working. More information is available [here](https://docs.servicenow.com/bundle/paris-now-intelligence/page/administer/virtual-agent/task/install-virtual-agent-api.html). 
2. Configure OAuth as per the documentation [here](https://docs.servicenow.com/bundle/paris-platform-administration/page/administer/security/task/t_SettingUpOAuth.html) and ensure you `Create an endpoint for external clients`. When prompted set the Redirect URL to `https://token.botframework.com/.auth/web/redirect`. You'll need the ClientID and Secret created as part of this for the next step. 
3. In the ServiceNow Admin portal, navigate to Outbound REST Message as part of System Web Services and select VA Bot to Bot. All responses from ServiceNow will be asynchronously sent back to your Bot.  Configure `postMessage` endpoint to point to the dedicated ServiceNow endpoint, now available on your bot after installing the package. The endpoint is the URL for your bot, which will be the URL of your deployed application, plus '/api/servicenow' (for example, `https://yourbotapp.azurewebsites.net/api/servicenow`). If testing locally, you can use a tool such as [ngrok](https://www.ngrok.com) (which you will likely already have installed if you have used the Bot Framework emulator previously) to tunnel through to your bot running locally and provide you with a publicly accessible URL for this. If you wish create an ngrok tunnel and obtain a URL to your bot, use the following command in a terminal window (this assumes your local bot is running on port 3980, alter the port numbers in the command if your bot is not).
 
```
ngrok.exe http 3980 -host-header="localhost:3980"
```

### Bot Channel Registration oAuth Connection Configuration

This ServiceNow handoff package expects OAuth security is being used and therefore users will be prompted to authenticate to ServiceNow on their first handoff. ServiceNow supports other authentication types which you can implement by cloning this repo and making the requisite changes.

- Navigate to the [Azure Portal](https://portal.azure.com) and find your Azure Bot Service channel registration entry.
- Click Configuration and scroll down to the OAuth configuration.
- Create a new oAUth Connection and select `Generic oAuth 2`
- Provide a name, e.g. ServiceNow. You'll need this for the next step.
- Set `ClientId` to the one created in the previous step.
- Set `Authorization URL` to `https://{YOUR_TENANT_NAME}.service-now.com/oauth_auth.do`
- Set `Token URL` to `https://{YOUR_TENANT_NAME}.service-now.com/oauth_token.do`
- Set `Refresh URL` to `https://{YOUR_TENANT_NAME}.service-now.com/oauth_token.do`
- Save and then Click Test Connection to validate a token can be retrieved.


### Composer component configuration

1. Within Composer, navigate to **Project Settings** and toggle the **Advanced Settings View (json)**.
2. Add the following settings at the root of your settings JSON, replacing the placeholders as described below. Note that the Connection Name should match the one created in the previous step.

```json
"Bot.Builder.Community.Components.Handoff.ServiceNow": {
  "ServiceNowTenant": "{YOUR_TENANT_NAME}.service-now.com",
  "ServiceNowAuthConnectionName": "ServiceNow"
}
```

### Triggering handoff from your bot

You can trigger handoff to LivePerson at any point by adding the **Send Handoff Activity** action in the designer. For example, you might add an natural language intent trigger to detect if the user asks to speak to a human.

![image](https://user-images.githubusercontent.com/3900649/115235187-d1204100-a111-11eb-8dee-bcfb63b76347.png)

When adding a **Send Handoff Activity** you have the option to provide Context information that can be passed to ServiceNow, provided as a JSON object. The Context currently supports `timezone`, `userId` and `emailId` which are optional to provide.

```json
{
  "timeZone": "Europe",
  "userId": "darrenj@hotmail.co.uk",
  "emailId": "djdada"
}
```

If successfully triggered, all messages from the user will be routed to ServiceNow (with responses from ServiceNow subsequently being forwarded to the user), until ServiceNow indicates in a response message that handoff has completed from it's perspective. After the handoff is completed, all messages from the user will once again be sent to, and handled by, the bot.
