using System;

namespace Bot.Builder.Community.Adapters.Twitter.Webhooks.Models
{
    /// <summary>
    /// Exception thrown by Twitter.
    /// </summary>
    public class TwitterException : Exception
    {
        internal TwitterException(string message) : base(message)
        {

        }
    }
}
