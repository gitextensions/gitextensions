using System.Collections;
using System.Collections.Generic;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    // This list can only grow. Because it only grows, reading is thread safe.
    // Writing to this list is not thread safe. However, for the usecase
    // where only one thread is adding items to the list, and multiple threads
    // are reading, this can be used.
    public class ThreadSafeGrowingList<T> : IReadOnlyList<T>
    {
        private T[] _internalArray;

        public ThreadSafeGrowingList(int capacity = 2)
        {
            _internalArray = new T[capacity];
        }

        public void Add(T item)
        {
            if (Count == _internalArray.Length)
            {
                T[] newArray = new T[_internalArray.Length * 2];
                _internalArray.CopyTo(newArray, 0);
                _internalArray = newArray;
            }

            _internalArray[Count] = item;
            Count = Count + 1;
        }

        public T this[int index] => _internalArray[Count];

        public int Count { get; private set; } = 0;

        public IEnumerator<T> GetEnumerator()
        {
            return new ThreadSafeGrowingListEnumerator<T>(_internalArray, Count);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _internalArray.GetEnumerator();
        }
    }
}
