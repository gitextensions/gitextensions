using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    internal sealed class Junction
    {
        #region State enum

        private const uint StateMask = 0xF000_0000;

        internal enum State
        {
            Unprocessed = 0x0000_0000,
            Processing = 0x1000_0000,
            Processed = 0x2000_0000,
        }

        #endregion

#if DEBUG
        private static uint debugIdNext;
        private readonly uint _debugId;
#endif

        private readonly List<Node> _nodes = new List<Node>(capacity: 3);
        private readonly Dictionary<Node, int> _nodeIndices = new Dictionary<Node, int>();

        /// <summary>
        /// We pack <see cref="ColorIndex"/>, <see cref="CurrentState"/>, <see cref="IsRelative"/> and <see cref="HighLight"/> into a single field.
        /// </summary>
        private uint _flags = 0x0000_FFFF;

        public int ColorIndex
        {
            get
            {
                var i = (int)(_flags & 0x0000_FFFF);
                return i == 0x0000_FFFF ? -1 : i;
            }
            set => _flags = (_flags & 0xFFFF_0000) | (uint)value;
        }

        public State CurrentState
        {
            get => (State)(_flags & StateMask);
            set => _flags = (_flags & 0x0FFF_FFFF) | (uint)value;
        }

        public bool IsRelative
        {
            get => (_flags & 0x0100_0000) != 0;
            set
            {
                if (value)
                {
                    _flags |= 0x0100_0000;
                }
                else
                {
                    _flags &= unchecked((uint)~0x0100_0000);
                }
            }
        }

        public bool HighLight
        {
            get => (_flags & 0x0200_0000) != 0;
            set
            {
                if (value)
                {
                    _flags |= 0x0200_0000;
                }
                else
                {
                    _flags &= unchecked((uint)~0x0200_0000);
                }
            }
        }

        public Junction(Node node, Node parent)
        {
#if DEBUG
            _debugId = debugIdNext++;
#endif

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
#if DEBUG
            _debugId = debugIdNext++;
#endif
            node.Ancestors.Remove(descendant);
            AddNode(node);
        }

        public Node Youngest => this[0];

        public Node Oldest => this[NodesCount - 1];

        public Node ChildOf(Node parent)
        {
            if (_nodeIndices.TryGetValue(parent, out var childIndex))
            {
                if (childIndex <= 0)
                {
                    throw new ArgumentException("Parent has no children:\n" + parent);
                }

                return _nodes[childIndex - 1];
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

        [CanBeNull]
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

        [CanBeNull]
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
#if DEBUG
            return string.Format("{3}: {0}--({2})--{1}", Youngest, Oldest, NodesCount, _debugId);
#else
            return string.Format("{3}: {0}--({2})", Youngest, Oldest, NodesCount);
#endif
        }
    }
}
