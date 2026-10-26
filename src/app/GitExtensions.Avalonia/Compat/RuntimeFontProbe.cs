using System.Security.Cryptography;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensions.Compat;

// parity-scaffolding: opt-in, unconfined Linux diagnostics; ordinary startup does nothing.
internal sealed class RuntimeFontProbe
{
    internal const string ReportPathEnvironmentVariable = "GITEXTENSIONS_RUNTIME_FONT_REPORT";

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    private static readonly string[] _requiredRoles = ["menu", "tree", "revisionGrid", "commitHeader"];
    private static readonly string[] _environmentNames =
    [
        "HOME", "XDG_CONFIG_HOME", "XDG_DATA_HOME", "XDG_CACHE_HOME", "XDG_RUNTIME_DIR",
        "FONTCONFIG_FILE", "FONTCONFIG_PATH", "LANG", "LC_ALL", "DISPLAY", "WAYLAND_DISPLAY", "FLATPAK_ID",
    ];

    private readonly string _reportPath;
    private readonly Window _mainWindow;
    private readonly DispatcherTimer _reportTimer;

    private RuntimeFontProbe(string reportPath, Window mainWindow)
    {
        _reportPath = reportPath;
        _mainWindow = mainWindow;
        _reportTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(100), DispatcherPriority.Background,
            (_, _) => WriteReport());
    }

    internal static bool IsSupportedRequest(string? reportPath, bool isLinux, string? flatpakId)
        => !string.IsNullOrWhiteSpace(reportPath) && isLinux && string.IsNullOrWhiteSpace(flatpakId);

    internal static void StartIfRequested(IClassicDesktopStyleApplicationLifetime desktop)
    {
        string? reportPath = Environment.GetEnvironmentVariable(ReportPathEnvironmentVariable);
        if (!IsSupportedRequest(reportPath, OperatingSystem.IsLinux(), Environment.GetEnvironmentVariable("FLATPAK_ID"))
            || desktop.MainWindow is not { } mainWindow)
        {
            return;
        }

        RuntimeFontProbe probe = new(Path.GetFullPath(reportPath ?? throw new InvalidOperationException("The font report path must be supplied.")), mainWindow);
        mainWindow.Closed += probe.MainWindow_Closed;
        probe._reportTimer.Start();
    }

    internal static bool WriteReportIfRequested(string reportPath, Action<Stream> writeReport)
    {
        // The smoke creates this request only after retaining its screenshot. Do not
        // resolve any fonts, create output, or inspect layouts before that handshake.
        if (!File.Exists(reportPath + ".request"))
        {
            return false;
        }

        string temporaryPath = reportPath + ".tmp";
        using (FileStream stream = File.Create(temporaryPath))
        {
            writeReport(stream);
        }

        File.Move(temporaryPath, reportPath, overwrite: true);
        return true;
    }

    internal static FontReport CreateReport(Window mainWindow)
    {
        // Snapshot existing measured glyphs first. Additional normal/bold/italic
        // queries can populate fallback caches, so they must follow the actual image.
        Dictionary<GlyphTypeface, FontData> fontData = [];
        ActualText[] actualText = mainWindow.GetVisualDescendants().OfType<TextBlock>()
            .Where(block => block.IsEffectivelyVisible)
            .Select(block => CaptureActualText(block, fontData))
            .ToArray();
        RoleCoverage[] roles = _requiredRoles.Select(role =>
        {
            int count = actualText.Count(text => text.Role == role && text.Status == "captured");
            return new RoleCoverage(role, count > 0 ? "captured" : "unsupported", count,
                count > 0 ? null : "No visible measured text run for this role at the post-screenshot request.");
        }).ToArray();

        FontManager fontManager = FontManager.Current;
        RequestedFont uiFont = CaptureRequestedFont("ui", AppSettings.Font);
        RequestedFont commitFont = CaptureRequestedFont("commit", AppSettings.CommitFont);
        RequestedFont defaultFont = new("default", fontManager.DefaultFontFamily.Name, 9, "Regular");
        ResolvedFace[] faces = new[] { defaultFont, uiFont, commitFont }
            .SelectMany(font => new[]
            {
                ResolveFace(fontManager, font, FontStyle.Normal, FontWeight.Normal, fontData),
                ResolveFace(fontManager, font, FontStyle.Normal, FontWeight.Bold, fontData),
                ResolveFace(fontManager, font, FontStyle.Italic, FontWeight.Normal, fontData),
            }).ToArray();
        Dictionary<string, string?> environment = _environmentNames.ToDictionary(name => name,
            Environment.GetEnvironmentVariable, StringComparer.Ordinal);

        return new FontReport(1, "captured", "postScreenshot", fontManager.DefaultFontFamily.Name,
            uiFont, commitFont, CaptureRequestedFont("systemDefault", WinFormsShims.SystemFonts.DefaultFont),
            CaptureRequestedFont("messageBox", WinFormsShims.SystemFonts.MessageBoxFont), environment,
            actualText, roles, faces);
    }

    internal static RequestedFont CaptureRequestedFont(string role, WinFormsShims.Font? font)
        => font is null ? new(role, null, null, null) : new(role, font.Name, font.Size,
            string.Join(", ", new[]
            {
                (WinFormsShims.FontStyle.Bold, "Bold"), (WinFormsShims.FontStyle.Italic, "Italic"),
                (WinFormsShims.FontStyle.Underline, "Underline"), (WinFormsShims.FontStyle.Strikeout, "Strikeout"),
            }.Where(flag => font.Style.HasFlag(flag.Item1)).Select(flag => flag.Item2).DefaultIfEmpty("Regular")));

    private static ActualText CaptureActualText(TextBlock block, Dictionary<GlyphTypeface, FontData> fontData)
    {
        string path = string.Join("/", block.GetVisualAncestors().OfType<Control>().Reverse()
            .Append(block).Select(control => string.IsNullOrEmpty(control.Name)
                ? control.GetType().Name
                : $"{control.GetType().Name}[{control.Name}]"));
        string role = GetRole(block);
        if (!block.IsMeasureValid)
        {
            return new ActualText(role, path, block.FontFamily.Name, block.FontSize, block.FontStyle.ToString(),
                block.FontWeight.ToString(), "unsupported", "The visible text layout is invalid; the diagnostic does not force layout.", []);
        }

        ActualRun[] runs = block.TextLayout.TextLines.SelectMany(line => line.TextRuns)
            .OfType<ShapedTextRun>().Select(run =>
            {
                GlyphTypeface glyphTypeface = run.ShapedBuffer.GlyphTypeface;
                return new ActualRun(run.Text.ToString(), run.Properties.Typeface.FontFamily.Name,
                    run.Properties.FontRenderingEmSize, glyphTypeface.FamilyName, glyphTypeface.Style.ToString(),
                    glyphTypeface.Weight.ToString(), glyphTypeface.FontSimulations.ToString(), CaptureFontData(glyphTypeface, fontData));
            }).ToArray();
        return new ActualText(role, path, block.FontFamily.Name, block.FontSize, block.FontStyle.ToString(),
            block.FontWeight.ToString(), runs.Length > 0 ? "captured" : "unsupported",
            runs.Length > 0 ? null : "The measured text layout contains no shaped glyph runs.", runs);
    }

    private static string GetRole(TextBlock block)
    {
        Control[] ancestors = block.GetVisualAncestors().OfType<Control>().ToArray();
        return ancestors.Any(control => control.GetType().Name == "CommitInfoHeader") ? "commitHeader"
            : ancestors.Any(control => control.GetType().Name == "RevisionGridControl") ? "revisionGrid"
            : ancestors.OfType<TreeView>().Any() ? "tree"
            : ancestors.OfType<Menu>().Any() ? "menu" : "uiText";
    }

    private static ResolvedFace ResolveFace(FontManager fontManager, RequestedFont font, FontStyle style, FontWeight weight,
        Dictionary<GlyphTypeface, FontData> fontData)
    {
        if (string.IsNullOrWhiteSpace(font.FamilyName))
        {
            return new ResolvedFace(font.Role, font.FamilyName, style.ToString(), weight.ToString(),
                "unsupported", "No configured family name is available.", null, null, null, null, null);
        }

        try
        {
            Typeface typeface = new(new FontFamily(font.FamilyName), style, weight);
            if (fontManager.TryGetGlyphTypeface(typeface, out GlyphTypeface? glyphTypeface) && glyphTypeface is not null)
            {
                return new ResolvedFace(font.Role, font.FamilyName, style.ToString(), weight.ToString(), "captured", null,
                    glyphTypeface.FamilyName, glyphTypeface.Style.ToString(), glyphTypeface.Weight.ToString(),
                    glyphTypeface.FontSimulations.ToString(), CaptureFontData(glyphTypeface, fontData));
            }

            return new ResolvedFace(font.Role, font.FamilyName, style.ToString(), weight.ToString(),
                "unsupported", "The platform font manager could not resolve the requested face.", null, null, null, null, null);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or NotSupportedException)
        {
            return new ResolvedFace(font.Role, font.FamilyName, style.ToString(), weight.ToString(),
                "unsupported", exception.Message, null, null, null, null, null);
        }
    }

    private static FontData CaptureFontData(GlyphTypeface glyphTypeface, Dictionary<GlyphTypeface, FontData> cache)
    {
        if (cache.TryGetValue(glyphTypeface, out FontData? data))
        {
            return data;
        }

        FontTable[] tables = new[] { "head", "name", "OS/2" }.Select(tag =>
            glyphTypeface.PlatformTypeface.TryGetTable(OpenTypeTag.Parse(tag), out ReadOnlyMemory<byte> table)
                ? new FontTable(tag, "captured", table.Length, Convert.ToHexString(SHA256.HashData(table.Span)).ToLowerInvariant(), null)
                : new FontTable(tag, "unsupported", 0, null, "The actual platform typeface does not expose this table.")).ToArray();
        data = new FontData(glyphTypeface.PlatformTypeface.GetType().FullName, glyphTypeface.PlatformTypeface.FamilyName,
            glyphTypeface.GlyphCount, glyphTypeface.Metrics, tables);
        cache.Add(glyphTypeface, data);
        return data;
    }

    private void WriteReport()
    {
        if (!File.Exists(_reportPath + ".request"))
        {
            return;
        }

        Stop();
        try
        {
            WriteReportIfRequested(_reportPath, stream => JsonSerializer.Serialize(stream, CreateReport(_mainWindow), _jsonOptions));
        }
        catch (Exception exception)
        {
            // Diagnostic failure must not create a product/JIT dialog. The smoke requires
            // the completed report and fails explicitly if it cannot be written.
            Console.Error.WriteLine($"Runtime font diagnostic failed: {exception}");
        }
    }

    private void MainWindow_Closed(object? sender, EventArgs e) => Stop();

    private void Stop()
    {
        _reportTimer.Stop();
        _mainWindow.Closed -= MainWindow_Closed;
    }

    internal sealed record FontReport(int SchemaVersion, string Status, string Phase, string DefaultFontFamily,
        RequestedFont UiFont, RequestedFont CommitFont, RequestedFont SystemDefaultFont, RequestedFont MessageBoxFont,
        IReadOnlyDictionary<string, string?> Environment, IReadOnlyList<ActualText> ActualText,
        IReadOnlyList<RoleCoverage> Roles, IReadOnlyList<ResolvedFace> ResolvedFaces);

    internal sealed record RequestedFont(string Role, string? FamilyName, float? SizeInPoints, string? Style);

    internal sealed record ActualText(string Role, string VisualPath, string RequestedFamily, double FontSizeDip,
        string RequestedStyle, string RequestedWeight, string Status, string? UnsupportedReason, IReadOnlyList<ActualRun> Runs);

    internal sealed record ActualRun(string Text, string RequestedFamily, double FontSizeDip, string ResolvedFamily,
        string ResolvedStyle, string ResolvedWeight, string FontSimulations, FontData FontData);

    internal sealed record RoleCoverage(string Role, string Status, int CapturedTextCount, string? UnsupportedReason);

    internal sealed record ResolvedFace(string Role, string? RequestedFamily, string RequestedStyle, string RequestedWeight,
        string Status, string? UnsupportedReason, string? ResolvedFamily, string? ResolvedStyle, string? ResolvedWeight,
        string? FontSimulations, FontData? FontData);

    internal sealed record FontData(string? PlatformTypefaceType, string PlatformFamily, int GlyphCount, FontMetrics Metrics,
        IReadOnlyList<FontTable> Tables);

    internal sealed record FontTable(string Tag, string Status, int ByteLength, string? Sha256, string? UnsupportedReason);
}
