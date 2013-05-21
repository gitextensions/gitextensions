using System;

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
            long seconds;
            if (long.TryParse(unixTime, out seconds))
            {
                result = UnixEpoch.AddSeconds(seconds).ToLocalTime();
                return true;
            }

            result = default(DateTime);
            return false;
        }

        public static DateTime ParseUnixTime(string unixTime)
        {
            return UnixEpoch.AddSeconds(long.Parse(unixTime)).ToLocalTime();
        }
    }
}
