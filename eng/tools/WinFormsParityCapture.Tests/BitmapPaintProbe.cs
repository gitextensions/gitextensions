using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using AwesomeAssertions;
using GitUI.Properties;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

/// <summary>
///  Retains original-consumer bitmap diagnostics without constructing any controls.
/// </summary>
internal static class BitmapPaintProbe
{
    /// <summary>
    ///  Creates a unique directory owned by this consumer test execution.
    /// </summary>
    internal static string CreateDirectory()
    {
        string directory = Path.Combine(TestContext.CurrentContext.TestDirectory,
            "MainWindowMenuPaintProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        TestContext.Out.WriteLine($"nativeBitmapPaintEvidence={directory}");
        return directory;
    }

    /// <summary>
    ///  Clones an original embedded image so the consumer owns its disposal.
    /// </summary>
    internal static Bitmap ReadAsset(string asset)
    {
        Image image = typeof(Images).GetProperty(asset, BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as Image
            ?? throw new MissingMemberException(typeof(Images).FullName, asset);
        return image.Clone() as Bitmap
            ?? throw new InvalidOperationException("The original marker/menu image must retain bitmap pixels without being redrawn.");
    }

    /// <summary>
    ///  Uses the actual default bitmap context and rejects a non-native baseline.
    /// </summary>
    internal static Bitmap CreateBitmap(Size size, Color backdrop)
    {
        Bitmap bitmap = new(size.Width, size.Height, PixelFormat.Format32bppArgb);
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.DpiX.Should().Be(96, "the consumer must measure the native bitmap context, not manufacture it by resampling");
        graphics.DpiY.Should().Be(96);
        graphics.Clear(backdrop);
        return bitmap;
    }

    /// <summary>
    ///  Supplies only the data read by the original static image painter.
    /// </summary>
    internal static DataGridViewCellPaintingEventArgs CreateCellPaintArguments(Graphics graphics, Rectangle cellBounds)
    {
        // The public constructor requires a real DataGridView. The exact painter
        // reads only these properties; do not create a Control just to satisfy it.
        DataGridViewCellPaintingEventArgs arguments =
            (DataGridViewCellPaintingEventArgs)RuntimeHelpers.GetUninitializedObject(typeof(DataGridViewCellPaintingEventArgs));
        SetArgumentProperty(arguments, nameof(DataGridViewCellPaintingEventArgs.Graphics), graphics);
        SetArgumentProperty(arguments, nameof(DataGridViewCellPaintingEventArgs.CellBounds), cellBounds);
        SetArgumentProperty(arguments, nameof(DataGridViewCellPaintingEventArgs.ClipBounds), cellBounds);
        return arguments;
    }

    /// <summary>
    ///  Retains resolved ARGB pixels instead of symbolic system-color names.
    /// </summary>
    internal static string[][] Pixels(Bitmap bitmap) => Enumerable.Range(0, bitmap.Height)
        .Select(y => Enumerable.Range(0, bitmap.Width).Select(x => Argb(bitmap.GetPixel(x, y))).ToArray()).ToArray();

    /// <summary>
    ///  Returns the resolved color value, including alpha.
    /// </summary>
    internal static string Argb(Color color) => $"#{color.ToArgb():X8}";

    /// <summary>
    ///  Describes the actually painted bounds independently of image layout metadata.
    /// </summary>
    internal static object Ink(Bitmap bitmap, Color backdrop)
    {
        Rectangle bounds = Rectangle.Empty;
        int pixels = 0;
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, y).ToArgb() != backdrop.ToArgb())
                {
                    Rectangle pixel = new(x, y, 1, 1);
                    bounds = bounds.IsEmpty ? pixel : Rectangle.Union(bounds, pixel);
                    pixels++;
                }
            }
        }

        return new { bounds.X, bounds.Y, bounds.Width, bounds.Height, pixels };
    }

    /// <summary>
    ///  Hashes the decoded ARGB values without depending on PNG encoding metadata.
    /// </summary>
    internal static string PixelHash(Bitmap bitmap)
    {
        byte[] pixels = new byte[bitmap.Width * bitmap.Height * sizeof(int)];
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                BitConverter.GetBytes(bitmap.GetPixel(x, y).ToArgb()).CopyTo(pixels, ((y * bitmap.Width) + x) * sizeof(int));
            }
        }

        return Convert.ToHexString(SHA256.HashData(pixels));
    }

    /// <summary>
    ///  Requires exact resolved pixels from the original consumer and independent drawing formula.
    /// </summary>
    internal static void AssertPixels(Bitmap actual, Bitmap expected)
    {
        actual.Size.Should().Be(expected.Size);
        List<string> differences = [];
        for (int y = 0; y < actual.Height; y++)
        {
            for (int x = 0; x < actual.Width; x++)
            {
                if (actual.GetPixel(x, y).ToArgb() != expected.GetPixel(x, y).ToArgb())
                {
                    differences.Add($"({x},{y}) {Argb(actual.GetPixel(x, y))} != {Argb(expected.GetPixel(x, y))}");
                }
            }
        }

        differences.Should().BeEmpty();
    }

    /// <summary>
    ///  Retains diagnostics before the test's final assertions can fail.
    /// </summary>
    internal static void WriteReport(string directory, object report)
    {
        string json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(directory, "probe.json"), json);
    }

    private static void SetArgumentProperty(DataGridViewCellPaintingEventArgs arguments, string name, object value)
    {
        PropertyInfo property = typeof(DataGridViewCellPaintingEventArgs).GetProperty(name, BindingFlags.Public | BindingFlags.Instance)
            ?? throw new MissingMemberException(typeof(DataGridViewCellPaintingEventArgs).FullName, name);
        property.SetValue(arguments, value);
    }
}
