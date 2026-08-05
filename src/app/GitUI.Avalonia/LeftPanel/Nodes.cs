using Avalonia.Controls;

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
    /// This function is responsible for building the TreeViewItem structure that matches this Nodes's
    /// structure, recursively. Avalonia model nodes own their TreeViewItems, so the existing items are
    /// reordered and reused instead of assigning a recycled item to a different model node.
    /// </summary>
    internal void FillTreeViewNode(TreeViewItem treeViewNode)
    {
        // Avalonia requires each model node to retain its own TreeViewItem because handlers are attached to it.
        TreeViewItem[] desiredItems = [.. _nodesList.Select(node => node.TreeViewNode)];
        treeViewNode.Items.Clear();
        foreach (TreeViewItem item in desiredItems)
        {
            treeViewNode.Items.Add(item);
        }

        foreach (Node node in _nodesList)
        {
            node.Nodes.FillTreeViewNode(node.TreeViewNode);
        }
    }

    public void InsertNode(int index, Node node)
    {
        _nodesList.Insert(index, node);
    }
}
