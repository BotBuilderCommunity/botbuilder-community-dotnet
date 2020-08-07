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
    public class Guid_PromptTest
    {
        [TestMethod]
        public async Task Guid_Prompt()
        {
            var conversationState = new ConversationState(new MemoryStorage());
            var dialogState = conversationState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(conversationState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add number prompt to DialogSet.
            var guidPrompt = new GuidPrompt(nameof(GuidPrompt), defaultLocale: Culture.English);
            dialogs.Add(guidPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (!turnContext.Responded && results.Status == DialogTurnStatus.Empty && results.Status != DialogTurnStatus.Complete)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "send your azure id" } };
                    await dc.PromptAsync(nameof(GuidPrompt), options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var guidResult = (string)results.Result;

                    if (guidResult != null)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received guid: {guidResult}"), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Nothing recognized"), cancellationToken);
                    }
                }
            })
            .Send("hello")
            .AssertReply("send your azure id")
            .Send("my azure id is 7d7b0205-9411-4a29-89ac-b9cd905886fa")
            .AssertReply("Bot received guid: 7d7b0205-9411-4a29-89ac-b9cd905886fa")
            .StartTestAsync();
        }
    }
}
