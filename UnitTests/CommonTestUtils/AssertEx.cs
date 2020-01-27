// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace CommonTestUtils
{
    public static class AssertEx
    {
        public static async Task<Exception> ThrowsAsync(IResolveConstraint expression, AsyncTestDelegate code, string message, params object[] args)
        {
            Exception caughtException = null;
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

        public static async Task<Exception> ThrowsAsync(IResolveConstraint expression, AsyncTestDelegate code)
        {
            return await ThrowsAsync(expression, code, string.Empty, null);
        }

        public static async Task<Exception> ThrowsAsync(Type expectedExceptionType, AsyncTestDelegate code, string message, params object[] args)
        {
            return await ThrowsAsync(new ExceptionTypeConstraint(expectedExceptionType), code, message, args);
        }

        public static async Task<Exception> ThrowsAsync(Type expectedExceptionType, AsyncTestDelegate code)
        {
            return await ThrowsAsync(new ExceptionTypeConstraint(expectedExceptionType), code, string.Empty, null);
        }

        public static async Task<TActual> ThrowsAsync<TActual>(AsyncTestDelegate code, string message, params object[] args)
            where TActual : Exception
        {
            return (TActual)(await ThrowsAsync(typeof(TActual), code, message, args));
        }

        public static async Task<TActual> ThrowsAsync<TActual>(AsyncTestDelegate code)
            where TActual : Exception
        {
            return await ThrowsAsync<TActual>(code, string.Empty, null);
        }
    }
}
