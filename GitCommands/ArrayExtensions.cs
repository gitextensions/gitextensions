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
    }
}