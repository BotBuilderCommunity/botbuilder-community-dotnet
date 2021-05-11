## Rest Adaptive Dialogs for Bot Builder v4 .NET SDK

### Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [![NuGet version](https://img.shields.io/badge/NuGet-1.0.39-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Dialogs.Adaptive.Rest/) |

### Description
This is part of the [Bot Builder Community](https://github.com/garypretty/botbuilder-community) project which contains Bot Framework Components and other projects / packages for use with Bot Framework Composer and the Bot Builder .NET SDK v4.

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

	public class FindPetsByStatusAction : RestAction
	{
		[JsonProperty("$kind")]
        public new const string DeclarativeType = "PetStore.FindPetsByStatusAction";

		/// <summary>
        /// Initializes a new instance of the <see cref="FindPetsByStatusAction"/> class.
        /// </summary>
        /// <param name="callerPath">Caller Path.</param>
        /// <param name="callerLine">Caller Line.</param>
        [JsonConstructor]
        public FindPetsByStatusAction([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
        }

        [JsonProperty("baseUrl")]
        public StringExpression BaseUrl { get; set; }

        [JsonProperty("status")]
        public StringExpression Status { get; set; }

        public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {

            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            var dcState = dc.GetState();

            if (this.Disabled != null && this.Disabled.GetValue(dcState) == true)
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            var (instanceBaseUrl, instanceBaseUrlError) = this.BaseUrl.TryGetValue(dcState);
            if (instanceBaseUrlError != null)
            {
                throw new ArgumentException(instanceBaseUrlError);
            }

            var url = new System.Uri(new System.Uri(instanceBaseUrl + (instanceBaseUrl.EndsWith("/") ? "" : "/")), "pet").ToString();

            List<string> queryParameters = new List<string>();

            var (instanceStatus, instanceStatusError) = this.Status.TryGetValue(dcState);
            if (instanceStatusError != null)
            {
                throw new ArgumentException(instanceStatusError);
            }

            if (instanceStatus == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Status");
            }

            if (instanceStatus != null) {
                queryParameters.Add(string.Format("status={0}", System.Uri.EscapeDataString(instanceStatus)));
            }

            if (queryParameters.Count > 0)
            {
                url += "?" + string.Join("&", queryParameters);
            }

            // Create HTTP transport objects
            var httpRequest = new HttpRequestMessage();
            httpRequest.Method = new HttpMethod("GET");
            httpRequest.RequestUri = new System.Uri(url);

            // Serialize Request
            string requestContent = null;

            // Send Request
            if (cancellationToken != null)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            var httpResponse = await this.HttpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);

            HttpStatusCode statusCode = httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)statusCode != 200)
            {
                var ex = new HttpOperationException(string.Format("Operation returned an invalid status code '{0}'", statusCode));
                if (httpResponse.Content != null)
                {
                    _responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                else
                {
                    _responseContent = string.Empty;
                }
                ex.Request = new HttpRequestMessageWrapper(httpRequest, requestContent);
                ex.Response = new HttpResponseMessageWrapper(httpResponse, _responseContent);
                //if (_shouldTrace)
                //{
                //    ServiceClientTracing.Error(_invocationId, ex);
                //}
                httpRequest.Dispose();
                if (httpResponse != null)
                {
                    httpResponse.Dispose();
                }
                throw ex;
            }
            // Create Result
            var result = new HttpOperationResponse<FindByStatusResponse>();
            result.Request = httpRequest;
            result.Response = httpResponse;
            // Deserialize Response
            if ((int)statusCode == 200)
            {
                _responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    result.Body = SafeJsonConvert.DeserializeObject<FindByStatusResponse>(_responseContent);
                }
                catch (JsonException ex)
                {
                    httpRequest.Dispose();
                    if (httpResponse != null)
                    {
                        httpResponse.Dispose();
                    }
                    throw new SerializationException("Unable to deserialize the response.", _responseContent, ex);
                }
            }

            if (this.ResultProperty != null)
            {
                dcState.SetValue(this.ResultProperty.GetValue(dcState), result.Body);
            }

            return await dc.EndDialogAsync(result: result, cancellationToken: cancellationToken);
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
                            new FindPetsByStatusAction() {
                                BaseUrl = "=settings.baseUrl",
                                Status = "available",
                                resultProperty = "conversation.PetsByStatus",
                            },
                            new SendActivity("# ${ count(conversation.PetsByStatus) } are avalaible",
                        }
                    }
                }

```
