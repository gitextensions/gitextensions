using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using SkiaSharp;

namespace GitExtensionsTests;

internal static class GoldenImageVerifier
{
    // Per-channel difference below this is ignored (anti-aliasing variance between
    // Skia builds); a small share of pixels may exceed it before the test fails.
    private const int ChannelTolerance = 8;
    private const double MaxBadPixelShare = 0.002;

    public static void Verify(Control content, double width, double height, string goldenName)
    {
        Window window = new()
        {
            Width = width,
            Height = height,
            SizeToContent = SizeToContent.Manual,
            Content = content,
        };
        window.Show();

        try
        {
            Dispatcher.UIThread.RunJobs();
            WriteableBitmap? frame = window.CaptureRenderedFrame();
            frame.Should().NotBeNull("headless Skia rendering should produce a frame");

            using MemoryStream actualStream = new();
            frame!.Save(actualStream, PngBitmapEncoderOptions.Default);
            byte[] actualPng = actualStream.ToArray();
            string goldenPath = Path.Combine(
                GetGoldenDirectory(),
                $"{goldenName}.{GetPlatformName()}.png");

            if (Environment.GetEnvironmentVariable("GITEXT_UPDATE_GOLDEN_IMAGES") == "1")
            {
                Directory.CreateDirectory(Path.GetDirectoryName(goldenPath)!);
                File.WriteAllBytes(goldenPath, actualPng);
                Assert.Inconclusive($"Golden image regenerated at {goldenPath}; review and commit it, then rerun without GITEXT_UPDATE_GOLDEN_IMAGES.");
            }

            if (!File.Exists(goldenPath))
            {
                Assert.Fail($"Golden image {goldenPath} is missing; run this test with GITEXT_UPDATE_GOLDEN_IMAGES=1 to create it.");
            }

            AssertImagesMatch(File.ReadAllBytes(goldenPath), actualPng, goldenName);
        }
        finally
        {
            window.Close();
        }
    }

    private static void AssertImagesMatch(byte[] goldenPng, byte[] actualPng, string goldenName)
    {
        using SKBitmap golden = SKBitmap.Decode(goldenPng);
        using SKBitmap actual = SKBitmap.Decode(actualPng);

        if (golden.Width != actual.Width || golden.Height != actual.Height)
        {
            Assert.Fail($"Rendered image is {actual.Width}x{actual.Height}, the golden image {goldenName} is {golden.Width}x{golden.Height}. {SaveActual(actualPng, goldenName)}");
        }

        int badPixels = 0;
        for (int y = 0; y < golden.Height; y++)
        {
            for (int x = 0; x < golden.Width; x++)
            {
                SKColor expected = golden.GetPixel(x, y);
                SKColor got = actual.GetPixel(x, y);
                if (Math.Abs(expected.Red - got.Red) > ChannelTolerance
                    || Math.Abs(expected.Green - got.Green) > ChannelTolerance
                    || Math.Abs(expected.Blue - got.Blue) > ChannelTolerance
                    || Math.Abs(expected.Alpha - got.Alpha) > ChannelTolerance)
                {
                    badPixels++;
                }
            }
        }

        int maxBadPixels = (int)(golden.Width * golden.Height * MaxBadPixelShare);
        if (badPixels > maxBadPixels)
        {
            Assert.Fail($"{badPixels} pixels differ from the golden image {goldenName} (allowed: {maxBadPixels}). {SaveActual(actualPng, goldenName)} If the rendering change is intended, regenerate with GITEXT_UPDATE_GOLDEN_IMAGES=1.");
        }
    }

    private static string SaveActual(byte[] actualPng, string goldenName)
    {
        string path = Path.Combine(TestContext.CurrentContext.WorkDirectory, goldenName + ".actual.png");
        File.WriteAllBytes(path, actualPng);
        return $"Actual image saved to {path}.";
    }

    private static string GetGoldenDirectory([CallerFilePath] string thisFilePath = "")
        => Path.Combine(Path.GetDirectoryName(thisFilePath)!, "GoldenImages");

    private static string GetPlatformName()
        => OperatingSystem.IsWindows()
            ? "windows"
            : OperatingSystem.IsLinux()
                ? "linux"
                : "macos";
}
