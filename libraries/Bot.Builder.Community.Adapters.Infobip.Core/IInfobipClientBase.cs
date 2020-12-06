using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Infobip.Core.Models;

namespace Bot.Builder.Community.Adapters.Infobip.Core
{
    public interface IInfobipClientBase
    {
        /// <summary>
        /// Send the given message to Infobip.
        /// </summary>
        Task<InfobipResponseMessage> SendMessageAsync(InfobipOmniFailoverMessageBase infobipMessage, CancellationToken cancellationToken = default);
    }
}
