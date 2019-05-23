using System;
using System.Collections.Generic;
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
    public class WebhooksEnterpriseManager
    {

        public TwitterOptions Options { get; set; }

        public WebhooksEnterpriseManager(TwitterOptions context)
        {
            Options = context;
        }


        /// <summary>
        /// Retrieve a list of <see cref="WebhookRegistration"/> associated with the user (from the auth context).
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<WebhookRegistration>>> GetRegisteredWebhooks()
        {
            //string resourceUrl = $"https://api.twitter.com/1.1/account_activity/webhooks.json";
            var resourceUrl = $"https://api.twitter.com/1.1/account_activity/all/webhooks.json";

            HttpResponseMessage response;
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Add("Authorization", AuthHeaderBuilder.Build(Options, HttpMethod.Get, resourceUrl));

                response = await client.GetAsync(resourceUrl);
            }
            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var subs = JsonConvert.DeserializeObject<List<WebhookRegistration>>(jsonResponse);
                return new Result<List<WebhookRegistration>>(subs);
            }


            if (!string.IsNullOrEmpty(jsonResponse))
            {
                var err = JsonConvert.DeserializeObject<TwitterError>(jsonResponse);
                return new Result<List<WebhookRegistration>>(err);
            }
            else
            {
                return new Result<List<WebhookRegistration>>();
            }

        }

        /// <summary>
        /// Register a new webhook url using the current user (from the auth context).
        /// </summary>
        /// <param name="url">The webhook url to register.</param>
        /// <returns></returns>
        public async Task<Result<WebhookRegistration>> RegisterWebhook(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException(nameof(url));
            }

            //TODO: Provide a generic class to make Twitter API Requests.
            var urlParam = Uri.EscapeUriString(url);
            var resourceUrl = $"https://api.twitter.com/1.1/account_activity/webhooks.json?url={urlParam}";

            HttpResponseMessage response;
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Add("Authorization", AuthHeaderBuilder.Build(Options, HttpMethod.Post, resourceUrl));

                response = await client.PostAsync(resourceUrl, new StringContent(""));
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
            else
            {
                //TODO: Provide a way to return httpstatus code 

                return new Result<WebhookRegistration>();
            }

        }


        /// <summary>
        /// Unregister a webhook from current user (from the auth context) by Id.
        /// </summary>
        /// <param name="webhookId">The Webhook Id to unregister.</param>
        /// <returns></returns>
        public async Task<Result<bool>> UnregisterWebhook(string webhookId)
        {

            //TODO: Provide a generic class to make Twitter API Requests.

            var resourceUrl = $"https://api.twitter.com/1.1/account_activity/webhooks/{webhookId}.json";

            HttpResponseMessage response;
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Add("Authorization", AuthHeaderBuilder.Build(Options, HttpMethod.Delete, resourceUrl));

                response = await client.DeleteAsync(resourceUrl);
            }

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return new Result<bool>(true);
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                var err = JsonConvert.DeserializeObject<TwitterError>(jsonResponse);
                return new Result<bool>(err);
            }
            else
            {
                //TODO: Provide a way to return httpstatus code 

                return new Result<bool>();
            }

        }



    }
}
