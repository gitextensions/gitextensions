namespace GitUI
{
    /// <summary>Represents a single status update in a status feed.</summary>
    public class StatusFeedItem
    {
        /// <summary>Gets the severity of the update.</summary>
        public StatusSeverity Severity { get; set; }
        /// <summary>Gets the text of the update.</summary>
        public string Text { get; set; }
    }

    /// <summary>Specifies the severity of a status update.</summary>
    public enum StatusSeverity
    {
        /// <summary>Action succeeded.</summary>
        Success,
        //Warn,
        /// <summary>Action failed.</summary>
        Fail,
    }
}
