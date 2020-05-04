using System.Linq;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Zoom.Models
{
    public class ChatResponse
    {
        [JsonProperty(PropertyName = "robot_jid")]
        public string RobotJid { get; set; }

        [JsonProperty(PropertyName = "to_jid")]
        public string ToJid { get; set; }

        [JsonProperty(PropertyName = "account_id")]
        public string AccountId { get; set; }

        [JsonProperty(PropertyName = "is_markdown_support")]
        public bool IsMarkdownSupported {
            get
            {
                if (Content.Body == null)
                {
                    return false;
                }

                if (!Content.Body.All(i => i.GetType() == typeof(FieldsBodyItem)
                                           || i.GetType() == typeof(MessageItem)
                                           || !(i.GetType() == typeof(MessageBodyItemWithLink))))
                {
                    return false;
                }

                var fieldsItems = Content.Body.Where(i => i.GetType() == typeof(FieldsBodyItem)).Select(i => i as FieldsBodyItem);

                if (fieldsItems.Any(f => f.Fields.Any(i => i.Editable)))
                {
                    return false;
                }

                return true;
            }
        }

        public ChatResponseContent Content { get; set; }
    }
}
