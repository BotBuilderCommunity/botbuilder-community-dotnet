## Prompts for Bot Builder v4 .NET SDK

### Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [![NuGet version](https://img.shields.io/badge/NuGet-1.0.39-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Dialogs.Prompts/) |

### Description
This is part of the [Bot Builder Community Extensions](https://github.com/garypretty/botbuilder-community) project which contains various pieces of middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

This package contains additional prompts, beyond those offered out of the box by the Bot Builder v4 .NET SDK.

Currently the following Prompts are available;

| Prompt | Description |
| ------ | ------ |
| [Number with Unit](#number-with-unit-prompt) | Prompt a user for Currency, Temperature, Age, Dimension (distance). |
| [Number with Type](#number-with-Type-prompt) | Prompt a user for Ordinal, Temperature, Percentage,NumberRange,Number. |
| [Phone Number](#Phone-Number-prompt) | Prompt a user for PhoneNumber. |
| [SocialMedia](#Social-Medial-prompt) | Prompt a user for find mention,Hashtag. |
| [Email](#Email-prompt) | Prompt a user for Email. |
| [Guid](#Guid-prompt) | Prompt a user for Guid. |
| [InternetProtocol](#internet-protocol-prompt) | Prompt a user for IpAddress,Url. |
| [Adaptive Card](#adaptive-card-prompt) | Prompt a user using an Adaptive Card. |

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

Internally the Prompt uses the [Microsoft Text Recognizers](https://github.com/Microsoft/Recognizers-Text/tree/master/.NET) NumberWithPrompt recognizer.

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

The Prompt will return a NumberWithUnitResult object. This object contains a Value (dynamic) and a Unit (string). 
For example, if a user enters "twenty three dollars" when you are using the Currency prompt type, the resulting NumberWithUnitResult object will have Unit: "Dollar", Value: "23".
Below is an example of how you might use this result.

```cs

			var currencyPromptResult = (NumberWithUnitResult)results.Result;
			await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received Value: {currencyPromptResult.Value}, Unit: {currencyPromptResult.Unit}"), cancellationToken);

```

### Adaptive Card Prompt

#### Features

* Includes validation for specified required input fields
* Displays custom message if user replies via text and not card input
* Ensures input is only valid if it comes from the appropriate card (not one shown previous to prompt)

#### Usage

```csharp
// Load an adaptive card
var cardPath = Path.Combine("../Resources/", "adaptiveCard.json");
var cardJson = File.ReadAllText(cardPath);

// Configure settings - All optional
var promptSettings = new AdaptiveCardPromptSettings() {
    Card: card,
    InputFailMessage: 'Please fill out the adaptive card',
    RequiredInputIds: [
        'inputA',
        'inputB',
    ],
    MissingRequiredInputsMessage: 'The following inputs are required',
    AttemptsBeforeCardRedsiplayed: 5,
    PromptId: 'myCustomId'
}

// Initialize the prompt
var adaptiveCardPrompt = new AdaptiveCardPrompt('adaptiveCardPrompt', null, promptSettings);

// Add the prompt to your dialogs
dialogSet.add(adaptiveCardPrompt);

// Call the prompt
return await stepContext.PromptAsync(nameof(AdaptiveCardPrompt), new PromptOptions()
{
    Prompt = new Activity { Attachments = new List<Attachment>() { cardAttachment } }
});

// Use the result
var result = stepContext.Result as JObject;
var resultArray = result.Properties().Select(p => $"Key: {p.Name}  |  Value: {p.Value}").ToList();
var resultString = string.Join("\n", resultArray);
```

#### Adaptive Cards

Card authors describe their content as a simple JSON object. That content can then be rendered natively inside a host application, automatically adapting to the look and feel of the host. For example, Contoso Bot can author an Adaptive Card through the Bot Framework, and when delivered to Cortana, it will look and feel like a Cortana card. When that same payload is sent to Microsoft Teams, it will look and feel like Microsoft Teams. As more host apps start to support Adaptive Cards, that same payload will automatically light up inside these applications, yet still feel entirely native to the app. Users win because everything feels familiar. Host apps win because they control the user experience. Card authors win because their content gets broader reach without any additional work.

The Bot Framework provides support for Adaptive Cards.  See the following to learn more about Adaptive Cards.

- [Adaptive card](http://adaptivecards.io)
- [Send an Adaptive card](https://docs.microsoft.com/en-us/azure/bot-service/nodejs/bot-builder-nodejs-send-rich-cards?view=azure-bot-service-3.0&viewFallbackFrom=azure-bot-service-4.0#send-an-adaptive-card)

##### Getting Input Data From Adaptive Cards

In a `TextPrompt`, the user response is returned in the `Activity.Text` property, which only accepts strings. Because Adaptive Cards can contain multiple inputs, the user response is sent as a JSON object in `Activity.Value`, like so:

```json
{
    [...]
    "value": {
        "inputA": "response A",
        "inputB": "response B",
        [...etc]
    }
}
```

Because of this, it can be a little difficult to gather user input using an Adaptive Card within a dialog. The `AdaptiveCardPrompt` allows you to do so easily and returns the JSON object user response in `stepContext.Result`.

#### Number with Type Prompt

The Number with Type Prompt allows you to prompt for the following types of number;

* Ordinal
* Percentage
* NumberRange
* Number

Internally the Prompt uses the [Microsoft Text Recognizers](https://github.com/Microsoft/Recognizers-Text/tree/master/.NET) NumberWithType recognizer.

To use the Prompt, create a new instance of the Prompt, specifying the type of Prompt (e.g. Ordinal) using the second parameter.
Once you have created the instance of your Prompt, you can add it to your list of dialogs (e.g. within a ComponentDialog).

```cs

		var numberPrompt = new NumberWithTypePrompt("OrdinalPrompt", 
							NumberWithTypePromptType.Ordinal, defaultLocale: Culture.English);

```

Then, you can call the bot by specifying your PromptOptions and calling PromptAsync.

```cs

		var options = new PromptOptions 
			{ 
				Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a Ordinal number Info." } 
			};
		await dc.PromptAsync("OrdinalPrompt", options, cancellationToken);

```

The Prompt will return a NumberWithTypeResult object. This object contains a Value (dynamic) and a Text (string). 
For example, if a user enters "one millionth" when you are using the Ordinal prompt type, the resulting NumberWithTypeResult object will have Value: "1000000", Text: "one millionth".
Below is an example of how you might use this result.

```cs

	var OrdinalPromptResult = (NumberWithTypeResult)results.Result;
	await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received Value: {OrdinalPromptResult.Value}, Text: {OrdinalPromptResult.Text}"), cancellationToken);

```

#### Phone Number prompt

The PhoneNumberPrompt will extract a phone number from a message from the user;

To use the Prompt, create a new instance of the Prompt , Once you have created the instance of your Prompt, you can add it to your list of dialogs (e.g. within a ComponentDialog).
```cs

		var numberPrompt = new PhoneNumberPrompt(nameof(PhoneNumberPrompt), defaultLocale: Culture.English);
```

Then, you can call the bot by specifying your PromptOptions and calling PromptAsync.

```cs

		var options = new PromptOptions 
		{ 
			Prompt = new Activity { Type = ActivityTypes.Message, Text = "Hey send your phone number." } 
		};
		await dc.PromptAsync(nameof(PhoneNumberPrompt), options, cancellationToken);

```

The Prompt will return a result as string.
For example, if a user enters "my phone number is +91XXXXXXXXX" when you are using the PhoneNumber prompt type, the resulting phonenumber is "+91XXXXXXXXX"

```cs

	var phonenumberPromptResult = (string)results.Result;
	await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received Value: {phonenumberPromptResult}"), cancellationToken);

```

#### Social Medial prompt

The SocialMediaPrompt will extract one of the following types based on which SocialMediaPromptType enum value is passed in:

* Mention
* Hashtag

To use the Prompt, create a new instance of the Prompt, specifying the type of Prompt (e.g. Hashtag) using the second parameter.
Once you have created the instance of your Prompt, you can add it to your list of dialogs (e.g. within a ComponentDialog).

```cs

	var hashMediaPrompt = new SocialMediaPrompt(nameof(SocialMediaPrompt), SocialMediaPromptType.Hashtag, defaultLocale: Culture.English);
```

Then, you can call the bot by specifying your PromptOptions and calling PromptAsync.

```cs

		var options = new PromptOptions 
		{ 
			Prompt = new Activity { Type = ActivityTypes.Message, Text = "What are some of your favorite trends" } 
		};
		await dc.PromptAsync(nameof(SocialMediaPrompt), options, cancellationToken);

```

The Prompt will return a result as string.
For example, if a user enters "Trends? Does #WM35 count?" when you are using the SocialMediaPrompt prompt type, the resulting Hashtag is "#WM35"

```cs

	var hashResult = (string)results.Result;
	await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received HashTag: {hashResult}"), cancellationToken);

```

#### Email prompt

The EmailPrompt will extract an email address from a message from the user.

To use the Prompt, create a new instance of the Prompt , Once you have created the instance of your Prompt, you can add it to your list of dialogs (e.g. within a ComponentDialog).

```cs

	var numberPrompt = new EmailPrompt(nameof(EmailPrompt), defaultLocale: Culture.English);
```

Then, you can call the bot by specifying your PromptOptions and calling PromptAsync.

```cs

		var options = new PromptOptions 
		{ 
			Prompt = new Activity { Type = ActivityTypes.Message, Text = "Hey send your email." } 
		};
		await dc.PromptAsync(nameof(EmailPrompt), options, cancellationToken);

```

The Prompt will return a result as string.
For example, if a user enters "my email is bot@botbuilder-community.com" when you are using the Email prompt type, the resulting Email is "bot@botbuilder-community.com"

```cs

	var emailpromptresult = (string)results.Result;
	await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received Value: {emailpromptresult}"), cancellationToken);

```

#### Guid prompt

The GuidPrompt will extract a GUID from a message from the user.

To use the Prompt, create a new instance of the Prompt , Once you have created the instance of your Prompt, you can add it to your list of dialogs (e.g. within a ComponentDialog).

```cs

		var numberPrompt = new GuidPrompt(nameof(GuidPrompt), defaultLocale: Culture.English);
```

Then, you can call the bot by specifying your PromptOptions and calling PromptAsync.

```cs

		var options = new PromptOptions 
		{ 
			Prompt = new Activity { Type = ActivityTypes.Message, Text = "Send your azure id" } 
		};
		await dc.PromptAsync(nameof(GuidPrompt), options, cancellationToken);

```

The Prompt will return a result as string.
For example, if a user enters "my azure id is 7d7b0205-9411-4a29-89ac-b9cd905886fa" when you are using the Guid prompt type, the resulting Guid is "7d7b0205-9411-4a29-89ac-b9cd905886fa"

```cs

	var guidresult = (string)results.Result;
	await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received Value: {guidresult}"), cancellationToken);

```

#### Internet Protocol Prompt

The InternetTypePrompt will extract one of the following types based on which InternetTypePromptType enum value is passed in:

* IpAddress
* Url

To use the Prompt, create a new instance of the Prompt, specifying the type of Prompt (e.g. IpAddress) using the second parameter.
Once you have created the instance of your Prompt, you can add it to your list of dialogs (e.g. within a ComponentDialog).

```cs

	var internetAddressPrompt = new InternetProtocolPrompt(nameof(InternetProtocolPrompt), InternetProtocolPromptType.IpAddress, defaultLocale: Culture.English);
```

Then, you can call the bot by specifying your PromptOptions and calling PromptAsync.

```cs

		var options = new PromptOptions 
		{ 
			Prompt = new Activity { Type = ActivityTypes.Message, Text = "send your pc ip address" } 
		};
		await dc.PromptAsync(nameof(SocialMediaPrompt), options, cancellationToken);

```

The Prompt will return a result as string.
For example, if a user enters "my ip address is 192.0.0.1" when you are using the InternetTypePrompt prompt type, the resulting IpAddress is "192.0.0.1"

```cs

	var internetResult = (string)results.Result;
	await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received ip address: {internetResult}"), cancellationToken);

```