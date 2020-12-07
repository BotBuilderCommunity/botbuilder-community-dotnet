using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Cards.Translation
{
    public delegate Task<IList<string>> TranslateManyDelegate(IList<string> inputs, CancellationToken cancellationToken);
}