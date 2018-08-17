using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Middleware.AzureAdAuthentication
{
    public static class ActivityExtensions
    {
        public static bool UserHasJustSentMessage(this Activity activity)
        {
            return activity.Type == ActivityTypes.Message;
        }

        public static bool UserHasJustJoinedConversation(this Activity activity)
        {
            return activity.Type == ActivityTypes.ConversationUpdate && activity.MembersAdded.FirstOrDefault().Id != activity.Recipient.Id;
        }

        public static async Task DoWithTyping(this IActivity activity, Func<Task> action)
        {
            var cts = new CancellationTokenSource();

            activity.SendTypingActivity(cts.Token);

            await action.Invoke().ContinueWith(task => { cts.Cancel(); });
        }

        private static async Task SendTypingActivity(this IActivity iactivity, CancellationToken cancellationToken)
        {
            if (iactivity is Activity activity)
            {
                var connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                while (!cancellationToken.IsCancellationRequested)
                {
                    var isTypingReply = activity.CreateReply();
                    isTypingReply.Type = ActivityTypes.Typing;
                    await connector.Conversations.ReplyToActivityAsync(isTypingReply);

                    await Task.Delay(1000, cancellationToken).ContinueWith(task => { });
                }
            }
        }
    }
}
