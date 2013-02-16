using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Forms;

namespace GitUI
{
    /// <summary>Provides the ability to notify the user.</summary>
    public interface INotifier
    {
        /// <summary>Notifies the user about a status update.</summary>
        /// <param name="notification">Status update to show to user.</param>
        void Notify(Notification notification);
        /// <summary>Gets a notification batch.</summary>
        INotificationBatch StartBatch();
    }

    /// <summary>Provides the ability to notify the user about a batch process.</summary>
    public interface INotificationBatch
    {
        /// <summary>Notifies the user about the next update in the batch.</summary>
        void Next(Notification notification);
        /// <summary>Notifies the user about the complete batch process.</summary>
        void Last(Notification notification);
    }

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
            return new NotificationBatch();
        }

        class NotificationBatch : INotificationBatch
        {
            Subject<BatchNotification> _notifications = new Subject<BatchNotification>();

            public Guid BatchId { get; private set; }

            public NotificationBatch(Action<BatchNotification> onBatchNotification)
            {
                _notifications.Subscribe(Observer.Create(onBatchNotification).NotifyOn(UiScheduler));
                BatchId = Guid.NewGuid();
            }

            public void Next(Notification notification)
            {
                _notifications.OnNext(new BatchNotification(notification, BatchId));
            }

            void INotificationBatch.Last(Notification notification)
            {
                Next(notification);
                _notifications.OnCompleted();
                throw new NotImplementedException();
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

    /// <summary>Represents a single notification in a status feed.</summary>
    public class Notification
    {
        /// <summary><see cref="Notification"/> which isn't part of a batch of status updates.</summary>
        static readonly Guid loner = Guid.NewGuid();

        public Notification(StatusSeverity severity, string text)
            : this(severity, text, loner) { }

        Notification(StatusSeverity severity, string text, Guid batchId)
        {
            Severity = severity;
            Text = text;
            if (batchId == Guid.Empty)
            {
                throw new ArgumentException("Must specify a NON-empty GUID.", "batchId");
            }
        }

        /// <summary>Gets the severity of the update.</summary>
        public StatusSeverity Severity { get; private set; }
        /// <summary>Gets the text of the update.</summary>
        public string Text { get; private set; }
        ///// <summary></summary>
        //public Action OnClick { get; private set; }


        public override string ToString() { return Text; }
    }

    /// <summary>Specifies the severity of a status update.</summary>
    public enum StatusSeverity
    {
        /// <summary>Information from a long-running or passive action.</summary>
        Info = 0,
        /// <summary>Action succeeded.</summary>
        Success = 1,
        /// <summary>Possible long-running action which induced side effects.
        ///  Or, another action which may NOT have only an boolean result.</summary>
        Warn = 2,
        /// <summary>Action failed.</summary>
        Fail = 3,
    }
}
