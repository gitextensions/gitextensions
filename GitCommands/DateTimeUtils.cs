using System;
using System.Text.RegularExpressions;

namespace GitCommands
{
    public static class DateTimeUtils
    {
        /// <summary>
        /// Midnight 1 January 1970.
        /// </summary>
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static bool TryParseUnixTime(string unixTime, out DateTime result)
        {
            if (long.TryParse(unixTime, out var seconds))
            {
                result = UnixEpoch.AddSeconds(seconds).ToLocalTime();
                return true;
            }

            result = default;
            return false;
        }

        public static DateTime ParseUnixTime(string unixTime)
        {
            return UnixEpoch.AddSeconds(long.Parse(unixTime)).ToLocalTime();
        }

        public static DateTime ParseUnixTime(string s, Capture capture) => ParseUnixTime(s, capture.Index, capture.Length);

        public static DateTime ParseUnixTime(string s, int index, int count)
        {
            long unixTime = 0;

            while (count-- > 0)
            {
                var c = s[index++];
                var i = c - '0';

                if (i < 0 || i > 9)
                {
                    throw new FormatException("Invalid character in unix time string.");
                }

                unixTime = (unixTime * 10) + i;
            }

            return UnixEpoch.AddTicks(unixTime * TimeSpan.TicksPerSecond).ToLocalTime();
        }
    }
}
