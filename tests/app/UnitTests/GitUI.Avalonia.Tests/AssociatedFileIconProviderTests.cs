using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using GitCommands;
using GitExtensions.Shims.WinForms;
using GitUI.Compat;
using GitUI.Properties;

namespace GitExtensionsTests;

[TestFixture]
public sealed class AssociatedFileIconProviderTests
{
    [AvaloniaTest]
    public void FileAssociatedIconProvider_should_use_the_shim_host_and_preserve_its_extension_cache()
    {
        CountingIconSource source = new();
        IIconExtractor? original = ShimHost.IconExtractor;
        FileAssociatedIconProvider provider = new();
        provider.ResetCache();
        ShimHost.IconExtractor = new AssociatedFileIconExtractor(source);
        try
        {
            provider.Get("repository", "first.parity-icon-test")!.PlatformIcon.Should().BeSameAs(Images.File);
            provider.Get("other-repository", "second.PARITY-ICON-TEST")!.PlatformIcon.Should().BeSameAs(Images.File);
            provider.Get("repository", "README").Should().BeNull();

            source.Calls.Should().ContainSingle();
            source.Calls[0].RelativeFilePath.Should().EndWith("first.parity-icon-test");
        }
        finally
        {
            ShimHost.IconExtractor = original;
            provider.ResetCache();
        }
    }

    [AvaloniaTest]
    public void FreedesktopAssociatedFileIconSource_should_follow_shared_mime_and_icon_theme_data()
    {
        string root = Path.Combine(Path.GetTempPath(), $"GitExtensions.AssociatedIcon-{Guid.NewGuid():N}");
        string data = Path.Combine(root, "share");
        string mime = Path.Combine(data, "mime");
        string icons = Path.Combine(root, "icons");
        string icon16 = Path.Combine(icons, "theme", "16x16", "mimetypes", "visual-csharp.png");
        string icon32 = Path.Combine(icons, "theme", "32x32", "mimetypes", "visual-csharp.png");
        try
        {
            Directory.CreateDirectory(mime);
            Directory.CreateDirectory(Path.GetDirectoryName(icon16)!);
            Directory.CreateDirectory(Path.GetDirectoryName(icon32)!);
            File.WriteAllText(
                Path.Combine(mime, "globs2"),
                "40:text/plain:*.cs\n80:text/x-csharp:*.cs\n50:text/x-case-sensitive:*.CASE:cs\n");
            File.WriteAllText(Path.Combine(mime, "icons"), "text/x-csharp:visual-csharp\n");
            File.WriteAllText(Path.Combine(mime, "generic-icons"), "text/x-csharp:text-x-generic\n");
            using RenderTargetBitmap testIcon = new(new Avalonia.PixelSize(1, 1));
            testIcon.Save(icon16, PngBitmapEncoderOptions.Default);
            File.Copy(icon16, icon32);

            FreedesktopAssociatedFileIconSource source = new([data], [icons]);

            source.GetMimeType("sample.CS").Should().Be("text/x-csharp");
            source.GetMimeType("sample.case").Should().BeNull();
            source.GetMimeType("sample.CASE").Should().Be("text/x-case-sensitive");
            source.GetIconNames("text/x-csharp").Should().Equal(
                "visual-csharp",
                "text-x-csharp",
                "text-x-generic");
            source.FindPng("visual-csharp").Should().Be(icon16);
            using Bitmap image = (Bitmap)source.Get(root, "sample.cs")!;
            image.PixelSize.Should().Be(new Avalonia.PixelSize(1, 1));
        }
        finally
        {
            TestDirectory.Delete(root);
        }
    }

    [AvaloniaTest]
    [Platform(Include = "Win")]
    public void WindowsAssociatedFileIconSource_should_return_the_registered_shell_icon()
    {
        IImage? image = new WindowsAssociatedFileIconSource().Get(Path.GetTempPath(), "sample.txt");

        image.Should().NotBeNull();
        ((Bitmap)image!).PixelSize.Should().Be(new Avalonia.PixelSize(16, 16));
    }

    private sealed class CountingIconSource : IAssociatedFileIconSource
    {
        public List<(string WorkingDirectory, string RelativeFilePath)> Calls { get; } = [];

        public IImage? Get(string workingDirectory, string relativeFilePath)
        {
            Calls.Add((workingDirectory, relativeFilePath));
            return Images.File;
        }
    }
}
