using System;
using System.Collections.Generic;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid
{
    internal sealed class ParentChildNavigationHistory
    {
        private enum NavigationDirection
        {
            Parent,
            Child
        }

        private readonly Action<ObjectId> _setSelectedRevision;
        private NavigationDirection? _direction;
        private readonly Stack<ObjectId> _childHistory = new Stack<ObjectId>();
        private readonly Stack<ObjectId> _parentHistory = new Stack<ObjectId>();

        public ParentChildNavigationHistory(Action<ObjectId> setSelectedRevision)
        {
            _setSelectedRevision = setSelectedRevision;
        }

        public bool HasPreviousChild => _childHistory.Count > 0;
        public bool HasPreviousParent => _parentHistory.Count > 0;

        public void NavigateToPreviousParent(ObjectId current)
        {
            var parent = _parentHistory.Pop();
            Navigate(current, parent, NavigationDirection.Parent);
        }

        public void NavigateToPreviousChild(ObjectId current)
        {
            var child = _childHistory.Pop();
            Navigate(current, child, NavigationDirection.Child);
        }

        public void NavigateToChild(ObjectId current, ObjectId child)
        {
            Navigate(current, child, NavigationDirection.Child);
        }

        public void NavigateToParent(ObjectId current, ObjectId parent)
        {
            Navigate(current, parent, NavigationDirection.Parent);
        }

        private void Navigate(ObjectId current, ObjectId to, NavigationDirection direction)
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