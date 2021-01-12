using System.Threading.Tasks;
using Bot.Builder.Community.Middleware.TextRecognizer;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bot.Builder.Community.Middleware.Tests
{
    [TestClass]
    public class TextRecognizerMiddlewareTest
    {
        [TestMethod]
        [TestCategory("Middleware")]
        public async Task EmailRecognizer_Middleware()
        {
            TestAdapter testAdapter = new TestAdapter()
                .Use(new EmailRecognizerMiddleware());

            var emailActivity = MessageFactory.Text("my email id is r.vinoth@live.com");
            await testAdapter.ProcessActivityAsync(emailActivity, async (turnContext, cancellationToken) =>
            {
                var result = turnContext.TurnState["EmailEntities"].ToString();
                await turnContext.SendActivityAsync(MessageFactory.Text(result), cancellationToken);
            });

            var resultSet = testAdapter.ActiveQueue.Dequeue();

            Assert.AreEqual("r.vinoth@live.com", resultSet.Text);
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task PhoneNumberRecognizer_Middleware()
        {
            TestAdapter testAdapter = new TestAdapter()
                .Use(new PhoneNumberRecognizerMiddleware());

            var emailActivity = MessageFactory.Text("my phone number is +911234567890");
            await testAdapter.ProcessActivityAsync(emailActivity, async (turnContext, cancellationToken) =>
            {
                var result = turnContext.TurnState["PhoneNumberEntities"].ToString();
                await turnContext.SendActivityAsync(MessageFactory.Text(result), cancellationToken);
            });

            var resultSet = testAdapter.ActiveQueue.Dequeue();

            Assert.AreEqual("+911234567890", resultSet.Text);
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task SocialMediaRecognizer_Mention_Middleware()
        {
            TestAdapter testAdapter = new TestAdapter()
                .Use(new SocialMediaRecognizerMiddleware(SocialMediaPromptType.Mention));

            var socialActivity = MessageFactory.Text("Bot Framework Twitter handle is @msbotframework");
            await testAdapter.ProcessActivityAsync(socialActivity, async (turnContext, cancellationToken) =>
            {
                var result = turnContext.TurnState["SocialEntities"].ToString();
                await turnContext.SendActivityAsync(MessageFactory.Text(result), cancellationToken);
            });

            var resultSet = testAdapter.ActiveQueue.Dequeue();

            Assert.AreEqual("@msbotframework", resultSet.Text);
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task SocialMediaRecognizer_Hash_Middleware()
        {
            TestAdapter testAdapter = new TestAdapter()
                .Use(new SocialMediaRecognizerMiddleware(SocialMediaPromptType.Hashtag));

            var socialActivity = MessageFactory.Text("Follow the #botframework hashtag");
            await testAdapter.ProcessActivityAsync(socialActivity, async (turnContext, cancellationToken) =>
            {
                var result = turnContext.TurnState["SocialEntities"].ToString();
                await turnContext.SendActivityAsync(MessageFactory.Text(result), cancellationToken);
            });

            var resultSet = testAdapter.ActiveQueue.Dequeue();

            Assert.AreEqual("#botframework", resultSet.Text);
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task InternetProtocolRecognizer_Url_Middleware()
        {
            TestAdapter testAdapter = new TestAdapter()
                .Use(new InternetProtocolRecognizerMiddleware(InternetProtocolPromptType.Url));

            var socialActivity = MessageFactory.Text("Hey find bot community url https://github.com/BotBuilderCommunity");
            await testAdapter.ProcessActivityAsync(socialActivity, async (turnContext, cancellationToken) =>
            {
                var result = turnContext.TurnState["InternetEntities"].ToString();
                await turnContext.SendActivityAsync(MessageFactory.Text(result), cancellationToken);
            });

            var resultSet = testAdapter.ActiveQueue.Dequeue();

            Assert.AreEqual("https://github.com/BotBuilderCommunity", resultSet.Text);
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task InternetProtocolRecognizer_ipAddress_Middleware()
        {
            TestAdapter testAdapter = new TestAdapter()
                .Use(new InternetProtocolRecognizerMiddleware(InternetProtocolPromptType.IpAddress));

            var socialActivity = MessageFactory.Text("local ip address is 127.0.0.1");
            await testAdapter.ProcessActivityAsync(socialActivity, async (turnContext, cancellationToken) =>
            {
                var result = turnContext.TurnState["InternetEntities"].ToString();
                await turnContext.SendActivityAsync(MessageFactory.Text(result), cancellationToken);
            });

            var resultSet = testAdapter.ActiveQueue.Dequeue();

            Assert.AreEqual("127.0.0.1", resultSet.Text);
        }
    }
}