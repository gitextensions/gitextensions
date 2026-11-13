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
public sealed class OpenWithIntegrationTests
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

    [AvaloniaTest]
    public async Task File_context_action_should_use_the_installed_chooser_with_production_owner_and_path()
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
        Func<nint, string, uint, int> previousWindows = NativeApplicationChooser.TestAccessor.ShowWindowsDialog;
        Func<TopLevel, string?> previousPicker = MacApplicationPicker.TestAccessor.ShowPanel;
        Func<ProcessStartInfo, Task<bool>> previousStart = NativeApplicationChooser.TestAccessor.StartApplication;
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
            portal.TryLaunchAsync(Arg.Any<string>(), WinFormsShims.OsShellLaunchKind.OpenAs).Returns(true);
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
            int dialogs = 0;
            nint actualParent = 0;
            string? actualFile = null;
            uint actualFlags = 0;
            bool? popupOpen = null;
            NativeApplicationChooser.TestAccessor.ShowWindowsDialog = (parent, file, flags) =>
            {
                dialogs++;
                actualParent = parent;
                actualFile = file;
                actualFlags = flags;
                popupOpen = menu.IsOpen;
                return 0;
            };
            string application = Path.Combine(directory, "Chosen Application.app");
            Directory.CreateDirectory(application);
            TopLevel? pickerOwner = null;
            ProcessStartInfo? applicationLaunch = null;
            MacApplicationPicker.TestAccessor.ShowPanel = owner =>
            {
                pickerOwner = owner;
                popupOpen = menu.IsOpen;
                return application;
            };
            NativeApplicationChooser.TestAccessor.StartApplication = startInfo =>
            {
                applicationLaunch = startInfo;
                return Task.FromResult(true);
            };
            menu.Open(list);
            Dispatcher.UIThread.RunJobs();
            menu.IsOpen.Should().BeTrue();
            using (var frame = browse.CaptureRenderedFrame())
            {
                frame.Should().NotBeNull();
            }

            MenuItem action = list.FindControl<MenuItem>("tsmiOpenWorkingDirectoryFileWith")!;
            action.IsVisible.Should().BeTrue();
            action.IsEnabled.Should().BeTrue();
            action.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            Dispatcher.UIThread.RunJobs();

            if (OperatingSystem.IsWindows())
            {
                dialogs.Should().Be(1);
                popupOpen.Should().BeFalse("the native chooser must start after its popup has closed");
                actualParent.Should().Be(browse.TryGetPlatformHandle()?.Handle ?? 0);
                actualFile.Should().Be(target);
                actualFlags.Should().Be(4);
                await portal.DidNotReceiveWithAnyArgs().TryLaunchAsync(default!, default);
            }
            else if (OperatingSystem.IsLinux())
            {
                await portal.Received(1).TryLaunchAsync(target, WinFormsShims.OsShellLaunchKind.OpenAs);
                dialogs.Should().Be(0);
            }

            if (OperatingSystem.IsMacOS())
            {
                pickerOwner.Should().BeSameAs(browse);
                popupOpen.Should().BeFalse();
                applicationLaunch.Should().NotBeNull();
                applicationLaunch!.FileName.Should().Be("/usr/bin/open");
                applicationLaunch.ArgumentList.Should().Equal("-a", application, target);
                await portal.DidNotReceiveWithAnyArgs().TryLaunchAsync(default!, default);
                dialogs.Should().Be(0);
            }

            messages.DidNotReceiveWithAnyArgs().Show(default, default, default, default, default, default);
            await storage.DidNotReceiveWithAnyArgs().TryGetFileFromPathAsync(default!);
        }
        finally
        {
            browse?.Close();
            (browse as IDisposable)?.Dispose();
            NativeApplicationChooser.TestAccessor.ShowWindowsDialog = previousWindows;
            MacApplicationPicker.TestAccessor.ShowPanel = previousPicker;
            NativeApplicationChooser.TestAccessor.StartApplication = previousStart;
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
