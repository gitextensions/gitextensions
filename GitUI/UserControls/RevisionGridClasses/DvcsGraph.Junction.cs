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

            private readonly List<Node> _nodes = new List<Node>();
            private readonly Dictionary<Node, int> _nodeIndices = new Dictionary<Node, int>();

            private readonly uint _debugId;

            public State CurrentState = State.Unprocessed;
            public bool IsRelative;
            public bool HighLight;

            public Junction(Node aNode, Node aParent)
            {
                _debugId = debugIdNext++;

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
                _debugId = debugIdNext++;
                aNode.Ancestors.Remove(aDescendant);
                AddNode(aNode);
            }

            public Node Youngest => this[0];

            public Node Oldest => this[NodesCount - 1];

            public Node ChildOf(Node aParent)
            {
                if (_nodeIndices.TryGetValue(aParent, out var childIndex))
                {
                    if (childIndex > 0)
                    {
                        return _nodes[childIndex - 1];
                    }
                    else
                    {
                        throw new ArgumentException("Parent has no children:\n" + aParent.ToString());
                    }
                }

                throw new ArgumentException("Junction:\n" + ToString() + "\ndoesn't contain this parent:\n" + aParent.ToString());
            }

            public int NodesCount => _nodes.Count;

            public Node this[int index] => _nodes[index];

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
                if (!_nodeIndices.TryGetValue(aNode, out var index))
                {
                    return null;
                }

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
                return _nodeIndices.TryGetValue(child, out var childIndex) ? _nodes[childIndex + 1] : null;
            }

            private void AddNode(Node node)
            {
                _nodeIndices.Add(node, NodesCount);
                _nodes.Add(node);
            }

            private void RemoveNode(Node node)
            {
                _nodeIndices.Remove(node);
                _nodes.Remove(node);
            }

            public override string ToString()
            {
                return string.Format("{3}: {0}--({2})--{1}", Youngest, Oldest, NodesCount, _debugId);
            }
        }
    }
}
