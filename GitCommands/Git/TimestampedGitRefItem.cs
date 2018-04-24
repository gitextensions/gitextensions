using System;
using JetBrains.Annotations;

namespace GitCommands.Git
{
    public sealed class TimestampedGitRefItem
    {
        public TimestampedGitRefItem([NotNull] string line)
        {
            var columns = line.Split(new[] { ' ' }, 4);

            Date = DateTimeUtils.ParseUnixTime(columns[0]);

            var guid = columns[2];
            var completeName = columns[3];
            Ref = new GitRef(null, guid, completeName);
        }

        public DateTime Date { get; }

        [NotNull]
        public GitRef Ref { get; }
    }
}