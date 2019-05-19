using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Models;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Models.Twitter;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Twitter.Webhooks.Services
{
    /// <summary>
    /// This class is responsible to intercept incoming requests to the server
    /// and handle them properly.
    /// </summary>
    public class WebhookInterceptor
    {

        /// <summary>
        /// Twitter App Consumer Secret.
        /// Used to Hash incoming/ outgoing data.
        /// </summary>
        public string ConsumerSecret { get; set; }


        /// <summary>
        /// Create a new instance of <see cref="WebhookInterceptor"/> with your Twitter App Consumer Key.
        /// 
        /// </summary>
        /// <param name="consumerSecret">Twitter App Consumer Key, used for hashing.</param>
        public WebhookInterceptor(string consumerSecret)
        {
            ConsumerSecret = consumerSecret;
        }


        /// <summary>
        /// Intercept incoming requests to the server to handle them according to Twitter Documentation, Currently:
        ///     - Challenge Response Check.
        ///     - Incoming DirectMessage.
        /// </summary>
        /// <param name="requestMessage">Thr request message object you received.</param>
        /// <param name="OnDirectMessageRecieved">If this is an incoming direct message, this callback will be fired along with the message object <see cref="DirectMessageEvent"/>.</param>
        /// <returns>
        /// <see cref="InterceptionResult"/> Interception result.
        /// </returns>
        public async Task<InterceptionResult> InterceptIncomingRequest(HttpRequest requestMessage, Action<DirectMessageEvent> OnDirectMessageRecieved)
        {
            if (string.IsNullOrEmpty(ConsumerSecret))
            {
                throw new TwitterException("Consumer Secret can't be null.");
            }

            if (HttpMethods.IsGet(requestMessage.Method))
            {
                if (!requestMessage.Query.TryGetValue("crc_token", out var values))
                    return InterceptionResult.CreateUnhandled();

                // Challenge Response Check Request:
                var crcToken = values.First();
                var response = AcceptChallenge(crcToken);
                return InterceptionResult.CreateHandled(response);
            }
            else if (HttpMethods.IsPost(requestMessage.Method))
            {
                if (!requestMessage.Headers.TryGetValue("x-twitter-webhooks-signature", out var header))
                {
                    return InterceptionResult.CreateUnhandled();
                }

                var payloadSignature = header.First();
                var streamReader = new StreamReader(requestMessage.Body);
                var payload = await streamReader.ReadToEndAsync();


                var signatureMatch = IsValidTwitterPostRequest(payloadSignature, payload);
                if (!signatureMatch)
                {
                    //This is intersecting, a twitter signature key 'x-twitter-webhooks-signature' is there but it's wrong..!
                    //return InterceptionResult.CreateHandled(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest), requestMessage);
                    throw new TwitterException("Invalid signature");
                }

                try
                {
                    var dmResult = JsonConvert.DeserializeObject<WebhookDMResult>(payload);
                    DirectMessageEvent eventResult = dmResult;
                    eventResult.JsonSource = payload;

                    OnDirectMessageRecieved?.Invoke(eventResult);
                    return InterceptionResult.CreateHandled(string.Empty);
                }
                catch 
                {
                    return InterceptionResult.CreateHandled(string.Empty);
                    //Failed to deserialize twitter direct message,
                    //TODO: Handle in a better way.. perhaps send the json?
                }
            }
            return InterceptionResult.CreateUnhandled();
        }
        
        private string AcceptChallenge(string crcToken)
        {
            var hashKeyArray = Encoding.UTF8.GetBytes(ConsumerSecret);
            var crcTokenArray = Encoding.UTF8.GetBytes(crcToken);

            var hmacSha256Alg = new HMACSHA256(hashKeyArray);

            var computedHash = hmacSha256Alg.ComputeHash(crcTokenArray);

            var challengeToken = $"sha256={Convert.ToBase64String(computedHash)}";

            var responseToken = new CRCResponseToken
            {
                Token = challengeToken
            };

            var jsonResponse = JsonConvert.SerializeObject(responseToken);

            return jsonResponse;
        }

        private bool IsValidTwitterPostRequest(string twWebhookSignature, string payload)
        {
            var hashKeyArray = Encoding.UTF8.GetBytes(ConsumerSecret);
            var hmacSha256Alg = new HMACSHA256(hashKeyArray);

            var computedHash = hmacSha256Alg.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var localHashedSignature = $"sha256={Convert.ToBase64String(computedHash)}";

            return localHashedSignature == twWebhookSignature;
        }
    }
}
