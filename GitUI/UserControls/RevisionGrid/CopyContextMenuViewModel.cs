using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid
{
    public sealed class CopyContextMenuViewModel
    {
        public sealed class DetailItem
        {
            public DetailItem(string text, string value, int valueMaxLength = int.MaxValue, Keys shortcutKeys = Keys.None)
            {
                Text = $"{text}     ({value.ShortenTo(valueMaxLength)})";
                Value = value;
                ShortcutKeys = shortcutKeys;
            }

            public string Text { get; }
            public string Value { get; }
            public Keys ShortcutKeys { get; }
        }

        public IReadOnlyList<string> BranchNames { get; }
        public IReadOnlyList<string> TagNames { get; }
        public IReadOnlyList<DetailItem> DetailItems { get; }

        public bool SeparatorVisible => BranchNames.Any() || TagNames.Any();

        public CopyContextMenuViewModel([CanBeNull] GitRevision gitRevision)
        {
            if (gitRevision == null)
            {
                DetailItems = Array.Empty<DetailItem>();
                BranchNames = Array.Empty<string>();
                TagNames = Array.Empty<string>();
                return;
            }

            DetailItems = new[]
            {
                new DetailItem(Strings.CommitHash, gitRevision.Guid, 15, Keys.Control | Keys.C),
                new DetailItem(Strings.Message, gitRevision.Subject, 30),
                new DetailItem(Strings.Author, gitRevision.Author),
                new DetailItem(Strings.Date, gitRevision.CommitDate.ToString()),
            };

            var gitRefListsForRevision = new GitRefListsForRevision(gitRevision);
            BranchNames = gitRefListsForRevision.GetAllBranchNames();
            TagNames = gitRefListsForRevision.GetAllTagNames();
        }
    }
}