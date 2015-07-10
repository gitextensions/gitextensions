using System.Collections.Generic;
using System.Diagnostics;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor.Diff
{
    public class LinePrefixHelper
    {
        private readonly LineSegmentGetter _segmentGetter;

        public LinePrefixHelper(LineSegmentGetter segmentGetter)
        {
            _segmentGetter = segmentGetter;
        }

        public List<ISegment> GetLinesStartingWith(IDocument document, ref int beginIndex, string prefixStr, ref bool found)
        {
            var result = new List<ISegment>();

            while (beginIndex < document.TotalNumberOfLines)
            {
                var lineSegment = _segmentGetter.GetSegment(document, beginIndex);

                if (lineSegment.Length > 0
                    && DoesLineStartWith(document, lineSegment.Offset, prefixStr))
                {
                    found = true;
                    result.Add(lineSegment);
                    beginIndex++;
                }
                else
                {
                    if (found)
                        break;
                    else
                        beginIndex++;
                }
            }

            return result;
        }

        public bool DoesLineStartWith(IDocument document, int lineOffset, string prefixStr)
        {
            Debug.Assert(prefixStr.Length <= 2 && prefixStr.Length >= 1);
            if (prefixStr.Length == 1) return document.GetCharAt(lineOffset) == prefixStr[0];

            if (document.TextLength <= lineOffset + 1)
            {
                return false;
            }

            var firstChar = document.GetCharAt(lineOffset);
            var secondChar = document.GetCharAt(lineOffset + 1);

            return firstChar == prefixStr[0] && secondChar == prefixStr[1];
        }
    }
}