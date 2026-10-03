using System.Runtime.InteropServices;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Diagnostics;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.Menus;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;
using Size = Avalonia.Size;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class NativeToolStripSplitContentTests
{
    private static IEnumerable<TestCaseData> PreferredCases()
    {
        foreach (bool rightToLeft in new[] { false, true })
        {
            yield return new TestCaseData(9, "Segoe UI", false, false, false, rightToLeft, 76, 20, 44, 15);
            yield return new TestCaseData(11, "Segoe UI", false, false, false, rightToLeft, 86, 24, 54, 20);
            yield return new TestCaseData(18, "Segoe UI", false, false, false, rightToLeft, 119, 36, 87, 32);
            yield return new TestCaseData(22, "Segoe UI", false, false, false, rightToLeft, 140, 45, 108, 41);
            yield return new TestCaseData(9, "Segoe UI", true, false, false, rightToLeft, 78, 20, 46, 15);
            yield return new TestCaseData(18, "Consolas", false, false, false, rightToLeft, 122, 32, 90, 28);
            yield return new TestCaseData(9, "Segoe UI", false, false, true, rightToLeft, 99, 20, 67, 15);
            yield return new TestCaseData(11, "Segoe UI", false, false, true, rightToLeft, 116, 24, 84, 20);
            yield return new TestCaseData(18, "Segoe UI", false, false, true, rightToLeft, 166, 36, 134, 32);
            yield return new TestCaseData(22, "Segoe UI", false, false, true, rightToLeft, 200, 45, 168, 41);
            yield return new TestCaseData(11, "Segoe UI", false, true, true, rightToLeft, 115, 24, 83, 20);
        }
    }

    [AvaloniaTest]
    [TestCaseSource(nameof(PreferredCases))]
    public void Native_content_should_measure_the_actual_item_font_and_arrange_source_padded_image_and_caption(
        int points, string family, bool bold, bool italic, bool workingDirectory, bool rightToLeft,
        int nativeWidth, int nativeHeight, int nativeCaptionWidth, int nativeCaptionHeight)
    {
        IconSplitButton button = NewButton(points, family, bold, italic, workingDirectory, rightToLeft);
        Window window = NewWindow(button);
        try
        {
            window.Show();
            window.UpdateLayout();
            Size measured = CaptionSize(button);
            button.Bounds.Size.Should().Be(new Size(measured.Width + 32, Math.Max(16, measured.Height) + 4));
            if (OperatingSystem.IsWindows())
            {
                measured.Should().Be(new Size(nativeCaptionWidth, nativeCaptionHeight), "these are actual original TextRenderer values, not a caption-width fudge");
                button.Bounds.Size.Should().Be(new Size(nativeWidth, nativeHeight), "the source GetPreferredSize composes image, padded text and CommonLayoutOptions border2");
            }

            // ToolStrip's source owner supplies at least22px to this autosized item.
            // The item preferred-size algorithm alone returns20px at the default font.
            button.Height = Math.Max(22, button.Bounds.Height);
            window.UpdateLayout();
            RecordContentEvidence(window, button, "preferred-content");
            Image image = Image(button);
            TextBlock caption = Caption(button);
            int imageX = rightToLeft ? (int)button.Bounds.Width - 18 : 2;
            int captionX = rightToLeft ? 14 : 18;
            image.TranslatePoint(default, button).Should().Be(new Point(imageX, ((int)button.Bounds.Height - 16) / 2));
            image.Bounds.Size.Should().Be(new Size(16, 16));
            caption.TranslatePoint(default, button).Should().Be(new Point(captionX, ((int)button.Bounds.Height - (int)measured.Height) / 2));
            caption.Bounds.Size.Should().Be(measured);
            caption.FontFamily.Should().Be(button.FontFamily);
            caption.FontSize.Should().Be(button.FontSize);
            caption.FontStyle.Should().Be(button.FontStyle);
            caption.FontWeight.Should().Be(button.FontWeight);
            button.GetVisualDescendants().OfType<Button>().Single(part => part.Name == "PART_PrimaryButton").Content.Should().BeSameAs(button.Content);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(9, false)]
    [TestCase(9, true)]
    [TestCase(11, false)]
    [TestCase(11, true)]
    [TestCase(18, false)]
    [TestCase(18, true)]
    [TestCase(22, false)]
    [TestCase(22, true)]
    public void Image_only_source_preferred_size_should_not_reserve_an_invisible_caption_for_larger_fonts(int points, bool rightToLeft)
    {
        IconSplitButton button = NewButton(points, "Segoe UI", false, false, false, rightToLeft);
        button.Classes.Add("gitextensions-icon-only");
        Window window = NewWindow(button);
        try
        {
            window.Show();
            window.UpdateLayout();
            RecordContentEvidence(window, button, "image-only-content");
            button.Bounds.Size.Should().Be(new Size(32, 20));
            Caption(button).IsVisible.Should().BeFalse();
            button.Content.Should().Be("Branch", "DisplayStyle.Image does not remove the source translation text");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(9, false, false, 60, 2, 3, 0, 16, 2, 4, 44, 15)]
    [TestCase(9, false, false, 180, 28, 3, 16, 16, 44, 4, 44, 15)]
    [TestCase(9, true, false, 60, 58, 3, 0, 16, 14, 4, 44, 15)]
    [TestCase(9, true, false, 180, 136, 3, 16, 16, 40, 4, 44, 15)]
    [TestCase(9, false, true, 60, 2, 3, 0, 16, 2, 4, 44, 15)]
    [TestCase(9, false, true, 180, 2, 3, 16, 16, 18, 4, 67, 15)]
    [TestCase(9, true, true, 60, 12, 0, 0, 0, 14, 4, 44, 15)]
    [TestCase(9, true, true, 180, 162, 3, 16, 16, 95, 4, 67, 15)]
    [TestCase(22, false, false, 60, 2, 14, 0, 7, 2, 2, 44, 19)]
    [TestCase(22, false, false, 180, 12, 14, 16, 7, 28, 2, 108, 19)]
    [TestCase(22, true, false, 60, 12, 0, 0, 0, 14, 2, 44, 19)]
    [TestCase(22, true, false, 180, 152, 14, 16, 7, 24, 2, 108, 19)]
    [TestCase(22, false, true, 60, 2, 14, 0, 7, 2, 2, 44, 19)]
    [TestCase(22, false, true, 180, 2, 14, 0, 7, 2, 2, 164, 19)]
    [TestCase(22, true, true, 60, 12, 0, 0, 0, 14, 2, 44, 19)]
    [TestCase(22, true, true, 180, 12, 0, 0, 0, 14, 2, 164, 19)]
    public void Fixed_width_content_should_preserve_source_center_left_RTL_squeeze_and_client_clip(
        int points, bool rightToLeft, bool workingDirectory, int width,
        int imageX, int imageY, int imageWidth, int imageHeight, int captionX, int captionY, int captionWidth, int captionHeight)
    {
        IconSplitButton button = NewButton(points, "Segoe UI", false, false, workingDirectory, rightToLeft);
        button.Width = width;
        button.Height = 23;
        Window window = NewWindow(button);
        try
        {
            window.Show();
            window.UpdateLayout();
            RecordContentEvidence(window, button, "fixed-content");
            Image image = Image(button);
            TextBlock caption = Caption(button);
            Rect imageRect = new(image.TranslatePoint(default, button)!.Value, image.Bounds.Size);
            Rect captionRect = new(caption.TranslatePoint(default, button)!.Value, caption.Bounds.Size);
            if (OperatingSystem.IsWindows())
            {
                imageRect.Should().Be(new Rect(imageX, imageY, imageWidth, imageHeight), "this exact original renderer rectangle includes zero-size source clipping, not full-image containment");
                captionRect.Should().Be(new Rect(captionX, captionY, captionWidth, captionHeight));
            }

            captionRect.Width.Should().BeLessThanOrEqualTo(width - 16);
            captionRect.Height.Should().BeLessThanOrEqualTo(19);
            caption.ClipToBounds.Should().BeTrue();
            caption.TextTrimming.Should().Be(TextTrimming.None, "the original toolbar has no CharacterEllipsis flag");
            button.Bounds.Size.Should().Be(new Size(width, 23));
            button.ButtonBounds.Width.Should().Be(width - 12);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Image_only_item_with_null_source_content_should_still_keep_the_original_template_image(bool rightToLeft)
    {
        IconSplitButton button = NewButton(9, "Segoe UI", false, false, false, rightToLeft);
        button.Content = null;
        button.Classes.Add("gitextensions-icon-only");
        Window window = NewWindow(button);
        try
        {
            window.Show();
            window.UpdateLayout();
            RecordContentEvidence(window, button, "null-content");
            button.Bounds.Size.Should().Be(new Size(32, 20));
            Image(button).Source.Should().BeSameAs(button.Icon);
            Image(button).IsVisible.Should().BeTrue();
            Caption(button).IsVisible.Should().BeFalse();
            button.Content.Should().BeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    public void Source_image_allocation_should_be_real_painted_pixels_without_an_extra_primary_presenter_inset(bool rightToLeft, bool workingDirectory)
    {
        using WriteableBitmap sourceImage = OpaqueSourceImage();
        IconSplitButton button = NewButton(9, "Segoe UI", false, false, workingDirectory, rightToLeft);
        button.Icon = sourceImage;
        button.Background = Brushes.White;
        button.Width = 180;
        button.Height = 23;
        Window window = NewWindow(button);
        try
        {
            window.Show();
            window.UpdateLayout();
            Image image = Image(button);
            Point origin = image.TranslatePoint(default, button)!.Value;
            using WriteableBitmap frame = window.CaptureRenderedFrame()
                ?? throw new InvalidOperationException("The native toolbar content frame is unavailable.");
            RecordContentEvidence(window, button, "opaque-source-image", frame);
            ReadPixel(frame, (int)origin.X, (int)origin.Y).Should().Be(Colors.Magenta);
            ReadPixel(frame, (int)origin.X + 15, (int)origin.Y + 15).Should().Be(Colors.Magenta);
            ReadPixel(frame, (int)origin.X, (int)origin.Y - 1).Should().Be(Colors.White);
            string? evidence = Environment.GetEnvironmentVariable("GITEXT_SPLIT_STATE_EVIDENCE");
            if (!string.IsNullOrEmpty(evidence))
            {
                Path.IsPathFullyQualified(evidence).Should().BeTrue();
                Directory.CreateDirectory(evidence);
                using FileStream stream = File.Create(Path.Combine(evidence, $"source-content-rtl-{rightToLeft}-working-{workingDirectory}-{Guid.NewGuid():N}.png"));
                frame.Save(stream, PngBitmapEncoderOptions.Default);
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Native_content_template_scope_should_preserve_authored_local_templates_and_restore_both_routes()
    {
        IconSplitButton button = NewButton(9, "Segoe UI", false, false, false, false);
        FuncDataTemplate authoredTemplate = new(_ => true,
            (_, _) => new TextBlock { Name = "authoredCaption", Text = "Authored content", Width = 180, Height = 25 });
        button.ContentTemplate = authoredTemplate;
        Window window = NewWindow(button);
        try
        {
            window.Show();
            window.UpdateLayout();
            button.ContentTemplate.Should().BeSameAs(authoredTemplate);
            button.GetVisualDescendants().OfType<NativeToolStripSplitContent>().Should().BeEmpty();
            button.GetVisualDescendants().OfType<TextBlock>().Single(block => block.Name == "authoredCaption").Bounds.Width.Should().Be(180);
            button.ClearValue(ContentControl.ContentTemplateProperty);
            window.UpdateLayout();
            Caption(button).Text.Should().Be("Branch");
            button.Bounds.Width.Should().Be(CaptionSize(button).Width + 32);
            button.UseNativeToolStripLayout = false;
            window.UpdateLayout();
            button.GetVisualDescendants().OfType<NativeToolStripSplitContent>().Should().BeEmpty();
            button.GetVisualDescendants().OfType<StackPanel>().Should().Contain(panel => panel.Spacing == 4);
            button.UseNativeToolStripLayout = true;
            window.UpdateLayout();
            Caption(button).Text.Should().Be("Branch");
            button.ContentTemplate = authoredTemplate;
            window.UpdateLayout();
            button.GetVisualDescendants().OfType<NativeToolStripSplitContent>().Should().BeEmpty();
            button.UseNativeToolStripLayout = false;
            window.UpdateLayout();
            button.ContentTemplate.Should().BeSameAs(authoredTemplate);
            button.Content.Should().Be("Branch");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Live_font_caption_image_and_direction_changes_should_remeasure_without_replacing_source_content_or_parts()
    {
        IconSplitButton button = NewButton(9, "Segoe UI", false, false, false, false);
        Window window = NewWindow(button);
        try
        {
            window.Show();
            window.UpdateLayout();
            Button primary = button.GetVisualDescendants().OfType<Button>().Single(part => part.Name == "PART_PrimaryButton");
            double initial = button.Bounds.Width;
            button.FontSize = 22 * 96d / 72;
            button.Foreground = Brushes.Lime;
            window.UpdateLayout();
            button.Bounds.Width.Should().BeGreaterThan(initial);
            Caption(button).FontSize.Should().Be(button.FontSize);
            Caption(button).Foreground.Should().BeSameAs(button.Foreground);
            button.Content = "A considerably longer translated branch caption";
            window.UpdateLayout();
            double longer = button.Bounds.Width;
            button.Icon = null;
            button.FlowDirection = FlowDirection.RightToLeft;
            window.UpdateLayout();
            button.Bounds.Width.Should().Be(longer - 16);
            Image(button).IsVisible.Should().BeFalse();
            primary.Should().BeSameAs(button.GetVisualDescendants().OfType<Button>().Single(part => part.Name == "PART_PrimaryButton"));
            primary.Content.Should().BeSameAs(button.Content);
            button.UseNativeToolStripLayout = false;
            window.UpdateLayout();
            button.GetVisualDescendants().OfType<NativeToolStripSplitContent>().Should().BeEmpty();
            button.GetVisualDescendants().OfType<StackPanel>().Should().Contain(panel => panel.Spacing == 4,
                "the unrelated default IconSplitButton template is restored when the source opt-in is removed");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(9)]
    [TestCase(11)]
    [TestCase(18)]
    [TestCase(22)]
    public void Real_Browse_branch_item_should_leave_auto_width_owned_by_native_content_after_font_and_caption_changes(int points)
    {
        GitUI.ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        using FormBrowse form = new() { Width = 1800, Height = 600 };
        IconSplitButton branch = form.FindControl<IconSplitButton>("branchSelect")!;
        branch.FontFamily = new FontFamily("Segoe UI");
        branch.FontWeight = FontWeight.Normal;
        branch.FontStyle = FontStyle.Normal;
        branch.FontSize = points * 96d / 72;
        try
        {
            form.Show();
            form.UpdateLayout();
            RecordContentEvidence(form, branch, "real-browse-branch");
            double.IsNaN(branch.Width).Should().BeTrue("the old fixed39-pixel sizing subscriber must not override the real native content layout");
            branch.Bounds.Width.Should().Be(CaptionSize(branch).Width + 32);
            if (OperatingSystem.IsWindows())
            {
                branch.Bounds.Width.Should().Be(points switch { 9 => 76, 11 => 86, 18 => 119, _ => 140 },
                    "the actual original item preferred widths were independently recorded for all four requested fonts");
            }

            branch.Content = "A considerably longer translated branch caption";
            form.UpdateLayout();
            double.IsNaN(branch.Width).Should().BeTrue();
            branch.Bounds.Width.Should().Be(CaptionSize(branch).Width + 32);
            Caption(branch).FontSize.Should().Be(branch.FontSize);
            if (OperatingSystem.IsWindows() && points is 9 or 22)
            {
                branch.Bounds.Width.Should().Be(points == 9 ? 292 : 684, "these two translated-caption source sizes are native probe results, not the candidate's own measurement");
            }

            branch.Content.Should().Be("A considerably longer translated branch caption");
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void Real_Browse_working_directory_should_preserve_source_fixed_width_until_refresh_then_restore_auto_size()
    {
        GitUI.ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        int previousMinimum = AppSettings.RecentReposComboMinWidth;
        ShorteningRecentRepoPathStrategy previousShortening = AppSettings.ShorteningRecentRepoPathStrategy;
        using FormBrowse form = new() { Width = 1800, Height = 600 };
        WorkingDirectoryToolStripSplitButton button = form.FindControl<WorkingDirectoryToolStripSplitButton>("_NO_TRANSLATE_WorkingDir")!;
        button.FontFamily = new FontFamily("Segoe UI");
        button.FontWeight = FontWeight.Normal;
        button.FontStyle = FontStyle.Normal;
        button.FontSize = 12;
        try
        {
            form.Show();
            AppSettings.ShorteningRecentRepoPathStrategy = ShorteningRecentRepoPathStrategy.None;
            AppSettings.RecentReposComboMinWidth = 100;
            button.GetTestAccessor().RefreshContent("WorkingDir", []);
            form.UpdateLayout();
            button.Width.Should().Be(100);
            button.FontSize = 22 * 96d / 72;
            form.UpdateLayout();
            button.Width.Should().Be(100, "source AutoSize=false is not silently converted to a TextRenderer width on a font notification");
            button.GetTestAccessor().RefreshContent("WorkingDir", []);
            form.UpdateLayout();
            float captionWidth = (float)WinFormsGraphicsTextMeasurer.MeasureSize(button, "WorkingDir").Width;
            button.Width.Should().Be(Math.Max(100, (int)(captionWidth + 11 + 5)));
            double fixedWidth = button.Width;
            button.Content = "Direct source caption change";
            form.UpdateLayout();
            button.Width.Should().Be(fixedWidth);
            AppSettings.RecentReposComboMinWidth = 0;
            button.GetTestAccessor().RefreshContent("working_directory_with_underscores", []);
            form.UpdateLayout();
            double.IsNaN(button.Width).Should().BeTrue();
            Caption(button).Text.Should().Be("working_directory_with_underscores");
            button.Content.Should().Be("working_directory_with_underscores");
        }
        finally
        {
            AppSettings.RecentReposComboMinWidth = previousMinimum;
            AppSettings.ShorteningRecentRepoPathStrategy = previousShortening;
            form.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(9, 1, 82)]
    [TestCase(11, 1, 97)]
    [TestCase(18, 1, 148)]
    [TestCase(22, 1, 178)]
    [TestCase(9, 100, 100)]
    [TestCase(11, 100, 100)]
    [TestCase(18, 100, 148)]
    [TestCase(22, 100, 178)]
    public void Working_directory_fixed_width_should_use_source_GDIplus_float_then_integer_cast_not_padded_caption_plus_icon(
        int points, int minimum, int nativeWidth)
    {
        int previousMinimum = AppSettings.RecentReposComboMinWidth;
        ShorteningRecentRepoPathStrategy previousShortening = AppSettings.ShorteningRecentRepoPathStrategy;
        WorkingDirectoryToolStripSplitButton button = new()
        {
            UseNativeToolStripLayout = true,
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = points * 96d / 72,
            FontWeight = FontWeight.Normal,
            FontStyle = FontStyle.Normal,
        };
        try
        {
            AppSettings.ShorteningRecentRepoPathStrategy = ShorteningRecentRepoPathStrategy.None;
            AppSettings.RecentReposComboMinWidth = minimum;
            button.GetTestAccessor().RefreshContent("WorkingDir", []);
            button.Content.Should().Be("WorkingDir");
            float measured = (float)WinFormsGraphicsTextMeasurer.MeasureSize(button, "WorkingDir").Width;
            button.Width.Should().Be(Math.Max(minimum, (int)(measured + 11 + 5)));
            if (OperatingSystem.IsWindows())
            {
                button.Width.Should().Be(nativeWidth, "the original constructor's actual font/GDI+ values were independently recorded before this adapter");
            }

            button.MinWidth.Should().Be(0);
            button.ImageAlign.Should().Be(HorizontalAlignment.Left);
            button.TextAlign.Should().Be(HorizontalAlignment.Left);
            TranslationCompat.GetConvertMnemonics(button).Should().BeFalse();
            AppSettings.RecentReposComboMinWidth = 0;
            button.GetTestAccessor().RefreshContent("working_directory_with_underscores", []);
            double.IsNaN(button.Width).Should().BeTrue("source AutoSize is restored rather than retaining the last configured width");
            button.Content.Should().Be("working_directory_with_underscores");
        }
        finally
        {
            AppSettings.RecentReposComboMinWidth = previousMinimum;
            AppSettings.ShorteningRecentRepoPathStrategy = previousShortening;
        }
    }

    private static IconSplitButton NewButton(int points, string family, bool bold, bool italic, bool workingDirectory, bool rightToLeft)
        => new()
        {
            UseNativeToolStripLayout = true,
            FontFamily = new FontFamily(family),
            FontSize = points * 96d / 72,
            FontWeight = bold ? FontWeight.Bold : FontWeight.Normal,
            FontStyle = italic ? FontStyle.Italic : FontStyle.Normal,
            MinHeight = 0,
            MinWidth = 0,
            Height = double.NaN,
            Margin = default,
            FlowDirection = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
            Content = workingDirectory ? "WorkingDir" : "Branch",
            ImageAlign = workingDirectory ? HorizontalAlignment.Left : HorizontalAlignment.Center,
            TextAlign = workingDirectory ? HorizontalAlignment.Left : HorizontalAlignment.Center,
            Icon = new DrawingImage
            {
                Drawing = new GeometryDrawing
                {
                    Brush = Brushes.Magenta,
                    Geometry = new RectangleGeometry(new Rect(0, 0, 16, 16)),
                },
            },
        };

    private static Window NewWindow(Control button)
        => new() { Width = 1800, Height = 120, Content = new Canvas { Background = Brushes.White, Children = { button } } };

    private static Size CaptionSize(IconSplitButton button)
    {
        Size measured = WinFormsTextMeasurer.MeasureTextRenderer(button, button.Content as string ?? string.Empty);
        return new Size(Math.Ceiling(measured.Width), Math.Ceiling(measured.Height));
    }

    private static Image Image(IconSplitButton button) => button.GetVisualDescendants().OfType<Image>().Single();

    private static TextBlock Caption(IconSplitButton button)
    {
        NativeToolStripSplitContent[] content = button.GetVisualDescendants().OfType<NativeToolStripSplitContent>().ToArray();
        if (content.Length != 1)
        {
            TestContext.Out.WriteLine(JsonSerializer.Serialize(DescribeContent(button)));
        }

        content.Should().ContainSingle("the source native content panel must be materialized in the actual primary presenter's visual tree");
        return content.Single().Children.OfType<TextBlock>().Single();
    }

    private static WriteableBitmap OpaqueSourceImage()
    {
        WriteableBitmap image = new(new PixelSize(16, 16), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Premul);
        using ILockedFramebuffer buffer = image.Lock();
        int[] row = Enumerable.Repeat(unchecked((int)0xFFFF00FF), 16).ToArray();
        for (int y = 0; y < 16; y++)
        {
            Marshal.Copy(row, 0, IntPtr.Add(buffer.Address, y * buffer.RowBytes), row.Length);
        }

        return image;
    }

    private static object DescribeContent(IconSplitButton button)
        => new
        {
            button.UseNativeToolStripLayout,
            content = button.Content as string,
            bounds = button.Bounds.ToString(),
            background = button.Background?.ToString(),
            fontFamily = button.FontFamily.Name,
            button.FontSize,
            fontStyle = button.FontStyle.ToString(),
            fontWeight = button.FontWeight.ToString(),
            flowDirection = button.FlowDirection.ToString(),
            imageAlign = button.ImageAlign.ToString(),
            textAlign = button.TextAlign.ToString(),
            contentTemplateType = button.ContentTemplate?.GetType().FullName,
            contentTemplatePriority = button.GetDiagnostic(ContentControl.ContentTemplateProperty).Priority.ToString(),
            visualChildren = button.GetVisualDescendants().OfType<Control>().Select(control => new
            {
                type = control.GetType().FullName,
                control.Name,
                bounds = control.Bounds.ToString(),
                ownerOrigin = control.TranslatePoint(default, button)?.ToString(),
                control.ClipToBounds,
                control.IsVisible,
            }).ToArray(),
        };

    private static void RecordContentEvidence(Window window, IconSplitButton button, string stage, WriteableBitmap? frame = null)
    {
        string? evidence = Environment.GetEnvironmentVariable("GITEXT_SPLIT_CONTENT_EVIDENCE");
        if (string.IsNullOrEmpty(evidence))
        {
            return;
        }

        Path.IsPathFullyQualified(evidence).Should().BeTrue();
        Directory.CreateDirectory(evidence);
        string prefix = Path.Combine(evidence, $"{stage}-{Guid.NewGuid():N}");
        if (frame is not null)
        {
            using FileStream stream = File.Create(prefix + ".png");
            frame.Save(stream, PngBitmapEncoderOptions.Default);
        }
        else
        {
            using WriteableBitmap actualFrame = window.CaptureRenderedFrame()
                ?? throw new InvalidOperationException("The actual split-button content frame is unavailable.");
            using FileStream stream = File.Create(prefix + ".png");
            actualFrame.Save(stream, PngBitmapEncoderOptions.Default);
        }

        File.WriteAllText(prefix + ".json", JsonSerializer.Serialize(new
        {
            acquisitionMode = "actualHeadlessWindowFrame",
            imageEdits = false,
            stage,
            testName = TestContext.CurrentContext.Test.FullName,
            content = DescribeContent(button),
        }, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);
    }

    private static Color ReadPixel(WriteableBitmap bitmap, int x, int y)
    {
        using ILockedFramebuffer buffer = bitmap.Lock();
        byte[] pixel = new byte[4];
        Marshal.Copy(IntPtr.Add(buffer.Address, (y * buffer.RowBytes) + (x * 4)), pixel, 0, pixel.Length);
        return buffer.Format == PixelFormat.Bgra8888
            ? Color.FromArgb(pixel[3], pixel[2], pixel[1], pixel[0])
            : Color.FromArgb(pixel[3], pixel[0], pixel[1], pixel[2]);
    }
}
