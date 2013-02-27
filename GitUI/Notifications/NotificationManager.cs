using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.Notifications;
using Notification = GitUIPluginInterfaces.Notifications.Notification;

namespace GitUI.Notifications
{
    /// <summary>Manages notifications between publishers and subscribers.</summary>
    internal sealed class NotificationManager : INotifier, INotifications
    {
        static CurrentThreadScheduler UiScheduler = Scheduler.CurrentThread;
        /// <summary>&lt;<see cref="GitUICommands"/>: <see cref="NotificationManager"/>&gt;</summary>
        static Dictionary<GitUICommands, NotificationManager> uiCommandsToManager
            = new Dictionary<GitUICommands, NotificationManager>();

        /// <summary>Gets a new or the existing <see cref="NotificationManager"/> 
        /// associated with the specified <see cref="GitUICommands"/>.</summary>
        public static NotificationManager Get(GitUICommands uiCommands)
        {
            NotificationManager manager;
            if (uiCommandsToManager.TryGetValue(uiCommands, out manager))
            {
                return manager;
            }
            manager = new NotificationManager(uiCommands);
            uiCommandsToManager[uiCommands] = manager;
            return manager;
        }

        /// <summary>private implementation</summary>
        private NotificationManager(IGitUICommands uiCommands)
        {
            UiCommands = uiCommands;
        }

        Subject<Notification> _notifications = new Subject<Notification>();

        #region INotifier
        
        /// <summary>Publishes a notification.</summary>
        /// <param name="notification"><see cref="Notification"/> to publish.</param>
        public void Notify(Notification notification)
        {
            notification.Notifier = this;
            _notifications.OnNext(notification);
        }

        /// <summary>Gets the <see cref="INotifications"/> which contains the <see cref="INotifier"/>.</summary>
        INotifications INotifier.Notifications { get { return this; } }

        #endregion INotifier

        #region INotifications
        
        /// <summary>Gets the <see cref="IGitUICommands"/> which contains the <see cref="INotifications"/>.</summary>
        public IGitUICommands UiCommands { get; private set; }
        /// <summary>Gets a notifications publisher.</summary>
        public INotifier Notifier { get { return this; } }
        /// <summary>Gets a notifications provider.</summary>
        public IObservable<Notification> Notifications { get { return _notifications; } }
        
        #endregion INotifications
    
    }
}
