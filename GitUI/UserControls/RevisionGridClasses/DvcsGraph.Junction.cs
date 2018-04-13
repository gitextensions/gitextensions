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

            public Junction(Node node, Node parent)
            {
                _debugId = debugIdNext++;

                AddNode(node);
                if (node != parent)
                {
                    node.Ancestors.Add(this);
                    parent.Descendants.Add(this);
                    AddNode(parent);
                }
            }

            private Junction(Junction descendant, Node node)
            {
                // Private constructor used by split. This junction will be a
                // ancestor of an existing junction.
                _debugId = debugIdNext++;
                node.Ancestors.Remove(descendant);
                AddNode(node);
            }

            public Node Youngest => this[0];

            public Node Oldest => this[NodesCount - 1];

            public Node ChildOf(Node parent)
            {
                if (_nodeIndices.TryGetValue(parent, out var childIndex))
                {
                    if (childIndex > 0)
                    {
                        return _nodes[childIndex - 1];
                    }
                    else
                    {
                        throw new ArgumentException("Parent has no children:\n" + parent);
                    }
                }

                throw new ArgumentException("Junction:\n" + ToString() + "\ndoesn't contain this parent:\n" + parent);
            }

            public int NodesCount => _nodes.Count;

            public Node this[int index] => _nodes[index];

            public void Add(Node parent)
            {
                parent.Descendants.Add(this);
                Oldest.Ancestors.Add(this);
                AddNode(parent);
            }

            public void Remove(Node node)
            {
                RemoveNode(node);
                Oldest.Ancestors.Remove(this);
            }

            public Junction SplitIntoJunctionWith(Node node)
            {
                // The 'top' (Child->node) of the junction is retained by this.
                // The 'bottom' (node->Parent) of the junction is returned.
                if (!_nodeIndices.TryGetValue(node, out var index))
                {
                    return null;
                }

                var bottom = new Junction(this, node);

                // Add 1, since node was at the index
                index += 1;
                while (index < NodesCount)
                {
                    Node childNode = this[index];
                    RemoveNode(childNode);
                    childNode.Ancestors.Remove(this);
                    childNode.Descendants.Remove(this);
                    bottom.Add(childNode);
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
