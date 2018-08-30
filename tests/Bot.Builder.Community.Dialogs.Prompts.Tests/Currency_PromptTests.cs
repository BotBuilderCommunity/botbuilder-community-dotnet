using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.NumberWithUnit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Dialogs.Prompts.Tests
{
    [TestClass]
    public class Currency_PromptTests
    {

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NumberPromptWithEmptyIdShouldFail()
        {
            var emptyId = "";
            var numberPrompt = new CurrencyPrompt(emptyId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NumberPromptWithNullIdShouldFail()
        {
            var nullId = "";
            nullId = null;
            var numberPrompt = new CurrencyPrompt(nullId);
        }

        [TestMethod]
        public async Task CurrencyPrompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(convoState);

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add number prompt to DialogSet.
            var numberPrompt = new CurrencyPrompt("CurrencyPrompt", defaultLocale: Culture.English);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueAsync(cancellationToken);
                if (!turnContext.Responded && !results.HasActive && !results.HasResult)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a currency." } };
                    await dc.PromptAsync("CurrencyPrompt", options, cancellationToken);
                }
                else if (!results.HasActive && results.HasResult)
                {
                    var currencyPromptResult = (CurrencyPromptResult)results.Result;
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received Value: {currencyPromptResult.Value}, Unit: {currencyPromptResult.Unit}"), cancellationToken);
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
            .Send("£50.5")
            .AssertReply("Bot received Value: 50.5, Unit: Pound")
            .StartTestAsync();
        }
    }
}
