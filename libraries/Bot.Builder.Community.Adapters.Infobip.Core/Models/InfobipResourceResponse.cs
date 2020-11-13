using Microsoft.Bot.Schema;
using System.Collections.Generic;

namespace Bot.Builder.Community.Adapters.Infobip.Core.Models
{
    public class InfobipResourceResponse : ResourceResponse
    {
        public string ActivityId { get; set; }
        public IList<InfobipResponseMessage> ResponseMessages { get; set; }
    }
}
