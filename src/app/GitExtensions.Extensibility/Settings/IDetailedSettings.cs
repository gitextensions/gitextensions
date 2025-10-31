namespace GitExtensions.Extensibility.Settings;

public interface IDetailedSettings
{
    bool GetRemoteBranchesDirectlyFromRemote { get; set; }

    bool AddMergeLogMessages { get; set; }

    int MergeLogMessagesCount { get; set; }
}
