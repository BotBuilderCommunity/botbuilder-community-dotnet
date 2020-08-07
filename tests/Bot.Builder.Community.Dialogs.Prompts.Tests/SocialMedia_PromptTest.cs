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
    public class SocialMedia_PromptTest
    {
        [TestMethod]
        public async Task HashTag_Prompt()
        {
            var conversationState = new ConversationState(new MemoryStorage());
            var dialogState = conversationState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(conversationState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add number prompt to DialogSet.
            var hashMediaPrompt = new SocialMediaPrompt(nameof(SocialMediaPrompt), SocialMediaPromptType.Hashtag, defaultLocale: Culture.English);
            dialogs.Add(hashMediaPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (!turnContext.Responded && results.Status == DialogTurnStatus.Empty && results.Status != DialogTurnStatus.Complete)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "What are some of your favorite trends" } };
                    await dc.PromptAsync(nameof(SocialMediaPrompt), options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var hashResult = (string)results.Result;

                    if (hashResult != null)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received HashTag: {hashResult}"), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Nothing recognized"), cancellationToken);
                    }
                }
            })
            .Send("hello")
            .AssertReply("What are some of your favorite trends")
            .Send("Trends? Does #WM35 count?")
            .AssertReply("Bot received HashTag: #WM35")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Mention_Prompt()
        {
            var conversationState = new ConversationState(new MemoryStorage());
            var dialogState = conversationState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(conversationState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add number prompt to DialogSet.
            var socialMention = new SocialMediaPrompt(nameof(SocialMediaPrompt), SocialMediaPromptType.Mention, defaultLocale: Culture.English);
            dialogs.Add(socialMention);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (!turnContext.Responded && results.Status == DialogTurnStatus.Empty && results.Status != DialogTurnStatus.Complete)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "Hey , Who should I send a tweet to?" } };
                    await dc.PromptAsync(nameof(SocialMediaPrompt), options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var mentionResult = (string)results.Result;

                    if (mentionResult != null)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received Mention: {mentionResult}"), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Nothing recognized"), cancellationToken);
                    }
                }
            })
            .Send("hello")
            .AssertReply("Hey , Who should I send a tweet to?")
            .Send("Send a message to @VinothRajendran")
            .AssertReply("Bot received Mention: @VinothRajendran")
            .StartTestAsync();
        }
    }
}
