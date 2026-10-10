using Avalonia;
using Avalonia.Headless;
using GitExtensionsTests;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace GitExtensionsTests;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<GitExtensions.App>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions
            {
                // Render real frames with Skia so tests can capture and compare pixels
                // (golden-image tests); the default headless drawing produces no pixels.
                UseHeadlessDrawing = false,
            });
}
