using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using GitCommands;
using GitExtensions.Extensibility.Settings;
using GitUI.Hotkey;
using ResourceManager;
using ResourceManager.Hotkey;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class FormBrowseRepoSettingsPage : SettingsPageWithHeader
{
    private readonly TranslationString _outputHistoryTooltip
        = new("""
              The output displayed in the process dialog and some trace output is retained and shown in the output history.

              - With this set, the output history is displayed in a tab in the lower pane of the Browse Repository window.
              - With this unset, the output history is displayed in a panel docked to the lower left corner of the Browse Repository window.

              Focus the output history and (when displayed as panel) toggle its visibility using the hotkey {0}.
              """);
    private readonly ShellOption[] _shells =
    [
        new("bash", "git-bash.exe", "bash.exe", "sh.exe"),
        new("cmd", "cmd.exe"),
        new("pwsh", "pwsh.exe"),
        new("powershell", "powershell.exe"),
    ];
    private int _cboTerminalPreviousIndex = -1;

    public FormBrowseRepoSettingsPage()
        : this(EmptyServiceProvider.Instance)
    {
    }

    public FormBrowseRepoSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        cboTerminal.ItemsSource = _shells;
        cboTerminal.SelectionChanged += cboTerminal_SelectionChangeCommitted;
        cboTerminal.GotFocus += cboTerminal_Enter;
        InitializeComplete();
        string hotkey = serviceProvider.GetService(typeof(IHotkeySettingsManager)) is IHotkeySettingsManager manager
            ? manager.LoadHotkeys(FormBrowse.HotkeySettingsName)
                .GetShortcutDisplay(FormBrowse.Command.FocusOutputHistoryAndToggleIfPanel)
            : string.Empty;
        chkShowOutputHistoryAsTab.ToolTipText = string.Format(_outputHistoryTooltip.Text, hotkey);
    }

    protected override void Init(ISettingsPageHost pageHost)
    {
        base.Init(pageHost);
    }

    protected void OnRuntimeLoad()
    {
        // Avalonia Grid shares the first-column definition across every row in each table.
        // The two grids use the same Auto-sized label/control vocabulary and need no imperative width adjustment.
    }

    protected override void PageToSettings()
    {
        AppSettings.ShowConEmuTab.Value = chkShowConsoleTab.Checked;
        AppSettings.UseBrowseForFileHistory.Value = chkUseBrowseForFileHistory.Checked;
        AppSettings.UseDiffViewerForBlame.Value = chkUseDiffViewerForBlame.Checked;
        AppSettings.ShowGpgInformation.Value = chkShowGpgInformation.Checked;
        AppSettings.ShowFindInCommitFilesGitGrep.Value = chkShowFindInCommitFilesGitGrep.Checked;
        AppSettings.ShowRevisionGridTooltips.Value = chkShowRevisionGridTooltip.Checked;

        int outputHistoryDepth = (int)(_NO_TRANSLATE_OutputHistoryDepth.Value ?? 0);
        bool changed = AppSettings.ShowOutputHistoryAsTab.Value != chkShowOutputHistoryAsTab.Checked || AppSettings.OutputHistoryDepth.Value != outputHistoryDepth;
        if (changed)
        {
            AppSettings.ShowOutputHistoryAsTab.Value = chkShowOutputHistoryAsTab.Checked;
            AppSettings.OutputHistoryDepth.Value = outputHistoryDepth;
            AppSettings.OutputHistoryPanelVisible.Value = !chkShowOutputHistoryAsTab.Checked && outputHistoryDepth > 0;
        }

        if (cboTerminal.SelectedItem is ShellOption shell)
        {
            AppSettings.ConEmuTerminal.Value = shell.Name.ToLowerInvariant();
        }

        base.PageToSettings();
    }

    protected override void SettingsToPage()
    {
        chkShowConsoleTab.Checked = AppSettings.ShowConEmuTab.Value;
        chkUseBrowseForFileHistory.Checked = AppSettings.UseBrowseForFileHistory.Value;
        chkUseDiffViewerForBlame.Checked = AppSettings.UseDiffViewerForBlame.Value;
        chkShowGpgInformation.Checked = AppSettings.ShowGpgInformation.Value;
        chkShowFindInCommitFilesGitGrep.Checked = AppSettings.ShowFindInCommitFilesGitGrep.Value;
        chkShowRevisionGridTooltip.Checked = AppSettings.ShowRevisionGridTooltips.Value;
        chkShowOutputHistoryAsTab.Checked = AppSettings.ShowOutputHistoryAsTab.Value;
        _NO_TRANSLATE_OutputHistoryDepth.Value = Math.Clamp(
            AppSettings.OutputHistoryDepth.Value,
            (int)_NO_TRANSLATE_OutputHistoryDepth.Minimum,
            (int)_NO_TRANSLATE_OutputHistoryDepth.Maximum);

        cboTerminal.SelectedItem = _shells.FirstOrDefault(shell =>
            string.Equals(shell.Name, AppSettings.ConEmuTerminal.Value, StringComparison.InvariantCultureIgnoreCase))
            ?? _shells[0];

        base.SettingsToPage();
    }

    public static SettingsPageReference GetPageReference()
    {
        return new SettingsPageReferenceByType(typeof(FormBrowseRepoSettingsPage));
    }

    private void cboTerminal_SelectionChangeCommitted(object? sender, SelectionChangedEventArgs e)
    {
        if (IsLoadingSettings || cboTerminal.SelectedItem is not ShellOption shell)
        {
            return;
        }

        if (shell.HasExecutable)
        {
            return;
        }

        MessageBoxes.ShellNotFound(TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window);
        cboTerminal.SelectedIndex = _cboTerminalPreviousIndex;
    }

    private void cboTerminal_Enter(object? sender, RoutedEventArgs e)
    {
        _cboTerminalPreviousIndex = cboTerminal.SelectedIndex;
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormBrowseRepoSettingsPage page)
    {
        internal ComboBox Terminal => page.cboTerminal;
        internal GitUI.UserControls.Settings.SettingsCheckBox ShowConsoleTab => page.chkShowConsoleTab;
        internal GitUI.UserControls.Settings.SettingsCheckBox UseBrowseForFileHistory => page.chkUseBrowseForFileHistory;
        internal GitUI.UserControls.Settings.SettingsCheckBox UseDiffViewerForBlame => page.chkUseDiffViewerForBlame;
        internal GitUI.UserControls.Settings.SettingsCheckBox ShowGpgInformation => page.chkShowGpgInformation;
        internal GitUI.UserControls.Settings.SettingsCheckBox ShowGitGrep => page.chkShowFindInCommitFilesGitGrep;
        internal GitUI.UserControls.Settings.SettingsCheckBox ShowRevisionGridTooltip => page.chkShowRevisionGridTooltip;
        internal GitUI.UserControls.Settings.SettingsCheckBox ShowOutputHistoryAsTab => page.chkShowOutputHistoryAsTab;
        internal NumericUpDown OutputHistoryDepth => page._NO_TRANSLATE_OutputHistoryDepth;
    }

    private sealed record ShellOption(string Name, params string[] ExecutableNames)
    {
        internal bool HasExecutable => ExecutableNames.Any(name => PathUtil.TryFindShellPath(name, out _));

        public override string ToString() => Name;
    }
}
