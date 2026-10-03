using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using GitCommands;
using GitUI;
using GitUI.CommitInfo;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class CommitInfoBodyFontTests
{
    private const int SourceCommitMessageMinimumHeight = 1;
    private const string PlainParagraphs = "first short line\nsecond line with descenders gjpq\nlast line";
    private const string WrappedParagraph = "first short line second line with descenders gjpq last line repeated ordinary words for genuine RichEdit word wrapping";

    [SetUp]
    public void SetUp()
        => ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

    [AvaloniaTest]
    [TestCase("Segoe UI", 9, 15)]
    [TestCase("Segoe UI", 11, 20)]
    [TestCase("Segoe UI", 18, 32)]
    [TestCase("Segoe UI", 22, 40)]
    [TestCase("Consolas", 9, 14)]
    [TestCase("Consolas", 11, 18)]
    [TestCase("Consolas", 18, 28)]
    [TestCase("Consolas", 22, 34)]
    public void CommitInfo_body_and_refs_should_measure_configured_font_and_final_paragraph(string family, int points, int nativeLineHeight)
    {
        WinFormsShims.Font previousUiFont = AppSettings.Font;
        WinFormsShims.Font previousCommitFont = AppSettings.CommitFont;
        Window? window = null;
        try
        {
            // The real CommitInfo controls keep AXAML dynamic font resources. Publish
            // configured settings before attachment rather than overwriting a pending
            // resource binding with a direct property value that attachment replaces.
            AppSettings.Font = new WinFormsShims.Font(family, points);
            AppSettings.CommitFont = new WinFormsShims.Font(family, points);
            AvaloniaFontSettings.ApplyAppSettings();
            CommitInfo control = new();
            CommitInfo.TestAccessor accessor = control.GetTestAccessor();
            accessor.TableLayout.IsVisible = true;
            XhtmlTextBlock[] blocks = [accessor.CommitMessage, accessor.RevisionInfo];
            foreach (XhtmlTextBlock block in blocks)
            {
                block.SetXHTMLText(PlainParagraphs);
            }

            window = new Window { Width = 1200, Height = 900, Content = control };
            window.Show();
            RunLayout(window);
            foreach (XhtmlTextBlock block in blocks)
            {
                block.FontFamily.Name.Should().Be(family);
                block.FontSize.Should().Be(AvaloniaFontSettings.ToDeviceIndependentPixels(points));
                block.FontStyle.Should().Be(FontStyle.Normal);
                block.FontWeight.Should().Be(FontWeight.Normal);
                AssertSourceFontHeight(block, nativeLineHeight);
                GetLayout(block).TextLines.Should().HaveCount(3);
                AssertAutomaticContents(block, ReferenceEquals(block, accessor.CommitMessage) ? SourceCommitMessageMinimumHeight : 0);
                double ordinaryHeight = block.Bounds.Height;
                block.SetXHTMLText(PlainParagraphs + "\n");
                RunLayout(window);
                GetLayout(block).TextLines.Should().HaveCount(4,
                    "the source native contents rectangle includes the empty paragraph after a trailing newline");
                AssertAutomaticContents(block, ReferenceEquals(block, accessor.CommitMessage) ? SourceCommitMessageMinimumHeight : 0);
                block.Bounds.Height.Should().Be(ordinaryHeight + block.LineHeight);
                block.Margin.Top.Should().Be(8);
                block.Margin.Bottom.Should().Be(8);
            }

            accessor.TableLayout.RowDefinitions[1].ActualHeight.Should()
                .Be(accessor.CommitMessageHeight + accessor.CommitMessage.Margin.Top + accessor.CommitMessage.Margin.Bottom);
            accessor.TableLayout.RowDefinitions[2].ActualHeight.Should()
                .Be(accessor.RevisionInfo.Bounds.Height + accessor.RevisionInfo.Margin.Top + accessor.RevisionInfo.Margin.Bottom);

            foreach (XhtmlTextBlock block in blocks)
            {
                block.Clear();
                RunLayout(window);
                block.GetPlainText().Should().BeEmpty();
                int anchoredMinimum = ReferenceEquals(block, accessor.CommitMessage) ? SourceCommitMessageMinimumHeight : 0;
                block.Bounds.Height.Should().Be(block.LineHeight + anchoredMinimum,
                    "an empty source RichEdit still retains one native-font paragraph");
                block.MinHeight.Should().Be(block.LineHeight);
                double.IsNaN(block.Height).Should().BeTrue();
            }
        }
        finally
        {
            window?.Close();
            AppSettings.Font = previousUiFont;
            AppSettings.CommitFont = previousCommitFont;
            AvaloniaFontSettings.ApplyAppSettings();
        }
    }

    [AvaloniaTest]
    [TestCase("Segoe UI", 9, 15)]
    [TestCase("Segoe UI", 11, 20)]
    [TestCase("Segoe UI", 18, 32)]
    [TestCase("Segoe UI", 22, 40)]
    [TestCase("Consolas", 9, 14)]
    [TestCase("Consolas", 11, 18)]
    [TestCase("Consolas", 18, 28)]
    [TestCase("Consolas", 22, 34)]
    public void Wrapped_body_should_measure_actual_visual_lines_after_width_font_and_empty_content_changes(string family, int points, int nativeLineHeight)
    {
        XhtmlTextBlock block = new() { TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Top };
        ConfigureFont(block, family, points);
        block.UseNativeContentsHeightMeasurement();
        block.SetXHTMLText(WrappedParagraph);
        Window window = new() { Width = 440, Height = 1200, Content = block };
        window.Show();
        try
        {
            RunLayout(window);
            AssertSourceFontHeight(block, nativeLineHeight);
            AssertAutomaticContents(block);
            int wideLines = GetLayout(block).TextLines.Count;
            double wideHeight = block.Bounds.Height;
            wideLines.Should().BeGreaterThan(1);
            block.GetPlainText().Should().Be(WrappedParagraph);
            block.GetPlainText().Should().NotContain("\n");
            double.IsNaN(block.Width).Should().BeTrue("wrapped bodies must not acquire the header's preferred native Width");
            block.MinWidth.Should().Be(0);

            window.Width = 160;
            RunLayout(window);
            GetLayout(block).TextLines.Count.Should().BeGreaterThan(wideLines);
            block.Bounds.Height.Should().BeGreaterThan(wideHeight);
            AssertAutomaticContents(block);

            window.Width = 440;
            RunLayout(window);
            GetLayout(block).TextLines.Should().HaveCount(wideLines);
            block.Bounds.Height.Should().Be(wideHeight);
            block.SetXHTMLText(WrappedParagraph + "\n");
            RunLayout(window);
            GetLayout(block).TextLines.Should().HaveCount(wideLines + 1);
            block.Bounds.Height.Should().Be(wideHeight + block.LineHeight);
            AssertAutomaticContents(block);

            block.SetXHTMLText(WrappedParagraph);
            double originalLineHeight = block.LineHeight;
            ConfigureFont(block, family, points * 2);
            RunLayout(window);
            block.LineHeight.Should().BeGreaterThan(originalLineHeight);
            block.Bounds.Height.Should().BeGreaterThan(wideHeight);
            AssertAutomaticContents(block);
            ConfigureFont(block, family, points);
            RunLayout(window);
            block.LineHeight.Should().Be(originalLineHeight);
            block.Bounds.Height.Should().Be(wideHeight);

            block.Clear();
            RunLayout(window);
            block.Bounds.Height.Should().Be(block.LineHeight);
            ConfigureFont(block, family, points * 2);
            RunLayout(window);
            block.LineHeight.Should().BeGreaterThan(originalLineHeight);
            block.Bounds.Height.Should().Be(block.LineHeight);
            ConfigureFont(block, family, points);
            RunLayout(window);
            block.Bounds.Height.Should().Be(originalLineHeight);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void CommitInfo_body_font_should_follow_CommitFont_and_refs_should_follow_UIFont_without_changing_header_width_mode()
    {
        WinFormsShims.Font previousUiFont = AppSettings.Font;
        WinFormsShims.Font previousCommitFont = AppSettings.CommitFont;
        Window? window = null;
        try
        {
            AppSettings.Font = new WinFormsShims.Font("Segoe UI", 11);
            AppSettings.CommitFont = new WinFormsShims.Font("Consolas", 18);
            AvaloniaFontSettings.ApplyAppSettings();
            CommitInfo control = new();
            CommitInfo.TestAccessor accessor = control.GetTestAccessor();
            accessor.TableLayout.IsVisible = true;
            accessor.CommitMessage.SetXHTMLText(PlainParagraphs);
            accessor.RevisionInfo.SetXHTMLText(PlainParagraphs);
            XhtmlTextBlock header = accessor.Header.GetTestAccessor().RevisionHeader;
            window = new Window { Width = 1200, Height = 900, Content = control };
            window.Show();
            RunLayout(window);
            accessor.CommitMessage.FontFamily.Name.Should().Be("Consolas");
            accessor.CommitMessage.FontSize.Should().Be(AvaloniaFontSettings.ToDeviceIndependentPixels(18));
            accessor.RevisionInfo.FontFamily.Name.Should().Be("Segoe UI");
            accessor.RevisionInfo.FontSize.Should().Be(AvaloniaFontSettings.ToDeviceIndependentPixels(11));
            AssertSourceFontHeight(accessor.CommitMessage, 28);
            AssertSourceFontHeight(accessor.RevisionInfo, 20);
            AssertAutomaticContents(accessor.CommitMessage, SourceCommitMessageMinimumHeight);
            AssertAutomaticContents(accessor.RevisionInfo);
            double headerWidth = header.Width;
            double headerMinWidth = header.MinWidth;
            header.Width.Should().Be(headerMinWidth);

            AppSettings.Font = new WinFormsShims.Font("Consolas", 22);
            AppSettings.CommitFont = new WinFormsShims.Font("Segoe UI", 9);
            AvaloniaFontSettings.ApplyAppSettings();
            RunLayout(window);
            accessor.CommitMessage.FontFamily.Name.Should().Be("Segoe UI");
            accessor.RevisionInfo.FontFamily.Name.Should().Be("Consolas");
            AssertSourceFontHeight(accessor.CommitMessage, 15);
            AssertSourceFontHeight(accessor.RevisionInfo, 34);
            AssertAutomaticContents(accessor.CommitMessage, SourceCommitMessageMinimumHeight);
            AssertAutomaticContents(accessor.RevisionInfo);
            header.Width.Should().Be(headerWidth);
            header.MinWidth.Should().Be(headerMinWidth);
        }
        finally
        {
            window?.Close();
            AppSettings.Font = previousUiFont;
            AppSettings.CommitFont = previousCommitFont;
            AvaloniaFontSettings.ApplyAppSettings();
        }
    }

    private static void AssertAutomaticContents(XhtmlTextBlock block, int anchoredMinimum = 0)
    {
        TextLayout layout = GetLayout(block);
        double.IsNaN(block.Height).Should().BeTrue();
        block.MinHeight.Should().Be(block.LineHeight);
        block.Bounds.Height.Should().Be(Math.Ceiling(layout.Height) + anchoredMinimum,
            "the actual visual lines determine contents; only the source parent's cached minimum-height anchor adds client space");
        layout.TextLines.All(line => line.Height == block.LineHeight).Should().BeTrue();
    }

    private static void AssertSourceFontHeight(XhtmlTextBlock block, int nativeLineHeight)
    {
        block.LineHeight.Should().Be(WinFormsRichEditTextMeasurer.GetLineHeight(block));
        if (OperatingSystem.IsWindows())
        {
            block.LineHeight.Should().Be(nativeLineHeight,
                "the independent native source-body probe measured this configured-font HFONT at native100");
        }
    }

    private static void ConfigureFont(XhtmlTextBlock block, string family, int points)
    {
        block.FontFamily = new FontFamily(family);
        block.FontSize = AvaloniaFontSettings.ToDeviceIndependentPixels(points);
        block.FontStyle = FontStyle.Normal;
        block.FontWeight = FontWeight.Normal;
    }

    private static TextLayout GetLayout(XhtmlTextBlock block)
        => block.TextLayout ?? throw new AssertionException("The rendered body must contain its actual measured text layout.");

    private static void RunLayout(Window window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }
}
