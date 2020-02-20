using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Cards.Management
{
    internal class PayloadMatchResult
    {
        private bool _alreadyFoundMatch;

        public IMessageActivity SavedActivity { get; private set; }

        public Attachment SavedAttachment { get; private set; }

        public object SavedAction { get; private set; }

        internal void Add(IMessageActivity savedActivity, Attachment savedAttachment, object savedAction)
        {
            // In all cases, null is used to indicate that either no match was found or more than one match was found.
            // If there's more than one match then there might as well be zero
            // because there's no way to determine which one is correct.
            SavedActivity = _alreadyFoundMatch && savedActivity != SavedActivity ? null : savedActivity;
            SavedAttachment = _alreadyFoundMatch && savedAttachment != SavedAttachment ? null : savedAttachment;
            SavedAction = _alreadyFoundMatch && savedAction != SavedAction ? null : savedAction;

            _alreadyFoundMatch = true;
        }
    }
}