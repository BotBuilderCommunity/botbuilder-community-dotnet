using System;
using Bot.Builder.Community.Adapters.Google.Core.Model;
using Bot.Builder.Community.Adapters.Google.Core.Model.Request;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapters.Google.Core.Helpers
{
    public static class Extensions
    {
        public static void EnsureUniqueUserIdInUserStorage(this ConversationRequest conversationRequest)
        {
            if (conversationRequest.User.UserStorage == null || !conversationRequest.User.UserStorage.ContainsKey("UserId"))
            {
                if (conversationRequest.User.UserStorage == null)
                {
                    conversationRequest.User.UserStorage = new JObject();
                }
                
                conversationRequest.User.UserStorage.Add("UserId", Guid.NewGuid().ToString());
            }
        }

        public static string GetUserIdFromUserStorage(this ConversationRequest payload)
        {
            if (payload.User.UserStorage != null && payload.User.UserStorage.ContainsKey("UserId"))
            {
                return payload.User.UserStorage["UserId"].ToString();
            }
            
            return null;
        }
    }
}
