using Avalonia.Controls;
using Avalonia.Interactivity;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Settings;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.CommandsDialogs.SettingsDialog.Plugins;
using GitUI.Compat;
using GitUI.Properties;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

public sealed partial class FormSettings : GitModuleForm, ISettingsPageHost
{
    public static readonly string HotkeySettingsName = "Scripts";

    #region Translation

    private readonly TranslationString _cantSaveSettings = new("Failed to save all settings");

    #endregion

    private static Type? _lastSelectedSettingsPageType;
    private readonly CommonLogic? _commonLogic;
    private readonly CheckSettingsLogic? _checkSettingsLogic;
    private readonly string _translatedTitle;
    private SettingsPageReference? _initialPage;
    private bool _pagesInitialized;
    private bool _saved;

    public FormSettings()
    {
        InitializeComponent();
        _translatedTitle = Text ?? "Settings";
        InitializeControls();
        InitializeComplete();
    }

    public FormSettings(IGitUICommands commands, SettingsPageReference? initialPage = null)
        : base(commands, enablePositionRestore: true)
    {
        InitializeComponent();
        _translatedTitle = Text ?? "Settings";
        _commonLogic = new CommonLogic(Module);
        _checkSettingsLogic = new CheckSettingsLogic(_commonLogic);
        _initialPage = initialPage;
        InitializeControls();
        InitializeComplete();
    }

    public CheckSettingsLogic CheckSettingsLogic => _checkSettingsLogic
        ?? throw new InvalidOperationException("Settings validation is unavailable in design mode.");

    private IEnumerable<ISettingsPage> SettingsPages => settingsTreeView.SettingsPages;

    public void GotoPage(SettingsPageReference settingsPageReference)
        => settingsTreeView.GotoPage(settingsPageReference);

    public void LoadAll() => LoadSettings();

    public void LoadSettings()
    {
        try
        {
            foreach (ISettingsPage settingsPage in SettingsPages)
            {
                settingsPage.LoadSettings();
            }
        }
        catch (Exception exception)
        {
            DialogResult = WinFormsShims.DialogResult.Abort;
            MessageBoxes.ShowError(this, exception.Message, "Failed to load settings");
            throw;
        }
    }

    public void SaveAll() => Save();

    public static WinFormsShims.DialogResult ShowSettingsDialog(
        IGitUICommands uiCommands,
        WinFormsShims.IWin32Window? owner,
        SettingsPageReference? initialPage = null)
    {
        WinFormsShims.DialogResult result = WinFormsShims.DialogResult.None;
        using FormSettings form = new(uiCommands, initialPage);
        AppSettings.UsingContainer(form._commonLogic!.DistributedSettingsSet.GlobalSettings, () =>
        {
            result = form.ShowDialog(owner);
        });
        return result;
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);
        InitializeSettingsPages();

        if (_checkSettingsLogic is null)
        {
            settingsTreeView.GotoPage(settingsPageReference: null);
            settingsTreeView.Focus();
            return;
        }

        using (WaitCursorScope.Enter())
        {
            LoadSettings();

            if (_initialPage is null && _lastSelectedSettingsPageType is not null)
            {
                _initialPage = new SettingsPageReferenceByType(_lastSelectedSettingsPageType);
            }

            settingsTreeView.GotoPage(_initialPage);
            settingsTreeView.Focus();
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (_saved)
        {
            SetDialogResultOnClose(WinFormsShims.DialogResult.OK);
        }

        base.OnClosing(e);
    }

    private void InitializeSettingsPages()
    {
        if (_pagesInitialized)
        {
            return;
        }

        _pagesInitialized = true;
        GitExtensionsSettingsGroup root = new();
        settingsTreeView.AddSettingsPage(root, parentPageReference: null, Images.GitExtensionsLogo16);
        IServiceProvider serviceProvider = TryGetUICommands(out IGitUICommands? commands)
            ? commands
            : EmptyServiceProvider.Instance;
        ChecklistSettingsPage checklistSettingsPage =
            SettingsPageBase.Create<ChecklistSettingsPage>(this, serviceProvider);
        settingsTreeView.AddSettingsPage(
            checklistSettingsPage,
            GitExtensionsSettingsGroup.GetPageReference(),
            icon: null,
            asRoot: true);
        settingsTreeView.AddSettingsPage(
            SettingsPageBase.Create<GeneralSettingsPage>(this, serviceProvider),
            GitExtensionsSettingsGroup.GetPageReference(),
            Images.GeneralSettings);
        settingsTreeView.AddSettingsPage(
            SettingsPageBase.Create<AppearanceSettingsPage>(this, serviceProvider),
            GitExtensionsSettingsGroup.GetPageReference(),
            Images.Appearance);
        settingsTreeView.AddSettingsPage(
            SettingsPageBase.Create<SortingSettingsPage>(this, serviceProvider),
            AppearanceSettingsPage.GetPageReference(),
            Images.SortBy);
        settingsTreeView.AddSettingsPage(
            SettingsPageBase.Create<ColorsSettingsPage>(this, serviceProvider),
            AppearanceSettingsPage.GetPageReference(),
            Images.Colors);
        settingsTreeView.AddSettingsPage(
            SettingsPageBase.Create<AppearanceFontsSettingsPage>(this, serviceProvider),
            AppearanceSettingsPage.GetPageReference(),
            Images.Font.AdaptLightness());
        settingsTreeView.AddSettingsPage(
            SettingsPageBase.Create<ConsoleStyleSettingsPage>(this, serviceProvider),
            AppearanceSettingsPage.GetPageReference(),
            Images.Console);
        settingsTreeView.AddSettingsPage(
            SettingsPageBase.Create<RevisionLinksSettingsPage>(this, serviceProvider),
            GitExtensionsSettingsGroup.GetPageReference(),
            Images.Link.AdaptLightness());
        settingsTreeView.AddSettingsPage(
            SettingsPageBase.Create<BuildServerIntegrationSettingsPage>(this, serviceProvider),
            GitExtensionsSettingsGroup.GetPageReference(),
            Images.Integration);
        settingsTreeView.AddSettingsPage(
            SettingsPageBase.Create<ScriptsSettingsPage>(this, serviceProvider),
            GitExtensionsSettingsGroup.GetPageReference(),
            Images.Console);
        settingsTreeView.AddSettingsPage(
            SettingsPageBase.Create<HotkeysSettingsPage>(this, serviceProvider),
            GitExtensionsSettingsGroup.GetPageReference(),
            Images.Hotkey);
        if (OperatingSystem.IsWindows())
        {
            settingsTreeView.AddSettingsPage(
                SettingsPageBase.Create<ShellExtensionSettingsPage>(this, serviceProvider),
                GitExtensionsSettingsGroup.GetPageReference(),
                Images.ShellExtensions);
        }

        settingsTreeView.AddSettingsPage(
            SettingsPageBase.Create<AdvancedSettingsPage>(this, serviceProvider),
            GitExtensionsSettingsGroup.GetPageReference(),
            Images.AdvancedSettings);
        settingsTreeView.AddSettingsPage(
            SettingsPageBase.Create<ConfirmationsSettingsPage>(this, serviceProvider),
            AdvancedSettingsPage.GetPageReference(),
            Images.BisectGood);
        settingsTreeView.AddSettingsPage(
            SettingsPageBase.Create<DetailedSettingsPage>(this, serviceProvider),
            GitExtensionsSettingsGroup.GetPageReference(),
            Images.Settings);
        settingsTreeView.AddSettingsPage(
            SettingsPageBase.Create<FormBrowseRepoSettingsPage>(this, serviceProvider),
            DetailedSettingsPage.GetPageReference(),
            Images.BranchFolder);
        settingsTreeView.AddSettingsPage(
            SettingsPageBase.Create<CommitDialogSettingsPage>(this, serviceProvider),
            DetailedSettingsPage.GetPageReference(),
            Images.CommitSummary);
        settingsTreeView.AddSettingsPage(
            SettingsPageBase.Create<DiffViewerSettingsPage>(this, serviceProvider),
            DetailedSettingsPage.GetPageReference(),
            Images.Diff);
        settingsTreeView.AddSettingsPage(
            SettingsPageBase.Create<BlameViewerSettingsPage>(this, serviceProvider),
            DetailedSettingsPage.GetPageReference(),
            Images.Blame);
        SshSettingsPage sshSettingsPage =
            SettingsPageBase.Create<SshSettingsPage>(this, serviceProvider);
        settingsTreeView.AddSettingsPage(
            sshSettingsPage,
            GitExtensionsSettingsGroup.GetPageReference(),
            Images.Key);
        checklistSettingsPage.SshSettingsPage = sshSettingsPage;

        GitSettingsGroup gitSettingsGroup = new();
        settingsTreeView.AddSettingsPage(gitSettingsGroup, parentPageReference: null, Images.GitLogo16);
        settingsTreeView.AddSettingsPage(
            SettingsPageBase.Create<GitSettingsPage>(this, serviceProvider),
            GitSettingsGroup.GetPageReference(),
            Images.FolderOpen);
        settingsTreeView.AddSettingsPage(
            SettingsPageBase.Create<GitConfigSettingsPage>(this, serviceProvider),
            GitSettingsGroup.GetPageReference(),
            Images.GeneralSettings);
        settingsTreeView.AddSettingsPage(
            SettingsPageBase.Create<GitConfigAdvancedSettingsPage>(this, serviceProvider),
            GitSettingsGroup.GetPageReference(),
            Images.AdvancedSettings);
        settingsTreeView.AddSettingsPage(
            SettingsPageBase.Create<GitRootIntroductionPage>(this, serviceProvider),
            GitSettingsGroup.GetPageReference(),
            icon: null,
            asRoot: true);

        PluginsSettingsGroup pluginsSettingsGroup = new();
        settingsTreeView.AddSettingsPage(pluginsSettingsGroup, parentPageReference: null, Images.Plugin);
        SettingsPageReference pluginsPageReference = PluginsSettingsGroup.GetPageReference();
        settingsTreeView.AddSettingsPage(
            SettingsPageBase.Create<PluginRootIntroductionPage>(this, serviceProvider),
            pluginsPageReference,
            icon: null,
            asRoot: true);

        lock (PluginRegistry.Plugins)
        {
            IOrderedEnumerable<(IGitPlugin plugin, PluginSettingsPage page)> pluginEntries = PluginRegistry.Plugins
                .Where(plugin => plugin.HasSettings)
                .Select(plugin => (
                    plugin,
                    page: PluginSettingsPage.CreateSettingsPageFromPlugin(this, plugin, serviceProvider)))
                .OrderBy(entry => entry.page.GetTitle(), StringComparer.CurrentCultureIgnoreCase);
            foreach ((IGitPlugin plugin, PluginSettingsPage page) entry in pluginEntries)
            {
                settingsTreeView.AddSettingsPage(
                    entry.page,
                    pluginsPageReference,
                    PluginIconProvider.GetIcon(entry.plugin));
            }
        }
    }

    private void OnSettingsPageSelected(object? sender, SettingsPageSelectedEventArgs e)
    {
        ISettingsPage settingsPage = e.SettingsPage;
        panelCurrentSettingsPage.Content = settingsPage.GuiControl;
        RememberSelectedSettingsPage(settingsPage);

        if (settingsPage.GuiControl is not Control control)
        {
            Text = _translatedTitle;
            return;
        }

        string title = settingsPage.GetTitle();
        if (settingsPage is PluginSettingsPage)
        {
            title = "Plugin: " + title;
        }

        Text = _translatedTitle + " - " + title;

        // Avalonia's previewer constructs the parameterless form without runtime services.
        if (_checkSettingsLogic is not null)
        {
            using (WaitCursorScope.Enter())
            {
                settingsPage.OnPageShown();
            }
        }

        labelInstantSaveNotice.IsVisible = settingsPage.IsInstantSavePage;
        buttonOk.IsEnabled = true;
        buttonCancel.IsEnabled = true;
        if (e.IsTriggeredByGoto)
        {
            control.Focus();
        }
    }

    private static void RememberSelectedSettingsPage(ISettingsPage settingsPage)
        => _lastSelectedSettingsPageType = settingsPage.GetType();

    private bool Save()
    {
        try
        {
            foreach (ISettingsPage settingsPage in SettingsPages)
            {
                settingsPage.SaveSettings();
            }

            if (_commonLogic is null)
            {
                return false;
            }

            _commonLogic.GitConfigSettingsSet.Save();
            _commonLogic.DistributedSettingsSet.Save();

            if (OperatingSystem.IsWindows())
            {
                FormFixHome.CheckHomePath();
            }

            // TODO: this method has a generic sounding name but only saves some specific settings
            AppSettings.SaveSettings();
            _saved = true;
            return true;
        }
        catch (SaveSettingsException exception) when (exception.InnerException is not null)
        {
            MessageBoxes.ShowError(this, exception.InnerException.Message, _cantSaveSettings.Text);
            return false;
        }
    }

    private void Ok_Click(object? sender, RoutedEventArgs e)
    {
        using (WaitCursorScope.Enter())
        {
            if (Save())
            {
                Close();
            }
        }
    }

    private void buttonCancel_Click(object? sender, RoutedEventArgs e)
        => Close();

    private void buttonDiscard_Click(object? sender, RoutedEventArgs e)
    {
        using (WaitCursorScope.Enter())
        {
            LoadSettings();
        }
    }

    private void buttonApply_Click(object? sender, RoutedEventArgs e)
    {
        using (WaitCursorScope.Enter())
        {
            Save();
        }
    }

    private void InitializeControls()
    {
        settingsTreeView.SettingsPageSelected += OnSettingsPageSelected;
        buttonOk.Click += Ok_Click;
        buttonCancel.Click += buttonCancel_Click;
        buttonDiscard.Click += buttonDiscard_Click;
        buttonApply.Click += buttonApply_Click;
        AcceptButton = buttonOk;

#if DEBUG
        buttonDiscard.IsVisible = true;
#endif
    }

    // parity-scaffolding: Exposes settings-shell lifecycle controls to the cross-platform parity tests.
    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormSettings form)
    {
        public SettingsTreeViewUserControl SettingsTreeView => form.settingsTreeView;

        public Control? CurrentPage => form.panelCurrentSettingsPage.Content as Control;

        public Button OkButton => form.buttonOk;

        public Button CancelButton => form.buttonCancel;

        public Button DiscardButton => form.buttonDiscard;

        public Button ApplyButton => form.buttonApply;

        public Control InstantSaveNotice => form.labelInstantSaveNotice;

        public void InitializePages() => form.InitializeSettingsPages();

        public void MarkSaved() => form._saved = true;

        public bool SaveSettings() => form.Save();
    }
}

internal sealed class EmptyServiceProvider : IServiceProvider
{
    public static EmptyServiceProvider Instance { get; } = new();

    private EmptyServiceProvider()
    {
    }

    public object? GetService(Type serviceType) => null;
}
