using GitCommands;
using GitExtensions.Extensibility.Settings;
using GitExtUtils;
using GitUI.Hotkey;
using GitUI.Shells;
using ResourceManager;
using ResourceManager.Hotkey;

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
    private readonly ShellProvider _shellProvider = new();
    private int _cboTerminalPreviousIndex = -1;

    public FormBrowseRepoSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        cboTerminal.DisplayMember = "Name";
        InitializeComplete();
        string hotkey = serviceProvider.GetRequiredService<IHotkeySettingsManager>()
            .LoadHotkeys(FormBrowse.HotkeySettingsName)
            .GetShortcutDisplay(FormBrowse.Command.FocusOutputHistoryAndToggleIfPanel);
        chkShowOutputHistoryAsTab.ToolTipText = string.Format(_outputHistoryTooltip.Text, hotkey);
    }

    protected override void Init(ISettingsPageHost pageHost)
    {
        base.Init(pageHost);
    }

    protected override void OnRuntimeLoad()
    {
        // align 1st columns across all tables
        tlpnlGeneral.AdjustWidthToSize(0, lblDefaultShell, chkUseBrowseForFileHistory, chkUseDiffViewerForBlame, chkShowFindInCommitFilesGitGrep, chkShowConsoleTab, chkShowGpgInformation);
        tlpnlTabs.AdjustWidthToSize(0, lblDefaultShell, chkUseBrowseForFileHistory, chkUseDiffViewerForBlame, chkShowFindInCommitFilesGitGrep, chkShowConsoleTab, chkShowGpgInformation);

        base.OnRuntimeLoad();
    }

    protected override void PageToSettings()
    {
        AppSettings.ShowConEmuTab.Value = chkShowConsoleTab.Checked;
        AppSettings.UseBrowseForFileHistory.Value = chkUseBrowseForFileHistory.Checked;
        AppSettings.UseDiffViewerForBlame.Value = chkUseDiffViewerForBlame.Checked;
        AppSettings.ShowGpgInformation.Value = chkShowGpgInformation.Checked;
        AppSettings.ShowFindInCommitFilesGitGrep.Value = chkShowFindInCommitFilesGitGrep.Checked;

        int outputHistoryDepth = (int)_NO_TRANSLATE_OutputHistoryDepth.Value;
        bool changed = AppSettings.ShowOutputHistoryAsTab.Value != chkShowOutputHistoryAsTab.Checked || AppSettings.OutputHistoryDepth.Value != outputHistoryDepth;
        if (changed)
        {
            AppSettings.ShowOutputHistoryAsTab.Value = chkShowOutputHistoryAsTab.Checked;
            AppSettings.OutputHistoryDepth.Value = outputHistoryDepth;
            AppSettings.OutputHistoryPanelVisible.Value = !chkShowOutputHistoryAsTab.Checked && outputHistoryDepth > 0;
        }

        AppSettings.ConEmuTerminal.Value = ((IShellDescriptor)cboTerminal.SelectedItem).Name.ToLowerInvariant();

        base.PageToSettings();
    }

    protected override void SettingsToPage()
    {
        chkShowConsoleTab.Checked = AppSettings.ShowConEmuTab.Value;
        chkUseBrowseForFileHistory.Checked = AppSettings.UseBrowseForFileHistory.Value;
        chkUseDiffViewerForBlame.Checked = AppSettings.UseDiffViewerForBlame.Value;
        chkShowGpgInformation.Checked = AppSettings.ShowGpgInformation.Value;
        chkShowFindInCommitFilesGitGrep.Checked = AppSettings.ShowFindInCommitFilesGitGrep.Value;
        chkShowOutputHistoryAsTab.Checked = AppSettings.ShowOutputHistoryAsTab.Value;
        _NO_TRANSLATE_OutputHistoryDepth.Value = Math.Clamp(AppSettings.OutputHistoryDepth.Value, _NO_TRANSLATE_OutputHistoryDepth.Minimum, _NO_TRANSLATE_OutputHistoryDepth.Maximum);

        foreach (IShellDescriptor shell in _shellProvider.GetShells())
        {
            cboTerminal.Items.Add(shell);

            if (string.Equals(shell.Name, AppSettings.ConEmuTerminal.Value, StringComparison.InvariantCultureIgnoreCase))
            {
                cboTerminal.SelectedItem = shell;
            }
        }

        base.SettingsToPage();
    }

    public static SettingsPageReference GetPageReference()
    {
        return new SettingsPageReferenceByType(typeof(FormBrowseRepoSettingsPage));
    }

    private void cboTerminal_SelectionChangeCommitted(object sender, EventArgs e)
    {
        if (!(cboTerminal.SelectedItem is IShellDescriptor shell))
        {
            return;
        }

        if (shell.HasExecutable)
        {
            return;
        }

        MessageBoxes.ShellNotFound(this);
        cboTerminal.SelectedIndex = _cboTerminalPreviousIndex;
    }

    private void cboTerminal_Enter(object sender, EventArgs e)
    {
        _cboTerminalPreviousIndex = cboTerminal.SelectedIndex;
    }
}
