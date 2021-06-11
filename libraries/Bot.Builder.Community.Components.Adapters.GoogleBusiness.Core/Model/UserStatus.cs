using System;

namespace Bot.Builder.Community.Components.Adapters.GoogleBusiness.Core.Model
{
    public class UserStatus
    {
        public bool IsTyping { get; set; }
        public bool RequestedLiveAgent { get; set; }
        public DateTime CreateTime { get; set; }
    }
}