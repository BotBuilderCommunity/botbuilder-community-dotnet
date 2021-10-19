# LivePerson Handoff Component for Bot Framework Composer

## Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [Available via NuGet](https://www.nuget.org/packages/Bot.Builder.Community.Components.Handoff.LivePerson/) |

## Description

This is part of the [Bot Builder Community](https://github.com/botbuildercommunity) project which contains open source Bot Framework Composer components, along with other extensions, including middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

This component provides integration with the LivePerson platform, enabling handoff of a conversation to a LivePerson automated or human agent.

## Usage

* [Prerequisites](#prerequisites)
* [Composer component installation](#composer-component-installation)
* [LivePerson configuration](#liveperson-configuration)
* [Composer component configuration](#composer-component-configuration)
* [Triggering handoff from your bot](#triggering-handoff-from-your-bot)

### Prerequisites

* A LivePerson account with sufficient permissions to login to create / manage apps at  [https://connector-api.dev.liveperson.net/](https://connector-api.dev.liveperson.net/). If you do not have this you can create a trial account for free at https://www.liveperson.com.

### Composer component installation

1. Navigate to the Bot Framework Composer **Package Manager**.
2. Change the filter to **Community packages**.
3. Search for 'liverperson' and install **Bot.Builder.Community.Components.Handoff.LivePerson**

![image](https://user-images.githubusercontent.com/3900649/115234449-f3fe2580-a110-11eb-95f4-e549cd14ceea.png)

### LivePerson Configuration

1. If you do not already have one, create a LivePerson account at https://www.liveperson.com/.
2. Sign into the LivePerson Connector App Hub at https://connector-api.dev.liveperson.net/, using your LivePerson account.
3. Click **Create App**.
4. Provide a name and description for your LivePerson application.
5. You now need to set the **Webhooks endpoint** to the dedicated LivePerson endpoint, now available on your bot after installing the package. The endpoint is the URL for your bot, which will be the URL of your deployed application, plus '/api/liveperson' (for example, `https://yourbotapp.azurewebsites.net/api/liveperson`). If testing locally, you can use a tool such as [ngrok](https://www.ngrok.com) (which you will likely already have installed if you have used the Bot Framework emulator previously) to tunnel through to your bot running locally and provide you with a publicly accessible URL for this. If you wish create an ngrok tunnel and obtain a URL to your bot, use the following command in a terminal window (this assumes your local bot is running on port 3978, alter the port numbers in the command if your bot is not).
 
```
ngrok.exe http 3978 -host-header="localhost:3978"
```

![image](https://user-images.githubusercontent.com/3900649/115234852-68d15f80-a111-11eb-865b-43085418c26a.png)

6. Once your app is configured, make a note of the application Id and secret, which have been generated automatically for you and displayed on the LivePerson Connector Hub.

![image](https://user-images.githubusercontent.com/3900649/115234991-90c0c300-a111-11eb-877f-0ddb8df6421f.png)

### Composer component configuration

1. Within Composer, navigate to **Project Settings** and toggle the **Advanced Settings View (json)**.
2. Add the following settings at the root of your settings JSON, replacing the placeholders as described below.

```json
"Bot.Builder.Community.Components.Handoff.LivePerson": {
    "LivePersonAccount": "<YOUR LIVEPERSON ACCOUNT ID>",
    "LivePersonClientId": "<YOUR LIVEPERSON APP ID>",
    "LivePersonClientSecret": "<YOUR LIVEPERSON APP SECRET>",
    "MicrosoftAppId": "<YOUR BOT'S MICROSOFT APP ID>"
}
```

> [!NOTE]
> It is currently a requirement that you have created and set a Microsoft App Id and Password for your bot, even when developing LivePerson handoff locally.  This requirement is temporary and will be removed in an upcoming release of the Bot Framework SDK / Composer.

### Triggering handoff from your bot

You can trigger handoff to LivePerson at any point by adding the **Send Handoff Activity** action in the designer. For example, you might add an natural language intent trigger to detect if the user asks to speak to a human.

![image](https://user-images.githubusercontent.com/3900649/115235187-d1204100-a111-11eb-8dee-bcfb63b76347.png)

When adding a **Send Handoff Activity** you have the option to provide Context information that can be passed to LivePerson, provided as a JSON object. The Context can contian a Skill, which can used as part of LivePerson routing rules and / or Engagement Attributes. Currently you can add EngagementAttributes of type 'ctmrinfo' and / or 'personal'. For more information on Engagement Attributes, visit https://developers.liveperson.com/engagement-attributes-types-of-engagement-attributes.html. 

```json
{
  "Skill": "Credit Cards",
  "EngagementAttributes": [
    {
      "Type": "ctmrinfo",
      "CustomerType": "vip",
      "SocialId": "123456789"
    }
  ]
}
```

If successfully triggered, all messages from the user will be routed to LivePerson (with responses from LivePerson subsequently being forwarded to the user), until LivePerson sends an event informing the bot that the handoff has completed. After the handoff is completed, all messages from the user will once again be sent to, and handled by, the bot.
