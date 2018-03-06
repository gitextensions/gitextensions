using System;
using System.Threading;
using GitUIPluginInterfaces;


namespace GitCommands
{
    public abstract class LockableNotifier : ILockableNotifier
    {
        private int _lockCount = 0;
        private bool _notifyRequested = false;

        protected abstract void InternalNotify();

        private void CheckNotify(int aLockCount)
        {
            if (aLockCount == 0 && _notifyRequested)
            {
                _notifyRequested = false;
                InternalNotify();
            }
        }
        /// <summary>
        /// notifies if is unlocked
        /// </summary>
        public void Notify()
        {
            _notifyRequested = true;
            CheckNotify(_lockCount);
        }

        /// <summary>
        /// locks raising notification
        /// </summary>
        public void Lock()
        {
            Interlocked.Increment(ref _lockCount);
        }

        /// <summary>
        /// unlocks raising notification
        /// to unlock raising notification, UnLock has to be called as many times as Lock was called
        /// </summary>
        /// <param name="requestNotify">true if Notify has to be called</param>
        public void UnLock(bool requestNotify)
        {
            int newCount = Interlocked.Decrement(ref _lockCount);

            if (newCount < 0)
            {
                throw new InvalidOperationException("There was no counterpart call to Lock");
            }

            if (requestNotify)
            {
                _notifyRequested = true;
            }

            CheckNotify(newCount);
        }

        /// <summary>
        /// true if raising notification is locked
        /// </summary>
        public bool IsLocked => _lockCount != 0;
    }

    public class ActionNotifier : LockableNotifier
    {
        private Action _NotifyAction;

        public ActionNotifier(Action aNotifyAction)
        {
            if (aNotifyAction == null)
            {
                throw new ArgumentNullException(nameof(aNotifyAction));
            }

            _NotifyAction = aNotifyAction;
        }

        protected override void InternalNotify()
        {
            _NotifyAction();
        }
    }
}
