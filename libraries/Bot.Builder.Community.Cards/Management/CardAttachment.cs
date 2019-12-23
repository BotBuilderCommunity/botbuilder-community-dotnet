using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Cards.Management
{
    internal class CardAttachment
    {
        public CardAttachment()
        {
        }

        public CardAttachment(Attachment attachment)
        {
            Attachment = attachment ?? throw new System.ArgumentNullException(nameof(attachment));
        }

        public Attachment Attachment { get; }
    }
}