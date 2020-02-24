using Alexa.NET.Response;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Alexa.Core.Attachments
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
