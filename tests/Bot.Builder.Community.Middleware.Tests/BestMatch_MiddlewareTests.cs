using System.Threading.Tasks;
using Bot.Builder.Community.Middleware.BestMatch;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bot.Builder.Community.Middleware.Tests
{
    [TestClass]
    public class BestMatch_MiddlewareTests
    {
        [TestMethod]
        [TestCategory("Middleware")]
        public async Task BestMatch_TestMiddleware_DefaultProps()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new TestBestMatchMiddleware());

            await new TestFlow(adapter, async (context) =>
                {
                    await Task.CompletedTask;
                })
                .Send("Hi There")
                .AssertReply("Well hello there. What can I do for you today?")
                .StartTest();
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task BestMatch_TestMiddleware_IgnoreCaseFalse_No_Match()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new TestBestMatchMiddleware());

            await new TestFlow(adapter, async (context) =>
                {
                    await context.SendActivity("Message from the bot");
                    await Task.CompletedTask;
                })
                .Send("HOWS IT GOING")
                .AssertReply("Message from the bot")
                .StartTest();
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task BestMatch_TestMiddleware_IgnoreCaseFalse_Match()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new TestBestMatchMiddleware());

            await new TestFlow(adapter, async (context) =>
                {
                    await context.SendActivity("Message from the bot");
                    await Task.CompletedTask;
                })
                .Send("hows it going")
                .AssertReply("I am great.")
                .StartTest();
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task BestMatch_TestMiddleware_CheckIgnoreAlphanumeric_Match()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new TestBestMatchMiddleware());

            await new TestFlow(adapter, async (context) =>
                {
                    await context.SendActivity("Message from the bot");
                    await Task.CompletedTask;
                })
                .Send("h#o$w$s i%t g#o%in#g")
                .AssertReply("I am great.")
                .StartTest();
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task BestMatch_TestMiddleware_NoMatch_Fall_Through_To_Bot()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new TestBestMatchMiddleware());

            await new TestFlow(adapter, async (context) =>
                {
                    await context.SendActivity("Message from the bot");
                    await Task.CompletedTask;
                })
                .Send("what is the meaning of life? This should not match with anything")
                .AssertReply("Message from the bot")
                .StartTest();
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task BestMatch_TestMiddleware_Test_Match_And_Next_Called()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new TestBestMatchMiddleware());

            await new TestFlow(adapter, async (context) =>
                {
                    await context.SendActivity("Message from the bot");
                    await Task.CompletedTask;
                })
                .Send("bye")
                .AssertReply("Bye")
                .AssertReply("Message from the bot")
                .StartTest();
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task BestMatch_TestMiddleware_Test_Delimited_List()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new TestBestMatchMiddleware());

            await new TestFlow(adapter, async (context) =>
                {
                    await context.SendActivity("Message from the bot");
                    await Task.CompletedTask;
                })
                .Send("much appreciated")
                .AssertReply("You're welcome.")
                .StartTest();
        }
    }



    public class TestBestMatchMiddleware : BestMatchMiddleware
    {
        [BestMatch(new string[] { "Hi", "Hi There", "Hello there", "Hey", "Hello",
                "Hey there", "Greetings", "Good morning", "Good afternoon", "Good evening", "Good day" },
            threshold: 0.5, ignoreCase: false, ignoreNonAlphaNumericCharacters: false)]
        public async Task HandleGreeting(ITurnContext context, string messageText, MiddlewareSet.NextDelegate next)
        {
            await context.SendActivity("Well hello there. What can I do for you today?");
        }

        [BestMatch(new string[] { "how goes it", "how do", "hows it going", "how are you",
            "how do you feel", "whats up", "sup", "hows things" }, ignoreCase: false)]
        public async Task HandleStatusRequest(ITurnContext context, string messageText, MiddlewareSet.NextDelegate next)
        {
            await context.SendActivity("I am great.");
        }

        [BestMatch(new string[] { "bye", "bye bye", "got to go",
            "see you later", "laters", "adios" })]
        public async Task HandleGoodbye(ITurnContext context, string messageText, MiddlewareSet.NextDelegate next)
        {
            await context.SendActivity("Bye");
            await next();
        }

        [BestMatch("thank you|thanks|much appreciated|thanks very much|thanking you", listDelimiter: '|')]
        public async Task HandleThanks(ITurnContext context, string messageText, MiddlewareSet.NextDelegate next)
        {
            await context.SendActivity("You're welcome.");
        }

        public override async Task NoMatchHandler(ITurnContext context, string messageText, MiddlewareSet.NextDelegate next)
        {
            await next();
        }
    }
}
