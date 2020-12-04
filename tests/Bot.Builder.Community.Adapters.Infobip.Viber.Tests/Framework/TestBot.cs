using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Infobip.Viber.Tests.Framework
{
    class TestBot : ActivityHandler
    {
        public Func<int, ITurnContext<IMessageActivity>, CancellationToken, Task> OnMessageActivity;
        public int OnMessageActivityInvocationCount = 0;

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await OnMessageActivity(OnMessageActivityInvocationCount++, turnContext, cancellationToken).ConfigureAwait(false);
        }
    }
}
