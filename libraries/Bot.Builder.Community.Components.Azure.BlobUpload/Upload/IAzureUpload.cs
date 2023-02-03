using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Bot.Builder.Community.Components.Azure.BlobUpload.Upload
{
    public interface IAzureUpload
    {
        Task<string> UploadAsync(string fileUrl, string fileName);
    }
}