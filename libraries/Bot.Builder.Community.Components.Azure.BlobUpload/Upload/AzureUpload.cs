using System;
using System.IO;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Azure;
using System.Net;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Components.Azure.BlobUpload.Upload
{
    internal class AzureUpload : IAzureUpload
    {
        readonly BlobServiceClient _blobServiceClient;
        BlobContainerClient _blobContainerClient;
        private readonly PublicAccessType _publicAccessType;
        private readonly DeleteSnapshotsOption _deleteSnapshotsOption;

        public AzureUpload(string connectionString, string containerName,
            PublicAccessType publicAccessType = PublicAccessType.None, DeleteSnapshotsOption deleteSnapshotsOption = DeleteSnapshotsOption.None)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
            _blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            this._publicAccessType = publicAccessType;
            this._deleteSnapshotsOption = deleteSnapshotsOption;

            _blobServiceClient = new BlobServiceClient(connectionString);

            CreateContainer(containerName);

            if (_blobContainerClient == null)
                throw new Exception("Create Container is failed");
        }
        private async void CreateContainer(string containerName)
        {
            try
            {
                _blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                if (_blobContainerClient != null)
                {
                    bool isExits = await _blobContainerClient.ExistsAsync();
                    
                    if (isExits)
                        return;

                    await _blobContainerClient.CreateIfNotExistsAsync(publicAccessType: _publicAccessType, cancellationToken: default);
                }

            }
            catch (RequestFailedException)
            {

            }
        }

        public async Task<string> UploadAsync(string fileUrl, string fileName)
        {
            var wc = new WebClient();
            var stream = new MemoryStream(wc.DownloadData(fileUrl));

            var blob = _blobContainerClient.GetBlobClient(fileName);

            if (_deleteSnapshotsOption == DeleteSnapshotsOption.None)
                return blob.Uri.AbsoluteUri;

            await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);

            var result = await blob.UploadAsync(stream);

            return blob.Uri.AbsoluteUri;
        }
    }
}
