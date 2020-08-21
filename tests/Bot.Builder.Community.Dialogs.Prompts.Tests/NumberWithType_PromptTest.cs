using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bot.Builder.Community.Dialogs.Prompts.Tests
{
    [TestClass]
    public class NumberWithType_PromptTest
    {
        [TestMethod]
        public async Task NumberWithTypePrompt_Type_Ordinal()
        {
            var conversationState = new ConversationState(new MemoryStorage());
            var dialogState = conversationState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(conversationState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add number prompt to DialogSet.
            var numberPrompt = new NumberWithTypePrompt("OrdinalPrompt", NumberWithTypePromptType.Ordinal, defaultLocale: Culture.English);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (!turnContext.Responded && results.Status == DialogTurnStatus.Empty && results.Status != DialogTurnStatus.Complete)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a OrdinalPrompt number Info." } };
                    await dc.PromptAsync("OrdinalPrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var currencyPromptResult = (NumberWithTypeResult)results.Result;

                    if (currencyPromptResult != null)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received Text: {currencyPromptResult.Text}, Value: {currencyPromptResult.Value}"), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Nothing recognized"), cancellationToken);
                    }
                }
            })
            .Send("hello")
            .AssertReply("Enter a OrdinalPrompt number Info.")
            .Send("Ok, eleventh")
            .AssertReply("Bot received Text: eleventh, Value: 11")
            .Send("hello")
            .AssertReply("Enter a OrdinalPrompt number Info.")
            .Send("hello , one millionth")
            .AssertReply("Bot received Text: one millionth, Value: 1000000")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task NumberWithTypePrompt_Type_Percentage()
        {
            var conversationState = new ConversationState(new MemoryStorage());
            var dialogState = conversationState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(conversationState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add number prompt to DialogSet.
            var numberPrompt = new NumberWithTypePrompt("PercentagePrompt", NumberWithTypePromptType.Percentage, defaultLocale: Culture.English);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (!turnContext.Responded && results.Status == DialogTurnStatus.Empty && results.Status != DialogTurnStatus.Complete)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a Percentage number Info." } };
                    await dc.PromptAsync("PercentagePrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var currencyPromptResult = (NumberWithTypeResult)results.Result;

                    if (currencyPromptResult != null)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received Text: {currencyPromptResult.Text}, Value: {currencyPromptResult.Value}"), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Nothing recognized"), cancellationToken);
                    }
                }
            })
            .Send("hello")
            .AssertReply("Enter a Percentage number Info.")
            .Send("Ok, I sent one hundred percents find that")
            .AssertReply("Bot received Text: one hundred percents, Value: 100%")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task NumberWithTypePrompt_Type_NumberRange()
        {
            var conversationState = new ConversationState(new MemoryStorage());
            var dialogState = conversationState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(conversationState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add number prompt to DialogSet.
            var numberPrompt = new NumberWithTypePrompt("NumberRangePrompt", NumberWithTypePromptType.NumberRange, defaultLocale: Culture.English);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (!turnContext.Responded && results.Status == DialogTurnStatus.Empty && results.Status != DialogTurnStatus.Complete)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a NumberRange number Info." } };
                    await dc.PromptAsync("NumberRangePrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var currencyPromptResult = (NumberWithTypeResult)results.Result;

                    if (currencyPromptResult != null)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received Text: {currencyPromptResult.Text}, Value: {currencyPromptResult.Value}"), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Nothing recognized"), cancellationToken);
                    }
                }
            })
            .Send("hello")
            .AssertReply("Enter a NumberRange number Info.")
            .Send("Ok, Hello find the range between 1982 and 1987")
            .AssertReply("Bot received Text: between 1982 and 1987, Value: [1982,1987)")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task NumberWithTypePrompt_Type_Number()
        {
            var conversationState = new ConversationState(new MemoryStorage());
            var dialogState = conversationState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(conversationState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add number prompt to DialogSet.
            var numberPrompt = new NumberWithTypePrompt("NumberPrompt", NumberWithTypePromptType.Number, defaultLocale: Culture.English);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (!turnContext.Responded && results.Status == DialogTurnStatus.Empty && results.Status != DialogTurnStatus.Complete)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a Number Info." } };
                    await dc.PromptAsync("NumberPrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var currencyPromptResult = (NumberWithTypeResult)results.Result;

                    if (currencyPromptResult != null)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received Text: {currencyPromptResult.Text}, Value: {currencyPromptResult.Value}"), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Nothing recognized"), cancellationToken);
                    }
                }
            })
            .Send("hello")
            .AssertReply("Enter a Number Info.")
            .Send("Ok, Total four projects in bot builder community")
            .AssertReply("Bot received Text: four, Value: 4")
            .StartTestAsync();
        }
    }
}
