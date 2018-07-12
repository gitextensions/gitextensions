using System.Collections.Generic;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    internal sealed class Node
    {
        public List<Junction> Ancestors { get; } = new List<Junction>(capacity: 2);
        public List<Junction> Descendants { get; } = new List<Junction>(capacity: 2);
        public ObjectId ObjectId { get; }

        public GitRevision Revision { get; set; }
        public RevisionNodeFlags Flags { get; set; }

        public int InLane { get; set; } = int.MaxValue;
        public int Index { get; set; } = int.MaxValue;

        public Node(ObjectId objectId) => ObjectId = objectId;

        public bool IsCheckedOut => Flags.HasFlag(RevisionNodeFlags.CheckedOut);
        public bool HasRef => Flags.HasFlag(RevisionNodeFlags.HasRef);

#if DEBUG
        public override string ToString() => Revision?.ToString() ?? $"{ObjectId} ({Index})";
#endif
    }
}