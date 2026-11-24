using System.Security.Cryptography;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Compat;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class RuntimeFontProbeTests
{
    [TestCase(null, true, null)]
    [TestCase("", true, null)]
    [TestCase(" ", true, null)]
    [TestCase("fonts.json", false, null)]
    [TestCase("fonts.json", true, "org.gitextensions.GitExtensions")]
    public void IsSupportedRequest_should_leave_ordinary_or_confined_startup_untouched(
        string? reportPath, bool isLinux, string? flatpakId)
    {
        RuntimeFontProbe.IsSupportedRequest(reportPath, isLinux, flatpakId).Should().BeFalse();
    }

    [TestCase(null)]
    [TestCase("")]
    public void IsSupportedRequest_should_accept_only_an_explicit_unconfined_Linux_report(string? flatpakId)
    {
        RuntimeFontProbe.IsSupportedRequest("fonts.json", isLinux: true, flatpakId).Should().BeTrue();
    }

    [TestCase(false, false, "Regular")]
    [TestCase(true, false, "Bold")]
    [TestCase(false, true, "Italic")]
    [TestCase(true, true, "Bold, Italic")]
    public void CaptureRequestedFont_should_report_actual_shim_style_flags(bool bold, bool italic, string expectedStyle)
    {
        WinFormsShims.FontStyle style = WinFormsShims.FontStyle.Regular;
        if (bold)
        {
            style |= WinFormsShims.FontStyle.Bold;
        }

        if (italic)
        {
            style |= WinFormsShims.FontStyle.Italic;
        }

        using WinFormsShims.Font font = new("configured-family", 11, style);
        RuntimeFontProbe.CaptureRequestedFont("ui", font).Should().Be(new RuntimeFontProbe.RequestedFont("ui", "configured-family", 11, expectedStyle));
    }

    [Test]
    public void WriteReportIfRequested_should_not_inspect_fonts_or_create_output_before_the_screenshot_request()
    {
        string temporaryDirectory = Path.Combine(Path.GetTempPath(), "GitExtensions.FontProbe." + Guid.NewGuid().ToString("N"));
        string reportPath = Path.Combine(temporaryDirectory, "fonts.json");
        bool invoked = false;

        RuntimeFontProbe.WriteReportIfRequested(reportPath, _ => invoked = true).Should().BeFalse();

        invoked.Should().BeFalse();
        Directory.Exists(temporaryDirectory).Should().BeFalse();
        File.Exists(reportPath).Should().BeFalse();
        File.Exists(reportPath + ".tmp").Should().BeFalse();
    }

    [Test]
    public void WriteReportIfRequested_should_publish_a_completed_report_after_the_retained_request()
    {
        string temporaryDirectory = Path.Combine(Path.GetTempPath(), "GitExtensions.FontProbe." + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryDirectory);
        string reportPath = Path.Combine(temporaryDirectory, "fonts.json");

        try
        {
            File.WriteAllText(reportPath + ".request", "postScreenshot");
            int calls = 0;
            RuntimeFontProbe.WriteReportIfRequested(reportPath, stream =>
            {
                calls++;
                File.Exists(reportPath).Should().BeFalse();
                JsonSerializer.Serialize(stream, new { phase = "postScreenshot", status = "captured" });
            }).Should().BeTrue();

            calls.Should().Be(1);
            using JsonDocument report = JsonDocument.Parse(File.ReadAllText(reportPath));
            report.RootElement.GetProperty("phase").GetString().Should().Be("postScreenshot");
            File.Exists(reportPath + ".request").Should().BeTrue();
            File.Exists(reportPath + ".tmp").Should().BeFalse();
        }
        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [AvaloniaTest]
    public void CreateReport_should_record_existing_glyph_families_before_resolving_additional_faces_without_changing_fonts()
    {
        FontFamily family = FontManager.Current.DefaultFontFamily;
        TextBlock[] blocks =
        [
            new() { Name = "regularSample", Text = "Regular sample", FontFamily = family, FontSize = 14 },
            new() { Name = "boldSample", Text = "Bold sample", FontFamily = family, FontSize = 14, FontWeight = FontWeight.Bold },
            new() { Name = "italicSample", Text = "Italic sample", FontFamily = family, FontSize = 14, FontStyle = FontStyle.Italic },
        ];
        StackPanel panel = new();
        foreach (TextBlock block in blocks)
        {
            panel.Children.Add(block);
        }

        Window window = new() { Width = 400, Height = 140, Content = panel };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        try
        {
            GlyphTypeface[] actualTypefaces = blocks.Select(block => block.TextLayout.TextLines
                .SelectMany(line => line.TextRuns).OfType<ShapedTextRun>().First().ShapedBuffer.GlyphTypeface).ToArray();
            string uiFamily = AppSettings.Font.Name;
            float uiSize = AppSettings.Font.Size;
            WinFormsShims.FontStyle uiStyle = AppSettings.Font.Style;
            string commitFamily = AppSettings.CommitFont.Name;
            float commitSize = AppSettings.CommitFont.Size;
            WinFormsShims.FontStyle commitStyle = AppSettings.CommitFont.Style;
            RuntimeFontProbe.FontReport report = RuntimeFontProbe.CreateReport(window);

            report.SchemaVersion.Should().Be(1);
            report.Phase.Should().Be("postScreenshot");
            report.DefaultFontFamily.Should().Be(family.Name);
            report.UiFont.Should().Be(RuntimeFontProbe.CaptureRequestedFont("ui", AppSettings.Font));
            report.CommitFont.Should().Be(RuntimeFontProbe.CaptureRequestedFont("commit", AppSettings.CommitFont));
            report.ResolvedFaces.Should().HaveCount(9);
            report.ResolvedFaces.Select(face => face.Role).Distinct().Should().BeEquivalentTo("default", "ui", "commit");
            report.Roles.Should().HaveCount(4).And.OnlyContain(role => role.Status == "unsupported" && role.UnsupportedReason != null);
            for (int index = 0; index < blocks.Length; index++)
            {
                RuntimeFontProbe.ActualText text = report.ActualText.Single(text => text.VisualPath.EndsWith("[" + blocks[index].Name + "]", StringComparison.Ordinal));
                text.Status.Should().Be("captured");
                text.Runs.Should().NotBeEmpty().And.OnlyContain(run => run.ResolvedFamily == actualTypefaces[index].FamilyName);
                foreach (RuntimeFontProbe.ActualRun run in text.Runs)
                {
                    run.FontData.GlyphCount.Should().BeGreaterThan(0);
                    run.FontData.Metrics.DesignEmHeight.Should().BeGreaterThan(0);
                    run.FontData.Metrics.Should().Be(actualTypefaces[index].Metrics);
                    run.FontData.Tables.Select(table => table.Tag).Should().BeEquivalentTo("head", "name", "OS/2");
                    run.FontData.Tables.Should().OnlyContain(table => table.Status == "captured"
                        ? table.ByteLength > 0 && table.Sha256 != null && table.Sha256.Length == 64 && table.UnsupportedReason == null
                        : table.Status == "unsupported" && table.Sha256 == null && table.UnsupportedReason != null);
                    foreach (RuntimeFontProbe.FontTable table in run.FontData.Tables)
                    {
                        bool available = actualTypefaces[index].PlatformTypeface.TryGetTable(OpenTypeTag.Parse(table.Tag), out ReadOnlyMemory<byte> actualTable);
                        table.Status.Should().Be(available ? "captured" : "unsupported");
                        if (available)
                        {
                            table.ByteLength.Should().Be(actualTable.Length);
                            table.Sha256.Should().Be(Convert.ToHexString(SHA256.HashData(actualTable.Span)).ToLowerInvariant());
                        }
                    }
                }

                blocks[index].FontFamily.Should().Be(family);
                blocks[index].FontSize.Should().Be(14);
            }

            blocks[1].FontWeight.Should().Be(FontWeight.Bold);
            blocks[2].FontStyle.Should().Be(FontStyle.Italic);
            AppSettings.Font.Name.Should().Be(uiFamily);
            AppSettings.Font.Size.Should().Be(uiSize);
            AppSettings.Font.Style.Should().Be(uiStyle);
            AppSettings.CommitFont.Name.Should().Be(commitFamily);
            AppSettings.CommitFont.Size.Should().Be(commitSize);
            AppSettings.CommitFont.Style.Should().Be(commitStyle);
            window.Content.Should().BeSameAs(panel);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void CreateReport_should_report_invalid_visible_layouts_as_unsupported_without_forcing_layout()
    {
        TextBlock block = new() { Name = "invalidSample", Text = "Existing text" };
        Window window = new() { Width = 300, Height = 100, Content = block };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        try
        {
            block.InvalidateMeasure();
            block.IsMeasureValid.Should().BeFalse();
            RuntimeFontProbe.FontReport report = RuntimeFontProbe.CreateReport(window);
            RuntimeFontProbe.ActualText text = report.ActualText.Single(text => text.VisualPath.EndsWith("[invalidSample]", StringComparison.Ordinal));

            text.Status.Should().Be("unsupported");
            text.UnsupportedReason.Should().Contain("does not force layout");
            text.Runs.Should().BeEmpty();
            block.IsMeasureValid.Should().BeFalse();
        }
        finally
        {
            window.Close();
        }
    }
}
