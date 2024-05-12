namespace GitExtensions.Extensibility.Settings;

public interface IDetailedSettings
{
    string SmtpServer { get; set; }

    int SmtpPort { get; set; }

    bool SmtpUseSsl { get; set; }

    bool GetRemoteBranchesDirectlyFromRemote { get; set; }

    bool AddMergeLogMessages { get; set; }

    int MergeLogMessagesCount { get; set; }
}
