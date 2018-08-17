using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Middleware.AzureAdAuthentication
{
    public interface IAuthTokenStorage
    {
        ConversationAuthToken LoadConfiguration(string id);
        void SaveConfiguration(ConversationAuthToken state);
    }

    public class ConversationAuthToken
    {
        public ConversationAuthToken(string id)
        {
            Id = id;
        }

        public string Id { get; set; }

        // Note this is stored in memory in plain text for demonstration purposes
        // use your common sense when applying this in your apps! i.e. take appropriate precautions 
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public DateTime ExpiresIn { get; set; }
    }

    public class InMemoryAuthTokenStorage : IAuthTokenStorage
    {
        private static readonly Dictionary<string, ConversationAuthToken> InMemoryDictionary = new Dictionary<string, ConversationAuthToken>();

        public ConversationAuthToken LoadConfiguration(string id)
        {
            if (InMemoryDictionary.ContainsKey(id))
            {
                return InMemoryDictionary[id];
            }

            return null;
        }

        public void SaveConfiguration(ConversationAuthToken state)
        {
            InMemoryDictionary[state.Id] = state;
        }
    }

    public class DiskAuthTokenStorage : IAuthTokenStorage
    {
        //private static readonly Dictionary<string, ConversationAuthToken> InMemoryDictionary = new Dictionary<string, ConversationAuthToken>();

        public ConversationAuthToken LoadConfiguration(string id)
        {
            id = "singletonkey";

            Dictionary<string, ConversationAuthToken> values = File.Exists("accessTokens.json") ? JsonConvert.DeserializeObject<Dictionary<string, ConversationAuthToken>>(File.ReadAllText("accessTokens.json")) : new Dictionary<string, ConversationAuthToken>();

            if (values.ContainsKey(id))
            {
                return values[id];
            }

            return null;
        }

        public void SaveConfiguration(ConversationAuthToken state)
        {
            state.Id  = "singletonkey";

            Dictionary<string, ConversationAuthToken> values = File.Exists("accessTokens.json") ? JsonConvert.DeserializeObject<Dictionary<string, ConversationAuthToken>>(File.ReadAllText("accessTokens.json")) : new Dictionary<string, ConversationAuthToken>();

            values[state.Id] = state;

            File.WriteAllText("accessTokens.json", JsonConvert.SerializeObject(values));
        }
    }

    // write azure key value token storage
}
