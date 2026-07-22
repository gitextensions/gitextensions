using WinFormsTaskDialogs = GitExtensions.Shims.WinForms.TaskDialogs;

namespace GitUI.Avalonia.Tests;

[TestFixture]
public sealed class ShimTaskDialogTests
{
    [Test]
    public async Task Task_dialog_should_return_the_host_selection_and_raise_its_click_event()
    {
        WinFormsTaskDialogs.TaskDialogButton ignored = new("Ignore");
        WinFormsTaskDialogs.TaskDialogButton openSettings = new("Open settings");
        bool clicked = false;
        openSettings.Click += (_, _) => clicked = true;
        WinFormsTaskDialogs.TaskDialogPage page = new()
        {
            Heading = "Authentication failed",
            Caption = "Build server integration",
            Icon = WinFormsTaskDialogs.TaskDialogIcon.Error,
            AllowCancel = true,
            Buttons = { openSettings, ignored },
        };
        StubTaskDialogHost host = new(openSettings);
        GitExtensions.Shims.WinForms.ShimHost.TaskDialogHost = host;

        WinFormsTaskDialogs.TaskDialogButton result = await WinFormsTaskDialogs.TaskDialog.ShowDialogAsync(page);

        result.Should().BeSameAs(openSettings);
        clicked.Should().BeTrue();
        host.Page.Should().BeSameAs(page);
    }

    private sealed class StubTaskDialogHost(WinFormsTaskDialogs.TaskDialogButton result) : WinFormsTaskDialogs.ITaskDialogHost
    {
        public WinFormsTaskDialogs.TaskDialogPage? Page { get; private set; }

        public Task<WinFormsTaskDialogs.TaskDialogButton> ShowDialogAsync(WinFormsTaskDialogs.TaskDialogPage page)
        {
            Page = page;
            return Task.FromResult(result);
        }
    }
}
