namespace ResourceManager;

public static class LocalizationHelpers
{
    private static DateTime RoundDateTime(DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
    }

    /// <summary>
    /// Takes a date/time which and determines a friendly string for time from now to be displayed for the relative time from the date.
    /// It is important to note that times are compared using the current timezone, so the date that is passed in should be converted
    /// to the local timezone before passing it in.
    /// </summary>
    /// <param name="originDate">Current date.</param>
    /// <param name="previousDate">The date to get relative time string for.</param>
    /// <param name="displayWeeks">Indicates whether to display weeks.</param>
    /// <returns>The human readable string for relative date.</returns>
    /// <see href="http://stackoverflow.com/questions/11/how-do-i-calculate-relative-time"/>
    public static string GetRelativeDateString(DateTime originDate, DateTime previousDate, bool displayWeeks = true)
    {
        TimeSpan ts = new(RoundDateTime(originDate).Ticks - RoundDateTime(previousDate).Ticks);
        double delta = Math.Abs(ts.TotalSeconds);

        if (delta < 60)
        {
            return TranslatedStrings.GetNSecondsAgoText(ts.Seconds);
        }

        if (delta < 45 * 60)
        {
            return TranslatedStrings.GetNMinutesAgoText(ts.Minutes);
        }

        if (delta < 24 * 60 * 60)
        {
            int hours = delta < 60 * 60 ? Math.Sign(ts.Minutes) * 1 : ts.Hours;
            return TranslatedStrings.GetNHoursAgoText(hours);
        }

        // 30.417 = 365 days / 12 months - note that the if statement only bothers with 30 days for "1 month ago" because ts.Days is int
        if (delta < (displayWeeks ? 7 : 30) * 24 * 60 * 60)
        {
            return TranslatedStrings.GetNDaysAgoText(ts.Days);
        }

        if (displayWeeks && delta < 30 * 24 * 60 * 60)
        {
            int weeks = Convert.ToInt32(ts.Days / 7.0);
            return TranslatedStrings.GetNWeeksAgoText(weeks);
        }

        if (delta < 365 * 24 * 60 * 60)
        {
            int months = Convert.ToInt32(ts.Days / 30.0);
            return TranslatedStrings.GetNMonthsAgoText(months);
        }

        int years = Convert.ToInt32(ts.Days / 365.0);
        return TranslatedStrings.GetNYearsAgoText(years);
    }

    public static string GetFullDateString(DateTimeOffset datetime)
    {
        // previous format "ddd MMM dd HH':'mm':'ss yyyy"
        return datetime.LocalDateTime.ToString("G");
    }
}
