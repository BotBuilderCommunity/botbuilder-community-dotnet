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
            : this(configuration["MessageBirdAccessKey"], configuration["MessageBirdSigningKey"], Convert.ToBoolean(configuration["MessageBirdUseWhatsAppSandbox"]))
        { }

        public MessageBirdAdapterOptions(string accessKey, string signingKey, bool useWhatsAppSandbox)
        {
            AccessKey = accessKey;
            SigningKey = signingKey;
            UseWhatsAppSandbox = useWhatsAppSandbox;
        }

        public string AccessKey { get; set; }

        public string SigningKey { get; set; }

        public bool UseWhatsAppSandbox { get; set; }
    }
}
