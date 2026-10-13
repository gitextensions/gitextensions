using GitExtensions.Extensibility.Git;

namespace GitUI;

partial class FileStatusList
{
    internal interface IStatusSorter
    {
        StatusNode CreateTreeSortedByPath(
            IEnumerable<GitItemStatus> statuses,
            bool flat,
            bool mergeSingleItemsWithFolder,
            Func<GitItemStatus, StatusNode> createNode);
    }

    internal sealed class StatusNode(string text)
    {
        public string Text { get; set; } = text;
        public object? Tag { get; set; }
        public StatusNode? Parent { get; private set; }
        public List<StatusNode> Nodes { get; } = [];

        public void Add(StatusNode node)
        {
            node.Parent = this;
            Nodes.Add(node);
        }

        public void Insert(int index, StatusNode node)
        {
            node.Parent = this;
            Nodes.Insert(index, node);
        }

        public bool Remove(StatusNode node)
        {
            if (!Nodes.Remove(node))
            {
                return false;
            }

            node.Parent = null;
            return true;
        }
    }
}
