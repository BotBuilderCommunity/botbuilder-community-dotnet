using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Xml;
using System.Xml.Linq;
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

            if (messageActivities.Any(a => !String.IsNullOrEmpty(a.Speak)))
            {
                var speakText = String.Join("<break strength=\"strong\"/>", messageActivities
                    .Select(a => !String.IsNullOrEmpty(a.Speak) ? StripSpeakTag(a.Speak) : NormalizeActivityText(a.TextFormat, a.Text))
                    .Where(s => !String.IsNullOrEmpty(s))
                    .Select(s => s));

                activity.Speak = $"<speak>{speakText}</speak>";
            }

            activity.Text = String.Join(". ", messageActivities
                .Select(a => NormalizeActivityText(a.TextFormat, a.Text))
                .Where(s => !String.IsNullOrEmpty(s))
                .Select(s => s.Trim(new char[] { ' ', '.' })));

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
            if (String.IsNullOrWhiteSpace(text))
            {
                return String.Empty;
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
                plainText = String.Empty;
            }

            if (!SecurityElement.IsValidText(plainText))
            {
                plainText = SecurityElement.Escape(plainText);
            }
            return plainText;
        }

        public static List<Suggestion> ConvertSuggestedActivitiesToSuggestionChips(SuggestedActions suggestedActions)
        {
            var suggestionChips = new List<Suggestion>();

            if (suggestedActions != null && suggestedActions.Actions != null && suggestedActions.Actions.Any())
            {
                foreach (var suggestion in suggestedActions.Actions)
                {
                    suggestionChips.Add(new Suggestion { Title = suggestion.Title });
                }
            }

            return suggestionChips;
        }
        public static string StripInvocation(string query, string invocationName)
        {
            if (query != null && (query.ToLower().StartsWith("talk to") || query.ToLower().StartsWith("speak to")
                                                                        || query.ToLower().StartsWith("i want to speak to") ||
                                                                        query.ToLower().StartsWith("ask")))
            {
                query = query.ToLower().Replace($"talk to", string.Empty);
                query = query.ToLower().Replace($"speak to", string.Empty);
                query = query.ToLower().Replace($"I want to speak to", string.Empty);
                query = query.ToLower().Replace($"ask", string.Empty);
            }

            query = query?.TrimStart().TrimEnd();

            if (!string.IsNullOrEmpty(invocationName)
                && query.ToLower().StartsWith(invocationName.ToLower()))
            {
                query = query.ToLower().Replace(invocationName.ToLower(), string.Empty);
            }

            return query?.TrimStart().TrimEnd();
        }

    }
}
