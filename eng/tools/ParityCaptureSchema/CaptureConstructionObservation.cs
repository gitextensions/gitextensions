using System.Text.Json.Serialization;

namespace GitExtensions.ParityCapture;

// parity-scaffolding: Live measurement provenance belongs only to capture manifests, never deterministic trees.

/// <summary>
///  Describes whether live factory endpoints support a bounded sizing-input inference.
/// </summary>
public enum CaptureConstructionBracketAssessment
{
    /// <summary>
    ///  Matching endpoints support only a bounded inference about intervening sizing input.
    /// </summary>
    StableBracketInference,

    /// <summary>
    ///  Changed text, context, offset, or reversed wall time prevents that inference.
    /// </summary>
    Ambiguous
}

/// <summary>
///  Identifies the coordinate unit of an observed date-column width.
/// </summary>
public enum CaptureConstructionWidthUnit
{
    /// <summary>
    ///  The width is the native control's unnormalized physical-pixel observation.
    /// </summary>
    PhysicalPixel,

    /// <summary>
    ///  The width is the portable control's observed device-independent width.
    /// </summary>
    DeviceIndependentPixel
}

/// <summary>
///  Retains capture-consumer observations around root creation, never a private constructor clock.
/// </summary>
public sealed record CaptureConstructionObservation
{
    private const string RootFactoryBoundary = "rootFactory";

    /// <summary>
    ///  Creates an observation whose boundary includes the capture factory's post-constructor preparation.
    /// </summary>
    /// <param name="boundary">The fixed observation boundary retained in manifest JSON.</param>
    [JsonConstructor]
    public CaptureConstructionObservation(string boundary = RootFactoryBoundary)
    {
        if (boundary != RootFactoryBoundary)
        {
            throw new InvalidDataException("Construction observations must use the rootFactory boundary.");
        }
    }

    /// <summary>
    ///  Gets the observed boundary, which does not identify an intercepted constructor clock read.
    /// </summary>
    public string Boundary => RootFactoryBoundary;

    /// <summary>
    ///  Gets the live sample taken immediately before invoking the root factory.
    /// </summary>
    public required CaptureConstructionClockSample BeforeFactory { get; init; }

    /// <summary>
    ///  Gets the live sample taken immediately after the root factory attempt completes, including failure.
    /// </summary>
    public required CaptureConstructionClockSample AfterFactory { get; init; }

    /// <summary>
    ///  Gets an inference from the endpoints, not proof of the constructor's private clock value.
    /// </summary>
    public required CaptureConstructionBracketAssessment Assessment { get; init; }

    /// <summary>
    ///  Gets date-column measurements after the factory returns a root, or an empty list if creation failed.
    /// </summary>
    public IReadOnlyList<CaptureConstructionDateColumn> AfterFactoryDateColumns { get; init; } = [];

    /// <summary>
    ///  Gets date-column measurements immediately before image acquisition, or an empty list if not reached.
    /// </summary>
    public IReadOnlyList<CaptureConstructionDateColumn> BeforeImageDateColumns { get; init; } = [];

    /// <summary>
    ///  Creates and validates manifest-only observations without replacing any application input.
    /// </summary>
    /// <param name="beforeFactory">The endpoint preceding the root factory attempt.</param>
    /// <param name="afterFactory">The endpoint following completion of that attempt.</param>
    /// <param name="afterFactoryDateColumns">The observed columns if a root was returned, or an empty list after creation failure.</param>
    /// <param name="beforeImageDateColumns">The observed columns before image acquisition.</param>
    /// <returns>The validated observation and its endpoint assessment.</returns>
    public static CaptureConstructionObservation Create(
        CaptureConstructionClockSample beforeFactory,
        CaptureConstructionClockSample afterFactory,
        IReadOnlyList<CaptureConstructionDateColumn> afterFactoryDateColumns,
        IReadOnlyList<CaptureConstructionDateColumn> beforeImageDateColumns)
    {
        ValidateSample(beforeFactory);
        ValidateSample(afterFactory);
        CaptureConstructionObservation observation = new()
        {
            BeforeFactory = beforeFactory,
            AfterFactory = afterFactory,
            Assessment = Assess(beforeFactory, afterFactory),
            AfterFactoryDateColumns = afterFactoryDateColumns,
            BeforeImageDateColumns = beforeImageDateColumns,
        };
        observation.Validate();
        return observation;
    }

    /// <summary>
    ///  Rejects contradictory or invalid metadata at the manifest deserialization boundary.
    /// </summary>
    internal void Validate()
    {
        ValidateSample(BeforeFactory);
        ValidateSample(AfterFactory);
        if (Assessment != Assess(BeforeFactory, AfterFactory))
        {
            throw new InvalidDataException("The construction assessment must agree with its observed endpoints.");
        }

        ValidateColumns(AfterFactoryDateColumns);
        ValidateColumns(BeforeImageDateColumns);
    }

    private static CaptureConstructionBracketAssessment Assess(
        CaptureConstructionClockSample beforeFactory,
        CaptureConstructionClockSample afterFactory)
    {
        // Matching endpoints bound an inference only: they do not observe the source's
        // private clock read or rule out an intervening wall-clock adjustment.
        return beforeFactory.Context == afterFactory.Context
            && beforeFactory.GeneralDateTimeText == afterFactory.GeneralDateTimeText
            && beforeFactory.ObservedAtLocal.Offset == afterFactory.ObservedAtLocal.Offset
            && afterFactory.ObservedAtUtc.Ticks >= beforeFactory.ObservedAtUtc.Ticks
                ? CaptureConstructionBracketAssessment.StableBracketInference
                : CaptureConstructionBracketAssessment.Ambiguous;
    }

    private static void ValidateSample(CaptureConstructionClockSample sample)
    {
        if (sample is null || sample.Context is not CaptureConstructionClockContext context)
        {
            throw new InvalidDataException("Construction clock samples must retain their context.");
        }

        if (sample.ObservedAtUtc.Kind != DateTimeKind.Utc
            || sample.ObservedAtLocal.UtcDateTime.Ticks != sample.ObservedAtUtc.Ticks)
        {
            throw new InvalidDataException("Construction UTC and local observations must identify the same UTC instant.");
        }

        RequireText(sample.GeneralDateTimeText, nameof(sample.GeneralDateTimeText));
        RequireText(context.CultureName, nameof(context.CultureName), allowEmpty: true);
        RequireText(context.UICultureName, nameof(context.UICultureName), allowEmpty: true);
        RequireText(context.ShortDatePattern, nameof(context.ShortDatePattern));
        RequireText(context.LongTimePattern, nameof(context.LongTimePattern));
        RequireValue(context.DateSeparator, nameof(context.DateSeparator));
        RequireValue(context.TimeSeparator, nameof(context.TimeSeparator));
        RequireValue(context.AMDesignator, nameof(context.AMDesignator));
        RequireValue(context.PMDesignator, nameof(context.PMDesignator));
        RequireText(context.CalendarType, nameof(context.CalendarType));
        RequireText(context.TimeZoneId, nameof(context.TimeZoneId));
        if (context.ConfiguredFont is not CaptureConstructionFont font)
        {
            throw new InvalidDataException("Construction context must retain its configured font.");
        }

        RequireText(font.Family, nameof(font.Family));
        if (!double.IsFinite(font.SizePoints) || font.SizePoints <= 0)
        {
            throw new InvalidDataException("The configured font size must be finite and positive.");
        }
    }

    private static void ValidateColumns(IReadOnlyList<CaptureConstructionDateColumn> columns)
    {
        if (columns is null)
        {
            throw new InvalidDataException("Construction column observations must be a list, including when empty.");
        }

        foreach (CaptureConstructionDateColumn column in columns)
        {
            if (column is null)
            {
                throw new InvalidDataException("Construction column observations must not contain null entries.");
            }

            RequireText(column.Owner, nameof(column.Owner));
            RequireText(column.Provider, nameof(column.Provider));
            if (column.ColumnIndex < 0 || !double.IsFinite(column.Width) || column.Width < 0
                || !Enum.IsDefined(column.Unit) || !double.IsFinite(column.RenderScale) || column.RenderScale <= 0
                || column.Dpi is not { X: > 0, Y: > 0 })
            {
                throw new InvalidDataException("Construction column measurements must retain valid units, widths, scale, and DPI.");
            }
        }
    }

    private static void RequireText(string value, string name, bool allowEmpty = false)
    {
        if (value is null || (!allowEmpty && string.IsNullOrWhiteSpace(value))
            || (allowEmpty && value.Length > 0 && string.IsNullOrWhiteSpace(value)))
        {
            throw new InvalidDataException($"Construction observation {name} must retain valid text.");
        }
    }

    private static void RequireValue(string value, string name)
    {
        if (value is null)
        {
            throw new InvalidDataException($"Construction observation {name} must retain its observed string.");
        }
    }
}

/// <summary>
///  Retains one coherent live instant and the actual general-date text observed by the consumer.
/// </summary>
public sealed record CaptureConstructionClockSample
{
    /// <summary>
    ///  Gets the UTC form of the observed instant.
    /// </summary>
    public required DateTime ObservedAtUtc { get; init; }

    /// <summary>
    ///  Gets the same instant with the observed local UTC offset.
    /// </summary>
    public required DateTimeOffset ObservedAtLocal { get; init; }

    /// <summary>
    ///  Gets the consumer's local general-date formatting, not the constructor's private text.
    /// </summary>
    public required string GeneralDateTimeText { get; init; }

    /// <summary>
    ///  Gets the formatting and settings context observed at this endpoint.
    /// </summary>
    public required CaptureConstructionClockContext Context { get; init; }
}

/// <summary>
///  Records the culture, timezone, and configured font surrounding a live factory sample.
/// </summary>
public sealed record CaptureConstructionClockContext
{
    /// <summary>
    ///  Gets the formatting culture name, including the empty invariant-culture name.
    /// </summary>
    public required string CultureName { get; init; }

    /// <summary>
    ///  Gets the UI culture name, independently of the formatting culture.
    /// </summary>
    public required string UICultureName { get; init; }

    /// <summary>
    ///  Gets the date pattern used by general-date formatting.
    /// </summary>
    public required string ShortDatePattern { get; init; }

    /// <summary>
    ///  Gets the time pattern used by general-date formatting.
    /// </summary>
    public required string LongTimePattern { get; init; }

    /// <summary>
    ///  Gets the date separator, which may be empty in a custom format.
    /// </summary>
    public required string DateSeparator { get; init; }

    /// <summary>
    ///  Gets the time separator, which may be empty in a custom format.
    /// </summary>
    public required string TimeSeparator { get; init; }

    /// <summary>
    ///  Gets the culture's optional morning designator.
    /// </summary>
    public required string AMDesignator { get; init; }

    /// <summary>
    ///  Gets the culture's optional afternoon designator.
    /// </summary>
    public required string PMDesignator { get; init; }

    /// <summary>
    ///  Gets the actual formatting calendar type.
    /// </summary>
    public required string CalendarType { get; init; }

    /// <summary>
    ///  Gets the observed local timezone identifier without normalizing platform identifiers.
    /// </summary>
    public required string TimeZoneId { get; init; }

    /// <summary>
    ///  Gets the configured font used by the source sizing algorithm.
    /// </summary>
    public required CaptureConstructionFont ConfiguredFont { get; init; }

    /// <summary>
    ///  Gets whether the relative-date branch was configured at this endpoint.
    /// </summary>
    public required bool RelativeDate { get; init; }
}

/// <summary>
///  Records configured font inputs without referencing a UI framework or native font type.
/// </summary>
public sealed record CaptureConstructionFont
{
    /// <summary>
    ///  Gets the configured font family, not a claim about a resolved fallback face.
    /// </summary>
    public required string Family { get; init; }

    /// <summary>
    ///  Gets the configured font size in points.
    /// </summary>
    public required double SizePoints { get; init; }

    /// <summary>
    ///  Gets whether bold is configured.
    /// </summary>
    public required bool Bold { get; init; }

    /// <summary>
    ///  Gets whether italic is configured.
    /// </summary>
    public required bool Italic { get; init; }

    /// <summary>
    ///  Gets whether underline is configured.
    /// </summary>
    public required bool Underline { get; init; }

    /// <summary>
    ///  Gets whether strikeout is configured.
    /// </summary>
    public required bool Strikeout { get; init; }
}

/// <summary>
///  Records an actual date column at one capture phase without inferring its constructor's initial width.
/// </summary>
public sealed record CaptureConstructionDateColumn
{
    /// <summary>
    ///  Gets the owning revision-grid name, or the root marker when that grid is the root.
    /// </summary>
    public required string Owner { get; init; }

    /// <summary>
    ///  Gets the actual source-shaped column provider type name.
    /// </summary>
    public required string Provider { get; init; }

    /// <summary>
    ///  Gets the observed column index within its owner.
    /// </summary>
    public required int ColumnIndex { get; init; }

    /// <summary>
    ///  Gets the observed width in the explicitly recorded unit.
    /// </summary>
    public required double Width { get; init; }

    /// <summary>
    ///  Gets the coordinate unit of the observed width.
    /// </summary>
    public required CaptureConstructionWidthUnit Unit { get; init; }

    /// <summary>
    ///  Gets the ambient DPI reported at the observation phase.
    /// </summary>
    public required CaptureDpi Dpi { get; init; }

    /// <summary>
    ///  Gets the observed native or portable render scale without normalizing the width.
    /// </summary>
    public required double RenderScale { get; init; }
}
