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
    public class Email_PromptTest
    {
        [TestMethod]
        public async Task Email_Prompt()
        {
            var conversationState = new ConversationState(new MemoryStorage());
            var dialogState = conversationState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(conversationState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add number prompt to DialogSet.
            var emailPrompt = new EmailPrompt(nameof(EmailPrompt), defaultLocale: Culture.English);
            dialogs.Add(emailPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (!turnContext.Responded && results.Status == DialogTurnStatus.Empty && results.Status != DialogTurnStatus.Complete)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "Kindly send your Email Id" } };
                    await dc.PromptAsync(nameof(EmailPrompt), options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var emailPromptResult = (string)results.Result;

                    if (emailPromptResult != null)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received email: {emailPromptResult}"), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Nothing recognized"), cancellationToken);
                    }
                }
            })
            .Send("hello")
            .AssertReply("Kindly send your Email Id")
            .Send("hey my email id is r.vinoth@live.com")
            .AssertReply("Bot received email: r.vinoth@live.com")
            .StartTestAsync();
        }
    }
}
