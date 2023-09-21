using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Dialogs.Prompts.Tests
{
    [TestClass]
    public class NumberWithUnit_PromptTests
    {

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NumberPromptWithEmptyIdShouldFail()
        {
            var emptyId = "";
            var numberPrompt = new NumberWithUnitPrompt(emptyId, NumberWithUnitPromptType.Currency);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NumberPromptWithNullIdShouldFail()
        {
            var nullId = "";
            nullId = null;
            var numberPrompt = new NumberWithUnitPrompt(nullId, NumberWithUnitPromptType.Currency);
        }

        [TestMethod]
        public async Task NumberWithUnitPrompt_Type_Currency()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add number prompt to DialogSet.
            var numberPrompt = new NumberWithUnitPrompt("CurrencyPrompt", NumberWithUnitPromptType.Currency, defaultLocale: Culture.English);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (!turnContext.Responded && results.Status == DialogTurnStatus.Empty && results.Status != DialogTurnStatus.Complete)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a currency." } };
                    await dc.PromptAsync("CurrencyPrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var currencyPromptResult = (NumberWithUnitResult)results.Result;

                    if (currencyPromptResult != null)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received Value: {currencyPromptResult.Value}, Unit: {currencyPromptResult.Unit}"), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Nothing recognized"), cancellationToken);
                    }
                }
            })
            .Send("hello")
            .AssertReply("Enter a currency.")
            .Send("42 dollars")
            .AssertReply("Bot received Value: 42, Unit: Dollar")
            .Send("hello")
            .AssertReply("Enter a currency.")
            .Send("twenty five dollars")
            .AssertReply("Bot received Value: 25, Unit: Dollar")
            .Send("hello")
            .AssertReply("Enter a currency.")
            .Send("?0.5")
            .AssertReply("Bot received Value: 50.5, Unit: Pound")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task NumberWithUnitPrompt_Type_Temperature()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add number prompt to DialogSet.
            var numberPrompt = new NumberWithUnitPrompt("NumberPrompt", NumberWithUnitPromptType.Temperature, defaultLocale: Culture.English);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (!turnContext.Responded && results.Status == DialogTurnStatus.Empty && results.Status != DialogTurnStatus.Complete)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a temperature." } };
                    await dc.PromptAsync("NumberPrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var numberWithUnitResult = (NumberWithUnitResult)results.Result;

                    if (numberWithUnitResult != null)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received Value: {numberWithUnitResult.Value}, Unit: {numberWithUnitResult.Unit}"), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Nothing recognized"), cancellationToken);
                    }
                }
            })
            .Send("hello")
            .AssertReply("Enter a temperature.")
            .Send("25 degrees celsius")
            .AssertReply("Bot received Value: 25, Unit: C")
            .Send("hello")
            .AssertReply("Enter a temperature.")
            .Send("50C")
            .AssertReply("Bot received Value: 50, Unit: C")
            .Send("hello")
            .AssertReply("Enter a temperature.")
            .Send("75F")
            .AssertReply("Bot received Value: 75, Unit: F")
            .Send("hello")
            .AssertReply("Enter a temperature.")
            .Send("100 degrees")
            .AssertReply("Bot received Value: 100, Unit: Degree")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task NumberWithUnitPrompt_Type_Age()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add number prompt to DialogSet.
            var numberPrompt = new NumberWithUnitPrompt("NumberPrompt", NumberWithUnitPromptType.Age, defaultLocale: Culture.English);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (!turnContext.Responded && results.Status == DialogTurnStatus.Empty && results.Status != DialogTurnStatus.Complete)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter an age." } };
                    await dc.PromptAsync("NumberPrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var numberWithUnitResult = (NumberWithUnitResult)results.Result;

                    if (numberWithUnitResult != null)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received Value: {numberWithUnitResult.Value}, Unit: {numberWithUnitResult.Unit}"), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Nothing recognized"), cancellationToken);
                    }
                }
            })
            .Send("hello")
            .AssertReply("Enter an age.")
            .Send("25 years old")
            .AssertReply("Bot received Value: 25, Unit: Year")
            .Send("hello")
            .AssertReply("Enter an age.")
            .Send("three months old")
            .AssertReply("Bot received Value: 3, Unit: Month")
            .Send("hello")
            .AssertReply("Enter an age.")
            .Send("twenty five years of age")
            .AssertReply("Bot received Value: 25, Unit: Year")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task NumberWithUnitPrompt_Type_Dimension()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add number prompt to DialogSet.
            var numberPrompt = new NumberWithUnitPrompt("NumberPrompt", NumberWithUnitPromptType.Dimension, defaultLocale: Culture.English);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (!turnContext.Responded && results.Status == DialogTurnStatus.Empty && results.Status != DialogTurnStatus.Complete)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter an age." } };
                    await dc.PromptAsync("NumberPrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var numberWithUnitResult = (NumberWithUnitResult)results.Result;

                    if (numberWithUnitResult != null)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received Value: {numberWithUnitResult.Value}, Unit: {numberWithUnitResult.Unit}"), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Nothing recognized"), cancellationToken);
                    }
                }
            })
            .Send("hello")
            .AssertReply("Enter an age.")
            .Send("6 miles")
            .AssertReply("Bot received Value: 6, Unit: Mile")
            .Send("hello")
            .AssertReply("Enter an age.")
            .Send("three and a half metres")
            .AssertReply("Bot received Value: 3.5, Unit: Meter")
            .Send("hello")
            .AssertReply("Enter an age.")
            .Send("twenty five kilometers")
            .AssertReply("Bot received Value: 25, Unit: Kilometer")
            .StartTestAsync();
        }
    }
}
