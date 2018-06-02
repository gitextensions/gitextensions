using System;

namespace GitCommands
{
    public static class DateTimeUtils
    {
        /// <summary>
        /// Midnight 1 January 1970.
        /// </summary>
        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime ParseUnixTime(string unixTime)
        {
            return UnixEpoch.AddSeconds(long.Parse(unixTime)).ToLocalTime();
        }
    }
}
