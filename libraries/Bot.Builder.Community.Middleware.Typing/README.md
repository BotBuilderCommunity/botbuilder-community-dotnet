## Typing Middleware
 
### Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [![NuGet version](https://img.shields.io/badge/NuGet-1.0.39-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Middleware.Typing/) |

### Description

This is part of the [Bot Builder Community Extensions](https://github.com/garypretty/botbuilder-community) project which contains various pieces of middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

This middleware will show a 'typing' event whenever a long running operation is occurring in your bot or other middeware components in the pipeline.

This is a good visual cue to the user that your bot is doing something.

### Installation

Available via NuGet package [Bot.Builder.Community.Middleware.Typing](https://www.nuget.org/packages/Bot.Builder.Community.Middleware.Typing/)

Install into your project using the following command in the package manager;
```
    PM> Install-Package Bot.Builder.Community.Middleware.Typing
```

### Usage

To ensure that users get appropriate feedback at all times, add this middleware to the start of the pipeline.

In your `Startup.cs` file, configure your bot type to use an instance of `TypingMiddleware`:

```
services.AddBot<Bot>((options) => {
    options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);
                
    options.Middleware.Add(new TypingMiddleware());
    // more middleware
});
```
