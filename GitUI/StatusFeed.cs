using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using GitUIPluginInterfaces;
using Notification = GitUIPluginInterfaces.Notification;

namespace GitUI
{
    /// <summary>Manages notifications between publishers and subscribers.</summary>
    internal sealed class NotificationManager : INotifier
    {
        // TODO: ?repo-specific/GitModule-specific notifications?
        // example: user executes a long-running process, closes the repo before it finishes

        protected static CurrentThreadScheduler UiScheduler = Scheduler.CurrentThread;

        /// <summary>singleton implementation</summary>
        private NotificationManager() { }
        /// <summary>Gets the <see cref="NotificationManager"/> instance.</summary>
        public static NotificationManager Instance { get { return _Instance.Value; } }
        static readonly Lazy<NotificationManager> _Instance
            = new Lazy<NotificationManager>(() => new NotificationManager());

        Subject<Notification> _notifications = new Subject<Notification>();
        Subject<BatchNotification> _notificationBatches = new Subject<BatchNotification>();


        public void Subscribe(
            Action<Notification> onNotification,
            Action<BatchNotification> onBatchNotification)
        {
            var observer =
                Observer
                    .Create(onNotification)
                    .NotifyOn(UiScheduler);
            _notifications.Subscribe(observer);
            _notificationBatches.Subscribe(
                Observer
                    .Create(onBatchNotification)
                    .NotifyOn(UiScheduler)
            );
        }

        void INotifier.Notify(Notification notification)
        {
            _notifications.OnNext(notification);
        }

        INotificationBatch INotifier.StartBatch()
        {
            return new NotificationBatch(_notificationBatches);
        }

        /// <summary>Re-routes <see cref="INotificationBatch"/> calls through <see cref="INotifier"/>'s subscribers.</summary>
        class NotificationBatch : INotificationBatch
        {
            Subject<BatchNotification> _batchNotifications;

            public Guid BatchId { get; private set; }

            public NotificationBatch(Subject<BatchNotification> batchNotifications)
            {
                _batchNotifications = batchNotifications;
                BatchId = Guid.NewGuid();
            }

            /// <summary>Notifies <see cref="INotifier"/>'s subscribers of a new batch notification.</summary>
            void Next(Notification notification)
            {
                _batchNotifications.OnNext(new BatchNotification(notification, BatchId));
            }

            void INotificationBatch.Next(Notification notification)
            {
                Next(notification);
            }

            void INotificationBatch.Last(Notification notification)
            {
                Next(notification);
                _batchNotifications.OnCompleted();
            }
        }
    }

    /// <summary>Single notification which is part of a batch.</summary>
    internal class BatchNotification
    {
        public Notification Notification { get; set; }

        public BatchNotification(Notification notification, Guid batchId)
        {
            Notification = notification;
            BatchId = batchId;
        }

        /// <summary>Indicates that a </summary>
        internal Guid BatchId { get; private set; }
        /// <summary>Indicates whether a <see cref="Notification"/> is part of a batch of notifications.</summary>
        internal bool IsBatch { get; private set; }
        internal BatchEntry BatchPart { get; private set; }

        internal enum BatchEntry
        {
            /// <summary>NOT part of a batch.</summary>
            No,
            First,
            /// <summary>Any notification that isn't the last.</summary>
            Next,
            /// <summary>Last notification in a batch.</summary>
            Last
        }

        /// <summary>Creates a notification which is part of a batch.</summary>
        static BatchNotification Batch(StatusSeverity severity, string text, Guid guid, BatchEntry entry)
        {
            return new BatchNotification(new Notification(severity, text), guid) { BatchPart = entry };
        }

        /// <summary>Ensures that the current notification is part of a batch AND not the last part.</summary>
        void EnsureValidBatch()
        {
            if (BatchPart == BatchEntry.No)
            {
                throw new NotSupportedException("Cannot specify a batch notification from a lone notification.");
            }
            if (BatchPart == BatchEntry.Last)
            {
                throw new NotSupportedException("This batch has already ended.");
            }
        }

        BatchNotification BatchFrom(StatusSeverity severity, string text, Guid guid, BatchEntry entry)
        {
            EnsureValidBatch();
            return Batch(severity, text, guid, entry);
        }

        /// <summary>Creates a new notification which will start a batch.</summary>
        public static BatchNotification BatchStart(StatusSeverity severity, string text)
        {
            return Batch(severity, text, Guid.NewGuid(), BatchEntry.First);
        }

        /// <summary>Creates the next notification in a batch.</summary>
        public BatchNotification BatchNext(StatusSeverity severity, string text)
        {
            return BatchFrom(severity, text, BatchId, BatchEntry.Next);
        }

        /// <summary>Creates the last notification in a batch.</summary>
        public BatchNotification BatchLast(StatusSeverity severity, string text)
        {
            return BatchFrom(severity, text, BatchId, BatchEntry.Last);
        }
    }
}
