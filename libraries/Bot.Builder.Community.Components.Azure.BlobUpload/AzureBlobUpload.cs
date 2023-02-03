using System;
using AdaptiveExpressions.Properties;
using Azure.Storage.Blobs.Models;
using Bot.Builder.Community.Components.Azure.BlobUpload.Upload;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Components.Azure.BlobUpload
{

    public class AzureBlobUpload : Dialog
    {
        [JsonProperty("$Kind")] 
        public const string Kind = "AzureBlobUpload";

        [JsonProperty("FileUrl")] 
        public StringExpression FileUrl { get; set; }

        [JsonProperty("FileName")] 
        public StringExpression FileName { get; set; }

        [JsonProperty("PublicAccessType")] 
        public EnumExpression<PublicAccessType> PublicAccessType { get; set; }

        [JsonProperty("DeleteSnapshotsOption")]
        public EnumExpression<DeleteSnapshotsOption> DeleteSnapshotsOption { get; set; }

        [JsonProperty("Containers")]
        public StringExpression Containers { get; set; }

        [JsonProperty("ConnectionString")] 
        public StringExpression ConnectionString { get; set; }

        [JsonProperty("resultProperty")] 
        public StringExpression ResultProperty { get; set; }

        IAzureUpload _dataUpload;

        public AzureBlobUpload([CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0) : base()
        {
            RegisterSourceLocation(sourceFilePath, sourceLineNumber);

        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null,
            CancellationToken cancellationToken = new CancellationToken())
        {

            IsPropertyIsValid(dc);

            CreateDataStorage(dc);

            var getUrl = FileUrl?.GetValue(dc.State);

            var fileName = FileName?.GetValue(dc.State);

            var result = await _dataUpload.UploadAsync(getUrl, fileName);

            if (ResultProperty != null)
            {
                dc.State.SetValue(this.ResultProperty.GetValue(dc.State), result);
            }

            return await dc.EndDialogAsync(result: result, cancellationToken: cancellationToken);
        }



        private void CreateDataStorage(DialogContext dc)
        {
            if (_dataUpload == null)
                return;

            var containerName = Containers?.GetValue(dc.State);

            var accessType = PublicAccessType?.GetValue(dc.State);

            var deleteSnapshotsOption = DeleteSnapshotsOption?.GetValue(dc.State);

            Enum.TryParse(accessType.ToString(), out PublicAccessType publicAccessType);

            Enum.TryParse(deleteSnapshotsOption.ToString(), out DeleteSnapshotsOption deleteSnapshotsType);

            var connectionString = ConnectionString?.GetValue(dc.State);

            _dataUpload = new AzureUpload(connectionString, containerName, publicAccessType, deleteSnapshotsType);

        }


        private void IsPropertyIsValid(DialogContext dc)
        {

            var getUrl = FileUrl?.GetValue(dc.State);


            if (string.IsNullOrEmpty(getUrl))
            {
                throw new Exception($"{nameof(AzureBlobUpload)} : FileUrl is required");
            }

            var fileName = FileName?.GetValue(dc.State);

            if (string.IsNullOrEmpty(fileName))
            {
                throw new Exception($"{nameof(AzureBlobUpload)} : FileName is required");
            }


            var container = Containers?.GetValue(dc.State);

            if (string.IsNullOrEmpty(container))
            {
                throw new Exception($"{nameof(AzureBlobUpload)} : Container name is required");
            }

            var connectionString = ConnectionString?.GetValue(dc.State);

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception($"{nameof(AzureBlobUpload)} : connection string is required");
            }

        }
    }

}