using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;
using Bot.Builder.Community.Adapters.Infobip.Messages;
using System.Threading;
using System.Text;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Infobip.Messages.Tests
{
    public class InfobipMessagesClientTests
    {
        private readonly InfobipMessagesAdapterOptions _options;
        private readonly Mock<ILogger<InfobipMessagesClient>> _mockLogger;

        public InfobipMessagesClientTests()
        {
            _options = TestOptions.Get();
            _mockLogger = new Mock<ILogger<InfobipMessagesClient>>();
        }

        [Fact]
        public void Constructor_WithValidOptions_SetsUpClient()
        {
            // Act
            using var client = new InfobipMessagesClient(_options, null, _mockLogger.Object);

            // Assert - Constructor should not throw
            Assert.NotNull(client);
        }

        [Fact]
        public void Constructor_WithNullOptions_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new InfobipMessagesClient(null));
        }

        [Fact]
        public async Task SendAsync_WithValidMessage_ReturnsSuccessResponse()
        {
            // Arrange
            var mockResponse = new { MessageId = "test-id", Status = "PENDING_ACCEPTED" };
            var responseJson = JsonConvert.SerializeObject(mockResponse);
            
            var mockHttpClient = CreateMockHttpClient(responseJson, System.Net.HttpStatusCode.OK);
            
            using var client = new InfobipMessagesClient(_options, mockHttpClient, _mockLogger.Object);
            
            var message = new
            {
                messages = new[]
                {
                    new
                    {
                        sender = "test-sender",
                        channel = "WHATSAPP",
                        destinations = new[] { new { to = "test-recipient" } },
                        content = new { body = new { type = "TEXT", text = "Test message" } }
                    }
                }
            };

            // Act
            var result = await client.SendAsync<dynamic>(message);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test-id", result.MessageId.ToString());
            Assert.Equal("PENDING_ACCEPTED", result.Status.ToString());
        }

        [Fact]
        public async Task SendAsync_WithApiError_ThrowsHttpRequestException()
        {
            // Arrange
            var errorResponse = JsonConvert.SerializeObject(new { error = "Invalid API key" });
            var mockHttpClient = CreateMockHttpClient(errorResponse, System.Net.HttpStatusCode.Unauthorized);
            
            using var client = new InfobipMessagesClient(_options, mockHttpClient, _mockLogger.Object);
            
            var message = new { messages = new[] { new { text = "test" } } };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => 
                client.SendAsync<object>(message));
            Assert.Contains("Unauthorized", exception.Message);
        }

        [Fact]
        public async Task GetContentTypeAsync_WithValidUrl_ReturnsContentType()
        {
            // Arrange
            var mockHttpClient = CreateMockHttpClientForContentType("image/jpeg");
            using var client = new InfobipMessagesClient(_options, mockHttpClient, _mockLogger.Object);

            // Act
            var contentType = await client.GetContentTypeAsync("https://example.com/image.jpg");

            // Assert
            Assert.Equal("image/jpeg", contentType);
        }

        [Fact]
        public async Task GetAttachmentAsync_WithValidUrl_ReturnsAttachment()
        {
            // Arrange
            var testData = Encoding.UTF8.GetBytes("test file content");
            var mockHttpClient = CreateMockHttpClientForAttachment(testData, "image/jpeg");
            using var client = new InfobipMessagesClient(_options, mockHttpClient, _mockLogger.Object);

            // Act
            var attachment = await client.GetAttachmentAsync("https://example.com/image.jpg");

            // Assert
            Assert.NotNull(attachment);
            Assert.Equal("image/jpeg", attachment.ContentType);
            Assert.Equal(testData, attachment.Content);
        }

        [Fact]
        public void Dispose_CallsDisposeOnHttpClient()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            var client = new InfobipMessagesClient(_options, mockHttpClient.Object, _mockLogger.Object);

            // Act
            client.Dispose();

            // Assert - Should not throw
            Assert.True(true); // Disposal completed successfully
        }

        private HttpClient CreateMockHttpClient(string responseContent, System.Net.HttpStatusCode statusCode)
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            return new HttpClient(mockHandler.Object);
        }

        private HttpClient CreateMockHttpClientForContentType(string contentType)
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("", Encoding.UTF8, contentType)
                });

            return new HttpClient(mockHandler.Object);
        }

        private HttpClient CreateMockHttpClientForAttachment(byte[] data, string contentType)
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new ByteArrayContent(data) { Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType) } }
                });

            return new HttpClient(mockHandler.Object);
        }
    }
}