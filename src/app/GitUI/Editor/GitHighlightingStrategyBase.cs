using GitExtensions.Extensibility.Git;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor
{
    internal abstract class GitHighlightingStrategyBase : IHighlightingStrategy
    {
        protected static HighlightColor ColorNormal { get; } = new(nameof(SystemColors.WindowText), bold: false, italic: false);

        private static HighlightColor ColorComment { get; } = new(Color.DarkGreen, bold: false, italic: false);

        private readonly DefaultHighlightingStrategy _defaultHighlightingStrategy = HighlightingManager.Manager.DefaultHighlighting;

        private readonly string? _commentString;

        protected GitHighlightingStrategyBase(
            string name,
            string? commentString = null)
        {
            Name = name;

            // This class used comments as is, and no decision was made about how to get it.
            // In addition, the latest version of Git could define a comment string.
            _commentString = commentString;
        }

        protected abstract void MarkTokens(IDocument document, IList<LineSegment> lines);

        protected bool TryHighlightComment(IDocument document, LineSegment line)
        {
            if (IsComment(document, line))
            {
                line.Words = new List<TextWord>(capacity: 1)
                    { new(document, line, 0, line.Length, ColorComment, hasDefaultColor: false) };
                return true;
            }

            return false;
        }

        #region IHighlightingStrategy

        public string Name { get; }

        public string[] Extensions => Array.Empty<string>();

        public Dictionary<string, string> Properties { get; } = [];

        public HighlightColor GetColorFor(string name)
        {
            return _defaultHighlightingStrategy.GetColorFor(name);
        }

        public void MarkTokens(IDocument document)
        {
            MarkTokens(document, document.LineSegmentCollection);
        }

        public void MarkTokens(IDocument document, List<LineSegment> lines)
        {
            MarkTokens(document, (IList<LineSegment>)lines);
        }

        #endregion
        #region Line classifiers

        protected bool IsComment(IDocument document, LineSegment line)
        {
            if (string.IsNullOrEmpty(_commentString) || line.Length == 0)
            {
                return false;
            }

            string lineText = document.GetText(line.Offset, line.Length).TrimStart();
            return lineText.StartsWith(_commentString);
        }

        protected static bool IsEmptyOrWhiteSpace(IDocument document, LineSegment line)
        {
            for (int i = 0; i < line.Length; i++)
            {
                char c = document.GetCharAt(line.Offset + i);

                if (!char.IsWhiteSpace(c))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
