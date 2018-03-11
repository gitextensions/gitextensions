using System;
using System.Collections.Generic;

namespace GitUI.UserControls.RevisionGridClasses
{
    internal class ParentChildNavigationHistory
    {
        private enum NavigationDirection
        {
            Parent,
            Child
        }

        private readonly Action<string> _setSelectedRevision;
        private NavigationDirection? _direction;
        private readonly Stack<string> _childHistory = new Stack<string>();
        private readonly Stack<string> _parentHistory = new Stack<string>();

        public ParentChildNavigationHistory(Action<string> setSelectedRevision)
        {
            _setSelectedRevision = setSelectedRevision;
        }

        public bool HasPreviousChild => _childHistory.Count > 0;

        public bool HasPreviousParent => _parentHistory.Count > 0;

        public void NavigateToPreviousParent(string current)
        {
            var parent = _parentHistory.Pop();
            Navigate(current, parent, NavigationDirection.Parent);
        }

        public void NavigateToPreviousChild(string current)
        {
            var child = _childHistory.Pop();
            Navigate(current, child, NavigationDirection.Child);
        }

        public void NavigateToChild(string current, string child)
        {
            Navigate(current, child, NavigationDirection.Child);
        }

        public void NavigateToParent(string current, string parent)
        {
            Navigate(current, parent, NavigationDirection.Parent);
        }

        private void Navigate(string current, string to, NavigationDirection direction)
        {
            _direction = direction;
            if (direction == NavigationDirection.Child)
            {
                _parentHistory.Push(current);
            }
            else
            {
                _childHistory.Push(current);
            }

            _setSelectedRevision(to);
            _direction = null;
        }

        public void Clear()
        {
            _childHistory.Clear();
            _parentHistory.Clear();
        }

        public void RevisionsSelectionChanged()
        {
            if (_direction == null)
            {
                Clear();
            }
        }
    }
}