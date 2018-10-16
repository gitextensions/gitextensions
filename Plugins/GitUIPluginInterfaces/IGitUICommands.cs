using System;
using System.Windows.Forms;
using GitCommands;
using JetBrains.Annotations;

namespace GitUIPluginInterfaces
{
    public interface IGitUICommands
    {
        event EventHandler<GitUIPostActionEventArgs> PostCommit;
        event EventHandler<GitUIEventArgs> PostRepositoryChanged;
        event EventHandler<GitUIPostActionEventArgs> PostSettings;
        event EventHandler<GitUIPostActionEventArgs> PostUpdateSubmodules;
        event EventHandler<GitUIEventArgs> PostBrowseInitialize;
        event EventHandler<GitUIEventArgs> PostRegisterPlugin;
        event EventHandler<GitUIEventArgs> PreCommit;

        [NotNull]
        IGitModule GitModule { get; }

        IGitRemoteCommand CreateRemoteCommand();

        /// <summary>
        /// RepoChangedNotifier.Notify() should be called after each action that changes repo state
        /// </summary>
        ILockableNotifier RepoChangedNotifier { get; }

        void StartCommandLineProcessDialog(IWin32Window owner, string command, ArgumentString arguments);
        bool StartCommandLineProcessDialog(IWin32Window owner, IGitCommand command);
        void StartBatchFileProcessDialog(string batchFile);

        bool StartRemotesDialog();
        bool StartSettingsDialog(IGitPlugin gitPlugin);
        void AddCommitTemplate(string key, Func<string> addingText);
        void RemoveCommitTemplate(string key);
    }
}