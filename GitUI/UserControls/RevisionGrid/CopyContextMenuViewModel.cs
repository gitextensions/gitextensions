using System;
using System.Collections.Generic;
using System.Linq;
using GitCommands;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid
{
    public sealed class CopyContextMenuViewModel
    {
        public sealed class DetailItem
        {
            public DetailItem(string text, string value, int valueMaxLength = int.MaxValue)
            {
                Text = $"{text}     ({value.ShortenTo(valueMaxLength)})";
                Value = value;
            }

            public string Text { get; }
            public string Value { get; }
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
                new DetailItem(Strings.GetCommitHashText(), gitRevision.Guid, 15),
                new DetailItem(Strings.GetMessageText(), gitRevision.Subject, 30),
                new DetailItem(Strings.GetAuthorText(), gitRevision.Author),
                new DetailItem(Strings.GetDateText(), gitRevision.CommitDate.ToString()),
            };

            var gitRefListsForRevision = new GitRefListsForRevision(gitRevision);
            BranchNames = gitRefListsForRevision.GetAllBranchNames();
            TagNames = gitRefListsForRevision.GetAllTagNames();
        }
    }
}