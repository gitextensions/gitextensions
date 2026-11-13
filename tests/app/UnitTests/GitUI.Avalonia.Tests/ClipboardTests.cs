using System.ComponentModel.Design;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class ClipboardTests
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

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        UserEnvironmentInformation.Initialise("9999999999999999999999999999999999abcdef", isDirty: true);
    }

    [AvaloniaTest]
    public async Task FormAbout_copy_button_should_use_the_installed_clipboard_and_production_main_window()
    {
        using AppSettingsTestScope settings = new();
        using ServiceContainer services = new();
        GitExtensions.ServiceContainerRegistry.RegisterServices(services);
        services.RemoveService(typeof(IUpdateCheckService));
        services.AddService<IUpdateCheckService>(Substitute.For<IUpdateCheckService>());
        string directory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.Clipboard-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        IClassicDesktopStyleApplicationLifetime desktop = (IClassicDesktopStyleApplicationLifetime)Avalonia.Application.Current!.ApplicationLifetime!;
        FieldInfo[] fields = typeof(GitExtensions.Shims.WinForms.ShimHost).GetFields(BindingFlags.Static | BindingFlags.NonPublic);
        object?[] previousServices = fields.Select(field => field.GetValue(null)).ToArray();
        Window? mainWindow = null;
        FormAbout? about = null;
        try
        {
            ShimServices.Install(desktop);
            desktop.MainWindow.Should().BeNull();
            GitUICommands commands = new(services, new GitModule(services.GetRequiredService<IGitExecutorProvider>(), directory));
            commands.StartBrowseDialog(owner: null).Should().BeTrue();
            mainWindow = desktop.MainWindow;
            mainWindow.Should().BeOfType<FormBrowse>();
            GitExtensions.Shims.WinForms.ShimHost.Clipboard.Should().BeOfType<AvaloniaClipboard>();
            about = new FormAbout();
            about.Show(mainWindow!);
            Dispatcher.UIThread.RunJobs();
            about.UpdateLayout();
            EnvironmentInfo environment = about.FindControl<EnvironmentInfo>("environmentInfo")!;
            IconButton copy = environment.GetTestAccessor().CopyButton;
            Point point = copy.TranslatePoint(new Point(copy.Bounds.Width / 2, copy.Bounds.Height / 2), about)!.Value;

            about.MouseDown(point, MouseButton.Left);
            about.MouseUp(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            string? copied = await mainWindow!.Clipboard!.TryGetTextAsync();
            copied.Should().StartWith(UserEnvironmentInformation.GetInformation());
        }
        finally
        {
            about?.Close();
            mainWindow?.Close();
            (mainWindow as IDisposable)?.Dispose();
            for (int index = 0; index < fields.Length; index++)
            {
                fields[index].SetValue(null, previousServices[index]);
            }

            TestDirectory.Delete(directory);
        }
    }
}
