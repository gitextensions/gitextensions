using System;

namespace GitUI
{
    /// <summary>Represents a single status update in a status feed.</summary>
    public class StatusFeedItem
    {
        public StatusFeedItem(StatusSeverity severity, string text)
        {
            Severity = severity;
            Text = text;
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
