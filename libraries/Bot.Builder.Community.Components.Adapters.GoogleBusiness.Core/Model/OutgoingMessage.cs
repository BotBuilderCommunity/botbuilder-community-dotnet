using System.Collections.Generic;

namespace Bot.Builder.Community.Components.Adapters.GoogleBusiness.Core.Model
{

    public class OutgoingMessage
    {
        public string Name { get; set; }
        public string MessageId { get; set; }
        public string Text { get; set; }
        public string Fallback { get; set; }
        public bool ContainsRichText { get; set; }
        public RichCardContent RichCard { get; set; }
        public List<Suggestion> Suggestions { get; set; } = new List<Suggestion>();

        public Representative Representative { get; set; } = new Representative()
        {
            DisplayName = "Contoso",
            RepresentativeType = "BOT"
        };

        public Image Image { get; set; }
    }
}
