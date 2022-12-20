using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class BlameViewerSettingsPage : SettingsPageWithHeader
    {
        public BlameViewerSettingsPage()
        {
            InitializeComponent();
            Text = "Blame Viewer";
            InitializeComplete();
        }

        protected override void SettingsToPage()
        {
            cbIgnoreWhitespace.Checked = AppSettings.IgnoreWhitespaceOnBlame;
            cbDetectMoveAndCopyInAllFiles.Checked = AppSettings.DetectCopyInAllOnBlame;
            cbDetectMoveAndCopyInThisFile.Checked = AppSettings.DetectCopyInFileOnBlame;

            cbDisplayAuthorFirst.Checked = AppSettings.BlameDisplayAuthorFirst;
            cbShowAuthor.Checked = AppSettings.BlameShowAuthor;
            cbShowAuthorDate.Checked = AppSettings.BlameShowAuthorDate;
            cbShowAuthorTime.Checked = AppSettings.BlameShowAuthorTime;
            cbShowLineNumbers.Checked = AppSettings.BlameShowLineNumbers;
            cbShowOriginalFilePath.Checked = AppSettings.BlameShowOriginalFilePath;
            cbShowAuthorAvatar.Checked = AppSettings.BlameShowAuthorAvatar;

            base.SettingsToPage();
        }

        protected override void PageToSettings()
        {
            AppSettings.IgnoreWhitespaceOnBlame = cbIgnoreWhitespace.Checked;
            AppSettings.DetectCopyInAllOnBlame = cbDetectMoveAndCopyInAllFiles.Checked;
            AppSettings.DetectCopyInFileOnBlame = cbDetectMoveAndCopyInThisFile.Checked;

            AppSettings.BlameDisplayAuthorFirst = cbDisplayAuthorFirst.Checked;
            AppSettings.BlameShowAuthor = cbShowAuthor.Checked;
            AppSettings.BlameShowAuthorDate = cbShowAuthorDate.Checked;
            AppSettings.BlameShowAuthorTime = cbShowAuthorTime.Checked;
            AppSettings.BlameShowLineNumbers = cbShowLineNumbers.Checked;
            AppSettings.BlameShowOriginalFilePath = cbShowOriginalFilePath.Checked;
            AppSettings.BlameShowAuthorAvatar = cbShowAuthorAvatar.Checked;

            base.PageToSettings();
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(BlameViewerSettingsPage));
        }
    }
}
