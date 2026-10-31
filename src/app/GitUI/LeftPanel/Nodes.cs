namespace GitUI.LeftPanel;

internal sealed class Nodes(Tree? tree) : IReadOnlyCollection<Node>
{
    private readonly List<Node> _nodesList = [];

    public int Count => _nodesList.Count;

    public Node? LastNode => _nodesList.Count > 0 ? _nodesList[^1] : null;

    public Tree? Tree { get; } = tree;

    public Node this[int index] => _nodesList[index];

    /// <summary>
    /// Adds a new node to the collection.
    /// </summary>
    /// <param name="node">The node to add.</param>
    public void AddNode(Node node)
    {
        _nodesList.Add(node);
    }

    public void AddNodes(IEnumerable<Node> nodes)
    {
        _nodesList.AddRange(nodes);
    }

    public void Clear()
    {
        _nodesList.Clear();
    }

    #region Enumerators

    public List<Node>.Enumerator GetEnumerator()
    {
        return _nodesList.GetEnumerator();
    }

    IEnumerator<Node> IEnumerable<Node>.GetEnumerator()
    {
        return _nodesList.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Returns all nodes of a given TNode type using depth-first, pre-order method.
    /// </summary>
    public IEnumerable<TNode> DepthEnumerator<TNode>() where TNode : NodeBase
    {
        foreach (Node node in this)
        {
            if (node is TNode node1)
            {
                yield return node1;
            }

            foreach (TNode subNode in node.Nodes.DepthEnumerator<TNode>())
            {
                yield return subNode;
            }
        }
    }

    #endregion

    /// <summary>
    /// This function is responsible for building the TreeNode structure that matches this Nodes's
    /// structure, recursively. To avoid needlessly recreating TreeNodes, it recycles existing ones
    /// if they are considered equal, so it's important to implement Equals on Node classes.
    /// </summary>
    internal void FillTreeViewNode(TreeNode treeViewNode)
    {
        HashSet<Node> prevNodes = [];

        for (int i = 0; i < treeViewNode.Nodes.Count; i++)
        {
            prevNodes.Add(Node.GetNode(treeViewNode.Nodes[i]));
        }

        int oldNodeIdx = 0;

        foreach (Node node in this)
        {
            if (oldNodeIdx < treeViewNode.Nodes.Count)
            {
                TreeNode treeNode = treeViewNode.Nodes[oldNodeIdx];
                Node oldNode = Node.GetNode(treeNode);

                if (!oldNode.Equals(node) && !prevNodes.Contains(node))
                {
                    treeViewNode.Nodes.Insert(oldNodeIdx, CreateTreeNode(node));
                }
                else
                {
                    node.TreeViewNode = treeNode;
                    node.Nodes.FillTreeViewNode(treeNode);
                }
            }
            else
            {
                treeViewNode.Nodes.Add(CreateTreeNode(node));
            }

            oldNodeIdx++;
        }

        while (oldNodeIdx < treeViewNode.Nodes.Count)
        {
            treeViewNode.Nodes.RemoveAt(oldNodeIdx);
        }

        return;

        static TreeNode CreateTreeNode(Node node)
        {
            // Build the whole new subtree before it is attached to the tree view. As soon as a TreeNode belongs to
            // a TreeView with a window handle, adding it, setting its text, image or colour is each a message to the
            // native control (and an invalidation), which is very slow for thousands of refs. A detached TreeNode
            // only stores the values, and adding the finished subtree sends a single insert per node.
            TreeNode treeNode = new(string.Empty);
            node.TreeViewNode = treeNode;
            node.Nodes.FillTreeViewNode(treeNode);

            return treeNode;
        }
    }

    public void InsertNode(int index, Node node)
    {
        _nodesList.Insert(index, node);
    }
}
