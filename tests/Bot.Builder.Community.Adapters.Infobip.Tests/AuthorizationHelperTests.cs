using Xunit;

namespace Bot.Builder.Community.Adapters.Infobip.Tests
{
    public class AuthorizationHelperTests
    {
        private InfobipAdapterOptions _infobipAdapterOptions;

        public AuthorizationHelperTests()
        {
            _infobipAdapterOptions = TestOptions.Get();
        }

        [Fact]
        public void VerifySignature()
        {
            const string signature = "SHA1=5d340080cdb91ce00e9f9455e750a7188f3b3203";

            const string payload = "{\"results\": [{" +
                                   "\"from\": \"38641669951\"," +
                                   "\"to\": \"447491163530\"," +
                                   "\"integrationType\": \"WHATSAPP\"," +
                                   "\"receivedAt\": \"2020-02-26T10:15:48.734+0000\"," +
                                   "\"messageId\": \"ABGGOGQWaZUfAgo-sD1q39_c8HDU\"," +
                                   "\"pairedMessageId\": null," +
                                   "\"callbackData\": null," +
                                   "\"message\": {" +
                                   "\"text\": \"MSBOT test app\"," +
                                   "\"type\": \"TEXT\"" +
                                   "}," +
                                   "\"contact\": {" +
                                   "\"name\": \"Bojan Vrhovnik\"" +
                                   "}," +
                                   "\"price\": {" +
                                   "\"pricePerMessage\": 0," +
                                   "\"currency\": \"GBP\"" +
                                   "}}]," +
                                   "\"messageCount\": 1," +
                                   "\"pendingMessageCount\": 0" +
                                   "}";

            var isCorrect = new AuthorizationHelper().VerifySignature(signature, payload, _infobipAdapterOptions.InfobipAppSecret);
            Assert.True(isCorrect);
        }
    }
}