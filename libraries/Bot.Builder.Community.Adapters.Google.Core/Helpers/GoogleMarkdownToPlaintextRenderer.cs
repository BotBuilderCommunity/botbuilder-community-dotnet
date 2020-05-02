using Microsoft.MarkedNet;

namespace Bot.Builder.Community.Adapters.Google.Core.Helpers
{
    /// <summary>
    /// Simple Markdown renderer to turn markdown into plain text for Google.
    /// </summary>
    public static class GoogleMarkdownToPlaintextRenderer
    {
        private static readonly Marked _marked = new Marked(new Options { EscapeHtml = false, Sanitize = false, Mangle = false, Renderer = new RemoveMarkupRenderer() });

        public static string Render(string source) => _marked.Parse(source);

        private class RemoveMarkupRenderer : MarkdownRenderer
        {
            private const string ListItemMarker = "$$ListItemMarker$$";

            public RemoveMarkupRenderer() : base() { }
            public RemoveMarkupRenderer(Options options) : base(options) { }

            public override string Blockquote(string quote) => string.Concat(quote, ". ");
            public override string Br() => ". ";
            public override string Code(string code, string lang, bool escaped) => code;
            public override string Codespan(string text) => text;
            public override string Del(string text) => text;
            public override string Em(string text) => text;
            public override string Heading(string text, int level, string raw) => string.Concat(text, ". ");
            public override string Hr() => ". ";
            public override string Html(string html) => string.Empty;
            public override string Image(string href, string title, string text) => title ?? text;
            public override string Link(string href, string title, string text)
            {
                if (title != null)
                {
                    return $"{title} {href}";
                }
                else if (text == href)
                {
                    // For standard links the title and href will be the same.
                    return href;
                }
                return $"{text} {href}";
            }
            public override string List(string body, bool ordered, int start)
            {
                if (ordered)
                {
                    for (int marker = start, markerIndex = body.IndexOf(ListItemMarker); markerIndex >= 0; markerIndex = body.IndexOf(ListItemMarker), ++marker)
                    {
                        body = body.Substring(0, markerIndex) + marker + ". " + body.Substring(markerIndex + ListItemMarker.Length);
                    }
                }
                else
                {
                    body = body.Replace(ListItemMarker, string.Empty);
                }
                return $"{body.Trim().TrimEnd(',')}. ";
            }
            public override string ListItem(string text) => $"{ListItemMarker}{text}, ";
            public override string Paragraph(string text) => string.Concat(text.Replace("\n", " ").TrimEnd('.'), ". ");
            public override string Strong(string text) => text;
            public override string Table(string header, string body) => string.Empty;
            public override string TableCell(string content, TableCellFlags flags) => string.Empty;
            public override string TableRow(string content) => string.Empty;
            public override string Text(string text) => text.TrimEnd('.');
            public override string Preprocess(string text) => text.Trim();
            public override string Postprocess(string text) => text.Trim();
        }
    }
}
