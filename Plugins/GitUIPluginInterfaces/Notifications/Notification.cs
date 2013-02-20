using System;

namespace GitUIPluginInterfaces.Notifications
{
    /// <summary>Represents a single notification message.</summary>
    public class Notification
    {
        public Notification(StatusSeverity severity, string text)
            : this(severity, text, DateTime.Now) { }

        public Notification(StatusSeverity severity, string text, DateTime timeOf)
        {
            Severity = severity;
            Text = text;
            TimeOf = timeOf;
        }

        /// <summary>Gets the time of the notification.</summary>
        public DateTime TimeOf { get; private set; }
        /// <summary>Gets the severity of the notification.</summary>
        public StatusSeverity Severity { get; private set; }
        /// <summary>Gets the notification text.</summary>
        public string Text { get; private set; }
        ///// <summary></summary>
        //public Action OnClick { get; private set; }

        public override string ToString() { return Text; }
    }
}
