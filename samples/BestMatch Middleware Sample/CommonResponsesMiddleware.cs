using Bot.Builder.Community.Middleware.BestMatch;
using Microsoft.Bot.Builder;
using System.Threading;
using System.Threading.Tasks;

namespace BestMatchMiddleware_Sample
{
    public class CommonResponsesMiddleware : BestMatchMiddleware
    {
        [BestMatch(new string[] { "Hi", "Hi There", "Hello there", "Hey", "Hello",
                "Hey there", "Greetings", "Good morning", "Good afternoon", "Good evening", "Good day", "yo" },
            threshold: 0.5, ignoreCase: true, ignoreNonAlphaNumericCharacters: true)]
        public async Task HandleGreeting(ITurnContext context, string messageText, NextDelegate next, CancellationToken cancellationToken)
        {
            await context.SendActivityAsync("Hi. What can I do for you today?");
        }

        [BestMatch(new string[] { "how goes it", "how do", "hows it going", "how are you",
            "how do you feel", "whats up", "sup", "hows things" })]
        public async Task HandleStatusRequest(ITurnContext context, string messageText, NextDelegate next, CancellationToken cancellationToken)
        {
            await context.SendActivityAsync("I'm good thanks. What can I help you with?");
        }

        [BestMatch(new string[] { "bye", "bye bye", "got to go",
            "see you later", "laters", "adios" })]
        public async Task HandleGoodbye(ITurnContext context, string messageText, NextDelegate next, CancellationToken cancellationToken)
        {
            await context.SendActivityAsync("Bye for now. Let me know if I can help you with anything else.");
        }

        [BestMatch(new string[]
        {
            "thanks", "that's great", "ty", "ty very much", "ty. that's great",
            "thank you", "thankyou", "thanks a lot", "brilliant thanks", "cheers", "cheers thanks"
        })]
        public async Task HandleThanks(ITurnContext context, string messageText, NextDelegate next, CancellationToken cancellationToken)
        {
            await context.SendActivityAsync("You're welcome");
        }
    }
}
