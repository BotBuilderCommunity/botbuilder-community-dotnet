using Bot.Builder.Community.Adapters.Infobip.Core;
using Microsoft.Bot.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Infobip.Core.Models;

namespace Bot.Builder.Community.Adapters.Infobip.Messages
{
    public interface IInfobipMessagesClient : IDisposable
    {
        /// <summary>
        /// Get attachment data from the specified URL.
        /// </summary>
        /// <param name="url">The URL to download the attachment from.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>An Attachment with the downloaded content.</returns>
        Task<Attachment> GetAttachmentAsync(string url, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the content type of a resource at the specified URL.
        /// </summary>
        /// <param name="url">The URL to check the content type for.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The content type of the resource.</returns>
        Task<string> GetContentTypeAsync(string url, CancellationToken cancellationToken = default);

        /// <summary>
        /// Send a message using the Messages API.
        /// </summary>
        /// <typeparam name="T">The expected response type.</typeparam>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The response from the Messages API.</returns>
        Task<T> SendAsync<T>(object message, CancellationToken cancellationToken = default);
    }
}