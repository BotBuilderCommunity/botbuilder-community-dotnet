using System.Collections.Generic;

namespace Bot.Builder.Community.Adapters.Zoom.Models
{
    public class InteractiveMessageSelectPayload : InteractiveMessagePayload
    {
        public List<SelectedItem> SelectedItems { get; set; }
    }

    public class SelectedItem
    {
        public string Value { get; set; }
    }
}