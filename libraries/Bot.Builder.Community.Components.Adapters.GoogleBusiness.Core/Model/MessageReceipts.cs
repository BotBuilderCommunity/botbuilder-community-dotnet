using System;

namespace Bot.Builder.Community.Components.Adapters.GoogleBusiness.Core.Model
{
    public class MessageReceipts
    {
        public Receipt[] Receipts { get; set; }
        public DateTime CreateTime { get; set; }
    }
}