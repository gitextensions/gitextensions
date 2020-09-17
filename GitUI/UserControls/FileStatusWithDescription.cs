using System;
using System.Collections.Generic;
using GitCommands;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitUI
{
    public class FileStatusWithDescription
    {
        public FileStatusWithDescription([CanBeNull] GitRevision firstRev, GitRevision secondRev, string summary, IReadOnlyList<GitItemStatus> statuses)
        {
            FirstRev = firstRev;
            SecondRev = secondRev ?? throw new ArgumentNullException(nameof(secondRev));
            Summary = summary ?? throw new ArgumentNullException(nameof(summary));
            Statuses = statuses ?? throw new ArgumentNullException(nameof(statuses));
        }

        public GitRevision FirstRev { get; }
        public GitRevision SecondRev { get; }
        public string Summary { get; }
        public IReadOnlyList<GitItemStatus> Statuses { get; }
    }
}
