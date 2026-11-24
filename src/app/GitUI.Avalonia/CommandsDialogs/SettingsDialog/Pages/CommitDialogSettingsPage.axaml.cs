using Avalonia.Controls;
using GitCommands;
using GitExtensions.Extensibility.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class CommitDialogSettingsPage : SettingsPageWithHeader
{
    public CommitDialogSettingsPage()
        : this(EmptyServiceProvider.Instance)
    {
    }

    public CommitDialogSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        InitializeComplete();
    }

    protected override void SettingsToPage()
    {
        chkShowErrorsWhenStagingFiles.IsChecked = AppSettings.ShowErrorsWhenStagingFiles;
        chkEnsureCommitMessageSecondLineEmpty.IsChecked = AppSettings.EnsureCommitMessageSecondLineEmpty;
        chkWriteCommitMessageInCommitWindow.IsChecked = AppSettings.UseFormCommitMessage;
        _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Value = AppSettings.CommitDialogNumberOfPreviousMessages;
        chkShowCommitAndPush.IsChecked = AppSettings.ShowCommitAndPush;
        chkShowResetWorkTreeChanges.IsChecked = AppSettings.ShowResetWorkTreeChanges;
        chkShowResetAllChanges.IsChecked = AppSettings.ShowResetAllChanges;
        chkAutocomplete.IsChecked = AppSettings.ProvideAutocompletion;
        cbRememberAmendCommitState.IsChecked = AppSettings.RememberAmendCommitState;

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        AppSettings.ShowErrorsWhenStagingFiles = chkShowErrorsWhenStagingFiles.IsChecked == true;
        AppSettings.EnsureCommitMessageSecondLineEmpty = chkEnsureCommitMessageSecondLineEmpty.IsChecked == true;
        AppSettings.UseFormCommitMessage = chkWriteCommitMessageInCommitWindow.IsChecked == true;
        AppSettings.CommitDialogNumberOfPreviousMessages = (int)(_NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Value ?? 1);
        AppSettings.ShowCommitAndPush = chkShowCommitAndPush.IsChecked == true;
        AppSettings.ShowResetWorkTreeChanges = chkShowResetWorkTreeChanges.IsChecked == true;
        AppSettings.ShowResetAllChanges = chkShowResetAllChanges.IsChecked == true;
        AppSettings.ProvideAutocompletion = chkAutocomplete.IsChecked == true;
        AppSettings.RememberAmendCommitState = cbRememberAmendCommitState.IsChecked == true;

        base.PageToSettings();
    }

    public static SettingsPageReference GetPageReference()
        => new SettingsPageReferenceByType(typeof(CommitDialogSettingsPage));

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(CommitDialogSettingsPage page)
    {
        internal CheckBox ShowErrorsWhenStagingFiles => page.chkShowErrorsWhenStagingFiles;
        internal CheckBox EnsureSecondLineEmpty => page.chkEnsureCommitMessageSecondLineEmpty;
        internal CheckBox WriteMessageInCommitWindow => page.chkWriteCommitMessageInCommitWindow;
        internal NumericUpDown PreviousMessages => page._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages;
        internal CheckBox ShowCommitAndPush => page.chkShowCommitAndPush;
        internal CheckBox ShowResetWorkTreeChanges => page.chkShowResetWorkTreeChanges;
        internal CheckBox ShowResetAllChanges => page.chkShowResetAllChanges;
        internal CheckBox Autocomplete => page.chkAutocomplete;
        internal CheckBox RememberAmendState => page.cbRememberAmendCommitState;
    }
}
