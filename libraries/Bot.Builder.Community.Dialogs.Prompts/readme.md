## Prompts for Bot Builder v4 .NET SDK

### Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [![NuGet version](https://img.shields.io/badge/NuGet-1.0.39-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Dialogs.Location/) |

### Description
This is part of the [Bot Builder Community Extensions](https://github.com/garypretty/botbuilder-community) project which contains various pieces of middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

This package contains additional prompts, beyond those offered out of the box by the Bot Builder v4 .NET SDK.

Currently the following Prompts are available;

* [Number with Unit (currency, temperature, dimension, age)](#number-with-unit-prompt)

### Installation

Available via NuGet package [Bot.Builder.Community.Dialogs.Prompts](https://www.nuget.org/packages/Bot.Builder.Community.Dialogs.Prompts/)

Install into your project using the following command in the package manager;
```
    PM> Install-Package Bot.Builder.Community.Dialogs.Prompts
```

### Usage

Below is example usage for each of the Prompts 

#### Number with Unit Prompt

The Number with Unit Prompt allows you to prompt for the following types of number;

* Currency
* Temperature
* Age
* Dimension (e.g. miles / meters)

To use the Prompt, create a new instance of the Prompt, specifying the type of Prompt (e.g. currency) using the second parameter.
Once you have created the instance of your Prompt, you can add it to your list of dialogs (e.g. within a ComponentDialog).

```cs

            var numberPrompt = new NumberWithUnitPrompt(
					"CurrencyPrompt", 
					NumberWithUnitPromptType.Currency, 
					defaultLocale: Culture.English);

```

Then, you can call the bot by specifying your PromptOptions and calling PromptAsync.

```cs

			var options = new PromptOptions 
				{ 
					Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a currency." } 
				};
            await dc.PromptAsync("CurrencyPrompt", options, cancellationToken);

```

The Prompt will return a NumberWithUnitResult object. Below is an example of how you might use this result.

```cs

			var currencyPromptResult = (NumberWithUnitResult)results.Result;
			await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received Value: {currencyPromptResult.Value}, Unit: {currencyPromptResult.Unit}"), cancellationToken);

```

