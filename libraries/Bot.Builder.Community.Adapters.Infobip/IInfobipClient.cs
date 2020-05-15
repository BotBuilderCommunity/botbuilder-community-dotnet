using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Infobip.Models;

namespace Bot.Builder.Community.Adapters.Infobip
{
    public interface IInfobipClient
    {
        /// <summary>
        /// Send the given message to Infobip.
        /// </summary>
        Task<InfobipResponseMessage> SendMessageAsync(InfobipOmniFailoverMessage infobipMessage, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the Content/Media Type for the resource at the given url.
        /// </summary>
        /// <remarks>
        /// Return null if the Media Type cannot be determined.
        /// </remarks>
        Task<string> GetContentTypeAsync(string url, CancellationToken cancellationToken = default);
    }
}
