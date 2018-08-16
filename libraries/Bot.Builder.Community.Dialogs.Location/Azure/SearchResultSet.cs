using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Bot.Builder.Community.Dialogs.Location.Azure
{
    [Serializable]
    public class SearchResultSet
    {
        /// <summary>
        /// The location list.
        /// </summary>
        [JsonProperty(PropertyName = "results")]
        public List<SearchResult> Results { get; set; }
    }
}
