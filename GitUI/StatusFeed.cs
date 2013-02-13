using System;

namespace GitUI
{
    /// <summary>Represents a single notification in a status feed.</summary>
    public class Notification
    {
        /// <summary><see cref="Notification"/> which isn't part of a batch of status updates.</summary>
        static readonly Guid loner = Guid.NewGuid();

        public Notification(StatusSeverity severity, string text)
            //: this(severity, text, loner)
        {
            Severity = severity;
            Text = text;
            //BatchId = loner;
        }

        //public Notification(StatusSeverity severity, string text, Guid batchId)
        //{
        //    Severity = severity;
        //    Text = text;
        //    if (batchId == Guid.Empty)
        //    {
        //        throw new ArgumentException("Must specify a NON-empty GUID.", "batchId");
        //    }
        //    BatchId = batchId;
        //}

        /// <summary>Gets the severity of the update.</summary>
        public StatusSeverity Severity { get; private set; }
        /// <summary>Gets the text of the update.</summary>
        public string Text { get; private set; }
        ///// <summary></summary>
        //public Action OnClick { get; private set; }
        /// <summary>Indicates that a </summary>
        //public Guid BatchId { get; private set; }

        public override string ToString() { return Text; }
    }

    /// <summary>Specifies the severity of a status update.</summary>
    public enum StatusSeverity
    {
        /// <summary>Information from a long-running or passive action.</summary>
        Info = -1,
        /// <summary>Action succeeded.</summary>
        Success = 0,
        /// <summary>Possible long-running action which induced side effects.
        ///  Or, another action which may NOT have only an boolean result.</summary>
        Warn = 1,
        /// <summary>Action failed.</summary>
        Fail = 2,
    }
}
