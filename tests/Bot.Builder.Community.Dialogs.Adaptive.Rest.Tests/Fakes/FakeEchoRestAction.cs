using System;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Bot.Builder.Community.Dialogs.Adaptive.Rest.Actions;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Dialogs.Adaptive.Rest.Tests.Fakes
{
    public class FakeEchoRestAction : RestAction
    {
        [JsonProperty("$kind")]
        public new const string DeclarativeType = "Tests.FakeRestAction";

        private HttpRequestMessage _httpRequest;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestAction"/> class.
        /// </summary>
        /// <param name="callerPath">Caller Path.</param>
        /// <param name="callerLine">Caller Line.</param>
        [JsonConstructor]
        public FakeEchoRestAction([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(new MirrorDelegatingHandler())
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        [JsonProperty("content")]
        public StringExpression Content { get; set; }

        public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            string url = "http://tempuri.norg";
             
            // Create HTTP transport objects
            this._httpRequest = null;

            this._httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
            };

            var dcState = dc.GetState();

            // Set content
            if (Content != null)
            {
                this._httpRequest.Content = new StringContent(this.Content.GetValue(dcState));
            }

            // Set Headers
            this._httpRequest.Headers.Add("x-ms-version", "2013-11-01");

            // Set Credentials
            cancellationToken.ThrowIfCancellationRequested();
            var response = await this.HttpClient.SendAsync(this._httpRequest, cancellationToken).ConfigureAwait(false);

            var result = await response.Content.ReadAsStringAsync();

            if (this.ResultProperty != null)
            {
                dcState.SetValue(this.ResultProperty.GetValue(dcState), result);
            }

            // return the actionResult as the result of this operation
            return await dc.EndDialogAsync(result: result, cancellationToken: cancellationToken);
        }
    }
}
