using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Bot.Builder.Community.Adapters.Infobip.Messages.Models;
using Bot.Builder.Community.Adapters.Infobip.Messages.Tests.Framework;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Bot.Builder.Community.Adapters.Infobip.Messages.Tests
{
    public class InfobipMessagesAdapterTests
    {
        private readonly Mock<IInfobipMessagesClient> _mockClient;
        private readonly Mock<ILogger<InfobipMessagesAdapter>> _mockLogger;
        private readonly InfobipMessagesAdapterOptions _adapterOptions;

        public InfobipMessagesAdapterTests()
        {
            _mockClient = new Mock<IInfobipMessagesClient>();
            _mockLogger = new Mock<ILogger<InfobipMessagesAdapter>>();
            _adapterOptions = TestOptions.Get();
        }

        [Fact]
        public async Task ProcessIncomingMessageActivity()
        {
            var incomingMessage = new InfobipIncomingMessage<InfobipMessagesIncomingResult>
            {
                Results = new List<InfobipMessagesIncomingResult>
                {
                    new InfobipMessagesIncomingResult
                    {
                        MessageId = "test-message-id",
                        From = "subscriber-number",
                        To = "messages-number",
                        ReceivedAt = System.DateTimeOffset.UtcNow,
                        Channel = "whatsapp",
                        Platform = "whatsapp",
                        Message = new InfobipMessagesIncomingMessage
                        {
                            Type = InfobipMessagesMessageTypes.Text,
                            Text = "Hello, bot!"
                        }
                    }
                }
            };

            var json = JsonConvert.SerializeObject(incomingMessage);
            var httpRequest = CreateHttpRequest(json);
            var httpResponse = new Mock<HttpResponse>();
            httpResponse.SetupProperty(r => r.StatusCode);

            // Set up AppSecret and dummy signature for test
            var adapterOptions = TestOptions.Get();
            adapterOptions.InfobipAppSecret = "test-secret";
            httpRequest.Headers["X-Hub-Signature"] = "dummy-signature";
        }

        private HttpRequest CreateHttpRequest(string json)
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
            request.ContentLength = request.Body.Length;
            request.ContentType = "application/json";
            request.Method = "POST";
            return request;
        }
    }
}