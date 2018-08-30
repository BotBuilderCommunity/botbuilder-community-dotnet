## Spell Check Middleware 
 
### Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [![NuGet version](https://img.shields.io/badge/NuGet-1.0.39-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Middleware.SpellCheck/) |
 
### Description

This is part of the [Bot Builder Community Extensions](https://github.com/garypretty/botbuilder-community) project which contains various pieces of middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

This middleware will spell check inbound text using Cognitive Services Spell Check and therefore requires a key. There is a free tier which meets most demo/PoC needs.  You can get more info at https://azure.microsoft.com/en-gb/services/cognitive-services/spell-check/

The implementation is naive at the moment in that it assumes that the suggestions are correct and replaces inbound text automatically. If you have more sophisticated needs please feel free to contribute!

### Installation

Available via NuGet package [Bot.Builder.Community.Middleware.SpellCheck](https://www.nuget.org/packages/Bot.Builder.Community.Middleware.SpellCheck/) 

Install into your project using the following command in the package manager;
```
    PM> Install-Package Bot.Builder.Community.Middleware.SpellCheck
```

### Usage

Typically I would place this middleware at the end of the pipeline, but it will work anywhere.  


```
services.AddBot<Bot>((options) => {
    options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);
	
	// more middleware
	options.Middleware.Add(new SpellCheckMiddleware(Configuration));
});
```

This requires an instance of `IConfiguration` passing to it.  Use the instance injected into the `Startup.cs` class.  

The configuration can be read from your `appsettings.json` file which needs the following keys

Note: SpellCheckCountryCode and SpellCheckMarket are optional for languages other than en/en-US.  For a full list of supported country and market codes see https://docs.microsoft.com/en-us/azure/cognitive-services/bing-spell-check/bing-spell-check-supported-languages.

```
{
  "SpellCheckKey": "<YOUR SPELL CHECK KEY HERE>",
  "SpellCheckCountryCode": "sv",
  "SpellCheckMarket": "sv-SE" 
}
```
