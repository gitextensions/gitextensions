namespace GitUI.LeftPanel
{
    internal sealed class Nodes : IEnumerable<Node>
    {
        private readonly List<Node> _nodesList = [];

        public Tree? Tree { get; }

        public Nodes(Tree? tree)
        {
            Tree = tree;
        }

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

        public IEnumerator<Node> GetEnumerator()
            => _nodesList.GetEnumerator();

        public void InsertNode(int index, Node node)
            => _nodesList.Insert(index, node);

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => GetEnumerator();

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
                TreeNode treeNode;

                if (oldNodeIdx < treeViewNode.Nodes.Count)
                {
                    treeNode = treeViewNode.Nodes[oldNodeIdx];
                    Node oldNode = Node.GetNode(treeNode);

                    if (!oldNode.Equals(node) && !prevNodes.Contains(node))
                    {
                        treeNode = treeViewNode.Nodes.Insert(oldNodeIdx, string.Empty);
                    }
                }
                else
                {
                    treeNode = treeViewNode.Nodes.Add(string.Empty);
                }

                node.TreeViewNode = treeNode;
                node.Nodes.FillTreeViewNode(treeNode);
                oldNodeIdx++;
            }

            while (oldNodeIdx < treeViewNode.Nodes.Count)
            {
                treeViewNode.Nodes.RemoveAt(oldNodeIdx);
            }
        }

        public int Count => _nodesList.Count;

        public Node? LastNode => _nodesList.Count > 0 ? _nodesList[^1] : null;
    }
}
