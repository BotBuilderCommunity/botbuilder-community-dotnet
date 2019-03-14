using Microsoft.Bot.Builder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Storage.Tests
{
    public class StorageBaseTests
    {
        protected async Task CreateObjectTest(IStorage storage)
        {
            var storeItems = new Dictionary<string, object>
            {
                ["CreatePoco1"] = new PocoItem() { Id = "1" },
                ["CreatePoco2"] = new PocoItem() { Id = "2" }
            };

            await storage.WriteAsync(storeItems);

            var readStoreItems = new Dictionary<string, object>(await storage.ReadAsync(storeItems.Keys.ToArray()));

            Assert.IsInstanceOfType(readStoreItems["CreatePoco1"], typeof(PocoItem));
            Assert.IsInstanceOfType(readStoreItems["CreatePoco2"], typeof(PocoItem));

            var createPoco1 = readStoreItems["CreatePoco1"] as PocoItem;

            Assert.IsNotNull(createPoco1, "CreatePoco1 should not be null");
            Assert.AreEqual("1", createPoco1.Id, "CreatePoco1.Id should be 1");

            var createPoco2 = readStoreItems["CreatePoco2"] as PocoItem;

            Assert.IsNotNull(createPoco2, "CreatePoco2 should not be null");
            Assert.AreEqual("2", createPoco2.Id, "CreatePoco2.Id should be 2");
        }

        protected async Task ReadUnknownTest(IStorage storage)
        {
            var result = await storage.ReadAsync(new[] { "unknown" });
            Assert.IsNotNull(result, "result should not be null");
            Assert.IsNull(result.FirstOrDefault(e => e.Key == "unknown").Value, "\"unknown\" key should have returned no value");
        }

        protected async Task UpdateObjectTest(IStorage storage)
        {
            var originalPocoItem = new PocoItem() { Id = "1", Count = 1 };

            // First write should work
            var dict = new Dictionary<string, object>()
            {
                { "PocoItem", originalPocoItem },
            };

            await storage.WriteAsync(dict);

            var loadedStoreItems = new Dictionary<string, object>(await storage.ReadAsync(new[] { "PocoItem" }));
            var updatePocoItem = loadedStoreItems["PocoItem"] as PocoItem;

            Assert.IsNotNull(updatePocoItem);
            Assert.AreEqual("1", updatePocoItem.Id, "UpdatePocoItem.Id should be 1");
            Assert.AreEqual(1, updatePocoItem.Count, "UpdatePocoItem.Count should be 1");

            // 2nd write should work.
            updatePocoItem.Count++;
            await storage.WriteAsync(loadedStoreItems);

            var reloadedStoreItems = new Dictionary<string, object>(await storage.ReadAsync(new[] { "PocoItem" }));
            var reloadedUpdatePocoItem = reloadedStoreItems["PocoItem"] as PocoItem;

            Assert.IsNotNull(reloadedUpdatePocoItem);
            Assert.AreEqual("1", reloadedUpdatePocoItem.Id, "ReloadedUpdatePocoItem.Id should be 1");
            Assert.AreEqual(2, reloadedUpdatePocoItem.Count, "ReloadedUpdatePocoItem.Count should be 2");
        }

        protected async Task DeleteObjectTest(IStorage storage)
        {
            // First write should work
            var dictionary = new Dictionary<string, object>()
            {
                ["DeletePoco1"] = new PocoItem() { Id = "1", Count = 1 }
            };

            await storage.WriteAsync(dictionary);

            var storeItems = await storage.ReadAsync(new[] { "DeletePoco1" });
            var deletePoco1 = storeItems.First().Value as PocoItem;

            Assert.IsNotNull(deletePoco1);
            Assert.AreEqual("1", deletePoco1.Id, "DeletePoco1.Id should be 1");
            Assert.AreEqual(1, deletePoco1.Count, "DeletePoco1.Count should be 1");

            await storage.DeleteAsync(new[] { "DeletePoco1" });
            var reloadedStoreItems = await storage.ReadAsync(new[] { "DeletePoco1" });

            Assert.AreEqual(0, reloadedStoreItems.Count, "No store item should have been found because it was deleted.");
        }

        protected async Task HandleCrazyKeys(IStorage storage)
        {
            var key = "\\Poc?oI/t#em 1";
            var storeItem = new PocoItem() { Id = "1" };

            var dictionary = new Dictionary<string, object>()
            {
                [key] = storeItem
            };

            await storage.WriteAsync(dictionary);

            var storeItems = await storage.ReadAsync(new[] { key });

            storeItem = storeItems.FirstOrDefault(si => si.Key == key).Value as PocoItem;

            Assert.IsNotNull(storeItem);
            Assert.AreEqual("1", storeItem.Id);
        }
    }

    public class PocoItem
    {
        public string Id { get; set; }

        public int Count { get; set; }

        public string[] ExtraBytes { get; set; }
    }
}
