using System.Globalization;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitUI.Compat;
using GitUI.LeftPanel;
using GitUI.Properties;
using Color = Avalonia.Media.Color;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class NativeTreeImageListTests
{
    private static readonly Color[] _backdrops =
    [
        Colors.White, Colors.Black, Color.FromRgb(50, 50, 50),
        Color.FromRgb(204, 232, 255), Color.FromRgb(43, 45, 58),
    ];

    public static IEnumerable<TestCaseData> AlphaCases()
    {
        foreach (string asset in new[] { "synthetic-black", "synthetic-white", "synthetic-gray", "synthetic-color" })
        {
            foreach (Color backdrop in _backdrops)
            {
                foreach (AlphaFormat alphaFormat in new[] { AlphaFormat.Unpremul, AlphaFormat.Premul })
                {
                    yield return new TestCaseData(asset, backdrop, alphaFormat)
                        .SetName($"Native_image_list_{asset}_{backdrop}_{alphaFormat}");
                }
            }
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCaseSource(nameof(AlphaCases))]
    public void Every_alpha_should_preserve_native_atlas_channels_and_source_over_draw(
        string asset, Color backdrop, AlphaFormat alphaFormat)
    {
        Color sourceColor = asset switch
        {
            "synthetic-black" => Colors.Black,
            "synthetic-white" => Colors.White,
            "synthetic-gray" => Color.FromRgb(85, 85, 85),
            "synthetic-color" => Color.FromRgb(53, 103, 173),
            _ => throw new ArgumentOutOfRangeException(nameof(asset)),
        };
        using WriteableBitmap source = CreateRamp(sourceColor, alphaFormat);
        IImage converted = NativeTreeImageListImage.Create(source);
        using RenderTargetBitmap atlas = Render(converted, null);
        using RenderTargetBitmap painted = Render(NativeTreeImageListImage.Create(source, () => backdrop), backdrop);
        byte[] atlasPixels = ReadPixels(atlas);
        byte[] paintedPixels = ReadPixels(painted);
        byte[] nativeChannels = Convert.FromHexString(GetNativePremultipliedChannels(asset));
        for (int alpha = 0; alpha <= byte.MaxValue; alpha++)
        {
            int sourceOffset = alpha * 3;
            Color native = Color.FromArgb((byte)alpha, nativeChannels[sourceOffset], nativeChannels[sourceOffset + 1], nativeChannels[sourceOffset + 2]);
            ReadPixel(atlasPixels, alpha).Should().Be(native, $"native DIB alpha {alpha}, asset {asset}");
            ReadPixel(paintedPixels, alpha).Should().Be(Composite(native, backdrop),
                $"independently corroborated ImageList_Draw/TREEVIEW alpha {alpha}, asset {asset}, backdrop {backdrop}");
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase("BranchLocalRoot", 0, 0, "4D343434")]
    [TestCase("BranchLocalRoot", 1, 0, "66404040")]
    [TestCase("Stash", 8, 0, "B2A86160")]
    [TestCase("Stash", 9, 0, "B2A8312F")]
    [TestCase("TagHorizontal", 4, 2, "2F232424")]
    [TestCase("TagHorizontal", 5, 2, "67414747")]
    public void Real_source_assets_should_preserve_independent_native_partial_alpha_vectors(
        string asset, int x, int y, string nativeArgb)
    {
        Bitmap source = asset switch
        {
            "BranchLocalRoot" => Images.BranchLocalRoot,
            "Stash" => Images.Stash,
            "TagHorizontal" => Images.TagHorizontal,
            _ => throw new ArgumentOutOfRangeException(nameof(asset)),
        };
        byte[] before = ReadPixels(source);
        IImage converted = NativeTreeImageListImage.Create(source);
        converted.Size.Should().Be(source.Size);
        Color expected = ParseArgb(nativeArgb);
        using RenderTargetBitmap atlas = Render(converted, null);
        ReadPixel(ReadPixels(atlas), x + (y * 16)).Should().Be(expected);
        foreach (Color backdrop in _backdrops)
        {
            using RenderTargetBitmap painted = Render(NativeTreeImageListImage.Create(source, () => backdrop), backdrop);
            ReadPixel(ReadPixels(painted), x + (y * 16)).Should().Be(Composite(expected, backdrop));
        }

        ReadPixels(source).Should().Equal(before, "the generated original resource must remain unchanged");
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Caption_replacement_should_not_repeat_conversion_or_change_native_icon_layout()
    {
        IImage converted = NativeTreeImageListImage.Create(Images.BranchLocalRoot);
        NativeTreeImageListImage.Create(Images.BranchLocalRoot).Should().BeSameAs(converted);
        NativeTreeImageListImage.Create(converted).Should().BeSameAs(converted);
        StackPanel first = (StackPanel)RepoObjectsTree.CreateHeader("Branches", Images.BranchLocalRoot);
        Image firstImage = first.Children.OfType<Image>().Single();
        StackPanel changed = (StackPanel)RepoObjectsTree.CreateHeader("Translated branches", firstImage.Source!);
        Image changedImage = changed.Children.OfType<Image>().Single();
        NativeTreeImageListImage.Create(changedImage.Source!).Should().BeSameAs(converted);
        changedImage.Width.Should().Be(16);
        changedImage.Height.Should().Be(16);
        changedImage.Stretch.Should().Be(Stretch.Uniform);
        changed.Spacing.Should().Be(3);
        changed.Children.OfType<NativeTreeTextBlock>().Single().Text.Should().Be("Translated branches");
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Non_bitmap_images_should_retain_their_defined_vector_renderer()
    {
        DrawingImage vector = new();
        NativeTreeImageListImage.Create(vector).Should().BeSameAs(vector);
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Live_color_edits_should_bound_cached_backdrops_without_disposing_recorded_images()
    {
        using WriteableBitmap source = CreateRamp(Color.FromRgb(53, 103, 173), AlphaFormat.Unpremul);
        NativeTreeImageListImage native = (NativeTreeImageListImage)NativeTreeImageListImage.Create(source);
        List<RenderTargetBitmap> retained = [];
        try
        {
            for (int index = 0; index < 20; index++)
            {
                Color backdrop = Color.FromRgb((byte)(index * 9), (byte)(index * 7), (byte)(index * 5));
                RenderTargetBitmap rendered = Render(NativeTreeImageListImage.Create(source, () => backdrop), backdrop);
                retained.Add(rendered);
                ReadPixel(ReadPixels(rendered), 77).Should().Be(Composite(Color.FromArgb(77, 49, 54, 60), backdrop));
                native.OpaqueVariantCount.Should().BeLessThanOrEqualTo(8);
            }

            ReadPixel(ReadPixels(retained[0]), 77).Should().Be(Color.FromRgb(49, 54, 60));
        }
        finally
        {
            foreach (RenderTargetBitmap rendered in retained)
            {
                rendered.Dispose();
            }
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Actual_header_backdrop_changes_should_invalidate_native_source_over_without_replacing_the_resource()
    {
        NativeTreeImageListControl image = new(Images.BranchLocalRoot) { Width = 16, Height = 16 };
        SolidColorBrush brush = new(Color.FromRgb(43, 45, 58));
        Border header = new() { Width = 16, Height = 16, Background = brush, Child = image };
        Canvas parent = new() { Background = Brushes.White, Children = { header } };
        Window window = new() { Width = 20, Height = 20, Content = parent };
        IImage original = image.Source!;
        Color native = Color.FromArgb(77, 52, 52, 52);
        try
        {
            window.Show();
            window.UpdateLayout();
            AssertFramePixel(window, Composite(native, brush.Color));
            brush.Color = Color.FromRgb(50, 50, 50);
            AssertFramePixel(window, Composite(native, brush.Color));
            header.Background = new SolidColorBrush(Color.FromRgb(204, 232, 255));
            AssertFramePixel(window, Composite(native, Color.FromRgb(204, 232, 255)));
            header.Background = Brushes.Transparent;
            parent.Background = new SolidColorBrush(Color.FromRgb(43, 45, 58));
            AssertFramePixel(window, Composite(native, Color.FromRgb(43, 45, 58)));
            image.Source.Should().BeSameAs(original, "live paint changes must not replace or repeat the native atlas conversion");
            header.Child = null;
            brush.Color = Colors.Magenta;
            header.Child = image;
            header.Background = brush;
            window.UpdateLayout();
            AssertFramePixel(window, Composite(native, brush.Color));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Focus_search_and_theme_backdrops_should_be_resolved_from_the_actual_repo_header_paint()
    {
        RepoObjectsTree control = new();
        control.SetRefs([]);
        TreeView tree = control.GetTestAccessor().Tree;
        TreeViewItem item = tree.Items.Cast<TreeViewItem>().First();
        Button other = new() { Content = "Other" };
        Window window = new() { Width = 360, Height = 250, Content = new StackPanel { Children = { control, other } } };
        window.Resources["GitExtensionsNativeTreeSelectionBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(204, 232, 255));
        try
        {
            window.Show();
            window.UpdateLayout();
            NativeTreeImageListControl image = ((StackPanel)item.Header!).Children.OfType<NativeTreeImageListControl>().Single();
            tree.Background = new SolidColorBrush(Color.FromRgb(43, 45, 58));
            AssertConsumerPixel(window, image, Color.FromRgb(43, 45, 58));
            tree.SelectedItem = item;
            item.Focus();
            AssertConsumerPixel(window, image, Color.FromRgb(204, 232, 255));
            window.Resources["GitExtensionsNativeTreeSelectionBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(50, 50, 50));
            AssertConsumerPixel(window, image, Color.FromRgb(50, 50, 50));
            other.Focus();
            AssertConsumerPixel(window, image, Color.FromRgb(43, 45, 58));
            item.Classes.Add("repo-search-result");
            window.Resources["GitExtensionsNativeTreeSearchBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(255, 255, 225));
            AssertConsumerPixel(window, image, Color.FromRgb(255, 255, 225));
            item.Classes.Remove("repo-search-result");
            AssertConsumerPixel(window, image, Color.FromRgb(43, 45, 58));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Unknown_opacity_and_nonuniform_backdrops_should_keep_the_defined_native_premultiplied_fallback()
    {
        NativeTreeImageListControl image = new(Images.BranchLocalRoot) { Width = 16, Height = 16 };
        Border header = new() { Width = 16, Height = 16, Background = Brushes.White, Child = image };
        Window window = new() { Width = 20, Height = 20, Content = header };
        Color native = Color.FromArgb(77, 52, 52, 52);
        try
        {
            window.Show();
            window.UpdateLayout();
            image.Opacity = 0.5;
            using (RenderTargetBitmap fallback = Render(image.Source!, null))
            {
                ReadPixel(ReadPixels(fallback), 0).Should().Be(native);
            }

            image.Opacity = 1;
            header.Opacity = 0.5;
            using (RenderTargetBitmap fallback = Render(image.Source!, null))
            {
                ReadPixel(ReadPixels(fallback), 0).Should().Be(native);
            }

            header.Opacity = 1;
            window.Opacity = 0.5;
            using (RenderTargetBitmap fallback = Render(image.Source!, null))
            {
                ReadPixel(ReadPixels(fallback), 0).Should().Be(native,
                    "an outer group's opacity must be checked beyond the nearer opaque white header");
            }

            window.Opacity = 1;
            header.Background = new LinearGradientBrush
            {
                GradientStops = new GradientStops { new GradientStop(Colors.Black, 0), new GradientStop(Colors.White, 1) },
            };
            using (RenderTargetBitmap fallback = Render(image.Source!, null))
            {
                ReadPixel(ReadPixels(fallback), 0).Should().Be(native);
            }

            header.Background = new SolidColorBrush(Color.FromArgb(128, 50, 50, 50));
            using (RenderTargetBitmap fallback = Render(image.Source!, null))
            {
                ReadPixel(ReadPixels(fallback), 0).Should().Be(native);
            }
        }
        finally
        {
            window.Close();
        }
    }

    private static void AssertFramePixel(Window window, Color expected)
    {
        Dispatcher.UIThread.RunJobs();
        using WriteableBitmap frame = window.CaptureRenderedFrame()
            ?? throw new InvalidOperationException("The source tree icon frame is unavailable.");
        ReadPixel(ReadPixels(frame), 0).Should().Be(expected);
    }

    private static void AssertConsumerPixel(Window window, Image image, Color backdrop)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Point point = image.TranslatePoint(default, window)!.Value;
        using WriteableBitmap frame = window.CaptureRenderedFrame()
            ?? throw new InvalidOperationException("The source tree icon frame is unavailable.");
        int index = (int)point.X + ((int)point.Y * frame.PixelSize.Width);
        ReadPixel(ReadPixels(frame), index).Should().Be(Composite(Color.FromArgb(77, 52, 52, 52), backdrop));
    }

    private static WriteableBitmap CreateRamp(Color color, AlphaFormat alphaFormat)
    {
        WriteableBitmap bitmap = new(new PixelSize(16, 16), new Vector(96, 96), PixelFormat.Bgra8888, alphaFormat);
        using ILockedFramebuffer buffer = bitmap.Lock();
        for (int y = 0; y < 16; y++)
        {
            byte[] row = new byte[16 * 4];
            for (int x = 0; x < 16; x++)
            {
                int alpha = x + (y * 16);
                int offset = x * 4;
                row[offset] = alphaFormat == AlphaFormat.Premul ? (byte)(((color.B * alpha) + 127) / 255) : color.B;
                row[offset + 1] = alphaFormat == AlphaFormat.Premul ? (byte)(((color.G * alpha) + 127) / 255) : color.G;
                row[offset + 2] = alphaFormat == AlphaFormat.Premul ? (byte)(((color.R * alpha) + 127) / 255) : color.R;
                row[offset + 3] = (byte)alpha;
            }

            Marshal.Copy(row, 0, buffer.Address + (y * buffer.RowBytes), row.Length);
        }

        return bitmap;
    }

    private static RenderTargetBitmap Render(IImage image, Color? backdrop)
    {
        RenderTargetBitmap bitmap = new(new PixelSize(16, 16), new Vector(96, 96));
        using DrawingContext context = bitmap.CreateDrawingContext();
        if (backdrop is Color color)
        {
            context.FillRectangle(new SolidColorBrush(color), new Rect(0, 0, 16, 16));
        }

        context.DrawImage(image, new Rect(0, 0, 16, 16));
        return bitmap;
    }

    private static byte[] ReadPixels(Bitmap bitmap)
    {
        using WriteableBitmap copy = new(bitmap.PixelSize, bitmap.Dpi, PixelFormat.Bgra8888, AlphaFormat.Premul);
        using ILockedFramebuffer buffer = copy.Lock();
        bitmap.CopyPixels(buffer);
        byte[] pixels = new byte[bitmap.PixelSize.Width * bitmap.PixelSize.Height * 4];
        int rowBytes = bitmap.PixelSize.Width * 4;
        for (int y = 0; y < bitmap.PixelSize.Height; y++)
        {
            Marshal.Copy(buffer.Address + (y * buffer.RowBytes), pixels, y * rowBytes, rowBytes);
        }

        return pixels;
    }

    private static Color ReadPixel(byte[] pixels, int index)
        => Color.FromArgb(pixels[(index * 4) + 3], pixels[(index * 4) + 2], pixels[(index * 4) + 1], pixels[index * 4]);

    private static Color Composite(Color premultiplied, Color backdrop)
        => Color.FromRgb(
            (byte)(premultiplied.R + (((backdrop.R * (255 - premultiplied.A)) + 127) / 255)),
            (byte)(premultiplied.G + (((backdrop.G * (255 - premultiplied.A)) + 127) / 255)),
            (byte)(premultiplied.B + (((backdrop.B * (255 - premultiplied.A)) + 127) / 255)));

    private static Color ParseArgb(string argb)
    {
        uint value = uint.Parse(argb, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return Color.FromArgb((byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value);
    }

    // These channels are independent native IMAGEINFO DIB readback for all 256 alpha values,
    // not the output of the portable conversion. ImageList_Draw and actual original TREEVIEW
    // PrintWindow separately corroborated every pixel over all five opaque backdrops.
    private static string GetNativePremultipliedChannels(string asset)
        => asset switch
        {
            "synthetic-black" =>
                "0000000101010202020202020303030404040505050606060606060707070808080909090909090A0A0A0B0B0B0C0C0C" +
                "0C0C0C0D0D0D0E0E0E0F0F0F0F0F0F101010111111111111121212131313131313141414151515151515161616161616" +
                "1717171818181818181919191A1A1A1A1A1A1B1B1B1B1B1B1C1C1C1C1C1C1D1D1D1E1E1E1E1E1E1F1F1F1F1F1F202020" +
                "202020212121212121222222222222232323232323242424242424252525252525252525262626272727272727272727" +
                "2828282828282828282929292929292A2A2A2A2A2A2A2A2A2B2B2B2B2B2B2C2C2C2C2C2C2C2C2C2C2C2C2D2D2D2D2D2D" +
                "2D2D2D2E2E2E2E2E2E2E2E2E2E2E2E2F2F2F2F2F2F2F2F2F303030303030303030313131313131313131313131313131" +
                "323232323232323232323232323232323232333333333333333333333333333333333333343434343434343434343434" +
                "343434343434343434343434343434343434343434353535353535353535353535353535353535353535353535353535" +
                "353535353535353535353535353535353535353535343434343434353535343434343434343434343434343434343434" +
                "343434343434343434333333343434333333333333333333333333323232333333323232323232323232323232313131" +
                "3232323131313131313131313030303030303030303030302F2F2F2F2F2F2F2F2F2F2F2F2F2F2F2E2E2E2E2E2E2D2D2D" +
                "2D2D2D2D2D2D2D2D2D2C2C2C2C2C2C2B2B2B2B2B2B2B2B2B2B2B2B2A2A2A2A2A2A292929292929292929282828282828" +
                "272727272727262626262626262626252525242424242424242424232323232323222222222222212121212121202020" +
                "2020201F1F1F1E1E1E1E1E1E1E1E1E1D1D1D1D1D1D1C1C1C1B1B1B1A1A1A1B1B1B1A1A1A191919181818181818171717" +
                "1717171616161515151414141414141414141313131212121111111010101010101010100F0F0F0E0E0E0D0D0D0C0C0C" +
                "0B0B0B0B0B0B0A0A0A0A0A0A090909080808070707070707060606050505040404030303020202020202010101000000",
            "synthetic-white" =>
                "0000000101010202020202020303030404040505050606060707070808080808080909090A0A0A0B0B0B0C0C0C0D0D0D" +
                "0D0D0D0E0E0E0F0F0F1010101111111212121313131313131414141515151616161717171818181919191919191A1A1A" +
                "1B1B1B1C1C1C1D1D1D1E1E1E1F1F1F1F1F1F202020212121222222232323242424252525262626272727282828282828" +
                "2929292A2A2A2B2B2B2C2C2C2D2D2D2E2E2E2F2F2F2F2F2F313131313131323232333333343434353535363636373737" +
                "3838383939393939393B3B3B3B3B3B3C3C3C3D3D3D3E3E3E3F3F3F404040414141424242434343444444454545464646" +
                "4747474747474848484949494A4A4A4B4B4B4C4C4C4D4D4D4E4E4E4F4F4F505050515151525252535353545454555555" +
                "5656565757575858585959595959595A5A5A5C5C5C5D5D5D5D5D5D5E5E5E5F5F5F606060616161626262636363646464" +
                "6565656666666767676868686969696A6A6A6B6B6B6C6C6C6D6D6D6E6E6E6F6F6F707070717171727272737373747474" +
                "7575757575757676767878787979797A7A7A7A7A7A7B7B7B7D7D7D7E7E7E7F7F7F808080808080818181838383848484" +
                "8585858686868787878787878989898A8A8A8B8B8B8C8C8C8D8D8D8E8E8E8F8F8F909090919191929292939393949494" +
                "9696969696969797979898989999999A9A9A9C9C9C9D9D9D9D9D9D9E9E9EA0A0A0A1A1A1A2A2A2A3A3A3A4A4A4A5A5A5" +
                "A6A6A6A7A7A7A8A8A8A9A9A9AAAAAAABABABACACACAEAEAEAFAFAFB0B0B0B1B1B1B1B1B1B2B2B2B4B4B4B5B5B5B6B6B6" +
                "B8B8B8B8B8B8B9B9B9BBBBBBBCBCBCBDBDBDBDBDBDBEBEBEC0C0C0C1C1C1C2C2C2C3C3C3C5C5C5C5C5C5C7C7C7C8C8C8" +
                "C9C9C9CACACACBCBCBCCCCCCCDCDCDCECECECFCFCFD0D0D0D1D1D1D2D2D2D4D4D4D5D5D5D6D6D6D7D7D7D8D8D8D9D9D9" +
                "DBDBDBDCDCDCDDDDDDDEDEDEDFDFDFE1E1E1E1E1E1E2E2E2E3E3E3E4E4E4E5E5E5E7E7E7E8E8E8E9E9E9EAEAEAEBEBEB" +
                "EDEDEDEEEEEEEFEFEFF0F0F0F1F1F1F2F2F2F3F3F3F5F5F5F6F6F6F7F7F7F8F8F8F9F9F9FAFAFAFCFCFCFDFDFDFFFFFF",
            "synthetic-gray" =>
                "0000000101010202020202020303030404040505050606060606060707070808080909090A0A0A0A0A0A0B0B0B0C0C0C" +
                "0D0D0D0E0E0E0E0E0E0F0F0F101010111111111111121212131313131313141414151515161616161616171717181818" +
                "1919191919191A1A1A1B1B1B1B1B1B1C1C1C1D1D1D1D1D1D1E1E1E1F1F1F1F1F1F202020212121212121222222232323" +
                "2323232424242525252525252626262626262727272727272828282929292929292A2A2A2B2B2B2B2B2B2C2C2C2C2C2C" +
                "2D2D2D2E2E2E2E2E2E2F2F2F2F2F2F303030303030313131313131323232333333333333343434343434353535353535" +
                "3636363636363737373737373838383838383939393939393A3A3A3A3A3A3B3B3B3B3B3B3C3C3C3C3C3C3C3C3C3D3D3D" +
                "3E3E3E3E3E3E3F3F3F3F3F3F3F3F3F404040404040414141414141414141424242424242434343434343444444444444" +
                "4444444545454545454545454646464646464646464747474848484848484848484848484848484A4A4A4A4A4A4A4A4A" +
                "4A4A4A4A4A4A4A4A4A4C4C4C4B4B4B4C4C4C4C4C4C4C4C4C4C4C4C4D4D4D4D4D4D4D4D4D4E4E4E4D4D4D4F4F4F4F4F4F" +
                "4E4E4E4F4F4F5050504F4F4F505050515151505050515151515151505050525252525252515151525252525252525252" +
                "535353535353535353535353545454535353545454545454545454545454555555545454555555555555555555555555" +
                "565656555555565656565656555555565656565656565656575757575757565656575757575757575757575757585858" +
                "575757575757575757575757585858585858575757575757595959575757585858585858585858585858595959585858" +
                "585858595959575757585858595959585858575757595959575757575757595959585858575757585858575757575757" +
                "595959575757575757585858575757575757585858575757565656585858565656575757585858565656565656575757" +
                "575757565656575757565656555555565656555555555555575757555555545454565656545454545454565656555555",
            "synthetic-color" =>
                "0000000101010202020202020303030404040505050606060606070707070808080909090A0A0A0A0A0B0B0B0B0C0C0C" +
                "0D0D0D0D0E0E0E0E0F0F0F10101010101111111112121213121313131414141415151516151617161718171718171819" +
                "18191A19191B191A1B1A1B1C1B1C1D1B1C1E1C1D1F1D1E1F1D1E201E1F211E20221F2022202123202224212325222326" +
                "22242623252723252824262925272925272A25282B26282C27292D272A2D282A2E282B2F292C302A2C302A2D312A2D32" +
                "2B2E322C2F332C2F342D30352D31362D31362E32372F32382F333830343A30343A31353B31353C31363C32363D32373E" +
                "33383E33383F333940343940343A41353A42353B43363B44363C44363C45373D46373E47383E47383F48383F49393F49" +
                "39404A39414B3A414B3A424C3A424D3B434E3B434E3B444F3C44503C44503C45513C45523D46533E47533E47543E4754" +
                "3E48553E48563F49573F49573F4A583F4A59404B59404B5B404B5B414C5C414C5C414D5D414D5D424D5F424E5F424E60" +
                "424F60424F6142506143506343506343516443516443526543526544526744536744536844536844536945546A45556A" +
                "45556B45556B45556C45556D46566E46566E45576F45576F465770465871455872465972465873465974465974455975" +
                "465A76465A76475A77465B78455B78465B78475B7A475C7A465C7B465C7B465D7B475D7C475D7D475E7E465D7E465E7F" +
                "465D7F475E80475F81465F81465F81465E83466083466083466085466085466085466186456187466187456288466189" +
                "45628944628A44618A46638B45628C45638D44638D44628C45648F45638F44648F436490436390446490446392436592" +
                "42649242649443649342639342659641649641659542659741649840659740659941659A41669940659A3F669B3E659A" +
                "40669C40669D3F659C3D669D3D659E3F669E3E679F3D65A03C679F3C65A03D67A23D67A23B66A23B67A43B66A33B67A3" +
                "3A67A53A66A63967A53A66A73967A73967A63765A83768AA3866A83868AA3767AB3565A93467AB3766AD3668AB3567AD",
            _ => throw new ArgumentOutOfRangeException(nameof(asset)),
        };
}
