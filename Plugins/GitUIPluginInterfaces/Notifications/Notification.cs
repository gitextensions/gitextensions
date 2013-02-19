using System;

namespace GitUIPluginInterfaces.Notifications
{
    //public interface INotification
    //{
    //    Guid Id { get; }

    //}

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
}
