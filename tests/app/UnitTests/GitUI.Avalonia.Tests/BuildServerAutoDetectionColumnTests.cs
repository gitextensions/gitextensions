using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Git;
using GitCommands.Settings;
using GitExtensions.Extensibility.BuildServerIntegration;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Plugins.GitHubActionsIntegration;
using GitExtUtils;
using GitUI;
using GitUI.BuildServerIntegration;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using Microsoft.VisualStudio.Composition;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class BuildServerAutoDetectionColumnTests
{
    [AvaloniaTest]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    [TestCase(false, false)]
    public async Task Detected_adapter_should_apply_display_preferences_before_any_build_arrives(bool showIcon, bool showText)
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        using AppSettingsTestScope settings = new();
        AppSettings.ShowBuildStatusIconColumn = showIcon;
        AppSettings.ShowBuildStatusTextColumn = showText;
        using ServiceContainer services = new();
        GitExtensions.ServiceContainerRegistry.RegisterServices(services);
        services.RemoveService(typeof(IUpdateCheckService));
        services.AddService<IUpdateCheckService>(Substitute.For<IUpdateCheckService>());
        string directory = Path.Combine(Path.GetTempPath(), $"GitExtensions.DetectedBuilds-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        FieldInfo providerField = typeof(ManagedExtensibility).GetField("_exportProvider", BindingFlags.Static | BindingFlags.NonPublic)!;
        FieldInfo loadedField = typeof(PluginRegistry).GetField("_isLoaded", BindingFlags.Static | BindingFlags.NonPublic)!;
        PropertyInfo registeredProperty = typeof(PluginRegistry).GetProperty(nameof(PluginRegistry.PluginsRegistered))!;
        object? previousProvider = providerField.GetValue(null);
        object? previousLoaded = loadedField.GetValue(null);
        bool previousRegistered = PluginRegistry.PluginsRegistered;
        IGitPlugin[] previousPlugins = [.. PluginRegistry.Plugins];
        IRepositoryHostPlugin[] previousHosters = [.. PluginRegistry.GitHosters];
        TaskCompletionSource<(IBuildServerWatcher Watcher, SettingsSource Config)> initialized = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource queried = new(TaskCreationOptions.RunContinuationsAsynchronously);
        IBuildServerAdapter adapter = Substitute.For<IBuildServerAdapter>();
        adapter.When(instance => instance.Initialize(Arg.Any<IBuildServerWatcher>(), Arg.Any<SettingsSource>(),
            Arg.Any<Action>(), Arg.Any<Func<ObjectId, bool>?>())).Do(call =>
            {
                initialized.TrySetResult((call.Arg<IBuildServerWatcher>(), call.Arg<SettingsSource>()));
            });
        adapter.GetRunningBuilds(Arg.Any<IScheduler>()).Returns(_ =>
        {
            queried.TrySetResult();
            return Observable.Never<BuildInfo>();
        });
        adapter.GetFinishedBuildsSince(Arg.Any<IScheduler>(), Arg.Any<DateTime?>()).Returns(Observable.Never<BuildInfo>());
        Type detector = typeof(GitHubActionsIntegrationMetadataAttribute).Assembly.GetType(
            "GitExtensions.Plugins.GitHubActionsIntegration.GitHubActionsAutoDetector", throwOnError: true)!;
        AttributedPartDiscoveryV1 discovery = new(Resolver.DefaultInstance);
        DiscoveredParts parts = await discovery.CreatePartsAsync([detector, typeof(ExportedAdapter)]);
        ComposableCatalog catalog = ComposableCatalog.Create(Resolver.DefaultInstance).AddParts(parts);
        CompositionConfiguration configuration = CompositionConfiguration.Create(catalog);
        configuration.ThrowOnErrors();
        using ExportProvider provider = RuntimeComposition.CreateRuntimeComposition(configuration).CreateExportProviderFactory().CreateExportProvider();
        FormBrowse? form = null;
        try
        {
            ExportedAdapter.Adapter = adapter;
            ManagedExtensibility.SetTestExportProvider(provider);
            PluginRegistry.Plugins.Clear();
            PluginRegistry.GitHosters.Clear();
            loadedField.SetValue(null, false);
            registeredProperty.SetValue(null, false);
            GitModule module = new(services.GetRequiredService<IGitExecutorProvider>(), directory);
            module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" }).Should().BeTrue();
            module.SetSetting("user.name", "Avalonia Test");
            module.SetSetting("user.email", "avalonia@example.com");
            File.WriteAllText(Path.Combine(directory, "file.txt"), "content\n");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "file.txt" }).Should().BeTrue();
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
            module.GitExecutable.RunCommand(new GitArgumentBuilder("remote")
                { "add", "origin", "https://github.com/parity-owner/parity-repository.git" }).Should().BeTrue();
            BuildServerSettings.ServerName[module.GetEffectiveSettings()].Should().BeNullOrEmpty();
            BuildServerSettings.IntegrationEnabled[module.GetEffectiveSettings()].Should().BeNull();
            form = new FormBrowse(new GitUICommands(services, module)) { Width = 1400, Height = 800 };
            BuildStatusColumnProvider column = form.RevisionGrid.ColumnProviders.OfType<BuildStatusColumnProvider>().Single();
            column.Column.IsVisible.Should().BeFalse("discovery has not yet published an available adapter");
            form.Show();
            await WaitUntilAsync(() => initialized.Task.IsCompleted);
            (IBuildServerWatcher watcher, SettingsSource configurationSource) = await initialized.Task;
            ((BuildServerWatcher)watcher).ColumnProvider.Should().BeSameAs(column);
            configurationSource.GetString("GitHubActionsOwner", null).Should().Be("parity-owner");
            configurationSource.GetString("GitHubActionsRepository", null).Should().Be("parity-repository");
            await WaitUntilAsync(() => queried.Task.IsCompleted);
            Settle(form);

            column.Column.IsVisible.Should().Be(showIcon || showText);
            column.Column.Width.Should().Be(new GridLength(showIcon && !showText ? 16 : 150));
            BuildServerSettings.IntegrationEnabled[module.GetEffectiveSettings()].Should().BeNull();
            BuildServerSettings.ServerName[module.GetEffectiveSettings()].Should().BeNullOrEmpty();
            Grid[] rows = [.. form.RevisionGrid.GetVisualDescendants().OfType<Grid>().Where(grid => grid.Classes.Contains("revision-row"))];
            rows.Should().NotBeEmpty();
            foreach (Grid row in rows)
            {
                row.ColumnDefinitions[column.Index].Width.Should().Be(column.Column.EffectiveWidth);
                row.Children.OfType<BuildStatusColumnProvider.BuildStatusTextBlock>().Single().IsVisible.Should().Be(showIcon || showText);
            }
        }
        finally
        {
            try
            {
                if (form is not null)
                {
                    await form.JoinLoadOperationsForTestAsync().WaitAsync(TimeSpan.FromSeconds(30));
                    form.RevisionGrid.CancelBackgroundTasks();
                    form.Close();
                    (form as IDisposable)?.Dispose();
                }
            }
            finally
            {
                providerField.SetValue(null, previousProvider);
                loadedField.SetValue(null, previousLoaded);
                registeredProperty.SetValue(null, previousRegistered);
                PluginRegistry.Plugins.Clear();
                PluginRegistry.Plugins.AddRange(previousPlugins);
                PluginRegistry.GitHosters.Clear();
                PluginRegistry.GitHosters.AddRange(previousHosters);
                ExportedAdapter.Adapter = null!;
                TestDirectory.Delete(directory);
            }
        }
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        for (int attempt = 0; attempt < 1500; attempt++)
        {
            Dispatcher.UIThread.RunJobs();
            if (condition())
            {
                return;
            }

            await Task.Delay(10);
        }

        condition().Should().BeTrue("the production adapter initialization and fetch should complete");
    }

    private static void Settle(Window form)
    {
        Dispatcher.UIThread.RunJobs();
        using var frame = form.CaptureRenderedFrame();
        frame.Should().NotBeNull();
        Dispatcher.UIThread.RunJobs();
    }

    public sealed class ExportedAdapter
    {
        [Export(typeof(IBuildServerAdapter))]
        [ExportMetadata(nameof(IBuildServerTypeMetadata.BuildServerType), GitHubActionsAdapter.PluginName)]
        [ExportMetadata(nameof(IBuildServerTypeMetadata.CanBeLoaded), "")]
        public static IBuildServerAdapter Adapter { get; set; } = null!;
    }
}
