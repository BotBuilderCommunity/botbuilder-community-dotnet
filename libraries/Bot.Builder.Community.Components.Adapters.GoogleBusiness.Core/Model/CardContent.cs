using System.Collections.Generic;

namespace Bot.Builder.Community.Components.Adapters.GoogleBusiness.Core.Model
{
    public class CardContent
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<Suggestion> Suggestions { get; set; } = new List<Suggestion>();
        public Media Media { get; set; }
    }
}