## Form Flow

### Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | | [![NuGet version](https://img.shields.io/badge/NuGet-1.0.0-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Dialogs.FormFlow/) |

### Description
This is part of the [Bot Builder Community Extensions](https://github.com/BotBuilderCommunity) project which contains various pieces of middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

This library is a netstandard2.0 port of [Microsoft.Bot.Builder.FormFlow](https://github.com/Microsoft/BotBuilder-v3/tree/master/CSharp/Library/Microsoft.Bot.Builder/FormFlow)  The Bot Builder V3 FormFlow documentation is useful for this library as well, just take note of the namespace changes.

### Installation

Available via NuGet package [Bot.Builder.Community.Dialogs.FormFlow](https://www.nuget.org/packages/Bot.Builder.Community.Dialogs.FormFlow/)

Install into your project using the following command in the package manager;
```
    PM> Install-Package Bot.Builder.Community.Dialogs.FormFlow
```

### Sample

A sample for using FormFlow with the v4 SDK can be found [here](../../samples/Form%20Flow%20Sample)

### Usage

FormDialog inherits from ComponentDialog

FormFlow dialogs can be added to a DialogSet:
```cs
_dialogs.Add(FormDialog.FromForm(SandwichOrder.BuildForm));
```

Or added to a component dialog:
```cs
public HotelsDialog() : base(nameof(HotelsDialog))
{
    var hotelsFormDialog = FormDialog.FromForm(this.BuildHotelsForm, FormOptions.PromptInStart);
    base.AddDialog(hotelsFormDialog);
}
```


### References
Take note of namespace changes from **Microsoft.Bot.Builder.FormFlow** to **Bot.Builder.Community.Dialogs.FormFlow**

* [Basic features of FormFlow](https://docs.microsoft.com/en-us/azure/bot-service/dotnet/bot-builder-dotnet-formflow)

* [Advanced features of FormFlow](https://docs.microsoft.com/en-us/azure/bot-service/dotnet/bot-builder-dotnet-formflow-advanced)

### License

Licensed under the MIT License.

