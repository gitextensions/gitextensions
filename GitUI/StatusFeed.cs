using System;

namespace GitUI
{
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
            BatchId = batchId;
            IsBatch = (batchId != loner);
        }

        /// <summary>Gets the severity of the update.</summary>
        public StatusSeverity Severity { get; private set; }
        /// <summary>Gets the text of the update.</summary>
        public string Text { get; private set; }
        ///// <summary></summary>
        //public Action OnClick { get; private set; }
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
        static Notification Batch(StatusSeverity severity, string text, Guid guid, BatchEntry entry)
        {
            return new Notification(severity, text, guid) { BatchPart = entry };
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

        Notification BatchFrom(StatusSeverity severity, string text, Guid guid, BatchEntry entry)
        {
            EnsureValidBatch();
            return Batch(severity, text, guid, entry);
        }

        /// <summary>Creates a new notification which will start a batch.</summary>
        public static Notification BatchStart(StatusSeverity severity, string text)
        {
            return Batch(severity, text, Guid.NewGuid(), BatchEntry.First);
        }

        /// <summary>Creates the next notification in a batch.</summary>
        public Notification BatchNext(StatusSeverity severity, string text)
        {
            return BatchFrom(severity, text, BatchId, BatchEntry.Next);
        }

        /// <summary>Creates the last notification in a batch.</summary>
        public Notification BatchLast(StatusSeverity severity, string text)
        {
            return BatchFrom(severity, text, BatchId, BatchEntry.Last);
        }

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
