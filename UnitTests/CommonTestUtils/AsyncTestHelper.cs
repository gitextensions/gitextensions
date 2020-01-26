using System;
using System.Diagnostics;

namespace CommonTestUtils
{
    public static class AsyncTestHelper
    {
        public static TimeSpan UnexpectedTimeout
        {
            get
            {
                return Debugger.IsAttached ? TimeSpan.FromHours(1) : TimeSpan.FromMinutes(1);
            }
        }
    }
}
