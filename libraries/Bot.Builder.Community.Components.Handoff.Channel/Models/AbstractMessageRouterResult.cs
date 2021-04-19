using System;

namespace Bot.Builder.Community.Components.Handoff.Channel.Models
{
    [Serializable]
    public abstract class MessageRouterResult
    {
        public string ErrorMessage
        {
            get;
            set;
        }

        protected MessageRouterResult()
        {
            ErrorMessage = string.Empty;
        }

        public abstract string ToJson();
    }
}