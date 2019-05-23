using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Models;

namespace Bot.Builder.Community.Adapters.Twitter.Webhooks.Authentication
{
    /// <summary>
    /// Twitter Authentication helper.
    /// </summary>
    public class AuthHeaderBuilder
    {

        /// <summary>
        /// Returns a ready 'OAuth ..' prefixed header to set in any call to Twitter API.
        /// </summary>
        /// <param name="options">Twitter app auth context.</param>
        /// <param name="method">The Request Http method.</param>
        /// <param name="requestUrl">The Request uri along with any query parameter.</param>
        /// <returns></returns>
        public static string Build(TwitterOptions options, HttpMethod method, string requestUrl)
        {

            if (!Uri.TryCreate(requestUrl, UriKind.RelativeOrAbsolute, out var resourceUri))
            {
                throw new TwitterException("Invalid Resource Url format.");
            }

            if (options == null || !options.IsValid)
            {
                throw new TwitterException("Invalid Twitter options.");
            }

            const string oauthVersion = "1.0";
            const string oauthSignatureMethod = "HMAC-SHA1";

            // It could be any random string..
            var oauthNonce = DateTime.Now.Ticks.ToString();

            var epochTimeStamp =
                (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

            var oauthTimestamp = Convert.ToInt64(epochTimeStamp).ToString();

            var signatureParams = new Dictionary<string, string>
            {
                {"oauth_consumer_key", options.ConsumerKey},
                {"oauth_nonce", oauthNonce},
                {"oauth_signature_method", oauthSignatureMethod},
                {"oauth_timestamp", oauthTimestamp},
                {"oauth_token", options.AccessToken},
                {"oauth_version", oauthVersion}
            };

            var qParams = resourceUri.GetParams();
            foreach (var qp in qParams)
            {
                signatureParams.Add(qp.Key, qp.Value);
            }

            var baseString = string.Join("&",
                signatureParams.OrderBy(kpv => kpv.Key).Select(kpv => $"{kpv.Key}={kpv.Value}"));

            var resourceUrl = requestUrl.Contains("?")
                ? requestUrl.Substring(0, requestUrl.IndexOf("?", StringComparison.Ordinal))
                : requestUrl;
            baseString = string.Concat(method.Method.ToUpper(), "&", Uri.EscapeDataString(resourceUrl), "&",
                Uri.EscapeDataString(baseString));

            var oauthSignatureKey = string.Concat(Uri.EscapeDataString(options.ConsumerSecret), "&",
                Uri.EscapeDataString(options.AccessSecret));

            string oauthSignature;
            using (var hasher = new HMACSHA1(Encoding.ASCII.GetBytes(oauthSignatureKey)))
            {
                oauthSignature = Convert.ToBase64String(hasher.ComputeHash(Encoding.ASCII.GetBytes(baseString)));
            }

            const string headerFormat = "OAuth oauth_nonce=\"{0}\", oauth_signature_method=\"{1}\", " +
                                        "oauth_timestamp=\"{2}\", oauth_consumer_key=\"{3}\", " +
                                        "oauth_token=\"{4}\", oauth_signature=\"{5}\", " +
                                        "oauth_version=\"{6}\"";

            return string.Format(headerFormat,
                Uri.EscapeDataString(oauthNonce),
                Uri.EscapeDataString(oauthSignatureMethod),
                Uri.EscapeDataString(oauthTimestamp),
                Uri.EscapeDataString(options.ConsumerKey),
                Uri.EscapeDataString(options.AccessToken),
                Uri.EscapeDataString(oauthSignature),
                Uri.EscapeDataString(oauthVersion));
        }
    }
}