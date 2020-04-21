## Rest Adaptive Dialogs for Bot Builder v4 .NET SDK

### Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [![NuGet version](https://img.shields.io/badge/NuGet-1.0.39-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Dialogs.Adaptive.Rest/) |

### Description
This is part of the [Bot Builder Community Extensions](https://github.com/garypretty/botbuilder-community) project which contains various pieces of middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

This package contains additional adaptive dialogs, beyond those offered out of the box by the Bot Builder v4 .NET SDK.

Currently the following Adaptive Dialogs are available;

| Actions | Description |
| ------ | ------ |
| [RestAction](#RestAction) | RestAction is the abstraction for accessing REST operations and their payload data types. |

### Installation

Available via NuGet package [Bot.Builder.Community.Dialogs.Adaptive.Rest](https://www.nuget.org/packages/Bot.Builder.Community.Dialogs.Adaptive.Rest/)

Install into your project using the following command in the package manager;
```
    PM> Install-Package Bot.Builder.Community.Dialogs.Adaptive.Rest
```

### Usage

Below is example usage for each of the Actions 

#### RestAction

The RestAction is an abstract action allows you to implement quickly based rest actions clients.


Internally the RestAction uses the [Microsoft Rest Client Runtime](https://github.com/stankovski/AutoRest/tree/master/ClientRuntimes/CSharp/Microsoft.Rest.ClientRuntime) and mimic the serviceclient base class.

To use the Action, create a new class inherited from RestAction.

```cs

	public class GetPetByIdAction : RestAction
	{
		[JsonProperty("$kind")]
        public new const string DeclarativeType = "PetStore.GetPetByIdAction";

		/// <summary>
        /// Initializes a new instance of the <see cref="GetPetByIdAction"/> class.
        /// </summary>
        /// <param name="callerPath">Caller Path.</param>
        /// <param name="callerLine">Caller Line.</param>
        [JsonConstructor]
        public GetPetByIdAction([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
        }

        public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
			... Do Stuff ...
		}
	}

```

Once you have created the instance of your RestAction implementation, you can add it to your list of dialogs (e.g. within a AdaptiveDialog) or use it within a declarative dialog.

```cs

	var dialog = new AdaptiveDialog()
            {
                AutoEndDialog = true,
                Triggers = new List<OnCondition>() {
                    new OnBeginDialog() {
                        Actions = new List<Dialog>() {
                            new GetPetByIdAction() {
                                BaseUrl = "=settings.baseUrl",
                                PetId = "=turn.petId",
                                resultProperty = "conversation.currentPet",
                            },
                            new SendActivity("# Pet name of id ${turn.petId} is ${conversation.currentPet.name"),
                        }
                    }
                }

```
