# TextRecognizer Middleware Component for Bot Framework Composer

## Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [Available via NuGet](https://www.nuget.org/packages/Bot.Builder.Community.Components.Handoff.LivePerson/) |

## Description

This is part of the [Bot Builder Community](https://github.com/botbuildercommunity) project which contains open source Bot Framework Composer components, along with other extensions, including middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

The Text Recognizer Middleware library is a compliment to the Text Recognizer custom input.These middleware components can be used to identify certain text sequences that you might want to alter prior to appearing on the chat window. For example, turning a URL into an actual link, or turning a hashtag into a link that points to a Twitter search.

## Usage

* [Composer component installation](#composer-component-installation)
* [Composer component configuration](#composer-component-configuration)
* [Getting the Sentiment Analysis result from Middleware](#getting-the-sentiment-analysis-result-from-middleware)

### Composer component installation

1. Navigate to the Bot Framework Composer **Package Manager**.
2. Change the filter to **Community packages**.
3. Search for 'sentiment analysis' and install **Bot.Builder.Community.Components.Middleware.TextRecognizer**

![image](https://user-images.githubusercontent.com/16351038/118396250-075dcb80-b692-11eb-8784-88aafc4e8124.png)

### Composer component configuration

1. Within Composer, navigate to **Project Settings** and toggle the **Advanced Settings View (json)**.
2. Add the following settings at the root of your settings JSON, replacing the placeholders as described below.

```json
"components": [
{
    "name": "Bot.Builder.Community.Components.Middleware.TextRecognizer",
    "settingsPrefix": "Bot.Builder.Community.Components.Middleware.TextRecognizer"
}

"Bot.Builder.Community.Components.Middleware.TextRecognizer": {
    "IsEmailEnable": true,
    "IsPhoneNumberEnable": true,
    "IsSocialMediaEnable": true,
    "MediaType": "Mention",
    "IsInternetProtocolEnable": true,
    "InternetProtocolType": "url",
    "Locale": "de-De"
  },

```

### NOTE
If you do not want to use the middleware or particular middleware, you can simply set the value of ..Enabled to false.

Example : Disable SocialMediaMiddleware
IsSocialMediaEnable : false


### Getting the TextRecognizer result from Middleware

Once you've configured the middleware component and enabled it from the Connections (`"..Enabled": true`) then this component will run on every message received (`ActivityType is Message`). 

You can now get the results using below settings;

Email Middleware
${turn.Activity.Conversation.EmailEntities}

PhoneNumber Middleware
${turn.Activity.Conversation.PhoneNumberEntities}

SocialMediaRecognizer Middleware
${turn.Activity.Conversation.MediaTypeEntities}

SocialMediaType
Mention
Hashtag

InternetProtocol Middleware
${turn.Activity.Conversation.InternetTypeEntities}

InternetProtocolType
IpAddress,
Url

#### Example
