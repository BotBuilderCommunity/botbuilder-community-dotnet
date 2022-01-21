namespace Bot.Builder.Community.Components.Adapters.GoogleBusiness.Core.Model
{
    public class Context
    {
        public string CustomContext { get; set; }
        public string EntryPoint { get; set; }
        public string PlaceId { get; set; }
        public string NearPlaceId { get; set; }
        public string ResolvedLocale { get; set; }
        public UserInfo UserInfo { get; set; }
        public Widget Widget { get; set; }
    }
}