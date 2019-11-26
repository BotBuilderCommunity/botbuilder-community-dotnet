using Alexa.NET.Response;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Alexa.Attachments
{
    public class CardAttachment : Attachment
    {
        public CardAttachment(ICard card)
        {
            Card = card;
        }

        public ICard Card { get; set; }
    }
}
