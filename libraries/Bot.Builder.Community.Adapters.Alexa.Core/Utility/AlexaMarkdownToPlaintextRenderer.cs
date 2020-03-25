using System;
using Microsoft.MarkedNet;

namespace Bot.Builder.Community.Adapters.Alexa.Core.Utility
{
    /// <summary>
    /// Simple Markdown renderer to turn markdown into plain text for Alexa.
    /// </summary>
    public static class AlexaMarkdownToPlaintextRenderer
    {
        private static readonly Marked _marked = new Marked(new Options { Renderer = new RemoveMarkupRenderer() });

        public static string Render(string source) => _marked.Parse(source);

        private class RemoveMarkupRenderer : MarkdownRenderer
        {
            private const string ListItemMarker = "$$ListItemMarker$$";

            public override string Blockquote(string quote) => string.Concat(Environment.NewLine, quote, Environment.NewLine);
            public override string Br() => Environment.NewLine;
            public override string Code(string code, string lang, bool escaped) => code;
            public override string Codespan(string text) => text;
            public override string Del(string text) => text;
            public override string Em(string text) => text;
            public override string Heading(string text, int level, string raw) => string.Concat(Environment.NewLine, text, Environment.NewLine);
            public override string Hr() => Environment.NewLine;
            public override string Html(string html) => string.Empty;
            public override string Image(string href, string title, string text) => title ?? text;
            public override string Link(string href, string title, string text) => $"{title ?? text} {href}";
            public override string List(string body, bool ordered, int start)
            {
                if (ordered)
                {
                    for (int marker = start, markerIndex = body.IndexOf(ListItemMarker); markerIndex >= 0; markerIndex = body.IndexOf(ListItemMarker), ++marker)
                    {
                        body = body.Substring(0, markerIndex) + Environment.NewLine + marker + " " + body.Substring(markerIndex + ListItemMarker.Length);
                    }
                }
                else
                {
                    body = body.Replace(ListItemMarker, Environment.NewLine);
                }
                return body;
            }
            public override string ListItem(string text) => $"{ListItemMarker}{text}";
            public override string Paragraph(string text) => string.Concat(Environment.NewLine, text, Environment.NewLine);
            public override string Strong(string text) => text;
            public override string Table(string header, string body) => string.Empty;
            public override string TableCell(string content, TableCellFlags flags) => string.Empty;
            public override string TableRow(string content) => string.Empty;
        }
    }
}
