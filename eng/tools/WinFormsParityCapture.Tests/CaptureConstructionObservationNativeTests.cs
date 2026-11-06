using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using AwesomeAssertions;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;
using GitExtensions.ParityCapture;
using GitExtUtils.GitUI;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

/// <summary>
///  Checks native construction inputs and column projection without creating controls, forms or HWNDs.
/// </summary>
[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class CaptureConstructionObservationNativeTests
{
    private const string ShortGeneralDateText = "10/22/2026 9:59:59 AM";
    private const string LongGeneralDateText = "10/22/2026 10:00:00 AM";
    private const string PortableSettingName = "IsPortable";
    private const BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;

    [TestCase("Segoe UI", 9f, FontStyle.Regular)]
    [TestCase("Consolas", 9f, FontStyle.Bold)]
    [TestCase("Arial", 11f, FontStyle.Italic)]
    public void Fixed_general_date_text_should_retain_the_source_TextRenderer_overload_widths_without_a_window(
        string family, float points, FontStyle style)
    {
        WithIsolatedSettings(() =>
        {
            using Font requested = new(family, points, style, GraphicsUnit.Point);
            requested.Name.Should().Be(family, "a substituted face is not the requested native font oracle");
            AppSettings.Font = requested;
            AppSettings.RelativeDate = false;
            Font configured = AppSettings.Font;
            DpiUtil.DpiX.Should().Be(96);
            DpiUtil.DpiY.Should().Be(96);
            int existingForms = Application.OpenForms.Count;
            Size shorter = TextRenderer.MeasureText(ShortGeneralDateText, configured);
            Size longer = TextRenderer.MeasureText(LongGeneralDateText, configured);
            shorter.Width.Should().BeGreaterThan(0);
            longer.Width.Should().BeGreaterThan(shorter.Width,
                "the unchanged source's default-padded two-argument overload sizes the actual general-date text");
            using DataGridViewTextBoxColumn column = new() { Width = longer.Width };
            column.Width.Should().Be(longer.Width);
            column.DataGridView.Should().BeNull("a standalone column must not construct a native grid or HWND");
            CaptureConstructionClockSample sample = NativeCaptureConstructionObservation.Sample();
            sample.Context.ConfiguredFont.Family.Should().Be(configured.Name);
            sample.Context.ConfiguredFont.SizePoints.Should().Be(configured.SizeInPoints);
            sample.Context.ConfiguredFont.Bold.Should().Be(configured.Bold);
            sample.Context.ConfiguredFont.Italic.Should().Be(configured.Italic);
            sample.Context.RelativeDate.Should().BeFalse();
            sample.ObservedAtLocal.UtcDateTime.Should().Be(sample.ObservedAtUtc);
            Application.OpenForms.Count.Should().Be(existingForms);
            RecordNativeOracle(new
            {
                test = TestContext.CurrentContext.Test.FullName,
                sourceOverload = "TextRenderer.MeasureText(string, Font)",
                configuredFont = sample.Context.ConfiguredFont,
                dpi = new { X = DpiUtil.DpiX, Y = DpiUtil.DpiY },
                shorter = new { text = ShortGeneralDateText, shorter.Width, shorter.Height },
                longer = new { text = LongGeneralDateText, longer.Width, longer.Height },
                standaloneColumnWidth = column.Width,
                scope = "Actual native fixed-string measurement only; no product constructor, private clock interception, native control, HWND or desktop capture.",
            });
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Column_projection_should_use_exact_provider_tag_identity_not_the_column_name(bool projectDateProvider)
    {
        using DataGridViewTextBoxColumn unrelated = new()
        {
            Name = "Date",
            Width = 109,
            Tag = new TestProviderTag("Another.Provider"),
        };
        using DataGridViewTextBoxColumn chosen = new()
        {
            Name = "NotTheDateColumnName",
            Width = 143,
            Tag = new TestProviderTag(projectDateProvider ? NativeCaptureConstructionObservation.DateColumnProviderName : "DateColumnProvider"),
        };
        IReadOnlyList<CaptureConstructionDateColumn> columns = NativeCaptureConstructionObservation.ProjectDateColumns(
            "RevisionGrid", [unrelated, chosen], 96, column => (column.Tag as TestProviderTag)?.ProviderName);
        columns.Should().HaveCount(projectDateProvider ? 1 : 0);
        if (projectDateProvider)
        {
            CaptureConstructionDateColumn observed = columns.Single();
            observed.Owner.Should().Be("RevisionGrid");
            observed.Provider.Should().Be(NativeCaptureConstructionObservation.DateColumnProviderName);
            observed.ColumnIndex.Should().Be(chosen.Index, "the projection retains the actual unattached-column index, not an invented grid index");
            observed.Width.Should().Be(chosen.Width);
            observed.Unit.Should().Be(CaptureConstructionWidthUnit.PhysicalPixel);
            observed.Dpi.X.Should().Be(96);
            observed.Dpi.Y.Should().Be(96);
            observed.RenderScale.Should().Be(1);
        }

        unrelated.DataGridView.Should().BeNull();
        chosen.DataGridView.Should().BeNull();
        TestContext.Out.WriteLine("Metadata projection seam only: test-owned provider tags and standalone columns; no product provider or grid construction.");
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Native_sample_should_observe_the_actual_relative_date_branch_without_substituting_a_clock(bool relativeDate)
    {
        WithIsolatedSettings(() =>
        {
            using Font requested = new("Segoe UI", 9, FontStyle.Underline | FontStyle.Strikeout);
            AppSettings.Font = requested;
            AppSettings.RelativeDate = relativeDate;
            using Font configured = AppSettings.Font;
            configured.Underline.Should().BeFalse("the original settings font format retains only bold and italic");
            configured.Strikeout.Should().BeFalse("the original settings font format retains only bold and italic");
            CaptureConstructionClockSample sample = NativeCaptureConstructionObservation.Sample();
            sample.Context.RelativeDate.Should().Be(relativeDate);
            sample.Context.ConfiguredFont.Underline.Should().Be(configured.Underline);
            sample.Context.ConfiguredFont.Strikeout.Should().Be(configured.Strikeout);
            sample.Context.CultureName.Should().Be(CultureInfo.CurrentCulture.Name);
            sample.Context.UICultureName.Should().Be(CultureInfo.CurrentUICulture.Name);
            sample.GeneralDateTimeText.Should().Be(sample.ObservedAtLocal.DateTime.ToString("G", CultureInfo.CurrentCulture));
            sample.ObservedAtUtc.Kind.Should().Be(DateTimeKind.Utc);
            TestContext.Out.WriteLine(JsonSerializer.Serialize(sample));
        });
    }

    private static void WithIsolatedSettings(Action action)
    {
        string directory = Path.Combine(Path.GetTempPath(), "GitExtensions.ConstructionObservationNative", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string settingsPath = Path.Combine(directory, "GitExtensions.settings");
        File.WriteAllText(settingsPath, "<?xml version=\"1.0\" encoding=\"utf-8\"?><dictionary />");
        InitializeAppSettingsWithoutRealConfiguration(directory);
        using GitExtSettingsCache cache = new(settingsPath, autoSave: false);
        DistributedSettings settings = new(lowerPriority: null, cache, SettingLevel.Unknown);
        AppSettings.UsingContainer(settings, action);

        // A first AppSettings initialization retains its cache watcher until host exit.
        TestContext.Out.WriteLine($"isolated original settings retained: {directory}");
    }

    private static void RecordNativeOracle(object observation)
    {
        string json = JsonSerializer.Serialize(observation, new JsonSerializerOptions { WriteIndented = true });
        TestContext.Out.WriteLine(json);
        string? evidence = Environment.GetEnvironmentVariable("GITEXT_CONSTRUCTION_CLOCK_EVIDENCE");
        if (string.IsNullOrWhiteSpace(evidence))
        {
            return;
        }

        string id = TestContext.CurrentContext.Test.ID;
        if (id.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new InvalidDataException("The actual NUnit case ID cannot own an evidence filename.");
        }

        Directory.CreateDirectory(evidence);
        string path = Path.Combine(evidence, $"native-measurement-{id}.json");
        using FileStream stream = File.Open(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json + Environment.NewLine);
        stream.Write(bytes);
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

    private sealed record TestProviderTag(string ProviderName);
}
