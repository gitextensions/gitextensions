using System.ComponentModel.Design;
using System.Diagnostics;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Platform;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class FileRevealTests
{
    private bool _previousDesktopLifetime;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _previousDesktopLifetime = TestAppBuilder.UseDesktopLifetime;
        TestAppBuilder.UseDesktopLifetime = true;
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() => TestAppBuilder.UseDesktopLifetime = _previousDesktopLifetime;

    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(false, false)]
    public async Task Native_reveal_should_select_the_complete_path_and_report_the_launch_result(bool windows, bool accepted)
    {
        Func<ProcessStartInfo, bool, Task<bool>> previous = NativeFileReveal.TestAccessor.StartProcess;
        string target = Path.Combine(Path.GetTempPath(), "parent folder", "chosen, file ž.txt");
        ProcessStartInfo? request = null;
        bool? wait = null;
        try
        {
            NativeFileReveal.TestAccessor.StartProcess = (info, waitForExit) =>
            {
                request = info;
                wait = waitForExit;
                return Task.FromResult(accepted);
            };

            bool result = windows
                ? await NativeFileReveal.RevealOnWindowsAsync(target)
                : await NativeFileReveal.RevealOnMacOSAsync(target);

            result.Should().Be(accepted);
            request.Should().NotBeNull();
            request!.UseShellExecute.Should().BeFalse();
            wait.Should().Be(!windows);
            if (windows)
            {
                request.FileName.Should().Be("explorer.exe");
                request.Arguments.Should().Be($"/select, {target.Quote()}");
                request.ArgumentList.Should().BeEmpty();
            }
            else
            {
                request.FileName.Should().Be("/usr/bin/open");
                request.ArgumentList.Should().Equal("-R", target);
            }
        }
        finally
        {
            NativeFileReveal.TestAccessor.StartProcess = previous;
        }
    }

    [AvaloniaTest]
    [TestCase("openFolderToolStripMenuItem", WinFormsShims.OsShellLaunchKind.ShowInDirectory)]
    [TestCase("openWithToolStripMenuItem", WinFormsShims.OsShellLaunchKind.OpenAs)]
    public async Task Conflict_actions_should_pass_the_real_selected_path_to_the_installed_shell(string actionName, WinFormsShims.OsShellLaunchKind kind)
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        using AppSettingsTestScope settings = new();
        using ServiceContainer services = new();
        GitExtensions.ServiceContainerRegistry.RegisterServices(services);
        string directory = Path.Combine(Path.GetTempPath(), $"GitExtensions.RevealConflict-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        string name = "chosen file ž.txt";
        string target = Path.Combine(directory, name);
        FieldInfo[] fields = typeof(WinFormsShims.ShimHost).GetFields(BindingFlags.Static | BindingFlags.NonPublic);
        object?[] previousServices = fields.Select(field => field.GetValue(null)).ToArray();
        var previousExecutable = OsShellUtil.TestAccessor.MockExecutable;
        FormResolveConflicts? form = null;
        try
        {
            OsShellUtil.TestAccessor.MockExecutable = null;
            GitModule module = new(services.GetRequiredService<IGitExecutorProvider>(), directory);
            module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "--initial-branch=main" }).Should().BeTrue();
            module.SetSetting("user.name", "Avalonia Test");
            module.SetSetting("user.email", "avalonia@example.com");
            Commit("base\n", "initial");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("checkout") { "--quiet", "-b", "side" }).Should().BeTrue();
            Commit("side\n", "side");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("checkout") { "--quiet", "main" }).Should().BeTrue();
            Commit("main\n", "main");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("merge") { "--no-edit", "side" }, throwOnErrorExit: false).Should().BeFalse();
            IClassicDesktopStyleApplicationLifetime desktop = (IClassicDesktopStyleApplicationLifetime)Avalonia.Application.Current!.ApplicationLifetime!;
            IXdgDesktopPortal portal = Substitute.For<IXdgDesktopPortal>();
            bool? popupAtLaunch = null;
            portal.TryLaunchAsync(target, kind).Returns(_ =>
            {
                popupAtLaunch = form?.ConflictedFiles.ContextMenu?.IsOpen;
                return true;
            });
            ShimServices.Install(desktop, portal, () => true);
            WinFormsShims.ShimHost.MessageBoxHost = Substitute.For<WinFormsShims.IMessageBoxHost>();
            GitUICommands commands = new(services, module);
            form = new FormResolveConflicts(commands, offerCommit: false);
            form.Show();
            Dispatcher.UIThread.RunJobs();
            form.ConflictedFiles.ItemCount.Should().Be(1);
            form.ConflictedFiles.SelectedIndex.Should().Be(0);
            ContextMenu menu = form.ConflictedFiles.ContextMenu!;
            menu.Open(form.ConflictedFiles);
            Dispatcher.UIThread.RunJobs();
            MenuItem action = form.FindControl<MenuItem>(actionName)!;
            action.IsEnabled.Should().BeTrue();

            action.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            Dispatcher.UIThread.RunJobs();

            await portal.Received(1).TryLaunchAsync(target, kind);
            if (kind == WinFormsShims.OsShellLaunchKind.OpenAs)
            {
                popupAtLaunch.Should().BeFalse("the external chooser must not run behind its still-open menu");
            }

            portal.ReceivedCalls().Where(call => call.GetMethodInfo().Name == nameof(IXdgDesktopPortal.TryLaunchAsync))
                .Should().ContainSingle();
            return;

            void Commit(string content, string message)
            {
                File.WriteAllText(target, content);
                module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", name.Quote() }).Should().BeTrue();
                module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", message }).Should().BeTrue();
            }
        }
        finally
        {
            form?.Close();
            (form as IDisposable)?.Dispose();
            using CancellationTokenSource deadline = new(TimeSpan.FromSeconds(15));
            await ThreadHelper.JoinPendingOperationsAsync(deadline.Token);
            deadline.Token.ThrowIfCancellationRequested();
            OsShellUtil.TestAccessor.MockExecutable = previousExecutable;
            for (int index = 0; index < fields.Length; index++)
            {
                fields[index].SetValue(null, previousServices[index]);
            }

            TestDirectory.Delete(directory);
        }
    }

    [AvaloniaTest]
    public async Task File_context_reveal_should_select_the_production_path_through_the_installed_shell()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        using AppSettingsTestScope settings = new();
        using ServiceContainer services = new();
        GitExtensions.ServiceContainerRegistry.RegisterServices(services);
        services.RemoveService(typeof(IUpdateCheckService));
        services.AddService<IUpdateCheckService>(Substitute.For<IUpdateCheckService>());
        string directory = Path.Combine(Path.GetTempPath(), $"GitExtensions.OpenWith-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        string name = "chosen file ž.txt";
        string target = Path.Combine(directory, name);
        IClassicDesktopStyleApplicationLifetime desktop = (IClassicDesktopStyleApplicationLifetime)Avalonia.Application.Current!.ApplicationLifetime!;
        FieldInfo[] fields = typeof(WinFormsShims.ShimHost).GetFields(BindingFlags.Static | BindingFlags.NonPublic);
        object?[] previousServices = fields.Select(field => field.GetValue(null)).ToArray();
        Func<ProcessStartInfo, bool, Task<bool>> previousReveal = NativeFileReveal.TestAccessor.StartProcess;
        var previousExecutable = OsShellUtil.TestAccessor.MockExecutable;
        FormBrowse? browse = null;
        try
        {
            OsShellUtil.TestAccessor.MockExecutable = null;
            GitModule module = new(services.GetRequiredService<IGitExecutorProvider>(), directory);
            module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" }).Should().BeTrue();
            module.SetSetting("user.name", "Avalonia Test");
            module.SetSetting("user.email", "avalonia@example.com");
            File.WriteAllText(target, "content\n");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", name.Quote() }).Should().BeTrue();
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
            IXdgDesktopPortal portal = Substitute.For<IXdgDesktopPortal>();
            portal.TryLaunchAsync(Arg.Any<string>(), WinFormsShims.OsShellLaunchKind.ShowInDirectory).Returns(true);
            ShimServices.Install(desktop, portal);
            WinFormsShims.IMessageBoxHost messages = Substitute.For<WinFormsShims.IMessageBoxHost>();
            WinFormsShims.ShimHost.MessageBoxHost = messages;
            IStorageProvider storage = Substitute.For<IStorageProvider>();
            storage.TryGetFileFromPathAsync(Arg.Any<Uri>()).Returns(Task.FromResult<IStorageFile?>(null));
            IStorageProviderFactory factory = Substitute.For<IStorageProviderFactory>();
            factory.CreateProvider(Arg.Any<TopLevel>()).Returns(storage);
            using IDisposable locator = InstallStorageFactory(factory);
            desktop.MainWindow.Should().BeNull();
            GitUICommands commands = new(services, module);
            commands.StartBrowseDialog(owner: null).Should().BeTrue();
            browse = desktop.MainWindow.Should().BeOfType<FormBrowse>().Subject;
            WinFormsShims.ShimHost.OsShell.Should().BeOfType<AvaloniaOsShell>();
            await WaitUntilAsync(() => browse.RevisionGrid.SelectedRevision?.ObjectId == module.GetCurrentCheckout()
                && browse.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!.Text!.EndsWith(" revisions", StringComparison.Ordinal));
            browse.CommitInfoTabControl.SelectedItem = browse.TreeTabPage;
            await WaitUntilAsync(() => browse.fileTree.FileStatusList.GitItemStatuses.Any(item => item.Name == name));
            FileStatusList list = browse.fileTree.FileStatusList;
            list.SelectFileOrFolder(RelativePath.From(name)).Should().BeTrue();
            list.TryGetUICommandsDirect(out IGitUICommands? actualCommands).Should().BeTrue();
            actualCommands.Should().BeSameAs(commands);
            using (var frame = browse.CaptureRenderedFrame())
            {
                frame.Should().NotBeNull();
            }

            ContextMenu menu = list.ContextMenu!;
            list.GetTestAccessor().ContextMenu.Should().BeSameAs(menu);
            ProcessStartInfo? reveal = null;
            bool? waitForExit = null;
            NativeFileReveal.TestAccessor.StartProcess = (request, wait) =>
            {
                reveal = request;
                waitForExit = wait;
                return Task.FromResult(true);
            };
            menu.Open(list);
            Dispatcher.UIThread.RunJobs();
            menu.IsOpen.Should().BeTrue();
            using (var frame = browse.CaptureRenderedFrame())
            {
                frame.Should().NotBeNull();
            }

            MenuItem action = list.FindControl<MenuItem>("tsmiShowInFolder")!;
            action.IsVisible.Should().BeTrue();
            action.IsEnabled.Should().BeTrue();
            action.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            Dispatcher.UIThread.RunJobs();

            if (OperatingSystem.IsLinux())
            {
                await portal.Received(1).TryLaunchAsync(target, WinFormsShims.OsShellLaunchKind.ShowInDirectory);
                reveal.Should().BeNull();
            }
            else
            {
                reveal.Should().NotBeNull();
                reveal!.UseShellExecute.Should().BeFalse();
                if (OperatingSystem.IsWindows())
                {
                    reveal.FileName.Should().Be("explorer.exe");
                    reveal.Arguments.Should().Be($"/select, {target.Quote()}");
                    waitForExit.Should().BeFalse();
                }
                else
                {
                    reveal.FileName.Should().Be("/usr/bin/open");
                    reveal.ArgumentList.Should().Equal("-R", target);
                    waitForExit.Should().BeTrue();
                }

                await portal.DidNotReceiveWithAnyArgs().TryLaunchAsync(default!, default);
            }

            messages.DidNotReceiveWithAnyArgs().Show(default, default, default, default, default, default);
            await storage.DidNotReceiveWithAnyArgs().TryGetFileFromPathAsync(default!);
        }
        finally
        {
            browse?.Close();
            (browse as IDisposable)?.Dispose();
            NativeFileReveal.TestAccessor.StartProcess = previousReveal;
            OsShellUtil.TestAccessor.MockExecutable = previousExecutable;
            for (int index = 0; index < fields.Length; index++)
            {
                fields[index].SetValue(null, previousServices[index]);
            }

            TestDirectory.Delete(directory);
        }
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        using CancellationTokenSource deadline = new(TimeSpan.FromSeconds(15));
        while (!condition())
        {
            Dispatcher.UIThread.RunJobs();
            await Task.Delay(10, deadline.Token);
        }
    }

    private static IDisposable InstallStorageFactory(IStorageProviderFactory factory)
    {
        Type locator = typeof(Avalonia.AvaloniaLocator);
        IDisposable scope = (IDisposable)locator.GetMethod("EnterScope")!.Invoke(null, null)!;
        try
        {
            object current = locator.GetProperty("CurrentMutable")!.GetValue(null)!;
            object binding = locator.GetMethod("Bind")!.MakeGenericMethod(typeof(IStorageProviderFactory)).Invoke(current, null)!;
            binding.GetType().GetMethod("ToConstant")!.MakeGenericMethod(typeof(IStorageProviderFactory)).Invoke(binding, [factory]);
            return scope;
        }
        catch
        {
            scope.Dispose();
            throw;
        }
    }
}
