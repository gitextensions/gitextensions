using System;
using System.Windows.Forms;
using GitCommands;
using ResourceManager;
using ResourceManager.Translation;

namespace GitUI
{
    public class Abort : ITranslate
    {
        /// <summary>
        /// Constructor used for translation
        /// </summary>
        internal Abort()
        {
            var translator = new Translator(Settings.Translation);
            translator.TranslateControl(this);
        }

        #region Translation strings
        private readonly TranslationString _abortCurrentOpperation =
            new TranslationString("You can abort the current operation by resetting changes." + Environment.NewLine + 
                "All changes since the last commit will be deleted." + Environment.NewLine + 
                Environment.NewLine + "Do you want to reset changes?");

        private readonly TranslationString _abortCurrentOpperationCaption = new TranslationString("Abort");

        private readonly TranslationString _areYouSureYouWantDeleteFiles =
            new TranslationString("Are you sure you want to DELETE all changes?" + Environment.NewLine + 
                Environment.NewLine + "This action cannot be made undone.");

        private readonly TranslationString _areYouSureYouWantDeleteFilesCaption = new TranslationString("WARNING!");
        #endregion

        public static bool ShowAbortMessage()
        {
            var strings = new Abort();
            if (MessageBox.Show(strings._abortCurrentOpperation.Text, strings._abortCurrentOpperationCaption.Text,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (MessageBox.Show(strings._areYouSureYouWantDeleteFiles.Text, strings._areYouSureYouWantDeleteFilesCaption.Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                    return true;
            }
            return false;
        }

        public static bool AbortCurrentAction(GitModule module)
        {
            if (ShowAbortMessage())
            {
                module.ResetHard("");
                return true;
            }
            return false;
        }

        public static bool AbortCurrentAction()
        {
            return AbortCurrentAction(Settings.Module);
        }
    }
}
