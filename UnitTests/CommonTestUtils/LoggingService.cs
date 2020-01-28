using System;
using System.Diagnostics;

namespace CommonTestUtils
{
    public static class LoggingService
    {
        public static void Log(string message, bool debugOnly = true)
        {
#if !DEBUG
            if (debugOnly)
            {
                return;
            }
#endif

            Console.WriteLine(message);
        }

        public static void Log(string message, Exception ex)
        {
            Log($"{message}: {ex.Demystify()}", debugOnly: false);
        }
    }
}
