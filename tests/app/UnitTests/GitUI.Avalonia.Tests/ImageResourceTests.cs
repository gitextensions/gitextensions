using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI.Theming;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using GitUI.Editor;
using GitUI.LeftPanel;
using GitUI.Properties;
using GitUI.Theming;
using ImagesResxToAvalonia;
using Microsoft.VisualStudio.Threading;
using DrawingColor = System.Drawing.Color;
using KnownColor = System.Drawing.KnownColor;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class ImageResourceTests
{
    [SetUp]
    public void SetUp()
        => ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

    [Test]
    public void Generated_images_api_should_match_Images_resx()
    {
        string repositoryRoot = FindRepositoryRoot();
        string resxPath = Path.Combine(repositoryRoot, "src", "app", "GitUI", "Properties", "Images.resx");
        string generatedPath = Path.Combine(repositoryRoot, "src", "app", "GitUI.Avalonia", "Properties", "Images.g.cs");

        File.ReadAllText(generatedPath).ReplaceLineEndings("\n").Should().Be(ImagesGenerator.Generate(resxPath));
    }

    [AvaloniaTest]
    public void Every_packaged_image_should_be_the_exact_Images_resx_source_file()
    {
        string repositoryRoot = FindRepositoryRoot();
        foreach ((string name, string resourcePath) in Images.ResourcePaths)
        {
            string sourcePath = resourcePath.StartsWith("Resources/Logo/", StringComparison.Ordinal)
                ? Path.Combine(repositoryRoot, "setup", "assets", "Logo", Path.GetFileName(resourcePath))
                : Path.Combine(repositoryRoot, "src", "app", "GitUI", resourcePath.Replace('/', Path.DirectorySeparatorChar));
            using Stream packaged = AssetLoader.Open(new Uri("avares://GitUI.Avalonia/" + resourcePath));
            using MemoryStream packagedBytes = new();
            packaged.CopyTo(packagedBytes);

            packagedBytes.ToArray().Should().Equal(File.ReadAllBytes(sourcePath),
                $"{name} must package the file selected by Images.resx without conversion");
        }
    }

    [AvaloniaTest]
    public void Every_generated_image_should_be_packaged_and_cached()
    {
        Images.ResourcePaths.Should().NotBeEmpty();

        Dictionary<string, PropertyInfo> bitmapProperties = typeof(Images)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(property => property.PropertyType == typeof(Bitmap))
            .ToDictionary(property => property.Name, StringComparer.Ordinal);
        bitmapProperties.Keys.Should().BeEquivalentTo(Images.ResourcePaths.Keys);

        foreach ((string name, string resourcePath) in Images.ResourcePaths)
        {
            Uri uri = new("avares://GitUI.Avalonia/" + resourcePath);
            using Stream stream = AssetLoader.Open(uri);
            stream.Length.Should().BeGreaterThan(0, $"{name} should resolve {uri}");

            Bitmap bitmap = (Bitmap)bitmapProperties[name].GetValue(null)!;
            bitmap.PixelSize.Width.Should().BeGreaterThan(0, $"{name} should decode {uri}");
            bitmap.PixelSize.Height.Should().BeGreaterThan(0, $"{name} should decode {uri}");
        }

        Images.Push.Should().BeSameAs(Images.Push);
        Images.Push.PixelSize.Width.Should().BeGreaterThan(0);
        Images.Push.PixelSize.Height.Should().BeGreaterThan(0);
        Images.ApplicationIcon.Should().BeSameAs(Images.ApplicationIcon);
    }

    [AvaloniaTest]
    public void Icon_controls_should_keep_translatable_content_and_render_the_cached_bitmap()
    {
        IconButton button = new()
        {
            Content = "_Push",
            Icon = Images.Push,
        };
        Window window = new()
        {
            Content = button,
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            button.Content.Should().Be("_Push", "the original .Text translation path expects string content");
            button.GetVisualDescendants()
                .OfType<Image>()
                .Should().ContainSingle()
                .Which.Source.Should().BeSameAs(Images.Push);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(1d, 48)]
    [TestCase(2d, 96)]
    public void Shared_icon_controls_should_render_at_16_DIP_without_bitmap_resizing(
        double renderScale,
        int expectedFramePixels)
    {
        IconButton button = new()
        {
            Width = 48,
            Height = 48,
            Content = "_Push",
            Icon = Images.Push,
        };
        Window window = new()
        {
            Width = 48,
            Height = 48,
            Content = button,
        };

        try
        {
            window.Show();
            window.SetRenderScaling(renderScale);
            Dispatcher.UIThread.RunJobs();

            Image image = button.GetVisualDescendants().OfType<Image>().Should().ContainSingle().Subject;
            image.Bounds.Width.Should().Be(16);
            image.Bounds.Height.Should().Be(16);
            WriteableBitmap frame = window.CaptureRenderedFrame()!;
            frame.PixelSize.Should().Be(new PixelSize(expectedFramePixels, expectedFramePixels));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Foreground_images_should_use_the_original_lightness_correction_for_dark_and_custom_themes()
    {
        ThemeSettings dark = CreateThemeSettings(
            ThemeId.DefaultDark,
            DrawingColor.FromArgb(0x32, 0x32, 0x32),
            new Dictionary<KnownColor, DrawingColor>());
        ThemeSettings custom = CreateThemeSettings(
            new ThemeId("p1-4-custom"),
            DrawingColor.FromArgb(0x2B, 0x2D, 0x3A),
            new Dictionary<KnownColor, DrawingColor>
            {
                [KnownColor.Window] = DrawingColor.FromArgb(0x21, 0x23, 0x30),
                [KnownColor.WindowText] = DrawingColor.FromArgb(0xEA, 0xEC, 0xF4),
            });
        Bitmap[] foregroundImages =
        [
            Images.Author,
            Images.Branch,
            Images.CollapseAll,
            Images.Develop,
            Images.DocumentTree,
            Images.ExpandAll,
            Images.EyeClosed,
            Images.EyeOpened,
            Images.FileStatusRenamed,
            Images.FileStatusRenamedOnlyA,
            Images.FileStatusRenamedOnlyB,
            Images.FileStatusRenamedSame,
            Images.FileStatusRenamedUnequal,
            Images.Font,
            Images.HelpCommandMerge,
            Images.HelpCommandMergeFastForward,
            Images.HelpCommandRebase,
            Images.HelpPullFetch,
            Images.HelpPullMerge,
            Images.HelpPullMergeFastForward,
            Images.HelpPullRebase,
            Images.EditColor,
            Images.Link,
            Images.Pull,
            Images.PullRequest,
            Images.RecoverLostObjects,
            Images.RemoteEnableAndFetch,
            Images.ShowWhitespace,
            Images.Translate,
            Images.UiScrollBar,
            Images.WhitespaceIgnore,
            Images.WhitespaceIgnoreAll,
            Images.WhitespaceIgnoreEol,
        ];

        BitmapLightnessExtensions.AdaptLightness(Images.Branch, ThemeSettings.Default)
            .Should().BeSameAs(Images.Branch);
        foreach (ThemeSettings settings in new[] { dark, custom })
        {
            DrawingColor text = AvaloniaThemeResources.ResolveSystemColor(settings, KnownColor.WindowText);
            DrawingColor background = AvaloniaThemeResources.ResolveSystemColor(settings, KnownColor.Window);
            foreach (Bitmap original in foregroundImages)
            {
                byte[] source = GetPixels(original);
                Bitmap adapted = BitmapLightnessExtensions.AdaptLightness(original, settings);
                byte[] actual = GetPixels(adapted);
                actual.Should().Equal(ReferenceAdapt(source, text, background));
                for (int alpha = 3; alpha < source.Length; alpha += 4)
                {
                    actual[alpha].Should().Be(source[alpha], "LightnessCorrection preserves alpha");
                }

                BitmapLightnessExtensions.AdaptLightness(original, settings).Should().BeSameAs(adapted,
                    "immutable Avalonia theme variants should be shared by every control");
            }
        }
    }

    [AvaloniaTest]
    public void File_status_images_should_preserve_the_original_status_and_comparison_colors()
    {
        GetArgbColors(Images.FileStatusAdded).Should().Contain(0xFF11D822u);
        GetArgbColors(Images.FileStatusRemoved).Should().Contain(0xFFEE6666u);
        GetArgbColors(Images.FileStatusModified).Should().Contain(0xFFFFEE88u);
        GetArgbColors(Images.FileStatusCopied).Should().Contain(0xFFFFFFFFu);
        GetArgbColors(Images.FileStatusRenamed).Should().Contain(0xFF000000u);
        GetArgbColors(Images.FileStatusUnknown).Should().Contain(0xFF2446ABu);

        GetArgbColors(Images.FileStatusAddedOnlyA).Should().Contain(0xFFA8A800u);
        GetArgbColors(Images.FileStatusAddedOnlyB).Should().Contain(0xFFBD7CFFu);
        GetArgbColors(Images.FileStatusAddedSame).Should().Contain(0xFF00A800u);
        GetArgbColors(Images.FileStatusAddedUnequal).Should().Contain(0xFFFF0000u);
    }

    [Test]
    public void Every_AXAML_menu_icon_should_declare_16_DIP_size()
    {
        string repositoryRoot = FindRepositoryRoot();
        List<string> failures = [];
        foreach (string path in Directory.EnumerateFiles(
                     Path.Combine(repositoryRoot, "src", "app", "GitUI.Avalonia"),
                     "*.axaml",
                     SearchOption.AllDirectories))
        {
            XDocument document = XDocument.Load(path);
            foreach (XElement image in document.Descendants().Where(element => element.Name.LocalName == nameof(Image)))
            {
                bool isMenuIcon = image.Ancestors().Any(element => element.Name.LocalName == "MenuItem.Icon");
                if (isMenuIcon
                    && (image.Attribute("Width")?.Value != "16"
                        || image.Attribute("Height")?.Value != "16"))
                {
                    failures.Add($"{Path.GetRelativePath(repositoryRoot, path)}: {image}");
                }
            }
        }

        failures.Should().BeEmpty("WinForms menu images render in a 16-DIP slot");
    }

    [AvaloniaTest]
    public void Existing_views_should_use_shared_icons_for_menus_trees_statuses_and_windows()
    {
        FormBrowse form = new();
        FormCommit commitForm = new();
        FileViewer fileViewer = new();
        RepoObjectsTree tree = new();
        FileStatusList fileStatusList = new();
        Window listWindow = new()
        {
            Width = 300,
            Height = 120,
            Content = fileStatusList,
        };

        try
        {
            form.Icon.Should().BeSameAs(Images.ApplicationIcon);
            MenuItem commit = form.FindControl<MenuItem>("commitToolStripMenuItem")!;
            commit.Icon.Should().BeOfType<Image>()
                .Which.Source.Should().BeSameAs(Images.RepoStateClean);
            form.FindControl<MenuItem>("translateToolStripMenuItem")!.Icon.Should().BeOfType<Image>()
                .Which.Source.Should().BeSameAs(Images.Translate.AdaptLightness());

            commitForm.FindControl<Image>("toolStripStatusBranchIcon")!.Source
                .Should().BeSameAs(Images.Branch.AdaptLightness());

            Image showWhitespaceButton = fileViewer.FindControl<Button>("showNonPrintChars")!.Content
                .Should().BeOfType<Image>().Subject;
            Image showWhitespaceMenu = fileViewer.FindControl<MenuItem>("showNonprintableCharactersToolStripMenuItem")!.Icon
                .Should().BeOfType<Image>().Subject;
            showWhitespaceButton.Source.Should().BeSameAs(Images.ShowWhitespace.AdaptLightness());
            showWhitespaceMenu.Source.Should().BeSameAs(showWhitespaceButton.Source);
            Image syntaxButton = fileViewer.FindControl<Button>("showSyntaxHighlighting")!.Content
                .Should().BeOfType<Image>().Subject;
            fileViewer.FindControl<MenuItem>("showSyntaxHighlightingToolStripMenuItem")!.Icon
                .Should().BeOfType<Image>().Which.Source.Should().BeSameAs(syntaxButton.Source);

            tree.SetRefs([]);
            TreeView treeMain = tree.FindControl<TreeView>("treeMain")!;
            TreeViewItem branches = treeMain.Items.Cast<TreeViewItem>().First();
            branches.Header.Should().BeOfType<StackPanel>()
                .Which.Children.OfType<Image>().Should().ContainSingle()
                .Which.Source.Should().BeSameAs(Images.BranchLocalRoot);

            fileStatusList.SetDiffs([new GitItemStatus("new-file.txt") { IsNew = true }]);
            listWindow.Show();
            Dispatcher.UIThread.RunJobs();
            fileStatusList.GetVisualDescendants()
                .OfType<Image>()
                .Should().Contain(image => ReferenceEquals(image.Source, Images.FileStatusAdded));
        }
        finally
        {
            listWindow.Close();
            commitForm.Close();
            form.Close();
        }
    }

    [AvaloniaTest]
    public void Dialog_icons_should_preserve_each_supported_semantic()
    {
        DialogIconFactory.Create(WinFormsShims.MessageBoxIcon.None).Should().BeNull();
        DialogIconFactory.Create(WinFormsShims.MessageBoxIcon.Information)
            .Should().BeOfType<Image>().Which.Source.Should().BeSameAs(Images.Information);
        DialogIconFactory.Create(WinFormsShims.MessageBoxIcon.Warning)
            .Should().BeOfType<Image>().Which.Source.Should().BeSameAs(Images.Warning);
        DialogIconFactory.Create(WinFormsShims.MessageBoxIcon.Error)
            .Should().BeOfType<Image>().Which.Source.Should().BeSameAs(Images.StatusBadgeError);
        DialogIconFactory.Create(WinFormsShims.MessageBoxIcon.Question).Should().BeOfType<Border>();

        DialogIconFactory.Create(TaskDialogIcon.Information)
            .Should().BeOfType<Image>().Which.Source.Should().BeSameAs(Images.Information);
        DialogIconFactory.Create(TaskDialogIcon.Warning)
            .Should().BeOfType<Image>().Which.Source.Should().BeSameAs(Images.Warning);
        DialogIconFactory.Create(TaskDialogIcon.Error)
            .Should().BeOfType<Image>().Which.Source.Should().BeSameAs(Images.StatusBadgeError);
    }

    private static string FindRepositoryRoot([CallerFilePath] string startPath = "")
    {
        DirectoryInfo? directory = new(Path.GetDirectoryName(startPath)!);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GitExtensions.Avalonia.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException($"Could not find the repository root from {startPath}.");
    }

    private static ThemeSettings CreateThemeSettings(
        ThemeId id,
        DrawingColor panelBackground,
        IReadOnlyDictionary<KnownColor, DrawingColor> systemColors)
    {
        Theme theme = new(
            new Dictionary<AppColor, DrawingColor> { [AppColor.PanelBackground] = panelBackground },
            systemColors,
            id);
        return new ThemeSettings(theme, Theme.Default, ThemeVariations.None, useSystemVisualStyle: false);
    }

    private static byte[] GetPixels(Bitmap bitmap)
    {
        WriteableBitmap copy = new(bitmap.PixelSize, bitmap.Dpi, PixelFormat.Bgra8888, AlphaFormat.Unpremul);
        using ILockedFramebuffer framebuffer = copy.Lock();
        bitmap.CopyPixels(framebuffer);
        byte[] pixels = new byte[framebuffer.Size.Width * framebuffer.Size.Height * 4];
        for (int y = 0; y < framebuffer.Size.Height; y++)
        {
            Marshal.Copy(
                IntPtr.Add(framebuffer.Address, y * framebuffer.RowBytes),
                pixels,
                y * framebuffer.Size.Width * 4,
                framebuffer.Size.Width * 4);
        }

        return pixels;
    }

    private static byte[] ReferenceAdapt(byte[] source, DrawingColor text, DrawingColor background)
    {
        byte[] result = (byte[])source.Clone();
        HslColor textColor = new(text);
        HslColor backgroundColor = new(background);
        for (int location = 0; location < result.Length; location += 4)
        {
            DrawingColor rgb = DrawingColor.FromArgb(result[location + 2], result[location + 1], result[location]);
            HslColor hsl = rgb.ToPerceptedHsl();
            HslColor transformed = new(
                hsl.H,
                hsl.L > 0.1 ? hsl.S : hsl.S * hsl.L / 0.1,
                textColor.L + (hsl.L * (backgroundColor.L - textColor.L)));
            DrawingColor actual = transformed.ToActualHsl(rgb).ToColor();
            result[location] = actual.B;
            result[location + 1] = actual.G;
            result[location + 2] = actual.R;
        }

        return result;
    }

    private static HashSet<uint> GetArgbColors(Bitmap bitmap)
    {
        byte[] pixels = GetPixels(bitmap);
        HashSet<uint> colors = [];
        for (int offset = 0; offset < pixels.Length; offset += 4)
        {
            uint argb = ((uint)pixels[offset + 3] << 24)
                | ((uint)pixels[offset + 2] << 16)
                | ((uint)pixels[offset + 1] << 8)
                | pixels[offset];
            colors.Add(argb);
        }

        return colors;
    }
}
