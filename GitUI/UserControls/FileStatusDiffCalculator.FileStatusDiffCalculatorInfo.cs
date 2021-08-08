using System;
using System.Collections.Generic;
using GitUIPluginInterfaces;

namespace GitUI
{
    partial class FileStatusDiffCalculator
    {
        private struct FileStatusDiffCalculatorInfo
        {
            public IReadOnlyList<GitRevision> Revisions { get; set; }
            public ObjectId? HeadId { get; set; }
        }
    }
}
