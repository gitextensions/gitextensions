using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GitCommands;
using GitExtensions;
using GitUI.Compat;
using GitUI.NBugReports;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class ApplicationStartupTests
{
    [TestCase(true, "wayland-0", true)]
    [TestCase(true, null, false)]
    [TestCase(true, "", false)]
    [TestCase(false, "wayland-0", false)]
    [Category("P0_6")]
    public void ShouldUseWayland_should_require_a_linux_wayland_session(
        bool isLinux,
        string? waylandDisplay,
        bool expected)
    {
        Program.ShouldUseWayland(isLinux, waylandDisplay).Should().Be(expected);
    }

    [Test]
    public void GetGitExtensionsFullPath_should_accept_the_Avalonia_entry_point()
    {
        AppSettings.TestAccessor accessor = AppSettings.GetTestAccessor();
        string originalPath = accessor.ApplicationExecutablePath;
        string avaloniaPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "GitExtensions.Avalonia.exe");

        try
        {
            accessor.ApplicationExecutablePath = avaloniaPath;

            AppSettings.GetGitExtensionsFullPath().Should().Be(avaloniaPath);
        }
        finally
        {
            accessor.ApplicationExecutablePath = originalPath;
        }
    }

    [AvaloniaTest]
    public void Dispatcher_should_keep_the_window_usable_after_the_user_ignores_a_UI_exception()
    {
        const string failure = "Recoverable dispatcher failure";
        Func<WinFormsShims.IWin32Window?, TaskDialogPage, TaskDialogButton> previousPresenter = UIReporter.TestAccessor.TaskDialogPresenter;
        int reports = 0;
        UIReporter.TestAccessor.TaskDialogPresenter = (_, page) =>
        {
            reports++;
            page.Heading.Should().Be(failure);
            TaskDialogButton ignore = page.Buttons.Single(button => button.Text == GitUI.TranslatedStrings.ButtonIgnore);
            ignore.PerformClick();
            return ignore;
        };
        Button button = new() { Content = "Throw" };
        Window window = new() { Content = button };
        button.Click += (_, _) => throw new InvalidOperationException(failure);
        bool continued = false;
        try
        {
            window.Show();
            Dispatcher.UIThread.Post(() => button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)));
            Dispatcher.UIThread.Post(() => continued = true);
            Action dispatch = () => Dispatcher.UIThread.RunJobs();

            dispatch.Should().NotThrow();

            reports.Should().Be(1);
            continued.Should().BeTrue();
            window.IsVisible.Should().BeTrue();
            window.Title = "Still usable";
            window.Title.Should().Be("Still usable");
        }
        finally
        {
            window.Close();
            UIReporter.TestAccessor.TaskDialogPresenter = previousPresenter;
        }
    }

    [AvaloniaTest]
    public void Dispatcher_should_fail_a_headless_test_when_an_unexpected_UI_error_is_reported()
    {
        const string failure = "Unexpected dispatcher failure";
        Button button = new() { Content = "Throw" };
        Window window = new() { Content = button };
        button.Click += (_, _) => throw new InvalidOperationException(failure);
        try
        {
            window.Show();
            Dispatcher.UIThread.Post(() => button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)));
            Action dispatch = () => Dispatcher.UIThread.RunJobs();

            dispatch.Should().Throw<AssertionException>().WithMessage($"Unhandled UI error:*{failure}*");
        }
        finally
        {
            window.Close();
        }
    }
}
