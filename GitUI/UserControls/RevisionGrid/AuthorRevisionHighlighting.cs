using System;
using System.Collections.Generic;
using System.Linq;
using GitCommands;
using GitCommands.Config;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitUI.UserControls
{
    internal sealed class AuthorRevisionHighlighting
    {
        [CanBeNull]
        public string AuthorEmailToHighlight { get; private set; }

        /// <returns><c>true</c> if the UI should be refreshed in response to this change.</returns>
        [MustUseReturnValue]
        public bool ProcessRevisionSelectionChange(IGitModule currentModule, IReadOnlyCollection<GitRevision> selectedRevisions)
        {
            if (selectedRevisions.Count > 1)
            {
                return false;
            }

            var revision = selectedRevisions.FirstOrDefault();

            var changed = !string.Equals(revision?.AuthorEmail, AuthorEmailToHighlight, StringComparison.OrdinalIgnoreCase);
            if (changed)
            {
                AuthorEmailToHighlight = revision != null
                    ? revision.AuthorEmail
                    : currentModule.GetEffectiveSetting(SettingKeyString.UserEmail);
                return true;
            }

            return false;
        }

        public bool IsHighlighted([CanBeNull] GitRevision revision)
        {
            if (string.IsNullOrWhiteSpace(revision?.AuthorEmail))
            {
                return false;
            }

            return string.Equals(revision.AuthorEmail, AuthorEmailToHighlight, StringComparison.OrdinalIgnoreCase);
        }
    }
}
