using System;
using JetBrains.Annotations;

namespace GitCommands
{
    public static class ArrayExtensions
    {
        [MustUseReturnValue]
        public static T[] Subsequence<T>([NotNull] this T[] array, int index, int length)
        {
            var sub = new T[length];
            Array.Copy(array, index, sub, 0, length);
            return sub;
        }

        [MustUseReturnValue]
        public static T[] Append<T>([NotNull] this T[] array, T element)
        {
            var larger = new T[array.Length + 1];
            Array.Copy(array, larger, array.Length);
            larger[array.Length] = element;
            return larger;
        }
    }
}