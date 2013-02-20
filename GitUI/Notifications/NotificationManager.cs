using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
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

        #region Singleton
        /// <summary>singleton implementation</summary>
        private NotificationManager() { }

        /// <summary>Gets the <see cref="NotificationManager"/> instance.</summary>
        public static NotificationManager Instance { get { return _Instance.Value; } }
        static readonly Lazy<NotificationManager> _Instance
            = new Lazy<NotificationManager>(() => new NotificationManager());

        #endregion Singleton
        
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
