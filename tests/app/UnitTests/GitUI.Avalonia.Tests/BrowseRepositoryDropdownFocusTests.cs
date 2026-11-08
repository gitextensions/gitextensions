using System.ComponentModel.Design;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.CommandsDialogs.Menus;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class BrowseRepositoryDropdownFocusTests
{
    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public async Task Refresh_should_preserve_an_open_repository_filter_and_otherwise_focus_revisions(bool dropdownOpen)
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        using AppSettingsTestScope settings = new();
        using ServiceContainer services = new();
        GitExtensions.ServiceContainerRegistry.RegisterServices(services);
        services.RemoveService(typeof(IUpdateCheckService));
        services.AddService<IUpdateCheckService>(Substitute.For<IUpdateCheckService>());
        string directory = Path.Combine(Path.GetTempPath(), $"GitExtensions.DropdownFocus-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        FormBrowse? form = null;
        try
        {
            GitModule module = new(services.GetRequiredService<IGitExecutorProvider>(), directory);
            module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" }).Should().BeTrue();
            module.SetSetting("user.name", "Avalonia Test");
            module.SetSetting("user.email", "avalonia@example.com");
            File.WriteAllText(Path.Combine(directory, "file.txt"), "content\n");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "file.txt" }).Should().BeTrue();
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
            form = new FormBrowse(new GitUICommands(services, module));
            form.Show();
            await WaitUntilAsync(() => form.RevisionGrid.SelectedRevision?.ObjectId == module.GetCurrentCheckout()
                && form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!.Text!.EndsWith(" revisions", StringComparison.Ordinal),
                () => $"Initial revision: selected={form.RevisionGrid.SelectedRevision?.ObjectId}, HEAD={module.GetCurrentCheckout()}, status={form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!.Text}");
            form.Width = form.ToolStripMain.PreferredSize.Width + form.ToolStripFilters.Strip.PreferredSize.Width + 100;
            Settle(form);
            WorkingDirectoryToolStripSplitButton selector = form.FindControl<WorkingDirectoryToolStripSplitButton>("_NO_TRANSLATE_WorkingDir")!;
            WorkingDirectoryToolStripSplitButton.TestAccessor popup = selector.GetTestAccessor();
            ListBox revisions = form.RevisionGrid.GetTestAccessor().Revisions;
            if (dropdownOpen)
            {
                Button primary = selector.GetVisualDescendants().OfType<Button>().Single(button => button.Name == "PART_PrimaryButton");
                Click(form, primary);
                Settle(form);
                popup.Menu.IsOpen.Should().BeTrue();
                TopLevel topLevel = TopLevel.GetTopLevel(popup.Filter)!;
                Click(topLevel, popup.Filter);
                topLevel.KeyTextInput("typed repository");
                popup.Filter.IsKeyboardFocusWithin.Should().BeTrue();
                popup.Filter.Text.Should().Be("typed repository");
            }
            else
            {
                form.ExecuteCommand(FormBrowse.Command.FocusFilter).Should().BeTrue();
                revisions.IsKeyboardFocusWithin.Should().BeFalse();
            }

            TaskCompletionSource reloaded = new(TaskCreationOptions.RunContinuationsAsynchronously);
            form.RevisionGrid.RevisionsLoaded += (_, _) => reloaded.TrySetResult();
            form.ExecuteCommand(FormBrowse.Command.Refresh).Should().BeTrue();
            await reloaded.Task.WaitAsync(TimeSpan.FromSeconds(15));
            using CancellationTokenSource deadline = new(TimeSpan.FromSeconds(15));
            await form.JoinLoadOperationsForTestAsync(deadline.Token);
            Settle(form);
            if (dropdownOpen)
            {
                popup.Menu.IsOpen.Should().BeTrue();
                popup.Filter.Text.Should().Be("typed repository");
                popup.Filter.IsKeyboardFocusWithin.Should().BeTrue();
                revisions.IsKeyboardFocusWithin.Should().BeFalse();
                popup.Menu.Hide();
            }
            else
            {
                revisions.IsKeyboardFocusWithin.Should().BeTrue();
            }
        }
        finally
        {
            form?.Close();
            (form as IDisposable)?.Dispose();
            TestDirectory.Delete(directory);
        }
    }

    private static void Click(TopLevel topLevel, Control control)
    {
        Point point = control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), topLevel)!.Value;
        topLevel.MouseDown(point, MouseButton.Left);
        topLevel.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    private static void Settle(Window form)
    {
        Dispatcher.UIThread.RunJobs();
        using var frame = form.CaptureRenderedFrame();
        frame.Should().NotBeNull();
        Dispatcher.UIThread.RunJobs();
    }

    private static async Task WaitUntilAsync(Func<bool> condition, Func<string> description)
    {
        using CancellationTokenSource deadline = new(TimeSpan.FromSeconds(15));
        while (!condition())
        {
            Dispatcher.UIThread.RunJobs();
            if (deadline.IsCancellationRequested)
            {
                throw new AssertionException(description());
            }

            await Task.Delay(10);
        }
    }
}
