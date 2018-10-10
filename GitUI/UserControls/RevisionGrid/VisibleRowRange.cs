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
        public int ToIndex { get; }

        public VisibleRowRange(int fromIndex, int toIndex)
        {
            Debug.Assert(fromIndex <= toIndex, "fromIndex <= toIndex");
            Debug.Assert(fromIndex >= 0, "fromIndex >= 0");
            Debug.Assert(toIndex >= 0, "toIndex >= 0");

            FromIndex = fromIndex;
            ToIndex = toIndex;
        }

        public int Count => ToIndex - FromIndex + 1;

        public bool Contains(int index) => index >= FromIndex && index <= ToIndex;

        public IEnumerator<int> GetEnumerator() => Enumerable.Range(FromIndex, Count).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public bool Equals(VisibleRowRange other) => FromIndex == other.FromIndex && ToIndex == other.ToIndex;
        public override bool Equals(object obj) => obj is VisibleRowRange range && Equals(range);
        public override int GetHashCode() => unchecked((FromIndex * 397) ^ ToIndex);
        public override string ToString() => $"[{FromIndex},{ToIndex}] {Count} row{(Count == 1 ? "" : "s")}";
        public static bool operator ==(VisibleRowRange left, VisibleRowRange right) => left.Equals(right);
        public static bool operator !=(VisibleRowRange left, VisibleRowRange right) => !left.Equals(right);
    }
}