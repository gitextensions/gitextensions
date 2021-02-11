using System;
using System.Collections.Generic;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI
{
    public class FileStatusWithDescription
    {
        public FileStatusWithDescription(GitRevision? firstRev, GitRevision secondRev, string summary, IReadOnlyList<GitItemStatus> statuses, ObjectId? baseA = null, ObjectId? baseB = null)
        {
            FirstRev = firstRev;
            SecondRev = secondRev ?? throw new ArgumentNullException(nameof(secondRev));
            Summary = summary ?? throw new ArgumentNullException(nameof(summary));
            Statuses = statuses ?? throw new ArgumentNullException(nameof(statuses));
            BaseA = baseA;
            BaseB = baseB;
        }

        public GitRevision? FirstRev { get; }
        public GitRevision SecondRev { get; }
        public string Summary { get; }
        public IReadOnlyList<GitItemStatus> Statuses { get; }
        public ObjectId? BaseA { get; }
        public ObjectId? BaseB { get; }
    }
}
