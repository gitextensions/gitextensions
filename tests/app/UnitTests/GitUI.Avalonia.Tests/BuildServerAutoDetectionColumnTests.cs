using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Git;
using GitCommands.Settings;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.BuildServerIntegration;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Plugins.GitHubActionsIntegration;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI;
using GitUI.BuildServerIntegration;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Compat;
using GitUI.Theming;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using Microsoft.VisualStudio.Composition;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using WinFormsShims = GitExtensions.Shims.WinForms;

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
        => await VerifyDetectedAdapterAsync(showIcon, showText, verifyPalette: false);

    [AvaloniaTest]
    public async Task Detected_adapter_should_render_build_status_in_regular_generic_monospace_at_the_ui_font_size()
    {
        using AppSettingsTestScope settings = new();
        WinFormsShims.Font previousUi = AppSettings.Font;
        WinFormsShims.Font previousMonospace = AppSettings.MonospaceFont;
        try
        {
            AppSettings.Font = new WinFormsShims.Font("Segoe UI", 12, WinFormsShims.FontStyle.Bold);
            AppSettings.MonospaceFont = new WinFormsShims.Font("Consolas", 20, WinFormsShims.FontStyle.Italic);
            AvaloniaFontSettings.ApplyAppSettings();
            await VerifyDetectedAdapterAsync(showIcon: true, showText: true, verifyPalette: false, verifyFont: true);
        }
        finally
        {
            AppSettings.Font = previousUi;
            AppSettings.MonospaceFont = previousMonospace;
            AvaloniaFontSettings.ApplyAppSettings();
        }
    }

    internal static async Task VerifyDetectedAdapterAsync(bool showIcon, bool showText, bool verifyPalette, bool verifyFont = false)
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        using AppSettingsTestScope settings = new(useSharedCache: true);
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
        using Subject<BuildInfo> running = new();
        using Subject<BuildInfo> finished = new();
        IBuildServerAdapter adapter = Substitute.For<IBuildServerAdapter>();
        adapter.When(instance => instance.Initialize(Arg.Any<IBuildServerWatcher>(), Arg.Any<SettingsSource>(),
            Arg.Any<Action>(), Arg.Any<Func<ObjectId, bool>?>())).Do(call =>
            {
                initialized.TrySetResult((call.Arg<IBuildServerWatcher>(), call.Arg<SettingsSource>()));
            });
        adapter.GetRunningBuilds(Arg.Any<IScheduler>()).Returns(_ =>
        {
            queried.TrySetResult();
            return running;
        });
        adapter.GetFinishedBuildsSince(Arg.Any<IScheduler>(), Arg.Any<DateTime?>()).Returns(finished);
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
            module.EffectiveSettings.LowerPriority!.LowerPriority!.SettingsCache.Should()
                .BeSameAs(AppSettings.SettingsContainer.SettingsCache,
                    "the original app preferences and detector writes share the global settings cache");
            BuildServerSettings.ServerName[module.GetEffectiveSettings()].Should().BeNullOrEmpty();
            BuildServerSettings.IntegrationEnabled[module.GetEffectiveSettings()].Should().BeNull();
            form = new FormBrowse(new GitUICommands(services, module), new BrowseArguments()) { Width = 1400, Height = 800 };
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
                BuildStatusColumnProvider.BuildStatusTextBlock cell = row.Children.OfType<BuildStatusColumnProvider.BuildStatusTextBlock>().Single();
                cell.IsVisible.Should().Be(showIcon || showText);
                if (verifyFont)
                {
                    cell.FontSize.Should().Be(16);
                    cell.FontStyle.Should().Be(FontStyle.Normal);
                    cell.FontWeight.Should().Be(FontWeight.Normal);
                    cell.FontFamily.Name.Should().Be(OperatingSystem.IsWindows()
                        ? "Courier New"
                        : OperatingSystem.IsMacOS() ? "Menlo" : "DejaVu Sans Mono");
                }
            }

            if (verifyPalette)
            {
                ObjectId head = module.GetCurrentCheckout();
                await WaitUntilAsync(() => running.HasObservers && finished.HasObservers
                    && form.RevisionGrid.GetRevision(head) is not null);
                GitRevision revision = form.RevisionGrid.GetRevision(head);
                BuildStatusColumnProvider.BuildStatusTextBlock cell = form.RevisionGrid.GetVisualDescendants()
                    .OfType<BuildStatusColumnProvider.BuildStatusTextBlock>()
                    .Single(candidate => ReferenceEquals(candidate.DataContext, revision));
                ListBoxItem row = cell.FindAncestorOfType<ListBoxItem>()!;
                (BuildStatus Status, Color Normal, Color Selected)[] cases =
                [
                    (BuildStatus.Success, Colors.DarkGreen, Colors.LightGreen),
                    (BuildStatus.Failure, Colors.DarkRed, Colors.Red),
                    (BuildStatus.InProgress, Colors.Blue, Color.FromRgb(130, 180, 240)),
                    (BuildStatus.Unstable, Colors.OrangeRed, Colors.OrangeRed),
                    (BuildStatus.Stopped, Colors.Gray, Colors.LightGray),
                ];
                int buildIndex = 0;
                foreach ((BuildStatus status, Color normal, Color selected) in cases)
                {
                    await PublishStatusAsync(status);
                    form.RevisionGrid._gridView.SelectedItem = null;
                    Settle(form);
                    row.IsSelected.Should().BeFalse();
                    cell.Foreground.Should().BeAssignableTo<ISolidColorBrush>().Which.Color.Should().Be(AdaptExpected(normal));

                    form.RevisionGrid._gridView.SelectedItem = revision;
                    Settle(form);
                    row.IsSelected.Should().BeTrue();
                    cell.Foreground.Should().BeAssignableTo<ISolidColorBrush>().Which.Color.Should().Be(AdaptExpected(selected));
                }

                await PublishStatusAsync(BuildStatus.Unknown);
                foreach (bool selected in new[] { false, true })
                {
                    form.RevisionGrid._gridView.SelectedItem = selected ? revision : null;
                    Settle(form);
                    row.IsSelected.Should().Be(selected);
                    cell.Foreground.Should().NotBeNull("the original Unknown status returns the inherited row foreground");
                    cell.Foreground.Should().BeSameAs(cell.FindAncestorOfType<ContentPresenter>()!.Foreground);
                }

                async Task PublishStatusAsync(BuildStatus status)
                {
                    BuildInfo info = new()
                    {
                        Id = $"palette-{++buildIndex}",
                        StartDate = DateTime.Today.AddMinutes(buildIndex),
                        CommitHashList = [head],
                        Status = status,
                        Description = status.ToString(),
                    };
                    (status == BuildStatus.InProgress ? running : finished).OnNext(info);
                    await WaitUntilAsync(() => ReferenceEquals(revision.BuildStatus, info));
                    cell.DataContext.Should().BeSameAs(revision);
                    cell.Text.Should().Be((showIcon ? info.StatusSymbol : string.Empty) + (showText ? info.Description : string.Empty));
                }
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

    private static Color AdaptExpected(Color color)
    {
        System.Drawing.Color original = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        System.Drawing.Color background = ThemeModule.Settings.Theme.GetColor(AppColor.PanelBackground);
        if (background.IsEmpty)
        {
            background = ThemeModule.Settings.InvariantTheme.GetColor(AppColor.PanelBackground);
        }

        System.Drawing.Color adapted = original.AdaptForeColor(background);
        return Color.FromArgb(adapted.A, adapted.R, adapted.G, adapted.B);
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
