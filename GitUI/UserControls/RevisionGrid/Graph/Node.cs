using System.Collections.Generic;
using GitCommands;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    internal sealed class Node
    {
        public List<Junction> Ancestors { get; } = new List<Junction>(capacity: 2);
        public List<Junction> Descendants { get; } = new List<Junction>(capacity: 2);
        public string ObjectId { get; }

        public GitRevision Data { get; set; }
        public DvcsGraph.DataTypes DataTypes { get; set; }
        public int InLane { get; set; } = int.MaxValue;
        public int Index { get; set; } = int.MaxValue;

        public Node(string objectId) => ObjectId = objectId;

        public bool IsActive => DataTypes.HasFlag(DvcsGraph.DataTypes.Active);
        public bool IsSpecial => DataTypes.HasFlag(DvcsGraph.DataTypes.Special);

        public override string ToString() => Data?.ToString() ?? $"{ObjectId} ({Index})";
    }
}