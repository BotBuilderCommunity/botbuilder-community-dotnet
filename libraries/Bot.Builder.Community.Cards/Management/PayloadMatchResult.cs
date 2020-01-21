using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards.Management
{
    internal class PayloadMatchResult
    {
        private ISet<IMessageActivity> ActivityMatches { get; } = new HashSet<IMessageActivity>();

        private ISet<Attachment> AttachmentMatches { get; } = new HashSet<Attachment>();

        private ISet<object> ActionMatches { get; } = new HashSet<object>();

        private IDictionary<Attachment, IMessageActivity> ActivitiesByAttachment { get; } = new Dictionary<Attachment, IMessageActivity>();

        private IDictionary<object, Attachment> AttachmentsByAction { get; } = new Dictionary<object, Attachment>();

        internal void Add(IMessageActivity savedActivity, Attachment savedAttachment, object savedAction)
        {
            ActivityMatches.Add(savedActivity);
            AttachmentMatches.Add(savedAttachment);
            ActionMatches.Add(savedAction);
            ActivitiesByAttachment.Add(savedAttachment, savedActivity);
            AttachmentsByAction.Add(savedAction, savedAttachment);
        }

        internal IMessageActivity FoundActivity() => GetMatch(ActivityMatches);

        internal Attachment FoundAttachment() => GetMatch(AttachmentMatches);

        internal IMessageActivity GetAttachmentParent(Attachment attachment) =>
            ActivitiesByAttachment.TryGetValue(attachment, out var activity) ? activity : null;

        private T GetMatch<T>(ISet<T> set)
            where T : class
        {
            // If there's more than one match then there might as well be zero
            // because there's no way to tell which is correct
            return set.Count() == 1 ? set.Single() : null;
        }
    }
}