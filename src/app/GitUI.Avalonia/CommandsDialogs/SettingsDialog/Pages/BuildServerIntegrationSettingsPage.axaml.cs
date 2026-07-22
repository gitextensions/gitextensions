using Avalonia.Controls;
using GitCommands.Remotes;
using GitCommands.Settings;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Extensions;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using Microsoft;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class BuildServerIntegrationSettingsPage : DistributedSettingsPage
{
    private readonly TranslationString _noneItem = new("None");
    private IConfigFileRemoteSettingsManager? _remotesManager;
    private JoinableTask<string[]>? _populateBuildServerTypeTask;

    public BuildServerIntegrationSettingsPage()
        : this(EmptyServiceProvider.Instance)
    {
    }

    public BuildServerIntegrationSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        BuildServerType.SelectionChanged += BuildServerType_SelectedIndexChanged;
        InitializeComplete();
    }

    protected override void Init(ISettingsPageHost pageHost)
    {
        base.Init(pageHost);

        _remotesManager = new ConfigFileRemoteSettingsManager(
            () => Module ?? throw new InvalidOperationException("A repository is required for build-server settings."));
        _populateBuildServerTypeTask = ThreadHelper.JoinableTaskFactory.RunAsync(
            async () =>
            {
                await TaskScheduler.Default.SwitchTo(alwaysYield: true);

                IEnumerable<Lazy<IBuildServerAdapter, IBuildServerTypeMetadata>> exports =
                    ManagedExtensibility.GetExports<IBuildServerAdapter, IBuildServerTypeMetadata>();
                string[] buildServerTypes =
                [
                    .. exports.Select(export =>
                    {
                        string? canBeLoaded = export.Metadata.CanBeLoaded;
                        return export.Metadata.BuildServerType.Combine(" - ", canBeLoaded)!;
                    }),
                ];

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                checkBoxEnableBuildServerIntegration.IsEnabled = true;
                checkBoxShowBuildResultPage.IsEnabled = true;
                BuildServerType.IsEnabled = true;

                string[] items = [.. new[] { _noneItem.Text }.Concat(buildServerTypes)];
                BuildServerType.ItemsSource = items;
                return items;
            });
    }

    public override bool IsInstantSavePage => false;

    protected override void SettingsToPage()
    {
        ThreadHelper.FileAndForget(
            async () =>
            {
                Validates.NotNull(_populateBuildServerTypeTask);

                await _populateBuildServerTypeTask.JoinAsync();
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                SettingsSource currentSettings = GetCurrentSettings();

                checkBoxEnableBuildServerIntegration.IsChecked = BuildServerSettings.IntegrationEnabled[currentSettings];
                checkBoxShowBuildResultPage.IsChecked = BuildServerSettings.ShowBuildResultPage[currentSettings];

                BuildServerType.SelectedItem = BuildServerSettings.ServerName[currentSettings] ?? _noneItem.Text;
                ActivateBuildServerSettingsControl();

                base.SettingsToPage();
            });
    }

    protected override void PageToSettings()
    {
        SettingsSource currentSettings = GetCurrentSettings();

        BuildServerSettings.ServerName[currentSettings] = GetSelectedBuildServerType();
        BuildServerSettings.IntegrationEnabled[currentSettings] = checkBoxEnableBuildServerIntegration.IsChecked;
        BuildServerSettings.ShowBuildResultPage[currentSettings] = checkBoxShowBuildResultPage.IsChecked;

        if (buildServerSettingsPanel.Content is IBuildServerSettingsUserControl control)
        {
            control.SaveSettings(BuildServerSettings.GetSettingsSource(currentSettings));
        }

        base.PageToSettings();
    }

    private void ActivateBuildServerSettingsControl()
    {
        if (buildServerSettingsPanel.Content is IDisposable disposable)
        {
            disposable.Dispose();
        }

        IBuildServerSettingsUserControl? control = CreateBuildServerSettingsUserControl();
        buildServerSettingsPanel.Content = null;

        if (control is null)
        {
            return;
        }

        if (control is not Control avaloniaControl)
        {
            throw new InvalidOperationException(
                $"Build-server settings control {control.GetType().FullName} is not an Avalonia control.");
        }

        control.LoadSettings(BuildServerSettings.GetSettingsSource(GetCurrentSettings()));
        buildServerSettingsPanel.Content = avaloniaControl;
    }

    private IBuildServerSettingsUserControl? CreateBuildServerSettingsUserControl()
    {
        if (BuildServerType.SelectedIndex <= 0 || Module is null || string.IsNullOrEmpty(Module.WorkingDir))
        {
            return null;
        }

        string defaultProjectName = Module.WorkingDir.Split(Delimiters.PathSeparators, StringSplitOptions.RemoveEmptyEntries)[^1];

        IEnumerable<Lazy<IBuildServerSettingsUserControl, IBuildServerTypeMetadata>> exports =
            ManagedExtensibility.GetExports<IBuildServerSettingsUserControl, IBuildServerTypeMetadata>();
        Lazy<IBuildServerSettingsUserControl, IBuildServerTypeMetadata>? selectedExport = exports
            .SingleOrDefault(export => export.Metadata.BuildServerType == GetSelectedBuildServerType());
        if (selectedExport is null)
        {
            return null;
        }

        IBuildServerSettingsUserControl buildServerSettingsUserControl = selectedExport.Value;
        Validates.NotNull(_remotesManager);
        IEnumerable<string> remoteUrls = _remotesManager.LoadRemotes(false)
            .Select(remote => string.IsNullOrEmpty(remote.PushUrl) ? remote.Url! : remote.PushUrl!);

        buildServerSettingsUserControl.Initialize(defaultProjectName, remoteUrls);
        return buildServerSettingsUserControl;
    }

    private string? GetSelectedBuildServerType()
    {
        if (BuildServerType.SelectedIndex == 0)
        {
            return null;
        }

        return BuildServerType.SelectedItem as string;
    }

    private void BuildServerType_SelectedIndexChanged(object? sender, EventArgs e)
        => ActivateBuildServerSettingsControl();

    public override void AddTranslationItems(ITranslation translation)
    {
        base.AddTranslationItems(translation);
        translation.AddTranslationItem(
            nameof(BuildServerIntegrationSettingsPage),
            "$this",
            "Text",
            Text ?? "Build server integration");
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        string neutralText = Text ?? "Build server integration";
        Text = translation.TranslateItem(
            nameof(BuildServerIntegrationSettingsPage),
            "$this",
            "Text",
            () => neutralText) ?? neutralText;
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(BuildServerIntegrationSettingsPage page)
    {
        public ContentControl buildServerSettingsPanel => page.buildServerSettingsPanel;
        public ComboBox BuildServerType => page.BuildServerType;
        public CheckBox checkBoxEnableBuildServerIntegration => page.checkBoxEnableBuildServerIntegration;
        public CheckBox checkBoxShowBuildResultPage => page.checkBoxShowBuildResultPage;
        public JoinableTask<string[]>? PopulateBuildServerTypeTask => page._populateBuildServerTypeTask;
    }
}
