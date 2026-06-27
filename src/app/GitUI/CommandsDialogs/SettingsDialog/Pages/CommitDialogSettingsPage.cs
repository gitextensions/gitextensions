using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class CommitDialogSettingsPage : SettingsPageWithHeader
{
    // Quick-fill presets for the most common OpenAI-compatible providers. "Custom" leaves the URL
    // untouched; every preset only fills the fields below, which remain editable.
    private static readonly (string Name, string BaseUrl, string Model)[] _aiProviderPresets =
    [
        ("Custom", "", ""),
        ("OpenAI", "https://api.openai.com/v1", "gpt-4o-mini"),
        ("GitHub Models", "https://models.github.ai/inference", "openai/gpt-4o-mini"),
        ("Google Gemini", "https://generativelanguage.googleapis.com/v1beta/openai/", "gemini-3.5-flash"),
        ("OpenRouter", "https://openrouter.ai/api/v1", "openai/gpt-4o-mini"),
        ("Groq", "https://api.groq.com/openai/v1", "llama-3.3-70b-versatile"),
        ("Ollama (local)", "http://localhost:11434/v1", "llama3.2"),
    ];

    public CommitDialogSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        cboAiCommitMessageProvider.Items.AddRange([.. _aiProviderPresets.Select(p => (object)p.Name)]);
        InitializeComplete();
    }

    protected override void SettingsToPage()
    {
        chkShowErrorsWhenStagingFiles.Checked = AppSettings.ShowErrorsWhenStagingFiles;
        chkEnsureCommitMessageSecondLineEmpty.Checked = AppSettings.EnsureCommitMessageSecondLineEmpty;
        chkWriteCommitMessageInCommitWindow.Checked = AppSettings.UseFormCommitMessage;
        _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Value = AppSettings.CommitDialogNumberOfPreviousMessages;
        chkShowCommitAndPush.Checked = AppSettings.ShowCommitAndPush;
        chkShowResetWorkTreeChanges.Checked = AppSettings.ShowResetWorkTreeChanges;
        chkShowResetAllChanges.Checked = AppSettings.ShowResetAllChanges;
        chkAutocomplete.Checked = AppSettings.ProvideAutocompletion;
        cbRememberAmendCommitState.Checked = AppSettings.RememberAmendCommitState;

        chkAiCommitMessageEnabled.Checked = AppSettings.AiCommitMessageEnabled.Value;
        txtAiCommitMessageBaseUrl.Text = AppSettings.AiCommitMessageApiBaseUrl.Value;
        txtAiCommitMessageModel.Text = AppSettings.AiCommitMessageModel.Value;
        txtAiCommitMessageApiKey.Text = AppSettings.AiCommitMessageApiKey;
        txtAiCommitMessageSystemPrompt.Text = NormalizeNewLines(AppSettings.AiCommitMessageSystemPrompt.Value);
        numAiCommitMessageMaxDiff.Value = Math.Clamp((decimal)AppSettings.AiCommitMessageMaxDiffLength.Value, numAiCommitMessageMaxDiff.Minimum, numAiCommitMessageMaxDiff.Maximum);
        cboAiCommitMessageProvider.SelectedIndex = GetProviderPresetIndex(AppSettings.AiCommitMessageApiBaseUrl.Value);

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        AppSettings.ShowErrorsWhenStagingFiles = chkShowErrorsWhenStagingFiles.Checked;
        AppSettings.EnsureCommitMessageSecondLineEmpty = chkEnsureCommitMessageSecondLineEmpty.Checked;
        AppSettings.UseFormCommitMessage = chkWriteCommitMessageInCommitWindow.Checked;
        AppSettings.CommitDialogNumberOfPreviousMessages = (int)_NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Value;
        AppSettings.ShowCommitAndPush = chkShowCommitAndPush.Checked;
        AppSettings.ShowResetWorkTreeChanges = chkShowResetWorkTreeChanges.Checked;
        AppSettings.ShowResetAllChanges = chkShowResetAllChanges.Checked;
        AppSettings.ProvideAutocompletion = chkAutocomplete.Checked;
        AppSettings.RememberAmendCommitState = cbRememberAmendCommitState.Checked;

        AppSettings.AiCommitMessageEnabled.Value = chkAiCommitMessageEnabled.Checked;
        AppSettings.AiCommitMessageApiBaseUrl.Value = txtAiCommitMessageBaseUrl.Text.Trim();
        AppSettings.AiCommitMessageModel.Value = txtAiCommitMessageModel.Text.Trim();
        AppSettings.AiCommitMessageApiKey = txtAiCommitMessageApiKey.Text.Trim();
        AppSettings.AiCommitMessageSystemPrompt.Value = txtAiCommitMessageSystemPrompt.Text;
        AppSettings.AiCommitMessageMaxDiffLength.Value = (int)numAiCommitMessageMaxDiff.Value;

        base.PageToSettings();
    }

    private static int GetProviderPresetIndex(string? baseUrl)
    {
        string normalized = (baseUrl ?? string.Empty).Trim().TrimEnd('/');
        for (int i = 1; i < _aiProviderPresets.Length; i++)
        {
            if (string.Equals(_aiProviderPresets[i].BaseUrl, normalized, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return 0; // Custom
    }

    // A multiline TextBox only renders "\r\n" as a line break, but the stored or default prompt may
    // use bare "\n", so normalise to the platform newline before displaying it.
    private static string NormalizeNewLines(string value)
        => value.Replace("\r\n", "\n").Replace('\r', '\n').Replace("\n", Environment.NewLine);

    private void AiProvider_SelectedIndexChanged(object? sender, EventArgs e)
    {
        // Don't overwrite the user's saved values while the page is being loaded.
        if (IsLoadingSettings || cboAiCommitMessageProvider.SelectedIndex <= 0)
        {
            return;
        }

        (_, string baseUrl, string model) = _aiProviderPresets[cboAiCommitMessageProvider.SelectedIndex];
        txtAiCommitMessageBaseUrl.Text = baseUrl;
        if (!string.IsNullOrWhiteSpace(model))
        {
            txtAiCommitMessageModel.Text = model;
        }
    }
}
