using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Cards.Management
{
    public delegate Task<Activity> GenerateCardsDelegate(ITurnContext turnContext, CancellationToken cancellationToken);
}