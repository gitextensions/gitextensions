namespace GitCommands;

public static class DateTimeUtils
{
    /// <summary>
    /// Parse unix time string
    /// </summary>
    /// <param name="unixTime">Unix time string</param>
    /// <returns>DateTime (local time)</returns>
    public static DateTime ParseUnixTime(string unixTime)
    {
        return ParseUnixTime(unixTime.AsSpan());
    }

    /// <summary>
    /// Parse unix time char span
    /// </summary>
    /// <param name="unixTime">Unix time char span</param>
    /// <returns>DateTime (local time)</returns>
    public static DateTime ParseUnixTime(ReadOnlySpan<char> unixTime)
    {
        return DateTime.UnixEpoch.AddSeconds(long.Parse(unixTime)).ToLocalTime();
    }

    /// <summary>
    /// Convert from DateTime to native Git time format (unix time)
    /// </summary>
    /// <param name="dateTime">DateTime</param>
    /// <returns>Unix time (seconds since 1970)</returns>
    public static long ToUnixTime(DateTime dateTime)
    {
        return (long)(dateTime.ToUniversalTime() - DateTime.UnixEpoch).TotalSeconds;
    }
}
