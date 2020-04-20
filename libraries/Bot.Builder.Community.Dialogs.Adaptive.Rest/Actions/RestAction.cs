using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Rest;
using Microsoft.Rest.TransientFaultHandling;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Dialogs.Adaptive.Rest.Actions
{
    /// <summary>
    /// RestAction is the abstraction for accessing REST operations and their payload data types.
    /// </summary>
    public abstract class RestAction : Dialog, IDisposable
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Community.RestAction";

        /// <summary>
        /// Indicates whether the ServiceClient has been disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestAction"/> class.
        /// </summary>
        /// <param name="callerPath">Caller Path.</param>
        /// <param name="callerLine">Caller Line.</param>
        [JsonConstructor]
        public RestAction([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : this(new HttpClientHandler())
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestAction"/> class.
        /// </summary>
        /// <param name="rootHandler">Base HttpClientHandler.</param>
        /// <param name="handlers">List of handlers from top to bottom (outer handler is the first in the list)</param>
        protected RestAction(params DelegatingHandler[] handlers)
            : this(new HttpClientHandler(), handlers)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestAction"/> class.
        /// </summary>
        /// <param name="rootHandler">Base HttpClientHandler.</param>
        /// <param name="handlers">List of handlers from top to bottom (outer handler is the first in the list)</param>
        protected RestAction(HttpClientHandler rootHandler, params DelegatingHandler[] handlers )
        {
            this.InitializeHttpClient(rootHandler, handlers);
        }

        /// <summary>
        /// Gets the HTTP pipelines for the given action.
        /// </summary>
        /// <returns>The client's HTTP pipeline.</returns>
        /// <value>
        /// The HTTP pipelines.
        /// </value>
        public virtual IEnumerable<HttpMessageHandler> HttpMessageHandlers
        {
            get
            {
                var handler = this.FirstMessageHandler;

                while (handler != null)
                {
                    yield return handler;

                    DelegatingHandler delegating = handler as DelegatingHandler;
                    handler = delegating != null ? delegating.InnerHandler : null;
                }
            }
        }

        /// <summary>
        /// Gets the UserAgent collection which can be augmented with custom
        /// user agent strings.
        /// </summary>
        public virtual HttpHeaderValueCollection<ProductInfoHeaderValue> UserAgent => this.HttpClient.DefaultRequestHeaders.UserAgent;

        /// <summary>
        /// Gets or sets an optional expression which if is true will disable this action.
        /// </summary>
        /// <example>
        /// "user.age > 18".
        /// </example>
        /// <value>
        /// A boolean expression.
        /// </value>
        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; }

        /// <summary>
        /// Gets or sets the property expression to store the query response.
        /// </summary>
        /// <value>
        /// The property expression to store the query response in.
        /// </value>
        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

        /// <summary>
        /// Gets or sets reference to the first HTTP handler (which is the start of send HTTP
        /// pipeline).
        /// </summary>
        protected HttpMessageHandler FirstMessageHandler { get; set; }

        /// <summary>
        /// Gets or sets reference to the innermost HTTP handler (which is the end of send HTTP
        /// pipeline).
        /// </summary>
        protected HttpClientHandler HttpClientHandler { get; set; }

        /// <summary>
        /// Gets or sets the HttpClient used for making HTTP requests.
        /// </summary>
        protected HttpClient HttpClient { get; set; }

        /// <summary>
        /// Sets retry policy for the client.
        /// </summary>
        /// <param name="retryPolicy">Retry policy to set.</param>
        public virtual void SetRetryPolicy(RetryPolicy retryPolicy)
        {
            if (retryPolicy == null)
            {
                retryPolicy = new RetryPolicy<TransientErrorIgnoreStrategy>(0);
            }

            RetryDelegatingHandler delegatingHandler = this.HttpMessageHandlers.OfType<RetryDelegatingHandler>().FirstOrDefault();
            if (delegatingHandler != null)
            {
                delegatingHandler.RetryPolicy = retryPolicy;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Dispose the ServiceClient.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the HttpClient and Handlers.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to releases only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                this.disposed = true;

                // Dispose the client
                this.HttpClient.Dispose();
                this.HttpClient = null;
                this.FirstMessageHandler = null;
                this.HttpClientHandler = null;
            }
        }

        private void InitializeHttpClient(HttpClientHandler httpClientHandler, params DelegatingHandler[] handlers)
        {
            this.HttpClientHandler = httpClientHandler;
            DelegatingHandler currentHandler = new RetryDelegatingHandler();
            currentHandler.InnerHandler = this.HttpClientHandler;

            if (handlers != null)
            {
                for (int i = handlers.Length - 1; i >= 0; --i)
                {
                    DelegatingHandler handler = handlers[i];

                    // Non-delegating handlers are ignored since we always 
                    // have RetryDelegatingHandler as the outer-most handler
                    while (handler.InnerHandler is DelegatingHandler)
                    {
                        handler = handler.InnerHandler as DelegatingHandler;
                    }

                    handler.InnerHandler = currentHandler;
                    currentHandler = handlers[i];
                }
            }

            var newClient = new HttpClient(currentHandler, false);
            this.FirstMessageHandler = currentHandler;
            this.HttpClient = newClient;
            Type type = this.GetType();
            this.HttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(
                type.FullName,
                this.GetAssemblyVersion()));
        }

        /// <summary>
        /// Get the assembly version of a service client.
        /// </summary>
        /// <returns>The assembly version of the client.</returns>
        private string GetAssemblyVersion()
        {
            Type type = this.GetType();
            string version =
                type
                    .GetTypeInfo()
                    .Assembly
                    .FullName
                    .Split(',')
                    .Select(c => c.Trim())
                    .First(c => c.StartsWith("Version=", StringComparison.OrdinalIgnoreCase))
                    .Substring("Version=".Length);
            return version;
        }
    }
}
