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
    public class PhoneNumber_PromptTest
    {
        [TestMethod]
        public async Task PhoneNumber_Prompt()
        {
            var conversationState = new ConversationState(new MemoryStorage());
            var dialogState = conversationState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(conversationState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add number prompt to DialogSet.
            var numberPrompt = new PhoneNumberPrompt(nameof(PhoneNumberPrompt), defaultLocale: Culture.English);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (!turnContext.Responded && results.Status == DialogTurnStatus.Empty && results.Status != DialogTurnStatus.Complete)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "Hey send your phone number" } };
                    await dc.PromptAsync(nameof(PhoneNumberPrompt), options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var phoneResult = (string)results.Result;

                    if (phoneResult != null)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received phone number: {phoneResult}"), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Nothing recognized"), cancellationToken);
                    }
                }
            })
            .Send("hello")
            .AssertReply("Hey send your phone number")
            .Send("yes sure , my number is +9112345678")
            .AssertReply("Bot received phone number: +9112345678")
            .StartTestAsync();
        }
    }
}
