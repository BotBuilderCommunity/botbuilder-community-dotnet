using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Cards.Management
{
    internal class CardActivity
    {
        public CardActivity(Attachment attachment)
        {
            Attachment = attachment ?? throw new System.ArgumentNullException(nameof(attachment));
        }

        public CardActivity(GenerateCardsDelegate generateCards)
        {
            GenerateCards = generateCards;
        }

        public GenerateCardsDelegate GenerateCards { get; }

        public Attachment Attachment { get; }
    }
}