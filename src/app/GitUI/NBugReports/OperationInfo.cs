namespace GitUI.NBugReports;

internal struct OperationInfo
{
    /// <summary>
    ///  Indicates whether the operation that caused the exception was an external operation.
    /// </summary>
    public bool IsExternalOperation { get; init; }

    /// <summary>
    ///  Indicates whether the operation that caused the exception was a git operation.
    /// </summary>
    public bool IsGitOperation { get; init; }

    /// <summary>
    ///  Indicates whether the operation that caused the exception was a user configured external operation.
    /// </summary>
    public bool IsUserExternalOperation { get; init; }

    /// <summary>
    ///  Indicates whether the operation that caused the exception is terminating the application.
    /// </summary>
    public bool IsTerminating { get; init; }

    public TaskDialogIcon Icon
        => IsExternalOperation || IsGitOperation || IsUserExternalOperation ? TaskDialogIcon.Warning : TaskDialogIcon.Error;
}
