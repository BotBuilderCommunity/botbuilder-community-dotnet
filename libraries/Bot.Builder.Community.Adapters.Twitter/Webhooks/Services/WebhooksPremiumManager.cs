using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Authentication;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Models;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Models.Twitter;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Twitter.Webhooks.Services
{

    /// <summary>
    /// This class will help in managing the webhooks registrations.
    /// </summary>
    public class WebhooksPremiumManager
    {
        /// <summary>
        /// Gets or sets the auth context.
        /// </summary>
        /// <value>The auth context.</value>
        public TwitterOptions Options { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Bot.Builder.Community.Adapters.Twitter.Webhooks.Services.WebhooksPremiumManager"/> class.
        /// </summary>
        /// <param name="context">Context.</param>
        public WebhooksPremiumManager(TwitterOptions context)
        {
            Options = context;
        }


        /// <summary>
        /// Retrieve a list of <see cref="WebhookRegistration"/> associated with the user (from the auth context).
        /// </summary>
        /// <returns></returns>
        public async Task<Result<WebhookResult>> GetRegisteredWebhooks()
        {
            var resourceUrl = $"https://api.twitter.com/1.1/account_activity/all/webhooks.json";

            HttpResponseMessage response;
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Add("Authorization", AuthHeaderBuilder.Build(Options, HttpMethod.Get, resourceUrl));

                response = await client.GetAsync(resourceUrl).ConfigureAwait(false);
            }
            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var subs = JsonConvert.DeserializeObject<WebhookResult>(jsonResponse);
                return new Result<WebhookResult>(subs);
            }

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                var err = JsonConvert.DeserializeObject<TwitterError>(jsonResponse);
                return new Result<WebhookResult>(err);
            }
            return new Result<WebhookResult>();
        }

        /// <summary>
        /// Register a new webhook url using the current user (from the auth context).
        /// </summary>
        /// <param name="url">The webhook url to register.</param>
        /// <param name="environmentName">Environment name</param>
        /// <returns></returns>
        public async Task<Result<WebhookRegistration>> RegisterWebhook(string url, string environmentName)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException(nameof(url));
            }

            //TODO: Provide a generic class to make Twitter API Requests.
            var urlParam = Uri.EscapeUriString(url);
            var resourceUrl = $"https://api.twitter.com/1.1/account_activity/all/{environmentName}/webhooks.json?url={urlParam}";

            HttpResponseMessage response;
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Add("Authorization", AuthHeaderBuilder.Build(Options, HttpMethod.Post, resourceUrl));

                response = await client.PostAsync(resourceUrl, new StringContent("")).ConfigureAwait(false);
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var sub = JsonConvert.DeserializeObject<WebhookRegistration>(jsonResponse);
                return new Result<WebhookRegistration>(sub);
            }

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                var err = JsonConvert.DeserializeObject<TwitterError>(jsonResponse);
                return new Result<WebhookRegistration>(err);
            }

            return new Result<WebhookRegistration>();

        }


        /// <summary>
        /// Unregister a webhook from current user (from the auth context) by Id.
        /// </summary>
        /// <param name="webhookId">The Webhook Id to unregister.</param>
        /// <returns></returns>
        public async Task<Result<bool>> UnregisterWebhook(string webhookId, string environmentName)
        {

            //TODO: Provide a generic class to make Twitter API Requests.

            var resourceUrl = $"https://api.twitter.com/1.1/account_activity/all/{environmentName}/webhooks/{webhookId}.json";

            HttpResponseMessage response;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", AuthHeaderBuilder.Build(Options, HttpMethod.Delete, resourceUrl));
                response = await client.DeleteAsync(resourceUrl).ConfigureAwait(false);
            }

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return new Result<bool>(true);
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(jsonResponse))
            {
                return new Result<bool>();
            }

            var err = JsonConvert.DeserializeObject<TwitterError>(jsonResponse);
            return new Result<bool>(err);

        }
    }
}
