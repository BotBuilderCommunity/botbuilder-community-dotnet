using System.Collections.Generic;
using Bot.Builder.Community.Adapters.Google.Model;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Google.Core.Model.Attachments
{
    public class ListAttachment : Attachment
    {
        public ListAttachment()
        {
            ContentType = "google/list-attachment";
        }

        public ListAttachment(string title, List<OptionItem> items, ListAttachmentStyle listStyle)
        {
            Title = title;
            Items = items;
            ListStyle = listStyle;

            ContentType = "google/list-attachment";
        }

        public string Title { get; set; }

        public List<OptionItem> Items { get; set; }

        public ListAttachmentStyle ListStyle { get; set; } = ListAttachmentStyle.List;
    }

    public enum ListAttachmentStyle
    {
        Carousel,
        List
    }
}
