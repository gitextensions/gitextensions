using System;
using System.Collections.Generic;

namespace GitUI.UserControls.RevisionGridClasses
{
    internal class NavigationHistory
    {
        // history of selected items (browse history)
        // head == currently selected item
        private readonly Stack<string> _prevItems = new Stack<string>();

        // backtracked items
        // head == item to show when navigating forward
        private readonly Stack<string> _nextItems = new Stack<string>();

        /// <summary>
        /// Sets curr as current visible item and resets forward history
        /// </summary>
        public void Push(string curr)
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
        /// Navigatees backward in history, returns item which should be selected, null if no previous item is available
        /// </summary>
        /// <exception cref="InvalidOperationException">When no previous history is available.</exception>
        public string NavigateBackward()
        {
            if (!CanNavigateBackward)
            {
                throw new InvalidOperationException();
            }

            string curr = _prevItems.Pop();
            string prev = _prevItems.Peek();
            _nextItems.Push(curr);
            return prev;
        }

        /// <summary>
        /// Returns whether CanNavigateForward is possible
        /// </summary>
        public bool CanNavigateForward => _nextItems.Count != 0;

        /// <summary>
        /// Navigatees forward in history, returns item which should be selected, null if no next item is available
        /// </summary>
        /// <exception cref="InvalidOperationException">When no forward history is available.</exception>
        public string NavigateForward()
        {
            if (!CanNavigateForward)
            {
                throw new InvalidOperationException();
            }

            string next = _nextItems.Pop();
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
