## Sentiment Analysis Middleware
 
### Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [![NuGet version](https://img.shields.io/badge/NuGet-1.0.184-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Middleware.SentimentAnalysis/) |

### Description
This is part of the [Bot Builder Community Extensions](https://github.com/garypretty/botbuilder-community) project which contains various pieces of middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

This middleware will record the sentiment of each incoming message text. It uses Cognitive Services Text Analytics API and therefore requires a key. There is a free tier which meets most demo/PoC needs.  You can get more info at https://azure.microsoft.com/en-us/services/cognitive-services/text-analytics/

The implementation detects the language of the text and get the sentiment of the same. Currently, nearly 17 languages are supported. Full list of supported languages can be seen at https://docs.microsoft.com/en-us/azure/cognitive-services/text-analytics/text-analytics-supported-languages

### Installation

Available via NuGet package [Bot.Builder.Community.Middleware.SentimentAnalysis](https://www.nuget.org/packages/Bot.Builder.Community.Middleware.SentimentAnalysis/)

Install into your project using the following command in the package manager;
```
    PM> Install-Package Bot.Builder.Community.Middleware.SentimentAnalysis
```

### Sample

A basic sample for using this component can be found [here](../../samples/Sentiment%20Middleware%20Sample).

### Usage

Typically I would place this middleware at the end of the pipeline, but it will work anywhere.  

```
services.AddBot<Bot>((options) => {
    options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);
	
	// Sentiment Middlware
        string apiKey = GetAPIKey(botConfig, "TextAnalytics");
        options.Middleware.Add(new Bot.Builder.Community.Middleware.SentimentAnalysis.SentimentMiddleware(apiKey));
});
```
This helper method might be helpful for you.

```
 private static string GetAPIKey(BotConfiguration config, string serviceName)
        {
            string Key = string.Empty;

            foreach (var service in config.Services)
            {
                switch (service.Type)
                {
                    case ServiceTypes.Generic:
                        {
                            if (!(service is ConnectedService sentiment))
                            {
                                throw new InvalidOperationException("The generic service is not configured correctly in your '.bot' file.");
                            }

                            if (service.Name == serviceName)
                            {
                                var genericService = service as GenericService;
                                Key = genericService.Configuration["key"];
                            }

                            break;
                        }
                }
            }

            return Key;
        }
```

Note this requires an instance of `IConfiguration` passing to it.  Use the instance injected into the `Startup.cs` class.  

The configuration can be read from your `<Your Bot Name>.bot` file (under services section) which needs the following key

```
{
      "type": "generic",
      "name": "TextAnalytics",
      "configuration": {
        "key": "<YOUR SENTIMENT ANALYIS KEY HERE>"
      },
      "id": "2"
}
```

### Code Sample

Working sample will be available in samples directory, expect it before 1st Jan, 2019. 
