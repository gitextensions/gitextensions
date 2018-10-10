using System;
using System.Collections.Generic;

namespace GitUI.Editor.Diff
{
    public sealed class DiffLinesInfo
    {
        private readonly Dictionary<int, DiffLineInfo> _diffLines = new Dictionary<int, DiffLineInfo>();

        public IReadOnlyDictionary<int, DiffLineInfo> DiffLines => _diffLines;

        /// <summary>
        /// Gets the maximum line number from either left or right version.
        /// </summary>
        public int MaxLineNumber { get; private set; }

        public void Add(DiffLineInfo diffLine)
        {
            _diffLines.Add(diffLine.LineNumInDiff, diffLine);
            MaxLineNumber = Math.Max(MaxLineNumber,
                Math.Max(diffLine.LeftLineNumber, diffLine.RightLineNumber));
        }
    }
}