using GitExtensions.Extensibility.Plugins;

namespace GitExtensions.Extensibility.Git;

public interface IGitUICommands : IServiceProvider
{
    event EventHandler<GitUIPostActionEventArgs> PostCommit;
    event EventHandler<GitUIEventArgs> PostRepositoryChanged;
    event EventHandler<GitUIPostActionEventArgs> PostSettings;
    event EventHandler<GitUIPostActionEventArgs> PostUpdateSubmodules;
    event EventHandler<GitUIEventArgs> PostBrowseInitialize;
    event EventHandler<GitUIEventArgs> PostRegisterPlugin;
    event EventHandler<GitUIEventArgs> PreCommit;

    IBrowseRepo? BrowseRepo { get; set; }

    IGitModule Module { get; }

    /// <summary>
    /// RepoChangedNotifier.Notify() should be called after each action that changes repo state
    /// </summary>
    ILockableNotifier RepoChangedNotifier { get; }

    IGitRemoteCommand CreateRemoteCommand();

    bool StartCommandLineProcessDialog(IWin32Window? owner, string? command, ArgumentString arguments);
    bool StartCommandLineProcessDialog(IWin32Window? owner, IGitCommand command);
    void StartBatchFileProcessDialog(string batchFile);

    /// <summary>
    /// Opens the FormRemotes.
    /// </summary>
    /// <param name="preselectRemote">Makes the FormRemotes initially select the given remote.</param>
    /// <param name="preselectLocal">Makes the FormRemotes initially show the tab "Default push behavior" and select the given local.</param>
    bool StartRemotesDialog(IWin32Window? owner, string? preselectRemote = null, string? preselectLocal = null);

    bool StartSettingsDialog(Type pageType);
    bool StartSettingsDialog(IGitPlugin gitPlugin);
    void AddCommitTemplate(string key, Func<string> addingText, Image? icon);
    void RemoveCommitTemplate(string key);
}
