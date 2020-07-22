namespace Bot.Builder.Community.Adapters.ActionsSDK.Core.Model.ContentItems
{
    public class Card
    {
        public string Title { get; set; }

        public string Subtitle { get; set; }

        public string Text { get; set; }

        public Image Image { get; set; }

        public ImageFill ImageFill { get; set; }

        public Link Button { get; set; }
    }
}