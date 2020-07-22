using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.ActionsSDK.Core.Model.ContentItems
{
    internal class InternalListContentItem : ContentItem
    {
        [JsonProperty(PropertyName = "list")]
        public InternalList InternalList { get; set; }
    }

    internal class InternalListItem
    {
        public string Key { get; set; }
    }
}
