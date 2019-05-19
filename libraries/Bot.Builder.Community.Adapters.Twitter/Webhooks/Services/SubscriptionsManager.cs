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
    /// This class will help in managing the webhook subscription.
    /// </summary>
    public class SubscriptionsManager
    {
        public TwitterOptions Options { get; set; }

        public SubscriptionsManager(TwitterOptions context)
        {
            Options = context;
        }


        /// <summary>
        /// Subscribe current user (from the auth context) to a webhook by Id.
        /// </summary>
        /// <param name="environmentName">The name of the environment to subscribe to.</param>
        /// <returns>true indicates successful subscription.</returns>
        public async Task<Result<bool>> Subscribe(string environmentName)
        {

            if (string.IsNullOrEmpty(environmentName))
            {
                throw new ArgumentException(nameof(environmentName));
            }
            var resourceUrl = $"https://api.twitter.com/1.1/account_activity/all/{environmentName}/subscriptions.json";
            HttpResponseMessage response;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", AuthHeaderBuilder.Build(Options, HttpMethod.Post, resourceUrl));

                response = await client.PostAsync(resourceUrl, new StringContent("")).ConfigureAwait(false);
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
            return new Result<bool>();

        }

        /// <summary>
        /// Checks if the current user (from the auth context) is subscribed to a webhook by Id.
        /// </summary>
        /// <param name="environmentName"></param>
        /// <returns>true indicates existed subscription.</returns>
        public async Task<Result<bool>> CheckSubscription(string environmentName)
        {

            if (string.IsNullOrEmpty(environmentName))
            {
                throw new ArgumentException(nameof(environmentName));
            }
            
            var resourceUrl = $"https://api.twitter.com/1.1/account_activity/all/{environmentName}/subscriptions.json";

            HttpResponseMessage response;
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Add("Authorization", AuthHeaderBuilder.Build(Options, HttpMethod.Get, resourceUrl));

                response = await client.GetAsync(resourceUrl).ConfigureAwait(false);
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return new Result<bool>(true);
            }

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                var err = JsonConvert.DeserializeObject<TwitterError>(jsonResponse);
                if(err.Errors.Count == 1 && err.Errors[0].Code == 34)
                {
                    // Twitter API will return : {"code":34,"message":"Sorry, that page does not exist."} if you try to check a webhook with 0 subscribers,
                    // Which means, you're not subscribed.
                    return new Result<bool>(false);
                }

                return new Result<bool>(err);
            }
            else
            {

                //TODO: Provide a way to return httpstatus code 
                return new Result<bool>();
            }


        }


        /// <summary>
        /// Unsubscribe current user (from the auth context) from a webhook by Id.
        /// </summary>
        /// <param name="webhookId">Webhook Id to unsubscribe from.</param>
        /// <returns>true indicates successful deletion.</returns>
        public async Task<Result<bool>> Unsubscribe(string webhookId)
        {

            if (string.IsNullOrEmpty(webhookId))
            {
                throw new ArgumentException(nameof(webhookId));
            }

            //TODO: Provide a generic class to make Twitter API Requests.
            var resourceUrl = $"https://api.twitter.com/1.1/account_activity/webhooks/{webhookId}/subscriptions.json";

            HttpResponseMessage response;
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Add("Authorization", AuthHeaderBuilder.Build(Options, HttpMethod.Delete, resourceUrl));

                response = await client.DeleteAsync(resourceUrl).ConfigureAwait(false);
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return new Result<bool>(true);
            }

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                var err = JsonConvert.DeserializeObject<TwitterError>(jsonResponse);
                if (err.Errors.Count == 1 && err.Errors[0].Code == 34)
                {
                    // Twitter API will return : {"code":34,"message":"Sorry, that page does not exist."} if you try to check a webhook with 0 subscribers,
                    // Which means, you're not subscribed.
                    return new Result<bool>(true);
                }

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
