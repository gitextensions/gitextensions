using System;
using System.Drawing;
using System.Windows.Forms;

namespace GitUIPluginInterfaces
{
    public interface IGitUICommands
    {
        event EventHandler<GitUIPostActionEventArgs> PostCommit;
        event EventHandler<GitUIBaseEventArgs> PostRepositoryChanged;
        event EventHandler<GitUIPostActionEventArgs> PostSettings;
        event EventHandler<GitUIPostActionEventArgs> PostUpdateSubmodules;
        event EventHandler<GitUIBaseEventArgs> PostBrowseInitialize;
        event EventHandler<GitUIBaseEventArgs> PostRegisterPlugin;
        event EventHandler<GitUIBaseEventArgs> PreCommit;

        IGitModule GitModule { get; }
        string GitCommand(string arguments);
        IGitRemoteCommand CreateRemoteCommand();
        void CacheAvatar(string email);
        Icon FormIcon { get; }

        /// <summary>
        /// RepoChangedNotifier.Notify() should be called after each action that changess repo state
        /// </summary>
        ILockableNotifier RepoChangedNotifier { get; }

        bool StartCommandLineProcessDialog(IWin32Window ownerForm, string command, string arguments);
        bool StartCommandLineProcessDialog(IGitCommand cmd, IWin32Window parentForm);
        bool StartBatchFileProcessDialog(string batchFile);

        bool StartRemotesDialog();
        bool StartSettingsDialog(IGitPlugin gitPlugin);
        void AddCommitTemplate(string key, Func<string> addingText);
        void RemoveCommitTemplate(string key);
    }
}