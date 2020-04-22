using Microsoft.Extensions.Logging;

namespace Bot.Builder.Community.Adapters.Alexa.Integration.AspNet.Core
{
    public class AlexaHttpAdapter : AlexaAdapter, IAlexaHttpAdapter
    {
        public AlexaHttpAdapter(bool validateRequests, ILogger logger = null) 
            : base(new AlexaAdapterOptions() { ValidateIncomingAlexaRequests = validateRequests }, logger)
        {
        }

        public AlexaHttpAdapter(AlexaAdapterOptions options = null, ILogger logger = null)
            : base(options, logger)
        {
        }
    }
}