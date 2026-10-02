using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;
using Size = Avalonia.Size;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class NativeTabHeaderTests
{
    [AvaloniaTest]
    [TestCase("_File tree", "File tree")]
    [TestCase("A&B__C", "A&B_C")]
    public void Header_should_measure_the_displayed_mnemonic_caption(string source, string display)
    {
        AccessText text = new() { Text = source };
        NativeTabHeaderPanel panel = new() { Children = { text } };
        panel.Measure(Size.Infinity);
        panel.DesiredSize.Width.Should().Be(Math.Ceiling(WinFormsTextMeasurer.MeasureSize(text, display).Width));
        text.Text.Should().Be(source);
    }

    [AvaloniaTest]
    [TestCase(9, "Commit", 84)]
    [TestCase(9, "Diff", 59)]
    [TestCase(9, "File tree", 81)]
    [TestCase(9, "GPG", 63)]
    [TestCase(9, "Output", 78)]
    [TestCase(11, "Commit", 93)]
    [TestCase(11, "File tree", 93)]
    public void Header_should_use_native_caption_advances_and_source_image_spacing(int points, string caption, int nativeWidth)
    {
        using RenderTargetBitmap bitmap = new(new PixelSize(16, 16));
        Image image = new() { Width = 16, Height = 16, Source = bitmap };
        TextBlock text = new() { FontFamily = new FontFamily("Segoe UI"), FontSize = points * 96d / 72, Text = caption };
        NativeTabHeaderPanel panel = new() { Children = { image, text } };
        panel.Measure(Size.Infinity);
        panel.Arrange(new Rect(panel.DesiredSize));
        double nativeCaptionWidth = Math.Ceiling(WinFormsTextMeasurer.MeasureSize(text, caption).Width);
        panel.DesiredSize.Width.Should().Be(nativeCaptionWidth + 16 + 8);
        text.Bounds.X.Should().Be(24);
        if (OperatingSystem.IsWindows())
        {
            (panel.DesiredSize.Width + 16).Should().Be(nativeWidth,
                "native tab padding includes the outline rather than adding Avalonia's border again");
        }
    }

    [AvaloniaTest]
    public void Header_should_remeasure_fixed_size_icons_and_release_removed_children()
    {
        using RenderTargetBitmap bitmap = new(new PixelSize(16, 16));
        Image image = new() { Width = 16, Height = 16, Source = bitmap };
        TextBlock text = new() { Text = "Commit" };
        NativeTabHeaderPanel panel = new() { Children = { image, text } };
        panel.Measure(Size.Infinity);
        double withImage = panel.DesiredSize.Width;
        image.Source = null;
        panel.Measure(Size.Infinity);
        panel.DesiredSize.Width.Should().Be(withImage - 24);
        image.Source = bitmap;
        panel.Measure(Size.Infinity);
        panel.DesiredSize.Width.Should().Be(withImage);
        panel.Children.Remove(image);
        panel.Measure(Size.Infinity);
        panel.DesiredSize.Width.Should().Be(withImage - 24);
        image.Source = null;
        panel.IsMeasureValid.Should().BeTrue("removed children no longer own the header's allocation");
    }

    [AvaloniaTest]
    public void Browse_header_allocations_should_follow_translation_font_and_icon_changes_without_selection_reflow()
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        FormBrowse form = new();
        form.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            TabItem page = form.CommitInfoTabPage;
            double before = page.Bounds.Width;
            page.Header = "Eine erheblich längere übersetzte Registerkarte";
            Dispatcher.UIThread.RunJobs();
            page.Bounds.Width.Should().BeGreaterThan(before);
            Rect translated = page.Bounds;
            form.CommitInfoTabControl.SelectedItem = form.DiffTabPage;
            Dispatcher.UIThread.RunJobs();
            page.Bounds.Should().Be(translated);
            page.FontSize = 20;
            Dispatcher.UIThread.RunJobs();
            page.Bounds.Width.Should().BeGreaterThan(translated.Width);
            NativeTabHeaderPanel panel = page.GetVisualDescendants().OfType<NativeTabHeaderPanel>().Single();
            Image image = panel.Children.OfType<Image>().Single();
            double withImage = panel.DesiredSize.Width;
            page.Icon = null;
            Dispatcher.UIThread.RunJobs();
            image.Source.Should().BeNull();
            panel.DesiredSize.Width.Should().Be(withImage - 24);
        }
        finally
        {
            form.Close();
        }
    }
}
