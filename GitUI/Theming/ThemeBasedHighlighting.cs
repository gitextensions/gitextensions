using System;
using System.Collections.Generic;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Theming
{
    public class ThemeBasedHighlighting : IHighlightingStrategyUsingRuleSets
    {
        private readonly IHighlightingStrategy _original;

        public ThemeBasedHighlighting(IHighlightingStrategy original) =>
            _original = original;

        public HighlightColor GetColorFor(string name) =>
            _original.GetColorFor(name)?.Transform();

        public string Name => _original.Name;

        public string[] Extensions => _original.Extensions;

        public Dictionary<string, string> Properties => _original.Properties;

        public void MarkTokens(IDocument document)
        {
            _original.MarkTokens(document);
            foreach (var line in document.LineSegmentCollection)
            {
                foreach (var word in line.Words)
                {
                    word.SyntaxColor = word.SyntaxColor?.Transform();
                }
            }
        }

        public void MarkTokens(IDocument document, List<LineSegment> lines)
        {
            _original.MarkTokens(document, lines);
            foreach (LineSegment line in lines)
            {
                foreach (var word in line.Words)
                {
                    word.SyntaxColor = word.SyntaxColor?.Transform();
                }
            }
        }

        public HighlightRuleSet GetRuleSet(Span span) =>
            (_original as IHighlightingStrategyUsingRuleSets ??
                throw new NotSupportedException())
            .GetRuleSet(span);

        public HighlightColor GetColor(IDocument document, LineSegment keyWord, int index, int length) =>
            (_original as IHighlightingStrategyUsingRuleSets ??
                throw new NotSupportedException())
            .GetColor(document, keyWord, index, length);
    }
}
