﻿# Sentiment Analysis Middleware Component for Bot Framework Composer

## Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [Available via NuGet](https://www.nuget.org/packages/Bot.Builder.Community.Components.Handoff.LivePerson/) |

## Description

This is part of the [Bot Builder Community](https://github.com/botbuildercommunity) project which contains open source Bot Framework Composer components, along with other extensions, including middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

This component enables the addition of the middleware component for Sentiment Analysis of each incoming message.  This component can extract the sentiment either offline (powered by ML.NET opensource framework) or online (powered by Azure Cognitive Services).

## Usage

* [Composer component installation](#composer-component-installation)
* [Composer component configuration](#composer-component-configuration)

### Composer component installation

1. Navigate to the Bot Framework Composer **Package Manager**.
2. Change the filter to **Community packages**.
3. Search for 'sentiment analysis' and install **Bot.Builder.Community.Components.Middleware.SentimentAnalysis**

`Image placeholder`

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

`Image placeholder`

