using GitCommands;
using GitExtensions.Extensibility.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class AiFilterSettingsPage : SettingsPageWithHeader
{
    public AiFilterSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        InitializeComplete();
    }

    public static SettingsPageReference GetPageReference()
    {
        return new SettingsPageReferenceByType(typeof(AiFilterSettingsPage));
    }

    protected override void SettingsToPage()
    {
        cboBackend.SelectedIndex = (int)AppSettings.AiFilterBackend.Value;
        txtClaudeCodeExecutable.Text = AppSettings.AiFilterClaudeCodeExecutable.Value;
        txtEndpoint.Text = AppSettings.AiFilterEndpoint.Value;
        txtModel.Text = AppSettings.AiFilterModel.Value;
        txtApiKey.Text = AppSettings.AiFilterApiKey.Value;
        txtAnthropicVersion.Text = AppSettings.AiFilterAnthropicVersion.Value;
        numMaxDiffChars.Value = Math.Clamp(AppSettings.AiFilterMaxDiffCharsPerFile.Value, (int)numMaxDiffChars.Minimum, (int)numMaxDiffChars.Maximum);

        chkImports.Checked = AppSettings.AiFilterImports.Value;
        chkCallerRenames.Checked = AppSettings.AiFilterCallerSiteRenames.Value;
        chkSyncToAsync.Checked = AppSettings.AiFilterSyncToAsync.Value;
        chkStyleOnly.Checked = AppSettings.AiFilterStyleOnly.Value;

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        AppSettings.AiFilterBackend.Value = (AiFilterBackend)Math.Max(0, cboBackend.SelectedIndex);
        AppSettings.AiFilterClaudeCodeExecutable.Value = txtClaudeCodeExecutable.Text.Trim();
        AppSettings.AiFilterEndpoint.Value = txtEndpoint.Text.Trim();
        AppSettings.AiFilterModel.Value = txtModel.Text.Trim();
        AppSettings.AiFilterApiKey.Value = txtApiKey.Text.Trim();
        AppSettings.AiFilterAnthropicVersion.Value = txtAnthropicVersion.Text.Trim();
        AppSettings.AiFilterMaxDiffCharsPerFile.Value = (int)numMaxDiffChars.Value;

        AppSettings.AiFilterImports.Value = chkImports.Checked;
        AppSettings.AiFilterCallerSiteRenames.Value = chkCallerRenames.Checked;
        AppSettings.AiFilterSyncToAsync.Value = chkSyncToAsync.Checked;
        AppSettings.AiFilterStyleOnly.Value = chkStyleOnly.Checked;

        base.PageToSettings();
    }
}
