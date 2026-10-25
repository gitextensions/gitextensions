using System.Drawing.Imaging;
using System.Text;
using GitExtUtils.GitUI.Theming;

namespace GitExtUtilsTests;

public class AdaptLightnessTests
{
    [SetUp]
    public void Setup()
    {
        // The lightness is only adapted for a theme which is not the default one
        Theme darkTheme = new(new Dictionary<AppColor, Color>(), new Dictionary<KnownColor, Color>(), ThemeId.DefaultDark);
        ColorHelper.ThemeSettings = new ThemeSettings(darkTheme, Theme.Default, ThemeVariations.None, useSystemVisualStyle: false);
    }

    [TearDown]
    public void TearDown()
    {
        ColorHelper.ThemeSettings = ThemeSettings.Default;
    }

    [Test]
    public void AdaptLightness_should_transform_a_bitmap_backed_by_a_closed_stream()
    {
        // Icon.ToBitmap() returns such a bitmap for a PNG compressed icon frame, which is what a
        // user script icon typically is
        using Icon icon = CreateIconWithPngFrame(size: 40);
        using Bitmap streamBacked = icon.ToBitmap();

        Bitmap? adapted = null;

        try
        {
            ((Action)(() => adapted = streamBacked.AdaptLightness())).Should()
                .NotThrow("the transformation must not depend on the bitmap still having its stream");

            adapted!.Size.Should().Be(streamBacked.Size);
            adapted.PixelFormat.Should().Be(PixelFormat.Format32bppArgb);
        }
        finally
        {
            adapted?.Dispose();
        }
    }

    [Test]
    public void AdaptLightness_should_leave_the_original_untouched()
    {
        using Bitmap original = new(4, 4, PixelFormat.Format32bppArgb);
        original.SetPixel(0, 0, Color.FromArgb(255, 30, 90, 150));

        using Bitmap adapted = original.AdaptLightness();

        original.GetPixel(0, 0).Should().Be(Color.FromArgb(255, 30, 90, 150));
        adapted.Should().NotBeSameAs(original);
    }

    private static Icon CreateIconWithPngFrame(int size)
    {
        byte[] png;
        using (Bitmap frame = new(size, size, PixelFormat.Format32bppArgb))
        {
            using (Graphics graphics = Graphics.FromImage(frame))
            {
                graphics.Clear(Color.FromArgb(255, 30, 90, 150));
            }

            using MemoryStream stream = new();
            frame.Save(stream, ImageFormat.Png);
            png = stream.ToArray();
        }

        MemoryStream ico = new();
        const int headerLength = 6 + 16;
        using (BinaryWriter writer = new(ico, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write((short)0); // reserved
            writer.Write((short)1); // an icon, not a cursor
            writer.Write((short)1); // a single image
            writer.Write((byte)size);
            writer.Write((byte)size);
            writer.Write((byte)0); // no palette
            writer.Write((byte)0); // reserved
            writer.Write((short)1); // colour planes
            writer.Write((short)32); // bits per pixel
            writer.Write(png.Length);
            writer.Write(headerLength);
            writer.Write(png);
        }

        ico.Position = 0;
        return new Icon(ico);
    }
}
