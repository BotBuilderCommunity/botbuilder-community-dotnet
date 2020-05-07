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
            if (!string.IsNullOrEmpty(conversationRequest.User.UserStorage))
            {
                var values = JObject.Parse(conversationRequest.User.UserStorage);
                if (!values.ContainsKey("UserId"))
                {
                    values.Add("UserId", Guid.NewGuid().ToString());       
                }
            }
        }

        public static string GetUserIdFromUserStorage(this ConversationRequest payload)
        {
            if (!string.IsNullOrEmpty(payload.User.UserStorage))
            {
                var values = JObject.Parse(payload.User.UserStorage);
                if (values.ContainsKey("UserId"))
                {
                    return values.GetValue("UserId").ToString();
                }
            }

            return null;
        }
    }
}
