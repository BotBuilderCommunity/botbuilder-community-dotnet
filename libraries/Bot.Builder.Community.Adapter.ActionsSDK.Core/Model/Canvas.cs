using System.Collections.Generic;

namespace Bot.Builder.Community.Adapters.ActionsSDK.Core.Model
{
    public class Canvas
    {
        public string Url { get; set; }

        public List<string> Data { get; set; }

        public bool SuppressMic { get; set; }
    }
}