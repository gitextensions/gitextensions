using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Headless;
using GitExtensionsTests;
using GitUI.NBugReports;
using NSubstitute;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace GitExtensionsTests;

public static class TestAppBuilder
{
    internal static bool UseDesktopLifetime { get; set; }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<HeadlessApplication>()
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

    private sealed class HeadlessApplication : GitExtensions.App
    {
        public override void RegisterServices()
        {
            // Desktop clipboard integration needs its external lifetime before framework initialization makes it immutable.
            if (UseDesktopLifetime)
            {
                ApplicationLifetime = Substitute.For<IClassicDesktopStyleApplicationLifetime>();
            }

            base.RegisterServices();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            // Clipboard integration exercises the production window commands without starting an interactive desktop lifetime.
            if (!UseDesktopLifetime)
            {
                base.OnFrameworkInitializationCompleted();
            }
        }
    }
}
