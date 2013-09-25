using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    public class MessageBoxes : Translate
    {
        private readonly TranslationString _error = new TranslationString("Error");
        private readonly TranslationString _notValidGitDirectory = new TranslationString("The current directory is not a valid git repository.");
        private readonly TranslationString _notValidGitSVNDirectory = new TranslationString("The current directory is not a valid git-svn repository.");
        private readonly TranslationString _unableGetSVNInformation = new TranslationString("Unable to determine upstream SVN information.");

        private readonly TranslationString _unresolvedMergeConflictsCaption = new TranslationString("Merge conflicts");
        private readonly TranslationString _unresolvedMergeConflicts = new TranslationString("There are unresolved merge conflicts, solve conflicts now?");

        private readonly TranslationString _middleOfRebaseCaption = new TranslationString("Rebase");
        private readonly TranslationString _middleOfRebase = new TranslationString("You are in the middle of a rebase, continue rebase?");

        private readonly TranslationString _middleOfPatchApplyCaption = new TranslationString("Patch apply");
        private readonly TranslationString _middleOfPatchApply = new TranslationString("You are in the middle of a patch apply, continue patch apply?");

        private const string _putty = "PuTTY";
        private readonly TranslationString _pageantNotFound = new TranslationString("Cannot load SSH key. PuTTY is not configured properly.");

        // internal for FormTranslate
        internal MessageBoxes()
        {
            Translator.Translate(this, AppSettings.CurrentTranslation);
        }

        private static MessageBoxes instance;

        private static MessageBoxes Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MessageBoxes();
                }
                return instance;
            }
        }
        
        public static void NotValidGitDirectory(IWin32Window owner)
        {
            MessageBox.Show(owner, Instance._notValidGitDirectory.Text, Instance._error.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void NotValidGitSVNDirectory(IWin32Window owner)
        {
            MessageBox.Show(owner, Instance._notValidGitSVNDirectory.Text, Instance._error.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void UnableGetSVNInformation(IWin32Window owner)
        {
            MessageBox.Show(owner, Instance._unableGetSVNInformation.Text, Instance._error.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    }
}
