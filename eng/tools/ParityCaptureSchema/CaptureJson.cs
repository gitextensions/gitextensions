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
