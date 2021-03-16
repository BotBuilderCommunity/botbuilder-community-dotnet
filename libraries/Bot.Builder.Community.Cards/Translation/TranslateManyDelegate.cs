using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Cards.Translation
{
    public delegate Task<IEnumerable<string>> TranslateManyDelegate(IEnumerable<string> inputs, CancellationToken cancellationToken);
}