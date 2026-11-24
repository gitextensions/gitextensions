using Avalonia.Controls;
using GitCommands;
using GitExtensions.Extensibility.Settings;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class BlameViewerSettingsPage : SettingsPageWithHeader
{
    private readonly TranslationString _blameWarningTooltip = new("Could prevent blame to calculate the accurate line number when blaming previous revisions.");

    public BlameViewerSettingsPage()
        : this(EmptyServiceProvider.Instance)
    {
    }

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
        cbIgnoreWhitespace.IsChecked = AppSettings.IgnoreWhitespaceOnBlame;
        cbDetectMoveAndCopyInThisFile.Checked = AppSettings.DetectCopyInFileOnBlame;
        cbDetectMoveAndCopyInAllFiles.Checked = AppSettings.DetectCopyInAllOnBlame;

        cbDisplayAuthorFirst.IsChecked = AppSettings.BlameDisplayAuthorFirst;
        cbShowAuthor.IsChecked = AppSettings.BlameShowAuthor;
        cbShowAuthorDate.IsChecked = AppSettings.BlameShowAuthorDate;
        cbShowAuthorTime.IsChecked = AppSettings.BlameShowAuthorTime;
        cbShowLineNumbers.IsChecked = AppSettings.BlameShowLineNumbers;
        cbShowOriginalFilePath.IsChecked = AppSettings.BlameShowOriginalFilePath;
        cbShowAuthorAvatar.IsChecked = AppSettings.BlameShowAuthorAvatar;

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        AppSettings.IgnoreWhitespaceOnBlame = cbIgnoreWhitespace.IsChecked == true;
        AppSettings.DetectCopyInAllOnBlame = cbDetectMoveAndCopyInAllFiles.Checked;
        AppSettings.DetectCopyInFileOnBlame = cbDetectMoveAndCopyInThisFile.Checked;

        AppSettings.BlameDisplayAuthorFirst = cbDisplayAuthorFirst.IsChecked == true;
        AppSettings.BlameShowAuthor = cbShowAuthor.IsChecked == true;
        AppSettings.BlameShowAuthorDate = cbShowAuthorDate.IsChecked == true;
        AppSettings.BlameShowAuthorTime = cbShowAuthorTime.IsChecked == true;
        AppSettings.BlameShowLineNumbers = cbShowLineNumbers.IsChecked == true;
        AppSettings.BlameShowOriginalFilePath = cbShowOriginalFilePath.IsChecked == true;
        AppSettings.BlameShowAuthorAvatar = cbShowAuthorAvatar.IsChecked == true;

        base.PageToSettings();
    }

    public static SettingsPageReference GetPageReference()
    {
        return new SettingsPageReferenceByType(typeof(BlameViewerSettingsPage));
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(BlameViewerSettingsPage page)
    {
        internal CheckBox IgnoreWhitespace => page.cbIgnoreWhitespace;
        internal GitUI.UserControls.Settings.SettingsCheckBox DetectCopyInFile => page.cbDetectMoveAndCopyInThisFile;
        internal GitUI.UserControls.Settings.SettingsCheckBox DetectCopyInAll => page.cbDetectMoveAndCopyInAllFiles;
        internal CheckBox DisplayAuthorFirst => page.cbDisplayAuthorFirst;
        internal CheckBox ShowAuthor => page.cbShowAuthor;
        internal CheckBox ShowAuthorDate => page.cbShowAuthorDate;
        internal CheckBox ShowAuthorTime => page.cbShowAuthorTime;
        internal CheckBox ShowLineNumbers => page.cbShowLineNumbers;
        internal CheckBox ShowOriginalFilePath => page.cbShowOriginalFilePath;
        internal CheckBox ShowAuthorAvatar => page.cbShowAuthorAvatar;
    }
}
