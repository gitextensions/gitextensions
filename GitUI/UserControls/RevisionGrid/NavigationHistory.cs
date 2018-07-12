using System;
using System.Collections.Generic;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid
{
    internal class NavigationHistory
    {
        // history of selected items (browse history)
        // head == currently selected item
        private readonly Stack<ObjectId> _prevItems = new Stack<ObjectId>();

        // backtracked items
        // head == item to show when navigating forward
        private readonly Stack<ObjectId> _nextItems = new Stack<ObjectId>();

        /// <summary>
        /// Sets <paramref name="curr"/> as current visible item and resets forward history
        /// </summary>
        public void Push(ObjectId curr)
        {
            if (_prevItems.Count == 0 || _prevItems.Peek() != curr)
            {
                _prevItems.Push(curr);
                _nextItems.Clear();
            }
        }

        /// <summary>
        /// Returns whether CanNavigateBackward is possible
        /// </summary>
        public bool CanNavigateBackward => _prevItems.Count > 1;

        /// <summary>
        /// Navigates backward in history, returns item which should be selected, null if no previous item is available
        /// </summary>
        /// <exception cref="InvalidOperationException">When no previous history is available.</exception>
        public ObjectId NavigateBackward()
        {
            if (!CanNavigateBackward)
            {
                throw new InvalidOperationException();
            }

            var curr = _prevItems.Pop();
            var prev = _prevItems.Peek();
            _nextItems.Push(curr);
            return prev;
        }

        /// <summary>
        /// Returns whether CanNavigateForward is possible
        /// </summary>
        public bool CanNavigateForward => _nextItems.Count != 0;

        /// <summary>
        /// Navigates forward in history, returns item which should be selected, null if no next item is available
        /// </summary>
        /// <exception cref="InvalidOperationException">When no forward history is available.</exception>
        public ObjectId NavigateForward()
        {
            if (!CanNavigateForward)
            {
                throw new InvalidOperationException();
            }

            var next = _nextItems.Pop();
            _prevItems.Push(next);
            return next;
        }

        /// <summary>
        /// Clears both backward and forward history
        /// </summary>
        public void Clear()
        {
            _prevItems.Clear();
            _nextItems.Clear();
        }
    }
}
