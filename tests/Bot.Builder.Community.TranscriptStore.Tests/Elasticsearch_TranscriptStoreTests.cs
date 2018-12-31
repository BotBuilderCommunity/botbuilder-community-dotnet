using Bot.Builder.Community.TranscriptStore.Elasticsearch;
using Elasticsearch.Net;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nest;
using Nest.JsonNetSerializer;
using System;
using System.Threading.Tasks;

namespace Bot.Builder.Community.TranscriptStore.Tests
{
    [TestClass]
    public class Elasticsearch_TranscriptStoreTests : TranscriptStoreBaseTests
    {
        private string elasticsearchEndpoint;
        private ITranscriptStore transcriptStore;

        public Elasticsearch_TranscriptStoreTests() { }

        [TestInitialize]
        public void Initialize()
        {
            // Get elasticsearch configuration from external file.
            var config = new ConfigurationBuilder()
                .AddJsonFile("elasticsearchtranscriptstoresettings.json")
                .Build();

            var elasticsearchTranscriptStoreOptions = new ElasticsearchTranscriptStoreOptions
            {
                ElasticsearchEndpoint = new Uri(config["Endpoint"]),
                UserName = config["UserName"],
                Password = config["Password"],
                IndexName = config["IndexName"]
            };

            elasticsearchEndpoint = config["Endpoint"];

            transcriptStore = new ElasticsearchTranscriptStore(elasticsearchTranscriptStoreOptions);
        }

        [TestCleanup]
        public async Task TestCleanUp()
        {
            if (transcriptStore != null)
            {
                // Get elasticsearch configuration from external file.
                var config = new ConfigurationBuilder()
                    .AddJsonFile("elasticsearchtranscriptstoresettings.json")
                    .Build();

                var elasticsearchTranscriptStoreOptions = new ElasticsearchTranscriptStoreOptions
                {
                    ElasticsearchEndpoint = new Uri(config["Endpoint"]),
                    UserName = config["UserName"],
                    Password = config["Password"],
                    IndexName = config["IndexName"]
                };

                var connectionPool = new SingleNodeConnectionPool(elasticsearchTranscriptStoreOptions.ElasticsearchEndpoint);
                var connectionSettings = new ConnectionSettings(connectionPool, sourceSerializer: JsonNetSerializer.Default);

                if (!string.IsNullOrEmpty(elasticsearchTranscriptStoreOptions.UserName) && !string.IsNullOrEmpty(elasticsearchTranscriptStoreOptions.Password))
                {
                    connectionSettings = connectionSettings.BasicAuthentication(elasticsearchTranscriptStoreOptions.UserName, elasticsearchTranscriptStoreOptions.Password);
                }

                var client = new ElasticClient(connectionSettings);
                try
                {
                    await client.DeleteIndexAsync(Indices.Index(config["IndexName"] + "-" + DateTime.Now.ToString("MM-dd-yyyy")));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error cleaning up resources: {0}", ex.ToString());
                }
            }
        }

        [TestMethod]
        [TestCategory("Transcript Store")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Ignore("Ignore this test as it requires an Elasticsearch environment to run.")]
        public void Constructor_Should_Throw_Exception_On_InvalidOptions()
        {
            // No Options. Should throw. 
            Assert.ThrowsException<ArgumentNullException>(() => new ElasticsearchTranscriptStore(null));

            // No Endpoint. Should throw. 
            Assert.ThrowsException<ArgumentNullException>(() => new ElasticsearchTranscriptStore(new ElasticsearchTranscriptStoreOptions
            {
                UserName = "testUserName",
                Password = "testPassword",
                IndexName = "testIndexName"
            }));

            // No Index name. Should throw. 
            Assert.ThrowsException<ArgumentNullException>(() => new ElasticsearchTranscriptStore(new ElasticsearchTranscriptStoreOptions
            {
                ElasticsearchEndpoint = new Uri(elasticsearchEndpoint),
                UserName = "testUserName",
                Password = "testPassword"
            }));
        }

        [TestMethod]
        [TestCategory("Transcript Store")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Ignore("Ignore this test as it requires an Elasticsearch environment to run.")]
        public async Task ElasticsearchTranscriptStore_LogSingleActivityTest()
        {
            await base.LogSingleActivityTest(transcriptStore);
        }

        [TestMethod]
        [TestCategory("Transcript Store")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Ignore("Ignore this test as it requires an Elasticsearch environment to run.")]
        public async Task ElasticsearchTranscriptStore_LogMultipleActivitiesTest()
        {
            await base.LogMultipleActivitiesTest(transcriptStore);
        }

        [TestMethod]
        [TestCategory("Transcript Store")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Ignore("Ignore this test as it requires an Elasticsearch environment to run.")]
        public async Task ElasticsearchTranscriptStore_RetrieveTranscriptsTest()
        {
            await base.RetrieveTranscriptsTest(transcriptStore);
        }

        [TestMethod]
        [TestCategory("Transcript Store")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Ignore("Ignore this test as it requires an Elasticsearch environment to run.")]
        public async Task ElasticsearchTranscriptStore_ListTranscriptsTest()
        {
            await base.ListTranscriptsTest(transcriptStore);
        }

        [TestMethod]
        [TestCategory("Transcript Store")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Ignore("Ignore this test as it requires an Elasticsearch environment to run.")]
        public async Task ElasticsearchTranscriptStore_DeleteTranscriptsTest()
        {
            await base.DeleteTranscriptsTest(transcriptStore);
        }
    }
}
