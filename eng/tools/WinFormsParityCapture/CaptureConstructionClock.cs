using System.Globalization;

namespace GitExtensions.ParityCapture;

// parity-scaffolding: Sampling the consumer boundary does not inject or intercept the application's clock.

/// <summary>
///  Samples capture-owned factory boundaries without substituting an application clock.
/// </summary>
internal static class CaptureConstructionClock
{
    /// <summary>
    ///  Takes one live instant and records its local formatting with the caller's configured font.
    /// </summary>
    /// <param name="font">The configured font observed by the capture consumer.</param>
    /// <param name="relativeDate">The consumer's observed relative-date setting.</param>
    /// <returns>A coherent UTC/local endpoint sample, not the source constructor's private clock read.</returns>
    internal static CaptureConstructionClockSample Sample(CaptureConstructionFont font, bool relativeDate)
        => Sample(DateTimeOffset.UtcNow, CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture, TimeZoneInfo.Local, font, relativeDate);

    /// <summary>
    ///  Formats a supplied observation instant without modifying the process or application clock.
    /// </summary>
    /// <param name="instant">The single instant observed at the consumer boundary.</param>
    /// <param name="culture">The formatting culture observed at that boundary.</param>
    /// <param name="uiCulture">The independently observed UI culture.</param>
    /// <param name="timeZone">The local timezone observed at that boundary.</param>
    /// <param name="font">The configured font observed at that boundary.</param>
    /// <param name="relativeDate">The observed relative-date setting.</param>
    /// <returns>The endpoint sample for deterministic consumer-contract tests or live capture.</returns>
    internal static CaptureConstructionClockSample Sample(
        DateTimeOffset instant,
        CultureInfo culture,
        CultureInfo uiCulture,
        TimeZoneInfo timeZone,
        CaptureConstructionFont font,
        bool relativeDate)
    {
        ArgumentNullException.ThrowIfNull(culture);
        ArgumentNullException.ThrowIfNull(uiCulture);
        ArgumentNullException.ThrowIfNull(timeZone);
        ArgumentNullException.ThrowIfNull(font);
        DateTimeOffset local = TimeZoneInfo.ConvertTime(instant, timeZone);
        DateTimeFormatInfo format = culture.DateTimeFormat;
        return new CaptureConstructionClockSample
        {
            ObservedAtUtc = instant.UtcDateTime,
            ObservedAtLocal = local,
            GeneralDateTimeText = local.DateTime.ToString("G", culture),
            Context = new CaptureConstructionClockContext
            {
                CultureName = culture.Name,
                UICultureName = uiCulture.Name,
                ShortDatePattern = format.ShortDatePattern,
                LongTimePattern = format.LongTimePattern,
                DateSeparator = format.DateSeparator,
                TimeSeparator = format.TimeSeparator,
                AMDesignator = format.AMDesignator,
                PMDesignator = format.PMDesignator,
                CalendarType = format.Calendar.GetType().FullName ?? format.Calendar.GetType().Name,
                TimeZoneId = timeZone.Id,
                ConfiguredFont = font,
                RelativeDate = relativeDate,
            },
        };
    }

    /// <summary>
    ///  Builds validated manifest-only observations while preserving endpoint and measurement values.
    /// </summary>
    /// <param name="beforeFactory">The endpoint preceding the root factory.</param>
    /// <param name="afterFactory">The endpoint following completion of the root factory attempt, including failure.</param>
    /// <param name="afterFactoryDateColumns">The actual date columns if a root was returned, or an empty list after creation failure.</param>
    /// <param name="beforeImageDateColumns">The actual date columns before image capture, or an empty list if not reached.</param>
    /// <returns>The validated factory bracket and its explicitly limited inference.</returns>
    internal static CaptureConstructionObservation Create(
        CaptureConstructionClockSample beforeFactory,
        CaptureConstructionClockSample afterFactory,
        IReadOnlyList<CaptureConstructionDateColumn> afterFactoryDateColumns,
        IReadOnlyList<CaptureConstructionDateColumn> beforeImageDateColumns)
        => CaptureConstructionObservation.Create(beforeFactory, afterFactory, afterFactoryDateColumns, beforeImageDateColumns);
}
