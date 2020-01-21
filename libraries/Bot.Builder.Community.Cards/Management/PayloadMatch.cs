using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards.Management
{
    internal class PayloadMatch
    {
        public PayloadMatch(IMessageActivity activity, Attachment attachment)
        {
            Activity = activity;
            Attachment = attachment;
        }

        public PayloadMatch(IMessageActivity activity, Attachment attachment, CardAction cardAction)
        {
            Activity = activity;
            Attachment = attachment;
            CardAction = cardAction;
        }

        public PayloadMatch(IMessageActivity activity, Attachment attachment, JObject submitAction)
        {
            Activity = activity;
            Attachment = attachment;
            SubmitAction = submitAction;
        }

        public IMessageActivity Activity { get; }

        public Attachment Attachment { get; }

        public CardAction CardAction { get; }
        
        public JObject SubmitAction { get; }
    }
}