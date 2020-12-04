using Bot.Builder.Community.Adapters.Infobip.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Adapters.Infobip.WhatsApp
{
    public interface IInfobipWhatsAppClient: IInfobipClientBase
    {
        /// <summary>
        /// Get the Content/Media Type for the resource at the given url.
        /// </summary>
        /// <remarks>
        /// Return null if the Media Type cannot be determined.
        /// </remarks>
        Task<string> GetContentTypeAsync(string url, CancellationToken cancellationToken = default);
    }
}
