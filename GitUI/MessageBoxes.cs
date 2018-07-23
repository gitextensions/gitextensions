using System.Windows.Forms;
using GitCommands;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI
{
    public class MessageBoxes : Translate
    {
        private readonly TranslationString _error = new TranslationString("Error");
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
        private readonly TranslationString _confirmDeleteRemoteBranch = new TranslationString("Do you want to delete the branch {0} from {1}?");

        // internal for FormTranslate
        internal MessageBoxes()
        {
            Translator.Translate(this, AppSettings.CurrentTranslation);
        }

        [CanBeNull] private static MessageBoxes instance;

        private static MessageBoxes Instance => instance ?? (instance = new MessageBoxes());

        public static void NotValidGitDirectory([CanBeNull] IWin32Window owner)
        {
            MessageBox.Show(owner, Instance._notValidGitDirectory.Text, Instance._error.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static bool UnresolvedMergeConflicts(IWin32Window owner)
        {
            return MessageBox.Show(owner, Instance._unresolvedMergeConflicts.Text, Instance._unresolvedMergeConflictsCaption.Text, MessageBoxButtons.YesNo) == DialogResult.Yes;
        }

        public static bool MiddleOfRebase(IWin32Window owner)
        {
            return MessageBox.Show(owner, Instance._middleOfRebase.Text, Instance._middleOfRebaseCaption.Text, MessageBoxButtons.YesNo) == DialogResult.Yes;
        }

        public static bool MiddleOfPatchApply(IWin32Window owner)
        {
            return MessageBox.Show(owner, Instance._middleOfPatchApply.Text, Instance._middleOfPatchApplyCaption.Text, MessageBoxButtons.YesNo) == DialogResult.Yes;
        }

        public static void PAgentNotFound(IWin32Window owner)
        {
            MessageBox.Show(owner, Instance._pageantNotFound.Text, _putty);
        }

        public static bool CacheHostkey(IWin32Window owner)
        {
            return MessageBox.Show(owner, Instance._serverHostkeyNotCachedText.Text, "SSH", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        public static bool ConfirmUpdateSubmodules(IWin32Window win)
        {
            var result = PSTaskDialog.cTaskDialog.ShowTaskDialogBox(
                Owner: win,
                Title: Instance._updateSubmodules.Text,
                MainInstruction: Instance._theRepositorySubmodules.Text,
                Content: Instance._updateSubmodulesToo.Text,
                ExpandedInfo: "",
                Footer: "",
                VerificationText: Instance._rememberChoice.Text,
                RadioButtons: "",
                CommandButtons: "",
                Buttons: PSTaskDialog.eTaskDialogButtons.YesNo,
                MainIcon: PSTaskDialog.eSysIcons.Question,
                FooterIcon: PSTaskDialog.eSysIcons.Information) == DialogResult.Yes;

            if (PSTaskDialog.cTaskDialog.VerificationChecked)
            {
                AppSettings.UpdateSubmodulesOnCheckout = result;
            }

            return result;
        }

        public static bool ConfirmDeleteRemoteBranch(IWin32Window owner, string branchName, string remote)
        {
            return MessageBox.Show(owner, string.Format(Instance._confirmDeleteRemoteBranch.Text, branchName, remote),
                "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }
    }
}
