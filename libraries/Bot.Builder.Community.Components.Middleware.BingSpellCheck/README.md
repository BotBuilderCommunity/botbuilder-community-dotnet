***Need to update ***

## Bing Spell Check Middleware 
 
### Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [![NuGet version](https://img.shields.io/badge/NuGet-1.0.61-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Middleware.SpellCheck/) |
 
### Description

This is part of the [Bot Builder Community](https://github.com/garypretty/botbuilder-community) project which contains Bot Framework Components and other projects / packages for use with Bot Framework Composer and the Bot Builder .NET SDK v4.

This middleware will Bing spell check inbound text using Bing Services Spell Check and therefore requires a key.

The implementation is naive at the moment in that it assumes that the suggestions are correct If you replaces inbound text automatically set property IsOverwrite = true. 

### Composer component installation

1. Navigate to the Bot Framework Composer **Package Manager**.
2. Change the filter to **Community packages**.
3. Search for 'TextRecognizer' and install **Bot.Builder.Community.Components.Middleware.TextRecognizer**

![image](https://user-images.githubusercontent.com/16264167/121935397-aedd2500-cd48-11eb-8f78-b6f72433a119.png)



```json
"components": [
{
    "name": "Bot.Builder.Community.Components.Middleware.BingSpellCheck",
    "settingsPrefix": "Bot.Builder.Community.Components.Middleware.BingSpellCheck"
}
```

```json
 "Bot.Builder.Community.Components.Middleware.BingSpellCheck": {
    "IsEnabled": true,
    "Key": "Bing Key",
    "MarketCode": "en-US",
    "IsOverwrite": false
  }
```

