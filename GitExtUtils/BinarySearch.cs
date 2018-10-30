using System;

namespace GitCommands
{
    public static class BinarySearch
    {
        /// <summary>
        /// Find first integer between min and (min + count - 1) where "predicate" returns true.
        /// </summary>
        /// <remarks>
        /// Assumes "predicate" is a non-decreasing function, i.e.
        /// if predicate(i) == true then predicate(i + n) == true for any positive n.
        /// </remarks>
        public static int Find(int min, int count, Func<int, bool> predicate)
        {
            if (count == 0)
            {
                return -1;
            }

            if (predicate(min))
            {
                return min;
            }

            if (count == 1)
            {
                return -1;
            }

            int halfCount = count / 2;
            int middle = min + halfCount;
            int searchRightHalfResult = Find(middle, count - halfCount, predicate);

            if (searchRightHalfResult > middle)
            {
                return searchRightHalfResult;
            }

            if (searchRightHalfResult == -1)
            {
                return -1;
            }

            // searchRightHalfResult == middle

            int newLeft = min + 1;
            int searchLeftResult = Find(newLeft, middle - newLeft, predicate);

            if (searchLeftResult == -1)
            {
                return middle;
            }

            return searchLeftResult;
        }
    }
}