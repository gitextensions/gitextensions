using System;
using System.ComponentModel;
using System.Linq;
using GitCommands;
using GitCommands.Config;
using GitUIPluginInterfaces;

namespace GitUI.Objects
{
    public interface IGitRemoteController
    {
        /// <summary>
        /// Gets the list of remotes configured in .git/config file.
        /// </summary>
        BindingList<GitRemote> Remotes { get; }

        void ConfigureRemotes(string remoteName);

        /// <summary>
        /// Returns the default remote for push operation.
        /// </summary>
        /// <param name="remote"></param>
        /// <param name="branch"></param>
        /// <returns>The <see cref="GitRef.Name"/> if found, otheriwse <see langword="null"/>.</returns>
        string GetDefaultPushRemote(GitRemote remote, string branch);

        /// <summary>
        /// Loads the remotes from the .git/config.
        /// </summary>
        void LoadRemotes();

        /// <summary>
        /// Removes the specified remote from .git/config file.
        /// </summary>
        /// <param name="remote">Remote to remove.</param>
        /// <returns>Output of the operation.</returns>
        string RemoveRemote(GitRemote remote);

        /// <summary>
        ///   Saves the remote details by creating a new or updating an existing remote entry in .git/config file.
        /// </summary>
        /// <param name="remote">An existing remote instance or <see langword="null"/> if creating a new entry.</param>
        /// <param name="remoteName">
        ///   <para>The remote name.</para>
        ///   <para>If updating an existing remote and the name changed, it will result in remote name change and prompt for "remote update".</para>
        /// </param>
        /// <param name="remoteUrl">
        ///   <para>The remote URL.</para>
        ///   <para>If updating an existing remote and the URL changed, it will result in remote URL change and prompt for "remote update".</para>
        /// </param>
        /// <param name="remotePushUrl">An optional alternative remote push URL.</param>
        /// <param name="remotePuttySshKey">An optional Putty SSH key.</param>
        /// <returns>Result of the operation.</returns>
        GitRemoteSaveResult SaveRemote(GitRemote remote, string remoteName, string remoteUrl, string remotePushUrl, string remotePuttySshKey);
    }

    public class GitRemoteController : IGitRemoteController
    {
        private static readonly object SyncRoot = new object();
        private readonly IGitModule _module;


        public GitRemoteController(IGitModule module)
        {
            _module = module;
            Remotes = new BindingList<GitRemote>();
        }


        /// <summary>
        /// Gets the list of remotes configured in .git/config file.
        /// </summary>
        public BindingList<GitRemote> Remotes { get; private set; }


        // TODO: moved verbatim from FormRemotes.cs, perhaps needs refactoring
        public void ConfigureRemotes(string remoteName)
        {
            var localConfig = _module.LocalConfigFile;

            foreach (var remoteHead in _module.GetRefs(true, true))
            {
                foreach (var localHead in _module.GetRefs(true, true))
                {
                    if (!remoteHead.IsRemote ||
                        localHead.IsRemote ||
                        !string.IsNullOrEmpty(localHead.GetTrackingRemote(localConfig)) ||
                        remoteHead.IsTag ||
                        localHead.IsTag ||
                        !remoteHead.Name.ToLower().Contains(localHead.Name.ToLower()) ||
                        !remoteHead.Name.ToLower().Contains(remoteName.ToLower()))
                    {
                        continue;
                    }

                    localHead.TrackingRemote = remoteName;
                    localHead.MergeWith = localHead.Name;
                }
            }
        }

        /// <summary>
        /// Returns the default remote for push operation.
        /// </summary>
        /// <param name="remote"></param>
        /// <param name="branch"></param>
        /// <returns>The <see cref="GitRef.Name"/> if found, otheriwse <see langword="null"/>.</returns>
        // TODO: moved verbatim from FormPush.cs, perhaps needs refactoring
        public string GetDefaultPushRemote(GitRemote remote, string branch)
        {
            if (remote == null)
            {
                throw new ArgumentNullException("remote");
            }

            Func<string, string, bool> isSettingForBranch = (setting, branchName) =>
            {
                var head = new GitRef(_module, string.Empty, setting);
                return head.IsHead && head.Name.Equals(branchName, StringComparison.OrdinalIgnoreCase);
            };

            var remoteHead = remote.Push
                                   .Select(s => s.Split(':'))
                                   .Where(t => t.Length == 2)
                                   .Where(t => isSettingForBranch(t[0], branch))
                                   .Select(t => new GitRef(_module, string.Empty, t[1]))
                                   .FirstOrDefault(h => h.IsHead);

            return remoteHead == null ? null : remoteHead.Name;
        }

        /// <summary>
        /// Loads the remotes from the .git/config.
        /// </summary>
        // TODO: candidate for Async implementations
        public void LoadRemotes()
        {
            if (_module == null)
            {
                return;
            }

            lock (SyncRoot)
            {
                Remotes.Clear();

                var gitRemotes = _module.GetRemotes().Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                if (gitRemotes.Any())
                {
                    var remotes = gitRemotes.Select(remote => new GitRemote
                    {
                        Name = remote,
                        Url = _module.GetSetting(string.Format(SettingKeyString.RemoteUrl, remote)),
                        Push = _module.GetSettings(string.Format(SettingKeyString.RemotePush, remote)).ToList(),
                        PushUrl = _module.GetSetting(string.Format(SettingKeyString.RemotePushUrl, remote)),
                        PuttySshKey = _module.GetSetting(string.Format(SettingKeyString.RemotePuttySshKey, remote)),
                    }).ToList();

                    Remotes.AddAll(remotes.OrderBy(x => x.Name));
                }
            }
        }

        /// <summary>
        /// Removes the specified remote from .git/config file.
        /// </summary>
        /// <param name="remote">Remote to remove.</param>
        /// <returns>Output of <see cref="IGitModule.RemoveRemote"/> operation.</returns>
        public string RemoveRemote(GitRemote remote)
        {
            if (remote == null)
            {
                throw new ArgumentNullException("remote");
            }
            return _module.RemoveRemote(remote.Name);
        }

        /// <summary>
        ///   Saves the remote details by creating a new or updating an existing remote entry in .git/config file.
        /// </summary>
        /// <param name="remote">An existing remote instance or <see langword="null"/> if creating a new entry.</param>
        /// <param name="remoteName">
        ///   <para>The remote name.</para>
        ///   <para>If updating an existing remote and the name changed, it will result in remote name change and prompt for "remote update".</para>
        /// </param>
        /// <param name="remoteUrl">
        ///   <para>The remote URL.</para>
        ///   <para>If updating an existing remote and the URL changed, it will result in remote URL change and prompt for "remote update".</para>
        /// </param>
        /// <param name="remotePushUrl">An optional alternative remote push URL.</param>
        /// <param name="remotePuttySshKey">An optional Putty SSH key.</param>
        /// <returns>Result of the operation.</returns>
        public GitRemoteSaveResult SaveRemote(GitRemote remote, string remoteName, string remoteUrl, string remotePushUrl, string remotePuttySshKey)
        {
            if (string.IsNullOrWhiteSpace(remoteName))
            {
                throw new ArgumentNullException("remoteName");
            }

            remoteName = remoteName.Trim();

            // if create a new remote or updated the url - we may need to perform "update remote"
            bool updateRemoteRequired = false;
            // if operation return anything back, relay that to the user
            var output = string.Empty;

            bool creatingNew = remote == null;
            if (creatingNew)
            {
                output = _module.AddRemote(remoteName, remoteUrl);
                updateRemoteRequired = true;
            }
            else
            {
                if (!string.Equals(remote.Name, remoteName, StringComparison.OrdinalIgnoreCase))
                {
                    // the name of the remote changed - perform rename
                    output = _module.RenameRemote(remote.Name, remoteName);
                }

                if (!string.Equals(remote.Url, remoteUrl, StringComparison.OrdinalIgnoreCase))
                {
                    // the remote url changed - we may need to update remote
                    updateRemoteRequired = true;
                }
            }

            UpdateSettings(string.Format(SettingKeyString.RemoteUrl, remoteName), remoteUrl);
            UpdateSettings(string.Format(SettingKeyString.RemotePushUrl, remoteName), remotePushUrl);
            UpdateSettings(string.Format(SettingKeyString.RemotePuttySshKey, remoteName), remotePuttySshKey);

            return new GitRemoteSaveResult(output, updateRemoteRequired);
        }


        private void UpdateSettings(string settingName, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                _module.SetSetting(settingName, value);
            }
            else
            {
                _module.UnsetSetting(settingName);
            }
        }
    }
}
