using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

// Use of the Microsoft namespace here is to match that of Assumes and Requires from the vs-validation library

namespace Microsoft
{
    internal static class Validates
    {
        // These methods work around the fact that the vs-validation library mentions, in
        // its exception message "Contact Microsoft Support", which does not apply in Git
        // Extensions.

        [DebuggerStepThrough]
        public static void NotNull<T>([ValidatedNotNull, NotNull] T? value)
            where T : class
        {
            if (value == null)
            {
                Fail("Value must not be null.");
            }
        }

        [DebuggerStepThrough]
        public static void Null<T>(T? value)
            where T : class
        {
            if (value != null)
            {
                Fail("Value must be null.");
            }
        }

        [DoesNotReturn]
        private static void Fail(string message) => throw new(message);
    }
}
