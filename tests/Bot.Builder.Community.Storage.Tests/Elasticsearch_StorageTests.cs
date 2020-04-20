using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Bot.Builder.Community.Storage.Elasticsearch;
using Elasticsearch.Net;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nest;
using Nest.JsonNetSerializer;

namespace Bot.Builder.Community.Storage.Tests
{
    [TestClass]
    public class ElasticsearchStorageTests : StorageBaseTests
    {
        private string elasticsearchEndpoint;
        private IStorage storage;

        public ElasticsearchStorageTests() { }

        [TestInitialize]
        public void Initialize()
        {
            // Get elasticsearch configuration from external file.
            var config = new ConfigurationBuilder()
                .AddJsonFile("elasticsearchstoragesettings.json")
                .Build();

            var elasticsearchStorageOptions = new ElasticsearchStorageOptions
            {
                ElasticsearchEndpoint = new Uri(config["Endpoint"]),
                UserName = config["UserName"],
                Password = config["Password"],
                IndexName = config["IndexName"],
                IndexMappingDepthLimit = int.Parse(config["IndexMappingDepthLimit"])
            };

            elasticsearchEndpoint = config["Endpoint"];

            storage = new ElasticsearchStorage(elasticsearchStorageOptions);
        }

        [TestCleanup]
        public async Task TestCleanUp()
        {
            if (storage != null)
            {
                // Get elasticsearch configuration from external file.
                var config = new ConfigurationBuilder()
                    .AddJsonFile("elasticsearchstoragesettings.json")
                    .Build();

                var elasticsearchStorageOptions = new ElasticsearchStorageOptions
                {
                    ElasticsearchEndpoint = new Uri(config["Endpoint"]),
                    UserName = config["UserName"],
                    Password = config["Password"],
                    IndexName = config["IndexName"],
                    IndexMappingDepthLimit = int.Parse(config["IndexMappingDepthLimit"])
                };
                var connectionPool = new SingleNodeConnectionPool(elasticsearchStorageOptions.ElasticsearchEndpoint);
                var connectionSettings = new ConnectionSettings(connectionPool, sourceSerializer: JsonNetSerializer.Default);

                if (!string.IsNullOrEmpty(elasticsearchStorageOptions.UserName) && !string.IsNullOrEmpty(elasticsearchStorageOptions.Password))
                {
                    connectionSettings = connectionSettings.BasicAuthentication(elasticsearchStorageOptions.UserName, elasticsearchStorageOptions.Password);
                }

                var client = new ElasticClient(connectionSettings);
                try
                {
                    await client.Indices.DeleteAsync(Indices.Index(config["IndexName"] + "-" + DateTime.Now.ToString("MM-dd-yyyy")));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error cleaning up resources: {0}", ex.ToString());
                }
            }
        }

        [TestMethod]
        [TestCategory("Storage")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Ignore("Ignore this test as it requires an Elasticsearch environment to run.")]
        public void Constructor_Should_Throw_Exception_On_InvalidOptions()
        {
            // No Options. Should throw. 
            Assert.ThrowsException<ArgumentNullException>(() => new ElasticsearchStorage(null));

            // No Endpoint. Should throw. 
            Assert.ThrowsException<ArgumentNullException>(() => new ElasticsearchStorage(new ElasticsearchStorageOptions
            {
                UserName = "testUserName",
                Password = "testPassword",
                IndexName = "testIndexName",
                IndexMappingDepthLimit = 100000,
            }));

            // No Index name. Should throw. 
            Assert.ThrowsException<ArgumentNullException>(() => new ElasticsearchStorage(new ElasticsearchStorageOptions
            {
                ElasticsearchEndpoint = new Uri(elasticsearchEndpoint),
                UserName = "testUserName",
                Password = "testPassword",
                IndexMappingDepthLimit = 100000,
            }));
        }

        [TestMethod]
        [TestCategory("Storage")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Ignore("Ignore this test as it requires an Elasticsearch environment to run.")]
        public async Task ElasticsearchStorage_CreateObjectTest()
        {
            await this.CreateObjectTest(storage);
        }

        [TestMethod]
        [TestCategory("Storage")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Ignore("Ignore this test as it requires an Elasticsearch environment to run.")]
        public async Task ElasticsearchStorage_ReadUnknownTest()
        {
            await this.ReadUnknownTest(storage);
        }

        [TestMethod]
        [TestCategory("Storage")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Ignore("Ignore this test as it requires an Elasticsearch environment to run.")]
        public async Task ElasticsearchStorage_UpdateObjectTest()
        {
            await this.UpdateObjectTest(storage);
        }


        [TestMethod]
        [TestCategory("Storage")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Ignore("Ignore this test as it requires an Elasticsearch environment to run.")]
        public async Task ElasticsearchStorage_DeleteObjectTest()
        {
            await this.DeleteObjectTest(storage);
        }

        [TestMethod]
        [TestCategory("Storage")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Ignore("Ignore this test as it requires an Elasticsearch environment to run.")]
        public async Task ElasticsearchStorage_HandleCrazyKeys()
        {
            await this.HandleCrazyKeys(storage);
        }
    }
}
