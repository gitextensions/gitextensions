using System;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using Microsoft.WindowsAPICodePack.Dialogs;
using ResourceManager;

namespace GitUI
{
    public class MessageBoxes : Translate
    {
        private readonly TranslationString _archiveRevisionCaption = new("Archive revision");

        private readonly TranslationString _failedToExecuteScript = new("Failed to execute script");

        private readonly TranslationString _failedToRunShell = new("Failed to run shell");

        private readonly TranslationString _notValidGitDirectory = new("The current directory is not a valid git repository.");

        private readonly TranslationString _unresolvedMergeConflictsCaption = new("Merge conflicts");
        private readonly TranslationString _unresolvedMergeConflicts = new("There are unresolved merge conflicts, solve conflicts now?");

        private readonly TranslationString _middleOfRebaseCaption = new("Rebase");
        private readonly TranslationString _middleOfRebase = new("You are in the middle of a rebase, continue rebase?");

        private readonly TranslationString _middleOfPatchApplyCaption = new("Patch apply");
        private readonly TranslationString _middleOfPatchApply = new("You are in the middle of a patch apply, continue patch apply?");

        private const string _putty = "PuTTY";
        private readonly TranslationString _pageantNotFound = new("Cannot load SSH key. PuTTY is not configured properly.");

        private readonly TranslationString _serverHostkeyNotCachedText =
            new TranslationString("The server's host key is not cached in the registry.\n\nDo you want to trust this host key and then try again?");

        private readonly TranslationString _updateSubmodules = new("Update submodules");
        private readonly TranslationString _theRepositorySubmodules = new("Update submodules on checkout?");
        private readonly TranslationString _updateSubmodulesToo = new("Since this repository has submodules, it's necessary to update them on every checkout.\r\n\r\nThis will just checkout on the submodule the commit determined by the superproject.");
        private readonly TranslationString _rememberChoice = new("Remember choice");

        private readonly TranslationString _reason = new("Reason");

        private readonly TranslationString _selectOnlyOneOrTwoRevisions = new("Select only one or two revisions. Abort.");

        private readonly TranslationString _shellNotFoundCaption = new("Shell not found");
        private readonly TranslationString _shellNotFound = new("The selected shell is not installed, or is not on your path.");
        private readonly TranslationString _resetChangesCaption = new("Reset changes");

        // internal for FormTranslate
        internal MessageBoxes()
        {
            Translator.Translate(this, AppSettings.CurrentTranslation);
        }

        private static MessageBoxes? instance;

        private static MessageBoxes Instance => instance ??= new();

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

        public static void PAgentNotFound(IWin32Window? owner)
            => ShowError(owner, Instance._pageantNotFound.Text, _putty);

        public static void SelectOnlyOneOrTwoRevisions(IWin32Window? owner)
            => ShowError(owner, Instance._selectOnlyOneOrTwoRevisions.Text, Instance._archiveRevisionCaption.Text);

        public static bool CacheHostkey(IWin32Window? owner)
            => Confirm(owner, Instance._serverHostkeyNotCachedText.Text, "SSH");

        public static bool ConfirmResetSelectedFiles(IWin32Window? owner, string text)
            => Confirm(owner, text, Instance._resetChangesCaption.Text);

        public static bool ConfirmResolveMergeConflicts(IWin32Window? owner)
            => Confirm(owner, Instance._unresolvedMergeConflicts.Text, Instance._unresolvedMergeConflictsCaption.Text);

        public static bool ConfirmUpdateSubmodules(IWin32Window? owner)
        {
            using var dialog = new Microsoft.WindowsAPICodePack.Dialogs.TaskDialog
            {
                OwnerWindowHandle = owner?.Handle ?? IntPtr.Zero,
                Text = Instance._updateSubmodulesToo.Text,
                InstructionText = Instance._theRepositorySubmodules.Text,
                Caption = Instance._updateSubmodules.Text,
                Icon = TaskDialogStandardIcon.Information,
                StandardButtons = TaskDialogStandardButtons.Yes | TaskDialogStandardButtons.No,
                FooterCheckBoxText = Instance._rememberChoice.Text,
                FooterIcon = TaskDialogStandardIcon.Information,
                StartupLocation = Microsoft.WindowsAPICodePack.Dialogs.TaskDialogStartupLocation.CenterOwner
            };

            bool result = dialog.Show() == TaskDialogResult.Yes;
            if (dialog.FooterCheckBoxChecked == true)
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
