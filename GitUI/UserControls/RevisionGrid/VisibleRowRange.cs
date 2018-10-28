using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GitUI.UserControls.RevisionGrid
{
    internal readonly struct VisibleRowRange : IEnumerable<int>, IEquatable<VisibleRowRange>
    {
        public int FromIndex { get; }
        public int Count { get; }

        public VisibleRowRange(int fromIndex, int count)
        {
            Debug.Assert(fromIndex >= 0, "fromIndex >= 0");
            Debug.Assert(count >= 0, "count >= 0");

            FromIndex = fromIndex;
            Count = count;
        }

        public bool Contains(int index) => index >= FromIndex && index < FromIndex + Count;

        public IEnumerator<int> GetEnumerator() => Enumerable.Range(FromIndex, Count).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public bool Equals(VisibleRowRange other) => FromIndex == other.FromIndex && Count == other.Count;
        public override bool Equals(object obj) => obj is VisibleRowRange range && Equals(range);
        public override int GetHashCode() => unchecked((FromIndex * 397) ^ Count);
        public override string ToString() => $"[{FromIndex}, {FromIndex + Count - 1}] {Count} row{(Count == 1 ? "" : "s")}";
        public static bool operator ==(VisibleRowRange left, VisibleRowRange right) => left.Equals(right);
        public static bool operator !=(VisibleRowRange left, VisibleRowRange right) => !left.Equals(right);
    }
}