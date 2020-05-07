using System;
using System.Collections.Generic;
using System.Linq;
using Bot.Builder.Community.Adapters.Google.Core.Model;
using Microsoft.Bot.Builder;

namespace Bot.Builder.Community.Adapters.Google
{
    public static class GoogleContextExtensions
    {
        //public static ConversationRequest GetGoogleRequestPayload(this ITurnContext context)
        //{
        //    try
        //    {
        //        return (ConversationRequest)context.Activity.ChannelData;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        //public static List<string> GoogleGetSurfaceCapabilities(this ITurnContext context)
        //{
        //    var payload = (ConversationRequest)context.Activity.ChannelData;
        //    var capabilities = payload.Surface.Capabilities.Select(c => c.Name);
        //    return capabilities.ToList();
        //}
    }
}
