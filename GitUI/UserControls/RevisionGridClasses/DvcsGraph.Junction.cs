using System;
using System.Collections.Generic;

namespace GitUI.RevisionGridClasses
{
    partial class DvcsGraph
    {
        private sealed class Junction
        {
            #region State enum

            public enum State
            {
                Unprocessed,
                Processing,
                Processed,
            }

            #endregion

            private static uint debugIdNext;

            private readonly List<Node> nodes = new List<Node>();
            private readonly Dictionary<Node, int> nodeIndices = new Dictionary<Node, int>();

            private readonly uint debugId;

            public State CurrentState = State.Unprocessed;
            public bool IsRelative;
            public bool HighLight;

            public Junction(Node aNode, Node aParent)
            {
                debugId = debugIdNext++;

                AddNode(aNode);
                if (aNode != aParent)
                {
                    aNode.Ancestors.Add(this);
                    aParent.Descendants.Add(this);
                    AddNode(aParent);
                }
            }

            private Junction(Junction aDescendant, Node aNode)
            {
                // Private constructor used by split. This junction will be a
                // ancestor of an existing junction.
                debugId = debugIdNext++;
                aNode.Ancestors.Remove(aDescendant);
                AddNode(aNode);
            }

            public Node Youngest
            {
                get { return this[0]; }
            }

            public Node Oldest
            {
                get { return this[NodesCount - 1]; }
            }

            public Node ChildOf(Node aParent)
            {
                int childIndex;
                if (nodeIndices.TryGetValue(aParent, out childIndex))
                {
                    if (childIndex > 0)
                        return nodes[childIndex - 1];
                    else
                        throw new ArgumentException("Parent has no children:\n" + aParent.ToString());
                }

                throw new ArgumentException("Junction:\n"+ ToString() +"\ndoesn't contain this parent:\n" + aParent.ToString());
            }

            public int NodesCount
            {
                get { return nodes.Count; }
            }

            public Node this[int index]
            {
                get { return nodes[index]; }
            }

            public void Add(Node aParent)
            {
                aParent.Descendants.Add(this);
                Oldest.Ancestors.Add(this);
                AddNode(aParent);
            }

            public void Remove(Node node)
            {
                RemoveNode(node);
                Oldest.Ancestors.Remove(this);
            }

            public Junction Split(Node aNode)
            {
                // The 'top' (Child->node) of the junction is retained by this.
                // The 'bottom' (node->Parent) of the junction is returned.
                int index;
                if (!nodeIndices.TryGetValue(aNode, out index))
                    return null;

                var bottom = new Junction(this, aNode);
                // Add 1, since aNode was at the index
                index += 1;
                while (index < NodesCount)
                {
                    Node node = this[index];
                    RemoveNode(node);
                    node.Ancestors.Remove(this);
                    node.Descendants.Remove(this);
                    bottom.Add(node);
                }

                return bottom;
            }

            public Node TryGetParent(Node child)
            {
                int childIndex;
                return nodeIndices.TryGetValue(child, out childIndex) ? nodes[childIndex + 1] : null;
            }

            private void AddNode(Node node)
            {
                nodeIndices.Add(node, NodesCount);
                nodes.Add(node);
            }

            private void RemoveNode(Node node)
            {
                nodeIndices.Remove(node);
                nodes.Remove(node);
            }

            public override string ToString()
            {
                return string.Format("{3}: {0}--({2})--{1}", Youngest, Oldest, NodesCount, debugId);
            }
        }
    }
}
