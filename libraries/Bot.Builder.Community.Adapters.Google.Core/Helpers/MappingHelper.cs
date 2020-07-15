using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Xml;
using System.Xml.Linq;
using Bot.Builder.Community.Adapters.Google.Core.Model.Response;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Google.Core.Helpers
{
    public static class MappingHelper
    {
        public static Activity MergeActivities(IList<Activity> activities)
        {
            var messageActivities = activities?.Where(a => a.Type == ActivityTypes.Message).ToList();

            if (messageActivities == null || messageActivities.Count == 0)
            {
                return null;
            }

            var activity = messageActivities.Last();
            var endWithPeriod = activity.Text?.TrimEnd().EndsWith(".") ?? false;

            if (messageActivities.Any(a => !String.IsNullOrEmpty(a.Speak)))
            {
                var speakText = String.Join("<break strength=\"strong\"/>", messageActivities
                    .Select(a => !String.IsNullOrEmpty(a.Speak) ? StripSpeakTag(a.Speak) : NormalizeActivityText(a.TextFormat, a.Text))
                    .Where(s => !String.IsNullOrEmpty(s))
                    .Select(s => s));

                activity.Speak = $"<speak>{speakText}</speak>";
            }

            activity.Text = String.Join(" ", messageActivities
                .Select(a => NormalizeActivityText(a.TextFormat, a.Text))
                .Where(s => !String.IsNullOrEmpty(s))
                .Select(s => s.TrimEnd(' ')));

            if (activity.Text.EndsWith(".") && !endWithPeriod)
            {
                activity.Text = activity.Text.TrimEnd('.');
            }

            activity.Attachments = messageActivities.Where(x => x.Attachments != null).SelectMany(x => x.Attachments).ToList();

            return activity;
        }

        public static string StripSpeakTag(string speakText)
        {
            try
            {
                var speakSsmlDoc = XDocument.Parse(speakText);
                
                if (speakSsmlDoc.Root != null && speakSsmlDoc.Root.Name.ToString().ToLowerInvariant() == "speak")
                {
                    using (var reader = speakSsmlDoc.Root.CreateReader())
                    {
                        reader.MoveToContent();
                        return reader.ReadInnerXml();
                    }
                }

                return speakText;
            }
            catch (XmlException)
            {
                return speakText;
            }
        }

        public static string NormalizeActivityText(string textFormat, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            // Default to markdown if it isn't specified.
            if (textFormat == null)
            {
                textFormat = TextFormatTypes.Markdown;
            }

            string plainText;
            if (textFormat.Equals(TextFormatTypes.Plain, StringComparison.Ordinal))
            {
                plainText = text;
            }
            else if (textFormat.Equals(TextFormatTypes.Markdown, StringComparison.Ordinal))
            {
                plainText = GoogleMarkdownToPlaintextRenderer.Render(text);
            }
            else // xml format or other unknown and unsupported format.
            {
                plainText = string.Empty;
            }

            if (!SecurityElement.IsValidText(plainText))
            {
                plainText = SecurityElement.Escape(plainText);
            }
            return plainText;
        }
    }
}
