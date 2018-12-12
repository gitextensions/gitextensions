using System.Collections;
using System.Collections.Generic;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    public class ThreadSafeGrowingListEnumerator<T> : IEnumerator<T>
    {
        public ThreadSafeGrowingListEnumerator(T[] arr, int length)
        {
            _collection = arr;
            _length = length;
        }

        private readonly T[] _collection;
        private int _index = -1;
        private readonly int _length;

        public T Current => _collection[_index];

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            _index++;
            return _index < _length;
        }

        public void Reset()
        {
            _index = -1;
        }

        public void Dispose()
        {
            // Nothing to dispose.
        }
    }
}
