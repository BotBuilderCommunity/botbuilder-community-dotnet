﻿using System.Collections.Generic;

namespace Bot.Builder.Community.Adapters.ActionsSDK.Core.Model.ContentItems
{
    internal class InternalList
    {
        public string Title { get; set; }
        
        public string Subtitle { get; set; }

        public List<InternalListItem> Items { get; set; }
    }
}