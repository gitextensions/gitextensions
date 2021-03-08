// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace CommonTestUtils
{
    public static class AssertEx
    {
        public static async Task<Exception?> ThrowsAsync(IResolveConstraint expression, AsyncTestDelegate code, string message, params object?[]? args)
        {
            Exception? caughtException = null;
            try
            {
                await code();
            }
            catch (Exception e)
            {
                caughtException = e;
            }

            Assert.That(caughtException, expression, message, args);
            return caughtException;
        }

        public static async Task<Exception?> ThrowsAsync(IResolveConstraint expression, AsyncTestDelegate code)
        {
            return await ThrowsAsync(expression, code, string.Empty, null);
        }

        public static async Task<Exception?> ThrowsAsync(Type expectedExceptionType, AsyncTestDelegate code, string message, params object?[]? args)
        {
            return await ThrowsAsync(new ExceptionTypeConstraint(expectedExceptionType), code, message, args);
        }

        public static async Task<Exception?> ThrowsAsync(Type expectedExceptionType, AsyncTestDelegate code)
        {
            return await ThrowsAsync(new ExceptionTypeConstraint(expectedExceptionType), code, string.Empty, null);
        }

        public static async Task<TActual?> ThrowsAsync<TActual>(AsyncTestDelegate code, string message, params object?[]? args)
            where TActual : Exception
        {
            return (TActual?)(await ThrowsAsync(typeof(TActual), code, message, args));
        }

        public static async Task<TActual?> ThrowsAsync<TActual>(AsyncTestDelegate code)
            where TActual : Exception
        {
            return await ThrowsAsync<TActual>(code, string.Empty, null);
        }

        public static void SequenceEqual<T>(
            IEnumerable<T> expected,
            IEnumerable<T> actual,
            IEqualityComparer<T>? comparer = null)
        {
            comparer ??= EqualityComparer<T>.Default;

            if (expected is ICollection c1 && actual is ICollection c2)
            {
                Assert.AreEqual(c1.Count, c2.Count, "Invalid collection count");
            }

            int index = 0;

            using var expectedEnumerator = expected.GetEnumerator();
            using var actualEnumerator = actual.GetEnumerator();

            while (true)
            {
                var expectedHasNext = expectedEnumerator.MoveNext();
                var actualHasNext = actualEnumerator.MoveNext();

                switch (expectedHasNext, actualHasNext)
                {
                    case (false, false):
                    {
                        // Both sequences end at the same point. We are finished.
                        return;
                    }

                    case (false, true):
                    {
                        throw new($"Expected sequence ended at index {index} while actual sequence has more items.");
                    }

                    case (true, false):
                    {
                        throw new($"Actual sequence ended at index {index} while expected sequence has more items.");
                    }

                    case (true, true):
                    {
                        if (!comparer.Equals(expectedEnumerator.Current, actualEnumerator.Current))
                        {
                            throw new($"Sequences differ at index {index}.\nExpect: {expectedEnumerator.Current}\nActual: {actualEnumerator.Current}");
                        }

                        break;
                    }
                }

                index++;
            }
        }
    }
}
