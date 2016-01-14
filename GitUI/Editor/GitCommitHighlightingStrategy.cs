using System;
using System.Collections.Generic;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System.Drawing;

namespace GitUI.Editor
{
    class GitCommitHighlightingStrategy : IHighlightingStrategy
    {
        readonly DefaultHighlightingStrategy _defaultHighlightingStrategy = new DefaultHighlightingStrategy();
        readonly Dictionary<string, string> _properties = new Dictionary<string, string>();

        public string Name
        {
            get
            {
                return "GitCommit";
            }
        }

        public string[] Extensions
        {
            get
            {
                return new string[] { };
            }
        }

        public Dictionary<string, string> Properties
        {
            get { return _properties; }
        }

        public HighlightColor GetColorFor(string name)
        {
            return _defaultHighlightingStrategy.GetColorFor(name);
        }

        public void MarkTokens(IDocument document, List<LineSegment> lines)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                LineSegment line = lines[i];
                line.Words = new List<TextWord>();

                ColorLineRedGreen(document, line);

                // and tell the text editor to redraw the changed line
                document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, line.LineNumber));
            }
        }

        void ColorLineRedGreen(IDocument document, LineSegment line)
        {
            AddWord(document, line, 0, line.Length, IsComment(document, line));
        }

        static bool IsComment(IDocument document, LineSegment line)
        {
            for (int i = 0; i < line.Length; i++)
            {
                var c = document.GetCharAt(line.Offset + i);
                if (Char.IsWhiteSpace(c))
                    continue;

                if (c == '#')
                    return true;

                return false;
            }
            return false;
        }

        void AddWord(IDocument document, LineSegment line, int startOffset, int endOffset, bool isComment)
        {
            if (startOffset == endOffset)
                return;
            var color = new HighlightColor(isComment ? Color.DarkGreen : Color.Black, false, false);
            line.Words.Add(new TextWord(document, line, startOffset, endOffset - startOffset, color, false));
        }

        public void MarkTokens(IDocument document)
        {
            MarkTokens(document, new List<LineSegment>(document.LineSegmentCollection));
        }
    }
}
