using System.Threading;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Cards.Translation
{
    public delegate Task<string> TranslateOneDelegate(string input, CancellationToken cancellationToken);
}