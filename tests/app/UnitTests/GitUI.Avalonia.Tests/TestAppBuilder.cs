using Avalonia;
using Avalonia.Headless;
using GitExtensionsTests;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace GitExtensionsTests;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<GitExtensions.App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
