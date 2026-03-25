using GitCommands;
using GitCommands.Utils;
using GitUI.CommandsDialogs.SettingsDialog.ShellExtension;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class ShellExtensionSettingsPage : SettingsPageWithHeader
{
    private const char Checked_InMenu = '0';
    private const char Indeterminate_InSubMenu = '1';
    private const char Unchecked_NotInMenu = '2';

    private readonly TranslationString _noItems = new("no items");
    private readonly TranslationString _menuHelp = new(@"* Checked: at top level for direct access
* Intermediate: in a cascaded context menu
* Unchecked: not added to the menu");

    private bool _isLoading = false;
    public ShellExtensionSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        UpdateRegistrationStatus();
        InitializeComplete();

        // when the dock is set in the designer it causes weird visual artifacts in scaled Windows environments
        _NO_TRANSLATE_chlMenuEntries.Dock = DockStyle.Fill;

        toolTip1.SetToolTip(menuHelp, _menuHelp.Text);
    }

    protected override void SettingsToPage()
    {
        _isLoading = true;
        for (int i = 0; i < AppSettings.CascadeShellMenuItems.Length; i++)
        {
            switch (AppSettings.CascadeShellMenuItems[i])
            {
                case Checked_InMenu:
                    _NO_TRANSLATE_chlMenuEntries.SetItemCheckState(i, CheckState.Checked);
                    break;
                case Indeterminate_InSubMenu:
                    _NO_TRANSLATE_chlMenuEntries.SetItemCheckState(i, CheckState.Indeterminate);
                    break;
                case Unchecked_NotInMenu:
                    _NO_TRANSLATE_chlMenuEntries.SetItemCheckState(i, CheckState.Unchecked);
                    break;
            }
        }

        _isLoading = false;

        cbAlwaysShowAllCommands.Checked = AppSettings.AlwaysShowAllCommands;

        UpdatePreview();

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        string l_CascadeShellMenuItems = "";

        for (int i = 0; i < _NO_TRANSLATE_chlMenuEntries.Items.Count; i++)
        {
            switch (_NO_TRANSLATE_chlMenuEntries.GetItemCheckState(i))
            {
                case CheckState.Indeterminate:
                    l_CascadeShellMenuItems += Indeterminate_InSubMenu;
                    break;
                case CheckState.Checked:
                    l_CascadeShellMenuItems += Checked_InMenu;
                    break;
                case CheckState.Unchecked:
                    l_CascadeShellMenuItems += Unchecked_NotInMenu;
                    break;
            }
        }

        AppSettings.CascadeShellMenuItems = l_CascadeShellMenuItems;
        AppSettings.AlwaysShowAllCommands = cbAlwaysShowAllCommands.Checked;

        base.PageToSettings();
    }

    private void chlMenuEntries_SelectedValueChanged(object sender, EventArgs e)
    {
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        string topLevel = "";
        string cascaded = "";

        for (int i = 0; i < _NO_TRANSLATE_chlMenuEntries.Items.Count; i++)
        {
            switch (_NO_TRANSLATE_chlMenuEntries.GetItemCheckState(i))
            {
                case CheckState.Checked:
                    topLevel += "GitExt " + _NO_TRANSLATE_chlMenuEntries.Items[i] + "\r\n";
                    break;
                case CheckState.Indeterminate:
                    cascaded += "       " + _NO_TRANSLATE_chlMenuEntries.Items[i] + "\r\n";
                    break;
            }
        }

        labelPreview.Text = topLevel;
        if (!string.IsNullOrWhiteSpace(cascaded))
        {
            labelPreview.Text += "Git Extensions > \r\n" + cascaded;
        }
        else if (string.IsNullOrWhiteSpace(topLevel))
        {
            labelPreview.Text += $"({_noItems.Text})";
        }
    }

    /// <summary>
    /// Handles the Click event of the RegisterButton control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    /// <remarks>
    /// This method attempts a tiered registration approach:
    /// <list type="number">
    /// <item>
    /// It first attempts to register the <see cref="ModernShellExtensionManager"/>,
    /// which supports the Windows 11 sparse package context menu.
    /// </item>
    /// <item>
    /// If modern registration fails or is not supported, it falls back to the
    /// <see cref="ShellExtensionManager"/> for the classic COM-based context menu.
    /// </item>
    /// </list>
    /// Finally, it calls <see cref="UpdateRegistrationStatus"/> to refresh the UI state.
    /// </remarks>
    private void RegisterButton_Click(object sender, EventArgs e)
    {
        ModernShellExtensionManager.Register();
        if (!ModernShellExtensionManager.IsRegistered())
        {
            ShellExtensionManager.Register();
        }

        UpdateRegistrationStatus();
    }

    /// <summary>
    /// Handles the Click event of the UnregisterButton control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    /// <remarks>
    /// This method performs a full cleanup of shell extensions by:
    /// <list type="bullet">
    /// <item>
    /// Removing the classic COM-based shell extension via <see cref="ShellExtensionManager"/>.
    /// </item>
    /// <item>
    /// Removing the modern sparse package registration via <see cref="ModernShellExtensionManager"/>.
    /// </item>
    /// </list>
    /// After attempting unregistration for both types, <see cref="UpdateRegistrationStatus"/>
    /// is called to ensure the UI reflects the current system state.
    /// </remarks>
    private void UnregisterButton_Click(object sender, EventArgs e)
    {
        if (ShellExtensionManager.IsRegistered())
        {
            ShellExtensionManager.Unregister();
        }

        if (ModernShellExtensionManager.IsRegistered())
        {
            ModernShellExtensionManager.Unregister();
        }

        UpdateRegistrationStatus();
    }

    /// <summary>
    /// Updates the UI state of registration-related controls based on the current system
    /// configuration and OS version.
    /// </summary>
    /// <remarks>
    /// This method evaluates the availability of shell extension files and their current
    /// registration status to toggle UI elements:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <see cref="gbExplorerIntegration"/>: Enabled only if legacy files exist and
    /// the OS is older than Windows 11.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <see cref="RegisterButton"/>: Disabled if either the classic (<see cref="ShellExtensionManager"/>)
    /// or modern (<see cref="ModernShellExtensionManager"/>) extension is already registered.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    private void UpdateRegistrationStatus()
    {
        bool legacyFilesExist = ShellExtensionManager.FilesExist();
        bool modernFilesExist = ModernShellExtensionManager.FilesExist();
        gbExplorerIntegration.Enabled = legacyFilesExist || modernFilesExist;

        bool legacyRegistered = ShellExtensionManager.IsRegistered();
        bool modernRegistered = ModernShellExtensionManager.IsRegistered();
        RegisterButton.Enabled = !(legacyRegistered || modernRegistered);
    }

    private void chlMenuEntries_ItemCheck(object sender, ItemCheckEventArgs e)
    {
        if (_isLoading)
        {
            return;
        }

        switch (e.CurrentValue)
        {
            case CheckState.Checked:
                e.NewValue = CheckState.Unchecked;
                break;

            case CheckState.Indeterminate:
                e.NewValue = CheckState.Checked;
                break;

            case CheckState.Unchecked:
                e.NewValue = CheckState.Indeterminate;
                break;
        }
    }

    private void menuHelp_Click(object sender, EventArgs e)
        => OsShellUtil.OpenUrlInDefaultBrowser(UserManual.UserManual.UrlFor("settings", "shell-extension"));
}
