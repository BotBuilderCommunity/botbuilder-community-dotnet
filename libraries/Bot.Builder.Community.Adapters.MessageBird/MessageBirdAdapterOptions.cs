using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Builder.Community.Adapters.MessageBird
{
    public class MessageBirdAdapterOptions
    {
        public MessageBirdAdapterOptions(IConfiguration configuration)
            : this(configuration["MessageBirdAccessKey"], configuration["MessageBirdSigningKey"])
        { }

        public MessageBirdAdapterOptions(string accessKey, string signingKey)
        {
            AccessKey = accessKey;
            SigningKey = signingKey;
        }
        public string AccessKey { get; set; }
        public string SigningKey { get; set; }
    }
}
