using Microsoft.Bot.Schema;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Adapters.Infobip
{
    public static class InfobipExtensions
    {
        public static async Task DownloadContent(this Attachment self)
        {
            var attachment = await InfobipClient.GetAttachmentAsync(self.ContentUrl).ConfigureAwait(false);
            self.Content = attachment?.Content;
            self.ContentType = attachment?.ContentType;
        }
    }
}
