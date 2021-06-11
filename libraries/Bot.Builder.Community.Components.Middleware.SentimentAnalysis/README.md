# Sentiment Analysis Middleware Component for Bot Framework Composer

## Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [Available via NuGet](https://www.nuget.org/packages/Bot.Builder.Community.Components.Handoff.LivePerson/) |

## Description

This is part of the [Bot Builder Community](https://github.com/botbuildercommunity) project which contains open source Bot Framework Composer components, along with other extensions, including middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

This component enables the addition of the middleware component for Sentiment Analysis of each incoming message. This component can extract the sentiment powered by [SentimentAnalyzer](https://www.nuget.org/packages/SentimentAnalyzer/) which encapsulates both offline (powered by ML.NET) and online (powered by Azure Cognitive Services) sentiment analysis.

## Usage

* [Composer component installation](#composer-component-installation)
* [Composer component configuration](#composer-component-configuration)
* [Getting the Sentiment Analysis result from Middleware](#getting-the-sentiment-analysis-result-from-middleware)

### Composer component installation

1. Navigate to the Bot Framework Composer **Package Manager**.
2. Change the filter to **Community packages**.
3. Search for 'sentiment analysis' and install **Bot.Builder.Community.Components.Middleware.SentimentAnalysis**

![image](https://user-images.githubusercontent.com/16351038/118396250-075dcb80-b692-11eb-8784-88aafc4e8124.png)

### Composer component configuration

1. Within Composer, navigate to **Project Settings** and toggle the **Advanced Settings View (json)**.
2. Add the following settings at the root of your settings JSON, replacing the placeholders as described below.

```json
"Bot.Builder.Community.Components.Middleware.SentimentAnalysis": {
"IsEnabled": true,
"APIKey": "",
"EndpointUrl": ""
}
```
> [!NOTE]
> If you do not want to use the middleware, you can simply set the value of IsEnabled to false. The above setting will work with offline sentiment analysis (powered by Microsoft ML.NET). However, if you want to use the Azure Text Analytics service to find out the Sentiments then you may have to provide API Key and Endpoint URL as shown below;

![image](https://user-images.githubusercontent.com/16351038/118396315-4be96700-b692-11eb-9778-dd38f18d8232.png)

### Getting the Sentiment Analysis result from Middleware

Once you've configured the middleware component and enabled it from the Connections (`"IsEnabled": true`) then this component will run on every message received (`ActivityType is Message`). 

You can now get the results using two different settings;

#### Offline Mode (without any external API) 

To use this, you just have to keep the settings as it is defined in the above example. In order to get it in Bot Framework Composer, you can use this syntax;

`${turn.Sentiment}`

![image](https://user-images.githubusercontent.com/16351038/118396606-97e8db80-b693-11eb-983d-039df10d2050.png)

This will give result as **True** for Positive and **False ** for Negative.

![image](https://user-images.githubusercontent.com/16351038/118396661-dda5a400-b693-11eb-8130-bf0c605059e0.png)

#### Online Mode (using Azure Text Analytics) 

To use this, you need to get the API key and an endpoint URL, once they both are populated, the component will automatically switch its mode to online and now the results will be fetched from Azure Text Analytics service. Same syntax as above;

`${turn.Sentiment}`

This will give result from 0.0 to 1 where 1 being Positive and 0 being Negative. 

![image](https://user-images.githubusercontent.com/16351038/118396744-3ffea480-b694-11eb-96d5-cb0a5191789e.png)

`The result is 0.1 (Negative)`

