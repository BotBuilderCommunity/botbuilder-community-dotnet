using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

                if (!Content.Body.All(i => i.GetType() == typeof(FieldsItem)
                                           || i.GetType() == typeof(MessageItem)
                                           || !(i.GetType() == typeof(MessageItemWithLink))))
                {
                    return false;
                }

                var fieldsItems = Content.Body.Where(i => i.GetType() == typeof(FieldsItem)).Select(i => i as FieldsItem);

                if (fieldsItems.Any(f => f.Fields.Any(i => i.Editable)))
                {
                    return false;
                }

                return true;
            }
        }

        public ChatResponseContent Content { get; set; }
    }

    public class ChatResponseContent
    {
        public Head Head { get; set; }
        public List<BodyItem> Body { get; set; }
    }

    public class Head
    {
        public string Text { get; set; }

        [JsonProperty(PropertyName = "sub_head")]
        public SubHead SubHead { get; set; }

        public Style Style { get; set; }
    }

    public class SubHead
    {
        public string Text { get; set; }

        public Style Style { get; set; }
    }

    public class Style
    {
        public string Color { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
    }

    public class BodyItem
    {
    }

    public class MessageItem : BodyItem
    {
        public string Type => "message";

        public string Text { get; set; }

        public Style Style { get; set; }
    }

    public class ActionsItem : BodyItem
    {
        public string Type => "actions";

        [JsonProperty(PropertyName = "items")]
        public List<ZoomAction> Actions { get; set; }
    }

    public class ZoomAction
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public string Style { get; set; }
    }

    public class FieldsItem : BodyItem
    {
        public string Type => "fields";

        [JsonProperty(PropertyName = "items")]
        public List<ZoomField> Fields { get; set; }
    }

    public class ZoomField
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public bool Editable { get; set; } = true;
        public Style Style { get; set; }
    }

    public class DropdownItem : BodyItem
    {
        public string Type => "select";

        public string Text { get; set; }

        [JsonProperty(PropertyName = "select_items")]
        public List<ZoomSelectItem> SelectItems { get; set; }

        [JsonProperty(PropertyName = "selected_item")]
        public ZoomSelectItem SelectedItem { get; set; }
    }

    public class ZoomSelectItem
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public Style Style { get; set; }
    }

    public class MessageItemWithLink : MessageItem
    {
        public string Link { get; set; }
    }

    public class MessageItemWithEditableText : MessageItem
    {
        public bool Editable { get; set; } = true;
    }

    public class AttachmentItem : BodyItem
    {
        public string Type => "attachments";

        [JsonProperty(PropertyName = "resource_url")]
        public Uri ResourceUrl { get; set; }

        [JsonProperty(PropertyName = "img_url")]
        public Uri ImageUrl { get; set; }

        public ZoomAttachmentInfo Information { get; set; }

        public FileExtensions Ext { get; set; }

        public int Size { get; set; }
    }

    public class ZoomAttachmentInfo
    {
        public ZoomAttachmentInfoContent Title { get; set; }

        public ZoomAttachmentInfoContent Description { get; set; }
    }

    public class ZoomAttachmentInfoContent
    {
        public string Text { get; set; }
        public Style Style { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum FileExtensions
    {
        pdf,
        txt,
        doc,
        xlsx,
        zip,
        jpeg,
        png
    }

    public class SectionsItem : BodyItem
    {
        public string Type => "section";

        [JsonProperty(PropertyName = "sidebar_color")]
        public Uri SidebarColor { get; set; }

        public List<BodyItem> Sections { get; set; } = new List<BodyItem>();

        public string Footer { get; set; }

        [JsonProperty(PropertyName = "footer_icon")]
        public Uri FooterIcon { get; set; }

        public string Ts { get; set; }
    }
}
