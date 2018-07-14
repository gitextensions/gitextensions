using JetBrains.Annotations;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    internal sealed class LaneJunctionDetail
    {
        private int _index;

        [CanBeNull]
        private Node _node;

        [CanBeNull]
        public Junction Junction { get; private set; }

        public LaneJunctionDetail([NotNull] Node n)
        {
            _node = n;
        }

        public LaneJunctionDetail([NotNull] Junction j)
        {
            Junction = j;
            Junction.ProcessingState = JunctionProcessingState.Processing;
        }

        public int Count
        {
            get
            {
                if (_node != null)
                {
                    return 1 - _index;
                }

                return Junction?.NodeCount - _index ?? 0;
            }
        }

        [CanBeNull]
        public Node Current => _node ?? (_index < Junction.NodeCount ? Junction[_index] : null);

        public bool IsClear => Junction == null && _node == null;

        public void Clear()
        {
            _node = null;
            Junction = null;
            _index = 0;
        }

        public void Next()
        {
            _index++;

            if (Junction != null && _index >= Junction.NodeCount)
            {
                Junction.ProcessingState = JunctionProcessingState.Processed;
            }
        }

#if DEBUG
        public override string ToString()
        {
            if (Junction != null)
            {
                var nodeName = _index < Junction.NodeCount
                    ? Junction[_index].ToString()
                    : "(null)";

                return $"{_index}/{Junction.NodeCount}~{nodeName}~{Junction}";
            }

            return _node != null
                ? $"{_index}/n~{_node}~(null)"
                : "X/X~(null)~(null)";
        }
#endif
    }
}