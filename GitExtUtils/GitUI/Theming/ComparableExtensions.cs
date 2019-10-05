using System;
using System.Drawing;

namespace GitExtUtils.GitUI.Theming
{
    public static class ComparableExtensions
    {
        public static bool IsWithin<TVal>(this TVal value, TVal minInclusive, TVal maxExclusive)
            where TVal : IComparable<TVal> =>
            value.CompareTo(minInclusive) >= 0 && value.CompareTo(maxExclusive) < 0;

        public static TVal WithinRange<TVal>(this TVal value, TVal min, TVal max)
            where TVal : IComparable<TVal> =>
            value.AtLeast(min).AtMost(max);

        public static TVal AtLeast<TVal>(this TVal value, TVal min)
            where TVal : IComparable<TVal> =>
            value.CompareTo(min) < 0 ? min : value;

        public static TVal AtMost<TVal>(this TVal value, TVal max)
            where TVal : IComparable<TVal> =>
            value.CompareTo(max) > 0 ? max : value;

        public static Size MultiplyBy(this Size original, Size multiplier) =>
            new Size(
                original.Width * multiplier.Width,
                original.Height * multiplier.Height);

        public static double Modulo(this double v, int d)
        {
            v = v % d;
            if (v < 0)
            {
                v += d;
            }

            return v;
        }
    }
}
