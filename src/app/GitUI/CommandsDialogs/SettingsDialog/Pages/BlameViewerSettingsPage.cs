using GitCommands;
using GitExtensions.Extensibility.Settings;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class BlameViewerSettingsPage : SettingsPageWithHeader
{
    private readonly TranslationString _blameWarningTooltip = new("Could prevent blame to calculate the accurate line number when blaming previous revisions.");

    public BlameViewerSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        InitializeComplete();
        cbDetectMoveAndCopyInThisFile.ToolTipText = _blameWarningTooltip.Text;
        cbDetectMoveAndCopyInAllFiles.ToolTipText = _blameWarningTooltip.Text;
    }

    protected override void SettingsToPage()
    {
        cbIgnoreWhitespace.Checked = AppSettings.IgnoreWhitespaceOnBlame.Value;
        cbDetectMoveAndCopyInThisFile.Checked = AppSettings.DetectCopyInFileOnBlame.Value;
        cbDetectMoveAndCopyInAllFiles.Checked = AppSettings.DetectCopyInAllOnBlame.Value;

        cbDisplayAuthorFirst.Checked = AppSettings.BlameDisplayAuthorFirst.Value;
        cbShowAuthor.Checked = AppSettings.BlameShowAuthor.Value;
        cbShowAuthorDate.Checked = AppSettings.BlameShowAuthorDate.Value;
        cbShowAuthorTime.Checked = AppSettings.BlameShowAuthorTime.Value;
        cbShowLineNumbers.Checked = AppSettings.BlameShowLineNumbers.Value;
        cbShowOriginalFilePath.Checked = AppSettings.BlameShowOriginalFilePath.Value;
        cbShowAuthorAvatar.Checked = AppSettings.BlameShowAuthorAvatar.Value;

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        AppSettings.IgnoreWhitespaceOnBlame.Value = cbIgnoreWhitespace.Checked;
        AppSettings.DetectCopyInAllOnBlame.Value = cbDetectMoveAndCopyInAllFiles.Checked;
        AppSettings.DetectCopyInFileOnBlame.Value = cbDetectMoveAndCopyInThisFile.Checked;

        AppSettings.BlameDisplayAuthorFirst.Value = cbDisplayAuthorFirst.Checked;
        AppSettings.BlameShowAuthor.Value = cbShowAuthor.Checked;
        AppSettings.BlameShowAuthorDate.Value = cbShowAuthorDate.Checked;
        AppSettings.BlameShowAuthorTime.Value = cbShowAuthorTime.Checked;
        AppSettings.BlameShowLineNumbers.Value = cbShowLineNumbers.Checked;
        AppSettings.BlameShowOriginalFilePath.Value = cbShowOriginalFilePath.Checked;
        AppSettings.BlameShowAuthorAvatar.Value = cbShowAuthorAvatar.Checked;

        base.PageToSettings();
    }

    public static SettingsPageReference GetPageReference()
    {
        return new SettingsPageReferenceByType(typeof(BlameViewerSettingsPage));
    }
}
