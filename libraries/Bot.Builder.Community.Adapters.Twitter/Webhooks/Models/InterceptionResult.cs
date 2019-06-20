using System.Net;
using System.Net.Http;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Services;

namespace Bot.Builder.Community.Adapters.Twitter.Webhooks.Models
{

    /// <summary>
    /// HttpRequestMessage Interception Result, See: <see cref="WebhookInterceptor"/>.
    /// </summary>
    public class InterceptionResult
    {
        /// <summary>
        /// Determines of the request is handled internally, in this case, you should return <see cref="Response"/>.
        /// </summary>
        public bool IsHandled { get; internal set; }

        /// <summary>
        /// The <see cref="HttpResponseMessage"/> to return to the client if <see cref="IsHandled"/> is true.
        /// If  <see cref="IsHandled"/> is false, this will be empty <see cref="HttpResponseMessage"/> with status code: <see cref="HttpStatusCode.OK"/>.
        /// </summary>
        public string Response { get; internal set; }

        /// <summary>
        /// The original request message, for reference purposes only.
        /// </summary>
        //public HttpRequestMessage RequestMessage { get; internal set; }

        private InterceptionResult()
        {

        }

        internal static InterceptionResult CreateHandled(string response)
        {
            return new InterceptionResult
            {
                IsHandled = true,
                //RequestMessage = request,
                Response = response
            };
        }

        internal static InterceptionResult CreateUnhandled()
        {
            return new InterceptionResult
            {
                IsHandled = false,
                //RequestMessage = request,
                Response = string.Empty
            };
        }

    }
}
