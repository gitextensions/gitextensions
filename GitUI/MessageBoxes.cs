using GitCommands;
using GitCommands.Config;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI
{
    public class MessageBoxes : Translate
    {
        private readonly TranslationString _cannotFindRevisionFilter = new(@"Revision ""{0}"" is not visible in the revision grid. Remove the revision filter.");
        private readonly TranslationString _cannotFindRevisionCaption = new("Cannot find revision");
        private readonly TranslationString _noRevisionFoundError = new("No revision found.");

        private readonly TranslationString _archiveRevisionCaption = new("Archive revision");

        private readonly TranslationString _failedToRunShell = new("Failed to run shell");

        private readonly TranslationString _notValidGitDirectory = new("The current directory is not a valid git repository.");

        private readonly TranslationString _unresolvedMergeConflictsCaption = new("Merge conflicts");
        private readonly TranslationString _unresolvedMergeConflicts = new("There are unresolved merge conflicts, solve conflicts now?");

        private readonly TranslationString _middleOfRebaseCaption = new("Rebase");
        private readonly TranslationString _middleOfRebase = new("You are in the middle of a rebase, continue rebase?");

        private readonly TranslationString _middleOfPatchApplyCaption = new("Patch apply");
        private readonly TranslationString _middleOfPatchApply = new("You are in the middle of a patch apply, continue patch apply?");

        private readonly TranslationString _serverHostkeyNotCachedText =
            new("The server's host key is not cached in the registry.\n\nDo you want to trust this host key and then try again?");

        private readonly TranslationString _updateSubmodules = new("Update submodules");
        private readonly TranslationString _theRepositorySubmodules = new("Update submodules on checkout?");
        private readonly TranslationString _updateSubmodulesToo = new("Since this repository has submodules, it's necessary to update them on every checkout.\r\n\r\nThis will just checkout on the submodule the commit determined by the superproject.");
        private readonly TranslationString _rememberChoice = new("Remember choice");

        private readonly TranslationString _reason = new("Reason");

        private readonly TranslationString _selectOnlyOneOrTwoRevisions = new("Select only one or two revisions. Abort.");

        private readonly TranslationString _shellNotFoundCaption = new("Shell not found");
        private readonly TranslationString _shellNotFound = new("The selected shell is not installed, or is not on your path.");

        private readonly TranslationString _submoduleDirectoryDoesNotExist = new(@"The directory ""{0}"" does not exist for submodule ""{1}"".");
        private readonly TranslationString _directoryDoesNotExist = new(@"The directory ""{0}"" does not exist.");
        private readonly TranslationString _cannotOpenSubmoduleCaption = new("Cannot open submodule");
        private readonly TranslationString _cannotOpenGitExtensionsCaption = new("Cannot open Git Extensions");

        // internal for FormTranslate
        internal MessageBoxes()
        {
            Translator.Translate(this, AppSettings.CurrentTranslation);
        }

        private static MessageBoxes? instance;

        private static MessageBoxes Instance => instance ??= new();

        public static void RevisionFilteredInGrid(IWin32Window? owner, ObjectId objectId)
            => ShowError(owner, string.Format(Instance._cannotFindRevisionFilter.Text, objectId.ToShortString()), Instance._cannotFindRevisionCaption.Text);

        public static void CannotFindGitRevision(IWin32Window? owner)
            => ShowError(owner, Instance._noRevisionFoundError.Text, Instance._cannotFindRevisionCaption.Text);

        public static void FailedToRunShell(IWin32Window? owner, string shell, Exception ex)
            => ShowError(owner, $"{Instance._failedToRunShell.Text} {shell.Quote()}.{Environment.NewLine}"
                                + $"{Instance._reason.Text}: {ex.Message}");

        public static void NotValidGitDirectory(IWin32Window? owner)
            => ShowError(owner, Instance._notValidGitDirectory.Text);

        public static void ShowGitConfigurationExceptionMessage(IWin32Window? owner, GitConfigurationException exception)
            => Show(owner,
                    string.Format(ResourceManager.TranslatedStrings.GeneralGitConfigExceptionMessage,
                                  exception.ConfigPath, Environment.NewLine, (exception.InnerException ?? exception).Message),
                    ResourceManager.TranslatedStrings.GeneralGitConfigExceptionCaption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

        public static bool MiddleOfRebase(IWin32Window? owner)
            => Confirm(owner, Instance._middleOfRebase.Text, Instance._middleOfRebaseCaption.Text);

        public static bool MiddleOfPatchApply(IWin32Window? owner)
            => Confirm(owner, Instance._middleOfPatchApply.Text, Instance._middleOfPatchApplyCaption.Text);

        public static void SelectOnlyOneOrTwoRevisions(IWin32Window? owner)
            => ShowError(owner, Instance._selectOnlyOneOrTwoRevisions.Text, Instance._archiveRevisionCaption.Text);

        public static void SubmoduleDirectoryDoesNotExist(IWin32Window? owner, string directory, string submoduleName)
            => ShowError(owner, string.Format(Instance._submoduleDirectoryDoesNotExist.Text, directory, submoduleName), Instance._cannotOpenSubmoduleCaption.Text);

        public static void SubmoduleDirectoryDoesNotExist(IWin32Window? owner, string directory)
            => ShowError(owner, string.Format(Instance._directoryDoesNotExist.Text, directory), Instance._cannotOpenSubmoduleCaption.Text);

        public static void GitExtensionsDirectoryDoesNotExist(IWin32Window? owner, string directory)
            => ShowError(owner, string.Format(Instance._directoryDoesNotExist.Text, directory), Instance._cannotOpenGitExtensionsCaption.Text);

        public static bool CacheHostkey(IWin32Window? owner)
            => Confirm(owner, Instance._serverHostkeyNotCachedText.Text, "SSH");

        public static bool ConfirmResolveMergeConflicts(IWin32Window? owner)
            => Confirm(owner, Instance._unresolvedMergeConflicts.Text, Instance._unresolvedMergeConflictsCaption.Text);

        public static bool ConfirmUpdateSubmodules(IWin32Window? owner)
        {
            TaskDialogPage page = new()
            {
                Text = Instance._updateSubmodulesToo.Text,
                Heading = Instance._theRepositorySubmodules.Text,
                Caption = Instance._updateSubmodules.Text,
                Icon = TaskDialogIcon.Information,
                Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
                Verification = new TaskDialogVerificationCheckBox
                {
                    Text = Instance._rememberChoice.Text
                },
                SizeToContent = true
            };

            bool result = TaskDialog.ShowDialog(owner?.Handle ?? IntPtr.Zero, page) == TaskDialogButton.Yes;
            if (page.Verification.Checked)
            {
                AppSettings.DontConfirmUpdateSubmodulesOnCheckout = result;
                AppSettings.UpdateSubmodulesOnCheckout = result;
            }

            return result;
        }

        public static void ShellNotFound(IWin32Window? owner)
            => ShowError(owner, Instance._shellNotFound.Text, Instance._shellNotFoundCaption.Text);

        public static void ShowError(IWin32Window? owner, string text, string? caption = null)
            => Show(owner, text, caption ?? TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);

        private static bool Confirm(IWin32Window? owner, string text, string caption)
            => Show(owner, text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

        private static DialogResult Show(IWin32Window? owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
            => MessageBox.Show(owner, text, caption, buttons, icon);
    }
}
