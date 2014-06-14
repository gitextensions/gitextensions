using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitUI.UserControls.RevisionGridClasses
{
    class NavigationHistory
    {
        // history of selected items (browse history)
        // head == currently selected item
        private readonly Stack<string> prevItems = new Stack<string>();
        // backtracked items
        // head == item to show when navigating forward
        private readonly Stack<string> nextItems = new Stack<string>();

        /// <summary>
        /// Sets curr as current visible item and resets forward history
        /// </summary>
        /// <param name="curr"></param>
        public void Push(string curr)
        {
            if ((prevItems.Count == 0) || !prevItems.Peek().Equals(curr))
            {
                prevItems.Push(curr);
                nextItems.Clear();
            }
        }

        /// <summary>
        /// Returns whether CanNavigateBackward is possible
        /// </summary>
        public bool CanNavigateBackward
        {
            get 
            {
                return (prevItems.Count > 1);
            }
        }

        /// <summary>
        /// Navigatees backward in history, returns item which should be selected, null if no previous item is available
        /// </summary>
        /// <exception cref="InvalidOperationException">When no previous history is available.</exception>
        public string NavigateBackward()
        {
            if (CanNavigateBackward)
            {
                string curr = prevItems.Pop();
                string prev = prevItems.Peek();
                nextItems.Push(curr);
                return prev;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Returns whether CanNavigateForward is possible
        /// </summary>
        public bool CanNavigateForward
        {
            get
            {
                return (nextItems.Count != 0);
            }
        }

        /// <summary>
        /// Navigatees forward in history, returns item which should be selected, null if no next item is available
        /// </summary>
        /// <exception cref="InvalidOperationException">When no forward history is available.</exception>
        public string NavigateForward()
        {
            if (CanNavigateForward)
            {
                string next = nextItems.Pop();
                prevItems.Push(next);
                return next;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Clears history and sets current item
        /// </summary>
        public void Reset(string curr)
        {
            Clear();
            prevItems.Push(curr);
        }

        /// <summary>
        /// Clears both backward and forward history
        /// </summary>
        public void Clear()
        {
            prevItems.Clear();
            nextItems.Clear();
        }
    }
}
