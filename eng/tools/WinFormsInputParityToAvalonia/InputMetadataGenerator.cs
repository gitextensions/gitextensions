using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace WinFormsInputParityToAvalonia;

// parity-scaffolding: Generates temporary WinForms-to-Avalonia input metadata for parity verification.
public static partial class InputMetadataGenerator
{
    private static readonly XNamespace XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

    public static string Generate(string winFormsRoot, string avaloniaRoot)
        => Generate([(winFormsRoot, avaloniaRoot)]);

    public static string Generate(IEnumerable<(string WinFormsRoot, string AvaloniaRoot)> sourceRoots)
    {
        List<ViewMetadata> views = [];
        foreach ((string winFormsRoot, string avaloniaRoot) in sourceRoots)
        {
            string fullWinFormsRoot = Path.GetFullPath(winFormsRoot);
            string fullAvaloniaRoot = Path.GetFullPath(avaloniaRoot);
            if (!Directory.Exists(fullWinFormsRoot) || !Directory.Exists(fullAvaloniaRoot))
            {
                throw new InvalidDataException("Every WinForms/Avalonia source-root pair must exist.");
            }

            foreach (string axamlPath in Directory.EnumerateFiles(fullAvaloniaRoot, "*.axaml", SearchOption.AllDirectories)
                         .Order(StringComparer.Ordinal))
            {
                string relativePath = Path.GetRelativePath(fullAvaloniaRoot, axamlPath);
                string designerPath = Path.Combine(
                    fullWinFormsRoot,
                    Path.ChangeExtension(relativePath, ".Designer.cs"));
                if (!File.Exists(designerPath))
                {
                    continue;
                }

                XDocument axaml = XDocument.Load(axamlPath);
                string? className = (string?)axaml.Root?.Attribute(XamlNamespace + "Class");
                if (className is null)
                {
                    // Abstract layout shells have matching Designer paths but no generated view class.
                    continue;
                }

                HashSet<string> controlNames = axaml.Root!.DescendantsAndSelf()
                    .Select(element => (string?)element.Attribute(XamlNamespace + "Name"))
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .ToHashSet(StringComparer.Ordinal)!;
                Dictionary<string, MutableControlMetadata> controls = controlNames.ToDictionary(
                    name => name,
                    _ => new MutableControlMetadata(),
                    StringComparer.Ordinal);

                foreach (string line in File.ReadLines(designerPath))
                {
                    Match match = AssignmentRegex().Match(line);
                    if (!match.Success || !controls.TryGetValue(match.Groups["field"].Value, out MutableControlMetadata? metadata))
                    {
                        continue;
                    }

                    string property = match.Groups["property"].Value;
                    string value = match.Groups["value"].Value.Trim();
                    switch (property)
                    {
                        case "TabIndex":
                            metadata.TabIndex = int.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "TabStop":
                            metadata.IsTabStop = bool.Parse(value);
                            break;
                        case "AccessibleName":
                            metadata.AccessibleName = UnescapeString(value);
                            break;
                        case "Anchor":
                            metadata.Anchor = ParseEnumFlags(value, "AnchorStyles.");
                            break;
                        case "Dock":
                            metadata.Dock = ParseEnum(value, "DockStyle.");
                            break;
                        case "AutoSize":
                            metadata.AutoSize = bool.Parse(value);
                            break;
                        case "Margin":
                            metadata.Margin = ParsePadding(value);
                            break;
                        case "Padding":
                            metadata.Padding = ParsePadding(value);
                            break;
                        case "TextAlign":
                            metadata.Alignment = ParseEnum(value, value.Contains("ContentAlignment.", StringComparison.Ordinal)
                                ? "ContentAlignment."
                                : "HorizontalAlignment.");
                            break;
                        case "BorderStyle":
                            metadata.BorderStyle = ParseEnum(value, "BorderStyle.");
                            break;
                        case "FlatStyle":
                            metadata.FlatStyle = ParseEnum(value, "FlatStyle.");
                            break;
                    }
                }

                ControlMetadata[] projected = controls
                    .Where(pair => pair.Value.HasValue)
                    .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                    .Select(pair => new ControlMetadata(
                        pair.Key,
                        pair.Value.TabIndex,
                        pair.Value.IsTabStop,
                        pair.Value.AccessibleName))
                    .ToArray();
                LayoutControlMetadata[] layout = controls
                    .Where(pair => pair.Value.HasLayoutValue)
                    .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                    .Select(pair => new LayoutControlMetadata(
                        pair.Key,
                        pair.Value.Anchor,
                        pair.Value.Dock,
                        pair.Value.AutoSize,
                        pair.Value.Margin,
                        pair.Value.Padding,
                        pair.Value.Alignment,
                        pair.Value.BorderStyle,
                        pair.Value.FlatStyle))
                    .ToArray();
                if (projected.Length > 0 || layout.Length > 0)
                {
                    views.Add(new ViewMetadata(className, projected, layout));
                }
            }
        }

        if (views.Count == 0)
        {
            throw new InvalidDataException("No matching AXAML and WinForms Designer input metadata was found.");
        }

        string[] duplicateClassNames = views
            .GroupBy(view => view.ClassName, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();
        if (duplicateClassNames.Length > 0)
        {
            throw new InvalidDataException($"Duplicate AXAML class metadata: {string.Join(", ", duplicateClassNames)}.");
        }

        StringBuilder builder = new();
        AppendLine("// <auto-generated />");
        AppendLine("// Generated by eng/tools/WinFormsInputParityToAvalonia from matching WinForms Designer files.");
        AppendLine();
        AppendLine("namespace GitUI.Compat;");
        AppendLine();
        AppendLine("// parity-scaffolding: Projects WinForms Designer input metadata until parity verification is removed.");
        AppendLine("internal static class WinFormsInputMetadata");
        AppendLine("{");
        AppendLine("    internal static IReadOnlyDictionary<string, IReadOnlyList<InputControlMetadata>> ByType { get; } =");
        AppendLine("        new Dictionary<string, IReadOnlyList<InputControlMetadata>>(StringComparer.Ordinal)");
        AppendLine("        {");
        foreach (ViewMetadata view in views.Where(view => view.Controls.Count > 0).OrderBy(view => view.ClassName, StringComparer.Ordinal))
        {
            AppendLine($"            [\"{EscapeString(view.ClassName)}\"] =");
            AppendLine("            [");
            foreach (ControlMetadata control in view.Controls)
            {
                string tabIndex = control.TabIndex?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "null";
                string isTabStop = control.IsTabStop?.ToString().ToLowerInvariant() ?? "null";
                string accessibleName = control.AccessibleName is null
                    ? "null"
                    : $"\"{EscapeString(control.AccessibleName)}\"";
                AppendLine($"                new(\"{EscapeString(control.FieldName)}\", {tabIndex}, {isTabStop}, {accessibleName}),");
            }

            AppendLine("            ],");
        }

        AppendLine("        };");
        AppendLine();
        AppendLine("    internal static IReadOnlyDictionary<string, IReadOnlyList<DesignerLayoutMetadata>> LayoutByType { get; } =");
        AppendLine("        new Dictionary<string, IReadOnlyList<DesignerLayoutMetadata>>(StringComparer.Ordinal)");
        AppendLine("        {");
        foreach (ViewMetadata view in views.Where(view => view.Layout.Count > 0).OrderBy(view => view.ClassName, StringComparer.Ordinal))
        {
            AppendLine($"            [\"{EscapeString(view.ClassName)}\"] =");
            AppendLine("            [");
            foreach (LayoutControlMetadata control in view.Layout)
            {
                string anchor = control.Anchor is null
                    ? "null"
                    : $"[{string.Join(", ", control.Anchor.Select(value => $"\"{EscapeString(value)}\""))}]";
                string dock = ToNullableString(control.Dock);
                string autoSize = control.AutoSize?.ToString().ToLowerInvariant() ?? "null";
                string margin = ToNullableThickness(control.Margin);
                string padding = ToNullableThickness(control.Padding);
                string alignment = ToNullableString(control.Alignment);
                string borderStyle = ToNullableString(control.BorderStyle);
                string flatStyle = ToNullableString(control.FlatStyle);
                AppendLine($"                new(\"{EscapeString(control.FieldName)}\", {anchor}, {dock}, {autoSize}, {margin}, {padding}, {alignment}, {borderStyle}, {flatStyle}),");
            }

            AppendLine("            ],");
        }

        AppendLine("        };");
        AppendLine("}");
        return builder.ToString();

        void AppendLine(string value = "")
        {
            builder.Append(value);
            builder.Append('\n');
        }
    }

    private static string UnescapeString(string value)
    {
        if (value.Length < 2 || value[0] != '"' || value[^1] != '"')
        {
            throw new InvalidDataException($"AccessibleName is not a supported string literal: {value}");
        }

        return Regex.Unescape(value[1..^1]);
    }

    private static string EscapeString(string value)
        => value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal);

    private static string ParseEnum(string value, string prefix)
    {
        if (!value.StartsWith(prefix, StringComparison.Ordinal))
        {
            throw new InvalidDataException($"Unsupported Designer enum value: {value}");
        }

        return value[prefix.Length..];
    }

    private static IReadOnlyList<string> ParseEnumFlags(string value, string prefix)
        => value.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => ParseEnum(part, prefix))
            .ToArray();

    private static ThicknessValue? ParsePadding(string value)
    {
        Match match = PaddingRegex().Match(value);
        if (!match.Success)
        {
            // TabControl.Padding is a Point controlling tab-header spacing, not Control.Padding.
            return null;
        }

        int[] values = match.Groups["value"].Captures
            .Select(capture => int.Parse(capture.Value, System.Globalization.CultureInfo.InvariantCulture))
            .ToArray();
        return values.Length switch
        {
            1 => new ThicknessValue(values[0], values[0], values[0], values[0]),
            4 => new ThicknessValue(values[0], values[1], values[2], values[3]),
            _ => throw new InvalidDataException($"Unsupported Designer Padding arity: {value}")
        };
    }

    private static string ToNullableString(string? value)
        => value is null ? "null" : $"\"{EscapeString(value)}\"";

    private static string ToNullableThickness(ThicknessValue? value)
        => value is null
            ? "null"
            : $"new Avalonia.Thickness({value.Left}, {value.Top}, {value.Right}, {value.Bottom})";

    [GeneratedRegex("^\\s*(?:this\\.)?(?<field>[A-Za-z_][A-Za-z0-9_]*)\\.(?<property>TabIndex|TabStop|AccessibleName|Anchor|Dock|AutoSize|Margin|Padding|TextAlign|BorderStyle|FlatStyle)\\s*=\\s*(?<value>.+);\\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex AssignmentRegex();

    [GeneratedRegex("^new Padding\\((?:(?<value>-?[0-9]+)\\s*,?\\s*)+\\)$", RegexOptions.CultureInvariant)]
    private static partial Regex PaddingRegex();

    private sealed class MutableControlMetadata
    {
        public int? TabIndex { get; set; }

        public bool? IsTabStop { get; set; }

        public string? AccessibleName { get; set; }

        public IReadOnlyList<string>? Anchor { get; set; }

        public string? Dock { get; set; }

        public bool? AutoSize { get; set; }

        public ThicknessValue? Margin { get; set; }

        public ThicknessValue? Padding { get; set; }

        public string? Alignment { get; set; }

        public string? BorderStyle { get; set; }

        public string? FlatStyle { get; set; }

        public bool HasValue => TabIndex is not null || IsTabStop is not null || AccessibleName is not null;

        public bool HasLayoutValue => Anchor is not null
            || Dock is not null
            || AutoSize is not null
            || Margin is not null
            || Padding is not null
            || Alignment is not null
            || BorderStyle is not null
            || FlatStyle is not null;
    }

    private sealed record ViewMetadata(
        string ClassName,
        IReadOnlyList<ControlMetadata> Controls,
        IReadOnlyList<LayoutControlMetadata> Layout);

    private sealed record ControlMetadata(string FieldName, int? TabIndex, bool? IsTabStop, string? AccessibleName);

    private sealed record LayoutControlMetadata(
        string FieldName,
        IReadOnlyList<string>? Anchor,
        string? Dock,
        bool? AutoSize,
        ThicknessValue? Margin,
        ThicknessValue? Padding,
        string? Alignment,
        string? BorderStyle,
        string? FlatStyle);

    private sealed record ThicknessValue(int Left, int Top, int Right, int Bottom);
}
