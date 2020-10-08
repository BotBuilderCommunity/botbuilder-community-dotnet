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
                .Use(new HandleActivityTypeMiddleware(ActivityTypes.Message, async (context, next, cancellationToken) =>
                {
                    await context.SendActivityAsync("Handling a message activity");
                    await next(cancellationToken);
                }));


            await new TestFlow(adapter, async (context, cancellationToken) =>
                {
                    await context.SendActivityAsync("Follow up message from bot");
                    await Task.CompletedTask;
                })
                .Send("foo")
                .AssertReply("Handling a message activity")
                .AssertReply("Follow up message from bot")
                .StartTestAsync();
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task ActivityFilter_TestMiddleware_HandleMessageActivityOnly()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new HandleActivityTypeMiddleware(ActivityTypes.ConversationUpdate, (context, next, cancellationToken) =>
                {
                    Assert.Fail("Incorrect activity filter ran");
                    throw new AssertFailedException("Incorrect activity filter ran");
                }))
                .Use(new HandleActivityTypeMiddleware(ActivityTypes.Message, async (context, next, cancellationToken) =>
                {
                    await context.SendActivityAsync("Handling a message activity");
                    await next(cancellationToken);
                })); 


            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                await context.SendActivityAsync("Follow up message from bot");
                await Task.CompletedTask;
            })
                .Send("foo")
                .AssertReply("Handling a message activity")
                .StartTestAsync();
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task ActivityFilter_TestMiddleware_MultipleHandlers()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new HandleActivityTypeMiddleware(ActivityTypes.Message, async (context, next, cancellationToken) =>
                {
                    await context.SendActivityAsync("Handler 1");
                    await next(cancellationToken);
                }))
                .Use(new HandleActivityTypeMiddleware(ActivityTypes.Message, async (context, next, cancellationToken) =>
                {
                    await context.SendActivityAsync("Handler 2");
                    await next(cancellationToken);
                }));


            await new TestFlow(adapter, async (context, cancellationToken) =>
                {
                    await context.SendActivityAsync("Follow up message from bot");
                    await Task.CompletedTask;
                })
                .Send("foo")
                .AssertReply("Handler 1")
                .AssertReply("Handler 2")
                .AssertReply("Follow up message from bot")
                .StartTestAsync();
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public void ActivityFilter_TestMiddleware_NullActivityType()
        {
            try
            {
                TestAdapter adapter = new TestAdapter()
                    .Use(new HandleActivityTypeMiddleware(null, async (context, next, cancellationToken) =>
                    {
                        await context.SendActivityAsync("Handler 1");
                        await next(cancellationToken);
                    }));
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentNullException));
            }
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public void ActivityFilter_TestMiddleware_EmptyActivityType()
        {
            try
            {
                TestAdapter adapter = new TestAdapter()
                    .Use(new HandleActivityTypeMiddleware("", async (context, next, cancellationToken) =>
                    {
                        await context.SendActivityAsync("Handler 1");
                        await next(cancellationToken);
                    }));
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentNullException));
            }
        }

    }
}
