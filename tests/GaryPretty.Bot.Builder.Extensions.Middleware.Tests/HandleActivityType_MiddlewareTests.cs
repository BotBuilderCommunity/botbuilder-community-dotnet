using System;
using System.Threading.Tasks;
using Bot.Builder.Community.Middleware.HandleActivityType;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bot.Builder.Community.Middleware.Tests
{
    [TestClass]
    public class HandleActivityType_MiddlewareTests
    {
        [TestMethod]
        [TestCategory("Middleware")]
        public async Task ActivityFilter_TestMiddleware_HandleActivityAndContinue()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new HandleActivityTypeMiddleware(ActivityTypes.Message, async (context, next) =>
                {
                    await context.SendActivity("Handling a message activity");
                    await next();
                }));


            await new TestFlow(adapter, async (context) =>
                {
                    await context.SendActivity("Follow up message from bot");
                    await Task.CompletedTask;
                })
                .Send("foo")
                .AssertReply("Handling a message activity")
                .AssertReply("Follow up message from bot")
                .StartTest();
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task ActivityFilter_TestMiddleware_HandleMessageActivityOnly()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new HandleActivityTypeMiddleware(ActivityTypes.ConversationUpdate, async (context, next) =>
                {
                    Assert.Fail("Incorrect activity filter ran");
                }))
                .Use(new HandleActivityTypeMiddleware(ActivityTypes.Message, async (context, next) =>
                {
                    await context.SendActivity("Handling a message activity");
                    await next();
                })); 


            await new TestFlow(adapter, async (context) =>
            {
                await context.SendActivity("Follow up message from bot");
                await Task.CompletedTask;
            })
                .Send("foo")
                .AssertReply("Handling a message activity")
                .StartTest();
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task ActivityFilter_TestMiddleware_MultipleHandlers()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new HandleActivityTypeMiddleware(ActivityTypes.Message, async (context, next) =>
                {
                    await context.SendActivity("Handler 1");
                    await next();
                }))
                .Use(new HandleActivityTypeMiddleware(ActivityTypes.Message, async (context, next) =>
                {
                    await context.SendActivity("Handler 2");
                    await next();
                }));


            await new TestFlow(adapter, async (context) =>
                {
                    await context.SendActivity("Follow up message from bot");
                    await Task.CompletedTask;
                })
                .Send("foo")
                .AssertReply("Handler 1")
                .AssertReply("Handler 2")
                .AssertReply("Follow up message from bot")
                .StartTest();
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task ActivityFilter_TestMiddleware_NullActivityType()
        {
            try
            {
                TestAdapter adapter = new TestAdapter()
                    .Use(new HandleActivityTypeMiddleware(null, async (context, next) =>
                    {
                        await context.SendActivity("Handler 1");
                        await next();
                    }));
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentNullException));
            }
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task ActivityFilter_TestMiddleware_EmptyActivityType()
        {
            try
            {
                TestAdapter adapter = new TestAdapter()
                    .Use(new HandleActivityTypeMiddleware("", async (context, next) =>
                    {
                        await context.SendActivity("Handler 1");
                        await next();
                    }));
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentNullException));
            }
        }

    }
}
