namespace GitUIPluginInterfaces.Notifications
{
    /// <summary>Specifies the severity of a notification/status update.</summary>
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
