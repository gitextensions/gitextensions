using System.Configuration;
using System.Drawing;
using System.Reflection;
using System.Text.Json;
using AwesomeAssertions;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;
using GitExtUtils.GitUI;
using GitUI;
using GitUI.UserControls.RevisionGrid;
using Microsoft.VisualStudio.Threading;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class RevisionGridFontMetricsTests
{
    private const string ProbeText = "By";
    private const string PortableSettingName = "IsPortable";
    private const string SettingsFileName = "GitExtensions.settings";
    private const BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

    [Test]
    [TestCaseSource(nameof(ConfiguredFonts))]
    public void RowTemplate_should_use_the_configured_family_style_and_Graphics_MeasureString_height(
        string family, float points, FontStyle style)
    {
        WithIsolatedSettings(settingsDirectory =>
        {
            using Font requested = new(family, points, style, GraphicsUnit.Point);
            requested.Name.Should().Be(family, "an unavailable family is not evidence for the requested font");
            requested.Style.Should().Be(style);
            AppSettings.Font = requested;
            WithOriginalGrid(grid =>
            {
                Font actual = GetNormalFont(grid);
                actual.Name.Should().Be(family);
                actual.Style.Should().Be(style);
                actual.SizeInPoints.Should().Be(points);
                RecordAndAssertMetrics(grid, actual, family, points, style, settingsDirectory, "constructor");
            });
        });
    }

    [Test]
    public void ApplySettings_should_remeasure_the_same_grid_without_using_its_ambient_control_font()
    {
        WithIsolatedSettings(settingsDirectory =>
        {
            using Font initial = new("Segoe UI", 9, FontStyle.Regular, GraphicsUnit.Point);
            AppSettings.Font = initial;
            WithOriginalGrid(grid =>
            {
                using Font ambient = new("Arial", 7, FontStyle.Bold, GraphicsUnit.Point);
                grid.Font = ambient;
                foreach ((string family, float points, FontStyle style) in new[]
                {
                    ("Arial", 11f, FontStyle.Italic),
                    ("Consolas", 18f, FontStyle.Bold),
                    ("Segoe UI", 22f, FontStyle.Bold | FontStyle.Italic),
                    ("Segoe UI", 9f, FontStyle.Regular),
                })
                {
                    using Font requested = new(family, points, style, GraphicsUnit.Point);
                    requested.Name.Should().Be(family);
                    AppSettings.Font = requested;
                    grid.ApplySettings();
                    Application.DoEvents();
                    Font actual = GetNormalFont(grid);
                    actual.Name.Should().Be(family);
                    actual.Style.Should().Be(style);
                    actual.SizeInPoints.Should().Be(points);
                    RecordAndAssertMetrics(grid, actual, family, points, style, settingsDirectory, "ApplySettings");
                }
            });
        });
    }

    private static IEnumerable<TestCaseData> ConfiguredFonts()
    {
        foreach (string family in new[] { "Segoe UI", "Arial", "Consolas" })
        {
            foreach (float points in new[] { 9f, 11f, 18f, 22f })
            {
                foreach (FontStyle style in new[]
                {
                    FontStyle.Regular,
                    FontStyle.Bold,
                    FontStyle.Italic,
                    FontStyle.Bold | FontStyle.Italic,
                })
                {
                    yield return new TestCaseData(family, points, style);
                }
            }
        }
    }

    private static Font GetNormalFont(RevisionDataGridView grid)
    {
        FieldInfo field = typeof(RevisionDataGridView).GetField("_normalFont", PrivateInstance)
            ?? throw new MissingFieldException(typeof(RevisionDataGridView).FullName, "_normalFont");
        return field.GetValue(grid) as Font
            ?? throw new InvalidDataException("The original grid has no initialized normal font.");
    }

    private static void RecordAndAssertMetrics(
        RevisionDataGridView grid,
        Font font,
        string requestedFamily,
        float requestedPoints,
        FontStyle requestedStyle,
        string settingsDirectory,
        string measurementRoute)
    {
        // This is the product's exact measurement boundary, not HFONT/TextRenderer or
        // an assumed ratio between a particular font's line spacing and design em.
        using Graphics graphics = Graphics.FromHwnd(grid.Handle);
        SizeF measured = graphics.MeasureString(ProbeText, font);
        Font boldFont = typeof(RevisionDataGridView).GetField("_boldFont", PrivateInstance)?.GetValue(grid) as Font
            ?? throw new MissingFieldException(typeof(RevisionDataGridView).FullName, "_boldFont");
        int sourceSpacing = DpiUtil.Scale(9);
        int expected = (int)measured.Height + sourceSpacing;
        int actualRowHeight = (int)(typeof(RevisionDataGridView).GetField("_rowHeight", PrivateInstance)?.GetValue(grid)
            ?? throw new MissingFieldException(typeof(RevisionDataGridView).FullName, "_rowHeight"));
        object report = new
        {
            sourceType = typeof(RevisionDataGridView).FullName,
            measurementRoute,
            dpiMode = "nativeMonitor",
            grid.DeviceDpi,
            graphicsDpiX = graphics.DpiX,
            graphicsDpiY = graphics.DpiY,
            dpiUtilDpiX = DpiUtil.DpiX,
            dpiUtilDpiY = DpiUtil.DpiY,
            requestedFamily,
            requestedPoints,
            requestedStyle = requestedStyle.ToString(),
            resolvedFamily = font.FontFamily.Name,
            font.Name,
            font.OriginalFontName,
            style = font.Style.ToString(),
            unit = font.Unit.ToString(),
            font.Size,
            font.SizeInPoints,
            boldFontFamily = boldFont.FontFamily.Name,
            boldFontStyle = boldFont.Style.ToString(),
            boldFontPoints = boldFont.SizeInPoints,
            fontEmHeight = font.FontFamily.GetEmHeight(font.Style),
            fontLineSpacing = font.FontFamily.GetLineSpacing(font.Style),
            fontCellAscent = font.FontFamily.GetCellAscent(font.Style),
            fontCellDescent = font.FontFamily.GetCellDescent(font.Style),
            fontHeightPixels = font.GetHeight(graphics),
            measureStringWidth = measured.Width,
            measureStringHeight = measured.Height,
            sourceTruncatedTextHeight = (int)measured.Height,
            sourceSpacing,
            expectedRowTemplateHeight = expected,
            rowTemplateHeight = grid.RowTemplate.Height,
            actualRowHeight,
            ambientControlFamily = grid.Font.Name,
            ambientControlPoints = grid.Font.SizeInPoints,
            ambientControlStyle = grid.Font.Style.ToString(),
            settingsDirectory,
            actualSettingsFilePath = AppSettings.SettingsContainer.SettingsCache.SettingsFilePath,
        };
        string directory = Path.Combine(AppContext.BaseDirectory, "RevisionGridFontProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string reportPath = Path.Combine(directory, "metrics.json");
        File.WriteAllText(reportPath, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        TestContext.Out.WriteLine($"original RevisionDataGridView font metrics: {reportPath}");
        TestContext.Out.WriteLine(JsonSerializer.Serialize(report));
        grid.DeviceDpi.Should().Be(96);
        graphics.DpiX.Should().Be(96);
        graphics.DpiY.Should().Be(96);
        DpiUtil.DpiX.Should().Be(96);
        DpiUtil.DpiY.Should().Be(96);
        sourceSpacing.Should().Be(9);
        grid.RowTemplate.Height.Should().Be(expected);
        actualRowHeight.Should().Be(expected);
        boldFont.FontFamily.Name.Should().Be(font.FontFamily.Name);
        boldFont.SizeInPoints.Should().Be(font.SizeInPoints);
        boldFont.Style.Should().Be(FontStyle.Bold, "the source replaces configured Italic rather than OR-ing it into its emphasis font");
        Path.GetFullPath(AppSettings.SettingsContainer.SettingsCache.SettingsFilePath).Should()
            .Be(Path.Combine(settingsDirectory, SettingsFileName));
    }

    private static void WithIsolatedSettings(Action<string> action)
    {
        string directory = Path.Combine(Path.GetTempPath(), "GitExtensions.RevisionGridFontProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string settingsPath = Path.Combine(directory, SettingsFileName);
        File.WriteAllText(settingsPath, "<?xml version=\"1.0\" encoding=\"utf-8\"?><dictionary />");
        InitializeAppSettingsWithoutRealConfiguration(directory);
        using GitExtSettingsCache cache = new(settingsPath, autoSave: false);
        DistributedSettings settings = new(lowerPriority: null, cache, SettingLevel.Unknown);
        AppSettings.UsingContainer(settings, () => action(directory));

        // Retain the isolated settings beside the reported directory until the test host
        // exits: a first AppSettings initialization owns a live FileSystemWatcher/cache.
        // The tranche cleanup removes these explicit temporary roots after preserving JSON.
        TestContext.Out.WriteLine($"isolated original settings retained: {directory}");
    }

    private static void InitializeAppSettingsWithoutRealConfiguration(string directory)
    {
        Type settingsType = typeof(AppSettings).Assembly.GetType("GitCommands.Properties.Settings", throwOnError: true)
            ?? throw new TypeLoadException("GitCommands.Properties.Settings");
        ApplicationSettingsBase configuration = settingsType.GetProperty("Default", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as ApplicationSettingsBase
            ?? throw new MissingMemberException(settingsType.FullName, "Default");
        _ = configuration[PortableSettingName];
        SettingsPropertyValue portable = configuration.PropertyValues[PortableSettingName]
            ?? throw new MissingMemberException(settingsType.FullName, PortableSettingName);
        object originalPortableValue = portable.PropertyValue;
        bool originalDirty = portable.IsDirty;
        FieldInfo frameworkPath = typeof(Application).GetField("s_executablePath", PrivateStatic)
            ?? throw new MissingFieldException(typeof(Application).FullName, "s_executablePath");
        object? originalFrameworkPath = frameworkPath.GetValue(null);
        string actualExecutablePath = Application.ExecutablePath;
        string isolatedExecutablePath = Path.Combine(directory, "GitExtensions.exe");
        try
        {
            // AppSettings' static constructor selects its initial cache before any public
            // isolation boundary is available. This test-only alias keeps that first read
            // and its migrations in the seeded temporary path, without editing product code.
            portable.PropertyValue = true;
            frameworkPath.SetValue(null, isolatedExecutablePath);
            _ = AppSettings.SettingsContainer;
            FieldInfo sourcePath = typeof(AppSettings).GetField("_applicationExecutablePath", PrivateStatic)
                ?? throw new MissingFieldException(typeof(AppSettings).FullName, "_applicationExecutablePath");
            if (Equals(sourcePath.GetValue(null), isolatedExecutablePath))
            {
                sourcePath.SetValue(null, actualExecutablePath);
            }
        }
        finally
        {
            frameworkPath.SetValue(null, originalFrameworkPath);
            portable.PropertyValue = originalPortableValue;
            portable.IsDirty = originalDirty;
        }
    }

    private static void WithOriginalGrid(Action<RevisionDataGridView> action)
    {
        FieldInfo managerField = typeof(ThreadHelper).GetField("_taskManager", PrivateStatic)
            ?? throw new MissingFieldException(typeof(ThreadHelper).FullName, "_taskManager");
        object? originalManager = managerField.GetValue(null);
        SynchronizationContext? originalSynchronizationContext = SynchronizationContext.Current;
        using Form host = new() { AutoScaleMode = AutoScaleMode.None, ClientSize = new Size(640, 200), ShowInTaskbar = false };
        using JoinableTaskContext context = new();
        PropertyInfo contextProperty = typeof(ThreadHelper).GetProperty(nameof(ThreadHelper.JoinableTaskContext), BindingFlags.Public | BindingFlags.Static)
            ?? throw new MissingMemberException(typeof(ThreadHelper).FullName, nameof(ThreadHelper.JoinableTaskContext));
        contextProperty.SetValue(null, context);
        try
        {
            using RevisionDataGridView grid = new() { Dock = DockStyle.Fill };
            host.Controls.Add(grid);
            host.Show();
            Application.DoEvents();
            action(grid);
        }
        finally
        {
            // Grid.Dispose cancels and joins its actual BackgroundUpdater/task-manager
            // lifecycle before the fixture releases its context; no capture-only loader stub.
            host.Close();
            managerField.SetValue(null, originalManager);
            SynchronizationContext.SetSynchronizationContext(originalSynchronizationContext);
        }
    }
}
