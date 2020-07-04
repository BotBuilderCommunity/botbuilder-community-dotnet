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
    public class InternetProtocol_PromptTest
    {
        [TestMethod]
        public async Task IpAddress_Prompt()
        {
            var conversationState = new ConversationState(new MemoryStorage());
            var dialogState = conversationState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(conversationState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add number prompt to DialogSet.
            var internetAddressPrompt = new InternetProtocolPrompt(nameof(InternetProtocolPrompt), InternetProtocolPromptType.IpAddress, defaultLocale: Culture.English);
            dialogs.Add(internetAddressPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (!turnContext.Responded && results.Status == DialogTurnStatus.Empty && results.Status != DialogTurnStatus.Complete)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "send your pc ip address" } };
                    await dc.PromptAsync(nameof(InternetProtocolPrompt), options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var addressResult = (string)results.Result;

                    if (addressResult != null)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received ip address: {addressResult}"), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Nothing recognized"), cancellationToken);
                    }
                }
            })
            .Send("hello")
            .AssertReply("send your pc ip address")
            .Send("my ip address is 192.0.0.1")
            .AssertReply("Bot received ip address: 192.0.0.1")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Url_Prompt()
        {
            var conversationState = new ConversationState(new MemoryStorage());
            var dialogState = conversationState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(conversationState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add number prompt to DialogSet.
            var urlPrompt = new InternetProtocolPrompt(nameof(InternetProtocolPrompt), InternetProtocolPromptType.Url, defaultLocale: Culture.English);
            dialogs.Add(urlPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (!turnContext.Responded && results.Status == DialogTurnStatus.Empty && results.Status != DialogTurnStatus.Complete)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "Hey , send Bot Builder Community address" } };
                    await dc.PromptAsync(nameof(InternetProtocolPrompt), options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var urlResult = (string)results.Result;

                    if (urlResult != null)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received url: {urlResult}"), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Nothing recognized"), cancellationToken);
                    }
                }
            })
            .Send("hello")
            .AssertReply("Hey , send Bot Builder Community address")
            .Send("yes sure ,https://github.com/BotBuilderCommunity")
            .AssertReply("Bot received url: https://github.com/BotBuilderCommunity")
            .StartTestAsync();
        }
    }
}
