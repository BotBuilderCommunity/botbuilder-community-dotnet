using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Components.AzureQueueStorage
{
    public class QueueStorage : Dialog
    {
        [JsonProperty("$Kind")] 
        public const string Kind = "BotBuilderCommunity.QueueStorage";

        [JsonProperty("connectionString")]
        public StringExpression ConnectionString { get; set; }

        [JsonProperty("queueName")]
        public StringExpression QueueName { get; set; }

        [JsonProperty("visibilityTimeout")]
        public StringExpression VisibilityTimeout { get; set; }

        [JsonProperty("timeToLive")]
        public StringExpression TimeToLive { get; set; }

        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }
        

        public QueueStorage([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0) : base()
        {
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            
            //Azure Storage connection string
            var connectionString = ConnectionString.GetValue(dc.State);

            if(string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            //Name of the storage queue where entities will be queued.
            var queueName = QueueName.GetValue(dc.State);

            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentNullException(nameof(queueName));
            }

            
            var azureQueueStorage = new Microsoft.Bot.Builder.Azure.Queues.AzureQueueStorage(connectionString, queueName);
            var activity = dc.Context.Activity;

            if(activity == null)
                throw new ArgumentNullException(nameof(activity));

            //Default value of 0. Cannot be larger than 7 days
            TimeSpan? tsVisibilityTimeout = null;

            if (VisibilityTimeout != null)
            {
                var visibilityTimeout = VisibilityTimeout.GetValue(dc.State);

                if (!string.IsNullOrEmpty(visibilityTimeout))
                {
                    tsVisibilityTimeout = TimeSpan.Parse(visibilityTimeout);
                }
            }

            //Specifies the time-to-live interval for the message.
            TimeSpan? tsTimeToLive = null;

            if (TimeToLive != null)
            {
                var timeToLive = TimeToLive.GetValue(dc.State);
        
                if (!string.IsNullOrEmpty(timeToLive))
                {
                    tsTimeToLive = TimeSpan.Parse(timeToLive);
                }
            }

            //Queue an Activity to an Azure.Storage.Queues.QueueClient
            var result = await azureQueueStorage.QueueActivityAsync(activity, tsVisibilityTimeout, tsTimeToLive, cancellationToken);

            if (this.ResultProperty != null)
            {
                dc.State.SetValue(this.ResultProperty.GetValue(dc.State), JObject.Parse(result));
            }

            return await dc.EndDialogAsync(result: result, cancellationToken: cancellationToken).ConfigureAwait(false);

        }
    }
}
