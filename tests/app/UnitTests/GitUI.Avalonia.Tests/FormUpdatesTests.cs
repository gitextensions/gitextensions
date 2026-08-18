using System.ComponentModel.Design;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.CommandsDialogs.Menus;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;

namespace GitExtensionsTests;

[TestFixture]
public sealed class FormUpdatesTests
{
    [AvaloniaTest]
    public async Task Update_dialog_should_parse_the_shared_feed_and_expose_host_appropriate_actions()
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        ConfigFile config = new(fileName: "");
        config.SetValue("Version \"99.1\".ReleaseType", "Major");
        config.SetValue("Version \"99.1\".DownloadPage", "https://example.test/GitExtensions-x64-99.1.msi");
        config.SetValue("Version \"99.1\".NetRuntimeVersion", "99.0.2");

        FormUpdates form = new(
            new Version(98, 0),
            _ => Task.FromResult(config.GetAsString()));
        GitExtensionsFormBase owner = new();

        form.SearchForUpdatesAndShow(owner, alwaysShow: false);
        await form.GetTestAccessor().JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(10));

        FormUpdates.TestAccessor accessor = form.GetTestAccessor();
        accessor.UpdateFound.Should().BeTrue();
        accessor.UpdateText.Text.Should().Be("There is a new version 99.1 of Git Extensions available");
        accessor.ChangeLog.IsVisible.Should().BeTrue();
        accessor.DirectDownload.IsVisible.Should().BeTrue();
        accessor.UpdateUrl.Should().Contain("GitExtensions-");
        accessor.UpdateNow.IsVisible.Should().Be(
            FormUpdates.TestAccessor.CanInstall(OperatingSystem.IsWindows(), AppSettings.IsPortable()));
        accessor.RequiredNetRuntime.IsVisible.Should().Be(OperatingSystem.IsWindows());
        if (OperatingSystem.IsWindows())
        {
            accessor.NetRuntimeDownloadUrl.Should().MatchRegex("[?&]arch=[a-z0-9]+&rid=win-[a-z0-9]+(?:&|$)");
        }
    }

    [AvaloniaTest]
    public void Update_dialog_should_preserve_existing_translation_keys()
    {
        ITranslation translation = Substitute.For<ITranslation>();
        FormUpdates form = new(new Version(98, 0), _ => Task.FromResult(string.Empty));

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(FormUpdates), "$this", "Text", "Check for update");
        translation.Received(1).AddTranslationItem(
            nameof(FormUpdates), "UpdateLabel", "Text", "Searching for updates");
        translation.Received(1).AddTranslationItem(
            nameof(FormUpdates), "linkRequiredDotNetRuntime", "Text",
            "Required: .NET {0} Desktop Runtime {1} or later {2}.x");
        translation.Received(1).AddTranslationItem(
            nameof(FormUpdates), "linkRequiredDotNetRuntime", "ToolTipText",
            Arg.Is<string>(text => text.StartsWith("Download latest .NET Desktop Runtime.", StringComparison.Ordinal)));
    }

    [AvaloniaTest]
    public void Update_install_policy_should_never_launch_the_windows_installer_on_other_hosts()
    {
        FormUpdates.TestAccessor.CanInstall(isWindows: true, isPortable: false).Should().BeTrue();
        FormUpdates.TestAccessor.CanInstall(isWindows: true, isPortable: true).Should().BeFalse();
        FormUpdates.TestAccessor.CanInstall(isWindows: false, isPortable: false).Should().BeFalse();
        FormUpdates.TestAccessor.CanInstall(isWindows: false, isPortable: true).Should().BeFalse();
    }

    [AvaloniaTest]
    public void Browse_should_expose_manual_and_weekly_update_checks_through_the_same_service()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        using ServiceContainer services = CreateServiceContainer();
        IUpdateCheckService updateService = Substitute.For<IUpdateCheckService>();
        services.AddService<IUpdateCheckService>(updateService);
        string workingDirectory = Path.Combine(
            Path.GetTempPath(),
            $"GitExtensions.Avalonia.UpdateTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(workingDirectory);

        bool originalCheckForUpdates = AppSettings.CheckForUpdates;
        DateTime originalLastUpdateCheck = AppSettings.LastUpdateCheck;
        try
        {
            AppSettings.CheckForUpdates = true;
            AppSettings.LastUpdateCheck = DateTime.Now.AddDays(-8);
            GitModule module = new(services.GetRequiredService<IGitExecutorProvider>(), workingDirectory);
            FormBrowse form = new(new GitUICommands(services, module));
            form.Show();
            try
            {
                Dispatcher.UIThread.RunJobs();
                updateService.Received(1).SearchForUpdatesAndShow(form, alwaysShow: false);
                AppSettings.LastUpdateCheck.Should().BeAfter(DateTime.Now.AddDays(-1));

                HelpToolStripMenuItem helpMenu = form.FindControl<HelpToolStripMenuItem>("helpToolStripMenuItem")
                    ?? throw new InvalidOperationException("The Help menu was not created.");
                MenuItem manualCheck = helpMenu.GetTestAccessor().CheckUpdatesMenuItem;
                manualCheck.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                updateService.Received(1).SearchForUpdatesAndShow(form, alwaysShow: true);
            }
            finally
            {
                form.Close();
            }
        }
        finally
        {
            AppSettings.CheckForUpdates = originalCheckForUpdates;
            AppSettings.LastUpdateCheck = originalLastUpdateCheck;
            Directory.Delete(workingDirectory, recursive: true);
        }
    }

    private static ServiceContainer CreateServiceContainer()
    {
        ServiceContainer services = new();
        GitExtUtils.ServiceContainerRegistry.RegisterServices(services);
        System.IO.Abstractions.FileSystem fileSystem = new();
        GitDirectoryResolver gitDirectoryResolver = new(fileSystem);
        RepositoryDescriptionProvider repositoryDescriptionProvider = new(gitDirectoryResolver);
        services.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
        services.AddService<IGitDirectoryResolver>(gitDirectoryResolver);
        services.AddService<IRepositoryDescriptionProvider>(repositoryDescriptionProvider);
        services.AddService<IAppTitleGenerator>(new AppTitleGenerator(repositoryDescriptionProvider));
        services.AddService<ILinkFactory>(new LinkFactory());
        GitCommands.ServiceContainerRegistry.RegisterServices(services);
        GitUI.ServiceContainerRegistry.RegisterServices(services);
        return services;
    }
}
