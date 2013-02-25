using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using GitCommands;
using GitUIPluginInterfaces.Notifications;
using Notification = GitUIPluginInterfaces.Notifications.Notification;

namespace GitUI.Notifications
{
    /// <summary>Manages notifications between publishers and subscribers.</summary>
    internal sealed class NotificationManager : INotifier, INotifications
    {
        // TODO: ?repo-specific/GitModule-specific notifications?
        // example: user executes a long-running process, closes the repo before it finishes

        static CurrentThreadScheduler UiScheduler = Scheduler.CurrentThread;
        static Dictionary<GitModule, NotificationManager> moduleToManager
            = new Dictionary<GitModule, NotificationManager>();

        /// <summary>Gets a new or the existing <see cref="NotificationManager"/> 
        /// associated with the specified <see cref="GitModule"/>.</summary>
        public static NotificationManager Get(GitModule git)
        {
            NotificationManager manager;
            if (moduleToManager.TryGetValue(git, out manager))
            {
                return manager;
            }
            manager = new NotificationManager();
            moduleToManager[git] = manager;
            return manager;
        }

        /// <summary>private implementation</summary>
        private NotificationManager() { }

        Subject<Notification> _notifications = new Subject<Notification>();

        public void Subscribe(Action<Notification> onNotification)
        {
            var observer =
                Observer
                    .Create(onNotification)
                    .NotifyOn(UiScheduler);
            _notifications.Subscribe(observer);
        }

        /// <summary>Publishes a notification.</summary>
        /// <param name="notification"><see cref="Notification"/> to publish.</param>
        public void Notify(Notification notification)
        {
            _notifications.OnNext(notification);
        }

        /// <summary>Gets a notifications publisher.</summary>
        public INotifier Notifier { get { return this; } }
        /// <summary>Gets a notifications provider.</summary>
        public IObservable<Notification> Notifications { get { return _notifications; } }
    }
}
