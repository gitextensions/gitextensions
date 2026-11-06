using Avalonia;
using Avalonia.Headless;
using GitExtensionsTests;
using GitUI.NBugReports;

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
            })
            .AfterSetup(_ =>
            {
                // No user can dismiss an unexpected report in a headless test; retain it as a test failure.
                UIReporter.TestAccessor.TaskDialogPresenter = (owner, page) =>
                    throw new AssertionException($"Unhandled UI error: {page.Heading}{Environment.NewLine}{page.Text}");
            });
}
