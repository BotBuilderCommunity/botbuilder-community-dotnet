using MessageBird;
using MessageBird.Exceptions;
using MessageBird.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Builder.Community.Adapters.MessageBird
{
    public class MessageBirdRequestAuthorization
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_messageBirdRequestSignature">MessageBird-Signature-JWT header value of incoming request</param>
        /// <param name="_signingKey">your MessageBird signing key</param>
        /// <param name="_requestBody">incoming request body</param>
        /// <param name="_messageBirdWebhookEndpointUrl">your endpoint URL for MessageBird webhooks like https://yourdomain.com/api/messagebird</param>
        /// <returns></returns>
        public bool VerifyJWT(string _messageBirdRequestSignature, string _signingKey, string _requestBody, string _messageBirdWebhookEndpointUrl)
        {
            RequestValidator validator = new RequestValidator(_signingKey);

            // requestSignature is the value of the 'MessageBird-Signature-JWT' HTTP header.
            string requestSignature = _messageBirdRequestSignature;
            string requestURL = _messageBirdWebhookEndpointUrl;
            byte[] requestBody = Encoding.UTF8.GetBytes(_requestBody);
            try
            {
                validator.ValidateSignature(requestSignature, requestURL, requestBody);
                return true;
            }
            catch (RequestValidationException e)
            {
                // The request is invalid, so respond accordingly.
                return false;
            }
        }
    }
}
