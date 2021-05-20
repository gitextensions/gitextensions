using System.Collections.Generic;
using System.Linq;
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
            return GetLinesStartingWith(document, ref beginIndex, new[] { prefixStr }, ref found);
        }

        public List<ISegment> GetLinesStartingWith(IDocument document, ref int beginIndex, string[] prefixStrs, ref bool found)
        {
            List<ISegment> result = new();

            while (beginIndex < document.TotalNumberOfLines)
            {
                var lineSegment = _segmentGetter.GetSegment(document, beginIndex);

                if (lineSegment.Length > 0
                    && DoesLineStartWith(document, lineSegment.Offset, prefixStrs))
                {
                    found = true;
                    result.Add(lineSegment);
                    beginIndex++;
                }
                else
                {
                    if (found)
                    {
                        break;
                    }

                    beginIndex++;
                }
            }

            return result;
        }

        public bool DoesLineStartWith(IDocument document, int lineOffset, string prefixStr)
        {
            if (prefixStr.Length == 1)
            {
                return document.GetCharAt(lineOffset) == prefixStr[0];
            }

            if (document.TextLength <= lineOffset + 1)
            {
                return false;
            }

            for (int i = 0; i < prefixStr.Length; i++)
            {
                if (document.GetCharAt(lineOffset + i) != prefixStr[i])
                {
                    return false;
                }
            }

            return true;
        }

        public bool DoesLineStartWith(IDocument document, int lineOffset, string[] prefixStrs)
        {
            return prefixStrs.Any(pre => DoesLineStartWith(document, lineOffset, pre));
        }
    }
}
