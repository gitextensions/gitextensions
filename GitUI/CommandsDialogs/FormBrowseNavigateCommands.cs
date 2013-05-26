using GitCommands;
using GitUI.CommandsDialogs.BrowseDialog;
using ResourceManager.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI.CommandsDialogs
{
    class FormBrowseNavigateCommands : ITranslate
    {
        private readonly TranslationString _noRevisionFoundError =
            new TranslationString("No revision found.");

        GitUICommands UICommands;
        GitModule Module;
        RevisionGrid RevisionGrid;

        public FormBrowseNavigateCommands(GitUICommands uiCommands, GitModule module, RevisionGrid revisionGrid)
        {
            UICommands = uiCommands;
            Module = module;
            RevisionGrid = revisionGrid;
        }

        public void Translate()
        {
            Translator.Translate(this, Settings.CurrentTranslation);
        }

        public virtual void AddTranslationItems(Translation translation)
        {
            TranslationUtl.AddTranslationItemsFromFields("FormBrowse", this, translation);
        }

        public virtual void TranslateItems(Translation translation)
        {
            TranslationUtl.TranslateItemsFromFields("FormBrowse", this, translation);
        }

        public void SelectCurrentRevisionExecute(FormBrowse formBrowse)
        {
            formBrowse.ExecuteCommand(GitUI.CommandsDialogs.FormBrowse.Commands.SelectCurrentRevision);
        }

        public void GotoCommitExcecute(Form parentWindow)
        {
            using (FormGoToCommit formGoToCommit = new FormGoToCommit(UICommands))
            {
                if (formGoToCommit.ShowDialog(parentWindow) != DialogResult.OK)
                    return;

                string revisionGuid = formGoToCommit.GetRevision();
                if (!string.IsNullOrEmpty(revisionGuid))
                {
                    RevisionGrid.SetSelectedRevision(new GitRevision(Module, revisionGuid));
                }
                else
                {
                    MessageBox.Show(parentWindow, _noRevisionFoundError.Text);
                }
            }
        }
    }
}
