﻿using System;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using JetBrains.Annotations;
using Microsoft.WindowsAPICodePack.Dialogs;
using ResourceManager;

namespace GitUI
{
    public class MessageBoxes : Translate
    {
        private readonly TranslationString _archiveRevisionCaption = new TranslationString("Archive revision");

        private readonly TranslationString _failedToExecuteScript = new TranslationString("Failed to execute script");

        private readonly TranslationString _failedToRunShell = new TranslationString("Failed to run shell");

        private readonly TranslationString _notValidGitDirectory = new TranslationString("The current directory is not a valid git repository.");

        private readonly TranslationString _unresolvedMergeConflictsCaption = new TranslationString("Merge conflicts");
        private readonly TranslationString _unresolvedMergeConflicts = new TranslationString("There are unresolved merge conflicts, solve conflicts now?");

        private readonly TranslationString _middleOfRebaseCaption = new TranslationString("Rebase");
        private readonly TranslationString _middleOfRebase = new TranslationString("You are in the middle of a rebase, continue rebase?");

        private readonly TranslationString _middleOfPatchApplyCaption = new TranslationString("Patch apply");
        private readonly TranslationString _middleOfPatchApply = new TranslationString("You are in the middle of a patch apply, continue patch apply?");

        private const string _putty = "PuTTY";
        private readonly TranslationString _pageantNotFound = new TranslationString("Cannot load SSH key. PuTTY is not configured properly.");

        private readonly TranslationString _serverHostkeyNotCachedText =
            new TranslationString("The server's host key is not cached in the registry.\n\nDo you want to trust this host key and then try again?");

        private readonly TranslationString _updateSubmodules = new TranslationString("Update submodules");
        private readonly TranslationString _theRepositorySubmodules = new TranslationString("Update submodules on checkout?");
        private readonly TranslationString _updateSubmodulesToo = new TranslationString("Since this repository has submodules, it's necessary to update them on every checkout.\r\n\r\nThis will just checkout on the submodule the commit determined by the superproject.");
        private readonly TranslationString _rememberChoice = new TranslationString("Remember choice");

        private readonly TranslationString _reason = new TranslationString("Reason");

        private readonly TranslationString _selectOnlyOneOrTwoRevisions = new TranslationString("Select only one or two revisions. Abort.");

        private readonly TranslationString _shellNotFoundCaption = new TranslationString("Shell not found");
        private readonly TranslationString _shellNotFound = new TranslationString("The selected shell is not installed, or is not on your path.");
        private readonly TranslationString _resetChangesCaption = new TranslationString("Reset changes");

        // internal for FormTranslate
        internal MessageBoxes()
        {
            Translator.Translate(this, AppSettings.CurrentTranslation);
        }

        [CanBeNull] private static MessageBoxes instance;

        private static MessageBoxes Instance => instance ?? (instance = new MessageBoxes());

        public static void FailedToExecuteScript(IWin32Window owner, string scriptKey, Exception ex)
            => ShowError(owner, $"{Instance._failedToExecuteScript.Text} {scriptKey.Quote()}.{Environment.NewLine}"
                                + $"{Instance._reason.Text}: {ex.Message}");

        public static void FailedToRunShell(IWin32Window owner, string shell, Exception ex)
            => ShowError(owner, $"{Instance._failedToRunShell.Text} {shell.Quote()}.{Environment.NewLine}"
                                + $"{Instance._reason.Text}: {ex.Message}");

        public static void NotValidGitDirectory([CanBeNull] IWin32Window owner)
            => ShowError(owner, Instance._notValidGitDirectory.Text);

        public static void ShowGitConfigurationExceptionMessage(IWin32Window owner, GitConfigurationException exception)
            => Show(owner,
                    string.Format(ResourceManager.Strings.GeneralGitConfigExceptionMessage,
                                  exception.ConfigPath, Environment.NewLine, (exception.InnerException ?? exception).Message),
                    ResourceManager.Strings.GeneralGitConfigExceptionCaption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

        public static bool MiddleOfRebase(IWin32Window owner)
            => Confirm(owner, Instance._middleOfRebase.Text, Instance._middleOfRebaseCaption.Text);

        public static bool MiddleOfPatchApply(IWin32Window owner)
            => Confirm(owner, Instance._middleOfPatchApply.Text, Instance._middleOfPatchApplyCaption.Text);

        public static void PAgentNotFound(IWin32Window owner)
            => ShowError(owner, Instance._pageantNotFound.Text, _putty);

        public static void SelectOnlyOneOrTwoRevisions(IWin32Window owner)
            => ShowError(owner, Instance._selectOnlyOneOrTwoRevisions.Text, Instance._archiveRevisionCaption.Text);

        public static bool CacheHostkey(IWin32Window owner)
            => Confirm(owner, Instance._serverHostkeyNotCachedText.Text, "SSH");

        public static bool ConfirmResetSelectedFiles(IWin32Window owner, string text)
            => Confirm(owner, text, Instance._resetChangesCaption.Text);

        public static bool ConfirmResolveMergeConflicts(IWin32Window owner)
            => Confirm(owner, Instance._unresolvedMergeConflicts.Text, Instance._unresolvedMergeConflictsCaption.Text);

        public static bool ConfirmUpdateSubmodules(IWin32Window win)
        {
            using var dialog = new TaskDialog
            {
                OwnerWindowHandle = win.Handle,
                Text = Instance._updateSubmodulesToo.Text,
                InstructionText = Instance._theRepositorySubmodules.Text,
                Caption = Instance._updateSubmodules.Text,
                Icon = TaskDialogStandardIcon.Information,
                StandardButtons = TaskDialogStandardButtons.Yes | TaskDialogStandardButtons.No,
                FooterCheckBoxText = Instance._rememberChoice.Text,
                FooterIcon = TaskDialogStandardIcon.Information,
                StartupLocation = TaskDialogStartupLocation.CenterOwner
            };

            bool result = dialog.Show() == TaskDialogResult.Yes;
            if (dialog.FooterCheckBoxChecked == true)
            {
                AppSettings.DontConfirmUpdateSubmodulesOnCheckout = result;
                AppSettings.UpdateSubmodulesOnCheckout = result;
            }

            return result;
        }

        public static void ShellNotFound([CanBeNull] IWin32Window owner)
            => ShowError(owner, Instance._shellNotFound.Text, Instance._shellNotFoundCaption.Text);

        public static void ShowError([CanBeNull] IWin32Window owner, string text, string caption = null)
            => Show(owner, text, caption ?? Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);

        private static bool Confirm([CanBeNull] IWin32Window owner, string text, string caption)
            => Show(owner, text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

        private static DialogResult Show([CanBeNull] IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
            => MessageBox.Show(owner, text, caption, buttons, icon);
    }
}
