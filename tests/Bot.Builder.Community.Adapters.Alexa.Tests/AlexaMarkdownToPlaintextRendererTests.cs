using System;
using System.Collections.Generic;
using System.Text;
using Bot.Builder.Community.Adapters.Alexa.Core.Helpers;
using Xunit;

namespace Bot.Builder.Community.Adapters.Alexa.Tests
{
    public class AlexaMarkdownToPlaintextRendererTests
    {
        [Fact]
        public void ConvertTextWithTrailingPeriod()
        {
            var md = "Text text.";
            var result = AlexaMarkdownToPlaintextRenderer.Render(md);
            Assert.Equal(md, result);
        }

        [Fact]
        public void ConvertTextWithNoTrailingPeriod()
        {
            var md = "Text text";
            var result = AlexaMarkdownToPlaintextRenderer.Render(md);
            // Trailing period is added because it is a paragraph. Alexa TTS doesn't mind either way.
            Assert.Equal("Text text.", result);
        }

        [Fact]
        public void ConvertTextLeadingTrailingWhitespace()
        {
            var md = "         Text text            ";
            var result = AlexaMarkdownToPlaintextRenderer.Render(md);
            Assert.Equal("Text text.", result);
        }

        [Fact]
        public void ConvertTextTrailingNewline()
        {
            var md = "Text text\n";
            var result = AlexaMarkdownToPlaintextRenderer.Render(md);
            Assert.Equal("Text text.", result);
        }

        [Fact]
        public void ConvertTextNoTrailingNewline()
        {
            var md = "Text text";
            var result = AlexaMarkdownToPlaintextRenderer.Render(md);
            Assert.Equal("Text text.", result);
        }

        [Fact]
        public void ConvertTextBrAndParagraphs()
        {
            var md = "Same line.\nSame line.  \n2nd line.\n\r3rd line.";
            var result = AlexaMarkdownToPlaintextRenderer.Render(md);
            Assert.Equal("Same line. Same line. 2nd line. 3rd line.", result);
        }

        [Fact]
        public void ConvertTextBrAndParagraphsNoSpacesBetween()
        {
            var md = "Same line.\nSame line.\n2nd line.\n\r3rd line.";
            var result = AlexaMarkdownToPlaintextRenderer.Render(md);
            Assert.Equal("Same line. Same line. 2nd line. 3rd line.", result);
        }
    }
}
