## Token Exchange Skill Handler for Composer and Adaptive Dialog Bots

### Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [![NuGet version](https://img.shields.io/badge/NuGet-1.0.39-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Components.TokenExchangeSkillHandler/) |

### Prerequisite Setup
Create the Azure AD identity [for SkillBot](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-authentication-sso#create-the-azure-ad-identity-for-skillbot)

Create the Azure AD identity [for RootBot](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-authentication-sso#create-the-azure-ad-identity-for-rootbot)

### Description
This package extends [Bot Framework Composer](https://docs.microsoft.com/en-us/composer/introduction) with a CloudSkillHandler BotComponent for enabling Single Sign On Token Exchange between a root bot and skill bot. 

The following new component is in this package:

| Actions | Description |
| ------ | ------ |
| [TokenExchangeComponent](#TokenExchangeComponent) | Installs the TokenExchangeSkillHandler. |

### Installation

This package can be installed from composers Package Manager screen. Just select the package from the list and click install.

![Package Manager](package-manager.png)

### Usage

Once installed you should find a Bot.Builder.Community.Components.TokenExchangeSkillHandler in the Components section of the config. Ensure there is also a root level section titled "Bot.Builder.Community.Components.TokenExchangeSkillHandler", with useTokenExchangeSkillHandler set to true and the proper Root Bot Oauth TokenExchangeConnectionName:

```json
  "runtimeSettings": {
    "components": [
      {
        "name": "Bot.Builder.Community.Components.TokenExchangeSkillHandler",
        "settingsPrefix": "Bot.Builder.Community.Components.TokenExchangeSkillHandler"
      }
    ],
    ...
  },
  "Bot.Builder.Community.Components.TokenExchangeSkillHandler": {
    "useTokenExchangeSkillHandler": true,
    "tokenExchangeConnectionName": "YourTokenExchangeConnectionName"
  },
  
```
