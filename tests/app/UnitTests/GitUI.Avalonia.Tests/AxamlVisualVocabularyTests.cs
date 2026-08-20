using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace GitExtensionsTests;

[TestFixture]
public sealed class AxamlVisualVocabularyTests
{
    private static readonly HashSet<string> VisualProperties =
    [
        "Background",
        "BorderBrush",
        "BorderThickness",
        "ColumnSpacing",
        "CornerRadius",
        "Fill",
        "FontSize",
        "Foreground",
        "Height",
        "Margin",
        "MaxHeight",
        "MaxWidth",
        "MinHeight",
        "MinWidth",
        "Padding",
        "RowSpacing",
        "Spacing",
        "Stroke",
        "Width",
    ];

    private static readonly HashSet<string> BrushProperties =
    [
        "Background",
        "BorderBrush",
        "Fill",
        "Foreground",
        "Stroke",
    ];

    private static readonly HashSet<string> LayoutElements =
    [
        "Border",
        "ColumnDefinition",
        "DistributedSettingsPage",
        "DockPanel",
        "GitExtensionsControl",
        "GitExtensionsForm",
        "GitExtensionsFormBase",
        "GitModuleForm",
        "Grid",
        "GridSplitter",
        "RowDefinition",
        "Separator",
        "SettingsPageWithHeader",
        "StackPanel",
        "UserControl",
        "Window",
        "WrapPanel",
    ];

    private static readonly HashSet<string> DimensionProperties =
    [
        "BorderThickness",
        "ColumnDefinitions",
        "ColumnSpacing",
        "CornerRadius",
        "FontSize",
        "Height",
        "Margin",
        "MaxHeight",
        "MaxWidth",
        "MinHeight",
        "MinWidth",
        "Padding",
        "RowDefinitions",
        "RowSpacing",
        "Spacing",
        "StrokeThickness",
        "Width",
    ];

    [Test]
    public void Views_should_consume_named_visual_values_or_designer_overrides()
    {
        (string viewRoot, string designerRoot) = GetSourceRoots();
        List<string> findings = [];
        XNamespace xaml = "http://schemas.microsoft.com/winfx/2006/xaml";

        foreach (string path in Directory.EnumerateFiles(viewRoot, "*.axaml", SearchOption.AllDirectories)
                     .Where(path => !Path.GetRelativePath(viewRoot, path).StartsWith("Styles" + Path.DirectorySeparatorChar, StringComparison.Ordinal)))
        {
            string relativePath = Path.GetRelativePath(viewRoot, path);
            bool isCompatibilityView = relativePath.StartsWith("Compat" + Path.DirectorySeparatorChar, StringComparison.Ordinal);
            string designerPath = Path.Combine(designerRoot, Path.ChangeExtension(relativePath, ".Designer.cs"));
            string? designerSource = File.Exists(designerPath) ? File.ReadAllText(designerPath) : null;
            XDocument document = XDocument.Load(path, LoadOptions.SetLineInfo);

            if (!isCompatibilityView && designerSource is null)
            {
                findings.Add($"{relativePath}: no WinForms Designer source exists for fixed visual overrides");
            }

            foreach (XElement element in document.Descendants())
            {
                if (element.Name.LocalName == "Setter")
                {
                    string? property = element.Attribute("Property")?.Value;
                    string? value = element.Attribute("Value")?.Value;
                    if (property is not null
                        && value is not null
                        && BrushProperties.Contains(property)
                        && IsLiteral(value))
                    {
                        findings.Add(FormatFinding(relativePath, element, $"{property} must reference a named brush"));
                    }

                    continue;
                }

                XAttribute? nameAttribute = element.Attribute(xaml + "Name");
                bool hasSemanticClass = !string.IsNullOrWhiteSpace(element.Attribute("Classes")?.Value);
                bool isLayout = LayoutElements.Contains(element.Name.LocalName);
                bool isRoot = ReferenceEquals(element, document.Root);

                foreach (XAttribute attribute in element.Attributes()
                             .Where(attribute => VisualProperties.Contains(attribute.Name.LocalName) && IsLiteral(attribute.Value)))
                {
                    string property = attribute.Name.LocalName;
                    if (BrushProperties.Contains(property))
                    {
                        findings.Add(FormatFinding(relativePath, element, $"{property} must reference a named brush"));
                        continue;
                    }

                    if (isCompatibilityView || isRoot || isLayout || hasSemanticClass)
                    {
                        continue;
                    }

                    string? fieldName = nameAttribute?.Value;
                    if (fieldName is null || designerSource is null)
                    {
                        findings.Add(FormatFinding(
                            relativePath,
                            element,
                            $"{property}=\"{attribute.Value}\" is neither a semantic class value nor a named Designer override"));
                    }
                }
            }
        }

        findings.Should().BeEmpty();
    }

    [Test]
    public void Repeated_view_shapes_should_use_the_shared_vocabulary()
    {
        (string viewRoot, _) = GetSourceRoots();
        List<string> findings = [];

        foreach (string path in Directory.EnumerateFiles(viewRoot, "*.axaml", SearchOption.AllDirectories)
                     .Where(path => !Path.GetRelativePath(viewRoot, path).StartsWith("Styles" + Path.DirectorySeparatorChar, StringComparison.Ordinal)))
        {
            string relativePath = Path.GetRelativePath(viewRoot, path);
            XDocument document = XDocument.Load(path, LoadOptions.SetLineInfo);
            foreach (XElement element in document.Descendants())
            {
                if (element.Name.LocalName == "Image"
                    && element.Attribute("Width")?.Value == "16"
                    && element.Attribute("Height")?.Value == "16")
                {
                    findings.Add(FormatFinding(relativePath, element, "16px icons must use gitextensions-icon-16"));
                }

                if (element.Name.LocalName == "TextBlock" && element.Attribute("Margin")?.Value == "6,3")
                {
                    findings.Add(FormatFinding(relativePath, element, "list header text must use gitextensions-list-header-text"));
                }

                if (element.Name.LocalName == "Border"
                    && element.Attribute("Padding")?.Value == "6,3"
                    && element.Attribute("BorderBrush")?.Value == "{DynamicResource GitExtensionsSectionBorderBrush}")
                {
                    findings.Add(FormatFinding(relativePath, element, "list header cells must use gitextensions-list-header-cell"));
                }

                if (element.Name.LocalName != "Style")
                {
                    continue;
                }

                string? selector = element.Attribute("Selector")?.Value;
                Dictionary<string, string> setters = element.Elements()
                    .Where(child => child.Name.LocalName == "Setter")
                    .ToDictionary(
                        child => child.Attribute("Property")!.Value,
                        child => child.Attribute("Value")!.Value,
                        StringComparer.Ordinal);
                if (selector == "CheckBox" && setters.GetValueOrDefault("Margin") == "0,-4")
                {
                    findings.Add(FormatFinding(relativePath, element, "compact settings checkboxes must use gitextensions-compact-checkboxes"));
                }

                if (selector == "ListBoxItem" && setters.GetValueOrDefault("HorizontalContentAlignment") == "Stretch")
                {
                    findings.Add(FormatFinding(relativePath, element, "stretch list rows must use the shared list classes"));
                }
            }
        }

        findings.Should().BeEmpty();
    }

    [Test]
    public void Layout_metrics_should_not_encode_125_percent_capture_pixels()
    {
        string repositoryRoot = GetRepositoryRoot();
        string appRoot = Path.Combine(repositoryRoot, "src", "app", "GitUI.Avalonia");
        string pluginRoot = Path.Combine(repositoryRoot, "src", "plugins");
        string gourceView = Path.Combine(pluginRoot, "Gource", "GourceStart.axaml");
        string gourceDesigner = Path.Combine(pluginRoot, "Gource", "GourceStart.Designer.cs");
        File.ReadAllText(gourceDesigner).Should().Contain("AutoScaleDimensions = new SizeF(120F, 120F)");

        List<string> findings = [];
        foreach (string path in Directory.EnumerateFiles(appRoot, "*.axaml", SearchOption.AllDirectories)
                     .Concat(Directory.EnumerateFiles(pluginRoot, "*.axaml", SearchOption.AllDirectories))
                     .Where(path => !string.Equals(path, gourceView, StringComparison.OrdinalIgnoreCase)))
        {
            XDocument document = XDocument.Load(path, LoadOptions.SetLineInfo);
            foreach (XAttribute attribute in document.Descendants()
                         .Attributes()
                         .Where(attribute => DimensionProperties.Contains(attribute.Name.LocalName)))
            {
                AddCaptureNormalizedNumbers(
                    findings,
                    Path.GetRelativePath(repositoryRoot, path),
                    ((IXmlLineInfo)attribute.Parent!).LineNumber,
                    attribute.Value);
            }
        }

        Regex codeMetric = new(
            @"(?:\b(?:Width|Height|MinWidth|MaxWidth|MinHeight|MaxHeight|FontSize|StrokeThickness)\s*=|new\s+(?:Thickness|GridLength)\s*\()[^;\r\n]*",
            RegexOptions.CultureInvariant);
        foreach (string path in Directory.EnumerateFiles(appRoot, "*.cs", SearchOption.AllDirectories))
        {
            int lineNumber = 0;
            foreach (string line in File.ReadLines(path))
            {
                lineNumber++;
                foreach (Match metric in codeMetric.Matches(line))
                {
                    AddCaptureNormalizedNumbers(
                        findings,
                        Path.GetRelativePath(repositoryRoot, path),
                        lineNumber,
                        metric.Value);
                }
            }
        }

        findings.Should().BeEmpty(
            "96-DPI WinForms dimensions map one-to-one to Avalonia DIPs; only a source Designer explicitly authored at another DPI may be normalized");
    }

    private static bool IsLiteral(string value) => !value.StartsWith('{');

    private static string FormatFinding(string relativePath, XElement element, string message)
    {
        IXmlLineInfo lineInfo = element;
        return $"{relativePath}:{lineInfo.LineNumber}: {message}";
    }

    private static (string ViewRoot, string DesignerRoot) GetSourceRoots([CallerFilePath] string thisFilePath = "")
    {
        string repositoryRoot = GetRepositoryRoot(thisFilePath);
        return (
            Path.Combine(repositoryRoot, "src", "app", "GitUI.Avalonia"),
            Path.Combine(repositoryRoot, "src", "app", "GitUI"));
    }

    private static string GetRepositoryRoot([CallerFilePath] string thisFilePath = "")
    {
        DirectoryInfo? directory = new(Path.GetDirectoryName(thisFilePath)!);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GitExtensions.Avalonia.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException($"Could not find the repository root from {thisFilePath}.");
    }

    private static void AddCaptureNormalizedNumbers(
        ICollection<string> findings,
        string relativePath,
        int lineNumber,
        string value)
    {
        foreach (Match match in Regex.Matches(value, @"\d+\.\d+", RegexOptions.CultureInvariant))
        {
            if (match.Index + match.Length < value.Length && value[match.Index + match.Length] == '*')
            {
                continue;
            }

            double number = double.Parse(match.Value, System.Globalization.CultureInfo.InvariantCulture);
            bool isFifthDipFraction = number != Math.Truncate(number)
                && Math.Abs((number * 5) - Math.Round(number * 5)) < 0.0000001;
            if (isFifthDipFraction)
            {
                findings.Add($"{relativePath}:{lineNumber}: {match.Value} looks like physical pixels divided by a 1.25 capture scale");
            }
        }
    }
}
