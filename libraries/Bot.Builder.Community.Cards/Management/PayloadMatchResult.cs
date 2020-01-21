using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards.Management
{
    internal class PayloadMatchResult
    {
        public List<PayloadMatch> Matches { get; } = new List<PayloadMatch>();

        internal void Add(IMessageActivity savedActivity, Attachment savedAttachment, CardAction savedAction)
        {
            Matches.Add(new PayloadMatch(savedActivity, savedAttachment, savedAction));
        }

        internal void Add(IMessageActivity savedActivity, Attachment savedAttachment, JObject submitAction)
        {
            Matches.Add(new PayloadMatch(savedActivity, savedAttachment, submitAction));
        }

        internal IMessageActivity FoundActivity() => GetMatchNode(match => match.Activity);

        internal Attachment FoundAttachment() => GetMatchNode(match => match.Attachment);

        private T GetMatchNode<T>(Func<PayloadMatch, T> selector)
            where T : class
        {
            var matches = Matches.Select(selector).Distinct();

            // If there's more than one match then there might as well be zero
            // because there's no way to tell which is correct
            return matches.Count() == 1 ? matches.Single() : null;
        }
    }
}