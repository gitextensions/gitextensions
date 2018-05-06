using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GitCommands;
using ResourceManager;

namespace GitUI.UserControls.RevisionGridClasses
{
    public class CopyContextMenuViewModel
    {
        public class DetailItem
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
        public bool SeparatorVisible => BranchNames.Any() || TagNames.Any();
        public IReadOnlyList<DetailItem> DetailItems { get; }

        public CopyContextMenuViewModel(GitRevision gitRevision)
        {
            if (gitRevision == null)
            {
                DetailItems = new DetailItem[0];
                BranchNames = new string[0];
                TagNames = new string[0];
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
            BranchNames = new ReadOnlyCollection<string>(gitRefListsForRevision.GetAllBranchNames());
            TagNames = new ReadOnlyCollection<string>(gitRefListsForRevision.GetAllTagNames());
        }
    }
}