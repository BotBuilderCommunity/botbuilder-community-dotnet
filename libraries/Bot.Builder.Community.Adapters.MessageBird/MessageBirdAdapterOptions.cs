using Microsoft.Extensions.Configuration;
using System;

namespace Bot.Builder.Community.Adapters.MessageBird
{
    public class MessageBirdAdapterOptions
    {
        public MessageBirdAdapterOptions()
        {

        }
        public MessageBirdAdapterOptions(IConfiguration configuration)
            : this(configuration["MessageBirdAccessKey"], configuration["MessageBirdSigningKey"], configuration["MessageBirdWebhookEndpointUrl"])
        { }

        public MessageBirdAdapterOptions(string accessKey, string signingKey, string messageBirdWebhookEndpointUrl)
        {
            AccessKey = accessKey;
            SigningKey = signingKey;
            MessageBirdWebhookEndpointUrl = messageBirdWebhookEndpointUrl;
        }

        public string AccessKey { get; set; }

        public string SigningKey { get; set; }

        public string MessageBirdWebhookEndpointUrl { get; set; }
    }
}
