using System;

namespace GitCommands
{
    public static class DateTimeUtils
    {
        /// <summary>
        /// Midnight 1 January 1970.
        /// </summary>
        private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Parse unix time string
        /// </summary>
        /// <param name="unixTime">Unix time string</param>
        /// <returns>DateTime (local time)</returns>
        public static DateTime ParseUnixTime(string unixTime)
        {
            return UnixEpoch.AddSeconds(long.Parse(unixTime)).ToLocalTime();
        }

        /// <summary>
        /// Convert from DateTime to native Git time format (unix time)
        /// </summary>
        /// <param name="dateTime">DateTime</param>
        /// <returns>Unix time (seconds since 1970)</returns>
        public static long ToUnixTime(DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - UnixEpoch).TotalSeconds;
        }
    }
}
