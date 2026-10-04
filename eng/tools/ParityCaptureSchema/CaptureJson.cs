using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace GitExtensions.ParityCapture;

/// <summary>
///  Provides the canonical JSON boundary shared by both capture frameworks.
/// </summary>
public static partial class CaptureJson
{
    private static readonly JsonSerializerOptions Options = CreateOptions();

    /// <summary>
    ///  Serializes a capture using deterministic property and enum formatting.
    /// </summary>
    public static string Serialize(CaptureDocument document)
    {
        Validate(document);
        if (document.Image.Acquisitions is { Count: 0 })
        {
            document = document with { Image = document.Image with { Acquisitions = null } };
        }

        return JsonSerializer.Serialize(document, Options) + Environment.NewLine;
    }

    /// <summary>
    ///  Deserializes and validates a capture.
    /// </summary>
    public static CaptureDocument Deserialize(string json)
    {
        CaptureDocument? document = JsonSerializer.Deserialize<CaptureDocument>(json, Options);
        if (document is null)
        {
            throw new InvalidDataException("The capture document is empty.");
        }

        Validate(document);
        return document;
    }

    /// <summary>
    ///  Validates schema invariants that both capture implementations must honor.
    /// </summary>
    public static void Validate(CaptureDocument document)
    {
        if (document.SchemaVersion != CaptureDocument.CurrentSchemaVersion)
        {
            throw new InvalidDataException($"Unsupported capture schema version {document.SchemaVersion}.");
        }

        if (document.Capture.StateStatus != CaptureStateStatus.Captured)
        {
            throw new InvalidDataException("A tree document may only describe a successfully captured state.");
        }

        if (document.Image.CaptureMethod == CaptureMethod.Unsupported)
        {
            throw new InvalidDataException("A captured tree must name the image API that produced it.");
        }

        if (document.Capture.ScalePercent is not (100 or 125 or 150 or 200))
        {
            throw new InvalidDataException($"Unsupported capture scale {document.Capture.ScalePercent}.");
        }

        if (document.Surfaces.Count == 0)
        {
            throw new InvalidDataException("A captured tree must contain at least one surface.");
        }

        ValidateAcquisitions(document);

        foreach (CaptureSurface surface in document.Surfaces)
        {
            ValidateNode(surface.Root);
        }
    }

    /// <summary>
    ///  Formats a resolved ARGB value without retaining its source color name.
    /// </summary>
    public static string FormatArgb(byte alpha, byte red, byte green, byte blue) =>
        $"#{alpha:X2}{red:X2}{green:X2}{blue:X2}";

    private static JsonSerializerOptions CreateOptions()
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }

    private static void ValidateAcquisitions(CaptureDocument document)
    {
        bool isComposite = document.Image.CaptureMethod == CaptureMethod.PrintWindowScreenGrabComposite;
        IReadOnlyList<CaptureImageAcquisition>? acquisitions = document.Image.Acquisitions;
        if (isComposite && acquisitions is not { Count: > 0 })
        {
            throw new InvalidDataException("A PrintWindow/screen composite must record its acquired regions.");
        }

        if (acquisitions is not { Count: > 0 })
        {
            return;
        }

        if (!isComposite)
        {
            throw new InvalidDataException("Supplemental screen acquisitions require the composite capture method.");
        }

        foreach (CaptureImageAcquisition acquisition in acquisitions)
        {
            CaptureSurface[] matchingSurfaces = document.Surfaces.Where(surface => surface.Role == acquisition.SurfaceRole).ToArray();
            CaptureSurface? surface = matchingSurfaces.Length == 1 ? matchingSurfaces[0] : null;
            CaptureRectangle region = acquisition.RegionPx;
            if (acquisition.SurfaceRole != "primary"
                || surface is null
                || acquisition.CaptureMethod != CaptureMethod.ScreenGrab
                || acquisition.Reason != "redrawDisabledNativeSurface"
                || region.X < 0 || region.Y < 0 || region.Width <= 0 || region.Height <= 0
                || (long)region.X + region.Width > surface.ScreenBoundsPx.Width
                || (long)region.Y + region.Height > surface.ScreenBoundsPx.Height)
            {
                throw new InvalidDataException("A supplemental acquisition must describe an in-bounds primary redraw-disabled native surface.");
            }
        }
    }

    private static void ValidateColors(CaptureColors colors)
    {
        IEnumerable<string?> fixedColors =
        [
            colors.Foreground,
            colors.Background,
            colors.Border,
            colors.SelectionForeground,
            colors.SelectionBackground,
            colors.InactiveSelectionForeground,
            colors.InactiveSelectionBackground,
            colors.DisabledForeground,
            colors.DisabledBackground,
            colors.GridLine
        ];

        foreach (string color in fixedColors.Concat(colors.Additional.Values).OfType<string>())
        {
            if (!ArgbRegex().IsMatch(color))
            {
                throw new InvalidDataException($"Resolved color '{color}' is not an uppercase #AARRGGBB value.");
            }
        }
    }

    private static void ValidateNode(CaptureNode node)
    {
        ValidateColors(node.Colors);
        foreach (CaptureColumn column in node.Columns)
        {
            ValidateColors(column.Colors);
        }

        foreach (CaptureNode child in node.Children)
        {
            ValidateNode(child);
        }
    }

    [GeneratedRegex("^#[0-9A-F]{8}$", RegexOptions.CultureInvariant)]
    private static partial Regex ArgbRegex();
}
