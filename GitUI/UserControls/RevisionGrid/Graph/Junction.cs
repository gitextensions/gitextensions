using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    internal enum JunctionProcessingState
    {
        Unprocessed = 0x0000_0000,
        Processing = 0x1000_0000,
        Processed = 0x2000_0000,
    }

    internal sealed class Junction
    {
#if DEBUG
        private static uint debugIdNext;
        private readonly uint _debugId;
#endif

        private readonly List<Node> _nodes = new List<Node>(capacity: 3);
        private readonly Dictionary<Node, int> _nodeIndices = new Dictionary<Node, int>();

        /// <summary>
        /// We pack <see cref="ColorIndex"/>, <see cref="ProcessingState"/>, <see cref="IsRelative"/> and <see cref="IsHighlighted"/> into a single field.
        /// </summary>
        private uint _flags = 0x0000_FFFF;

        public Junction(Node node, [CanBeNull] Node parent = null)
        {
#if DEBUG
            Debug.Assert(!ReferenceEquals(node, parent), "!ReferenceEquals(node, parent)");
            _debugId = debugIdNext++;
#endif

            AddNode(node);

            if (parent != null)
            {
                node.Ancestors.Add(this);
                parent.Descendants.Add(this);
                AddNode(parent);
            }
        }

        public Node this[int index] => _nodes[index];
        public Node Youngest => _nodes[0];
        public Node Oldest => _nodes[_nodes.Count - 1];
        public int NodeCount => _nodes.Count;

        public int ColorIndex
        {
            get
            {
                var i = (int)(_flags & 0x0000_FFFF);
                return i == 0x0000_FFFF ? -1 : i;
            }
            set => _flags = (_flags & 0xFFFF_0000) | (uint)value;
        }

        public JunctionProcessingState ProcessingState
        {
            get => (JunctionProcessingState)(_flags & 0xF000_0000);
            set => _flags = (_flags & 0x0FFF_FFFF) | (uint)value;
        }

        /// <summary>
        /// Gets and sets whether this junction is a direct ancestor or descendant of HEAD.
        /// </summary>
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

        /// <summary>
        /// Gets and sets whether this junction is highlighted, according to external highlighting rules.
        /// Highlighted junctions are rendered distinctly.
        /// </summary>
        public bool IsHighlighted
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

        public Node GetChildOf(Node parent)
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

            var bottom = new Junction(node);
            node.Ancestors.Remove(this);

            // Add 1, since node was at the index
            index += 1;
            while (index < NodeCount)
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
            _nodeIndices.Add(node, _nodes.Count);
            _nodes.Add(node);
        }

        private void RemoveNode(Node node)
        {
            _nodeIndices.Remove(node);
            _nodes.Remove(node);
        }

#if DEBUG
        public override string ToString() => $"{_debugId}: {Youngest}--({NodeCount})--{Oldest}";
#endif
    }
}
