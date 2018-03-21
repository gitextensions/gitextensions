using System.Collections.Generic;
using System.Linq;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;

namespace GitUI.UserControls
{
    internal class AuthorEmailBasedRevisionHighlighting
    {
        public enum SelectionChangeAction
        {
            NoAction,
            RefreshUserInterface,
        }

        private GitRevision _currentSelectedRevision;

        public string AuthorEmailToHighlight { get; private set; }

        public SelectionChangeAction ProcessRevisionSelectionChange(GitModule currentModule, IReadOnlyCollection<GitRevision> selectedRevisions)
        {
            if (selectedRevisions.Count > 1)
            {
                return SelectionChangeAction.NoAction;
            }

            var newSelectedRevision = selectedRevisions.FirstOrDefault();
            bool differentRevisionAuthorSelected =
                !AuthorEmailEqualityComparer.Instance.Equals(_currentSelectedRevision, newSelectedRevision);

            if (differentRevisionAuthorSelected)
            {
                AuthorEmailToHighlight = newSelectedRevision != null
                                             ? newSelectedRevision.AuthorEmail
                                             : currentModule.GetEffectiveSetting(SettingKeyString.UserEmail);
            }

            _currentSelectedRevision = newSelectedRevision;
            return differentRevisionAuthorSelected
                       ? SelectionChangeAction.RefreshUserInterface
                       : SelectionChangeAction.NoAction;
        }
    }
}
