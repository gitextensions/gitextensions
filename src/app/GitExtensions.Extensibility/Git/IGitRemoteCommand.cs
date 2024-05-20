namespace GitExtensions.Extensibility.Git;

public interface IGitRemoteCommand
{
    object? OwnerForm { get; set; }
    string? Remote { get; set; }
    string? Title { get; set; }
    string? CommandText { get; set; }
    bool ErrorOccurred { get; }
    string? CommandOutput { get; }

    event EventHandler<GitRemoteCommandCompletedEventArgs> Completed;

    void Execute();
}
