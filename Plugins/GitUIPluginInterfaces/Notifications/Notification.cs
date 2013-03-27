using System;
using System.Collections.Generic;

namespace GitUIPluginInterfaces.Notifications
{
    /// <summary>Represents a single time-stamped notification message.</summary>
    public class Notification
    {
        /// <summary>Creates a new notification with the specified severity and text.</summary>
        ///// <param name="notifier"><see cref="INotifier"/> which published the notification.</param>
        /// <param name="severity">Severity of the notification.</param>
        /// <param name="text">Text for the notification.</param>
        /// <param name="contextActions">The available actions which are relevant to the notification.</param>
        public Notification(
            //INotifier notifier,
            StatusSeverity severity,
            string text,
            params ContextAction[] contextActions)
            : this(severity, text, DateTime.Now, contextActions) { }

        /// <summary>Creates a new notification with the specified severity, text, and time.</summary>
        ///// <param name="notifier"><see cref="INotifier"/> which published the notification.</param>
        /// <param name="severity">Severity of the notification.</param>
        /// <param name="text">Text for the notification.</param>
        /// <param name="timeOf">The time of the notification.</param>
        /// <param name="contextActions">The available actions which are relevant to the notification.</param>
        public Notification(
            //INotifier notifier, 
            StatusSeverity severity,
            string text,
            DateTime timeOf,
            params ContextAction[] contextActions)
        {
            //Notifier = notifier;
            Severity = severity;
            Text = text;
            TimeOf = timeOf;
            ContextActions = contextActions;
        }

        /// <summary>Gets the <see cref="INotifier"/> which published the notification.</summary>
        public INotifier Notifier { get; set; }
        /// <summary>Gets the time of the notification.</summary>
        public DateTime TimeOf { get; private set; }
        /// <summary>Gets the severity of the notification.</summary>
        public StatusSeverity Severity { get; private set; }
        /// <summary>Gets the notification text.</summary>
        public string Text { get; private set; }
        ///// <summary></summary>
        //public Action OnClick { get; private set; }
        /// <summary>Gets the available actions which are relevant to the notification.</summary>
        public IEnumerable<ContextAction> ContextActions { get; private set; }

        public override string ToString() { return Text; }
    }
}
