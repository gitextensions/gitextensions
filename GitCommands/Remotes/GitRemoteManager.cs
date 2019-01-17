using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GitCommands.Config;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands.Remotes
{
    public interface IGitRemoteManager
    {
        void ConfigureRemotes(string remoteName);

        /// <summary>
        /// Returns the default remote for push operation.
        /// </summary>
        /// <returns>The <see cref="GitRef.Name"/> if found, otherwise <see langword="null"/>.</returns>
        string GetDefaultPushRemote(GitRemote remote, string branch);

        /// <summary>
        /// Loads the list of remotes configured in .git/config file.
        /// </summary>
        IEnumerable<GitRemote> LoadRemotes(bool loadDisabled);

        /// <summary>
        /// Removes the specified remote from .git/config file.
        /// </summary>
        /// <param name="remote">Remote to remove.</param>
        /// <returns>Output of the operation.</returns>
        string RemoveRemote(GitRemote remote);

        /// <summary>
        /// Returns true if input remote exists and is enabled.
        /// </summary>
        /// <param name="remoteName">Name of remote to check.</param>
        /// <returns>True if input remote exists and is enabled.</returns>
        bool EnabledRemoteExists(string remoteName);

        /// <summary>
        /// Returns true if input remote exists and is disabled.
        /// </summary>
        /// <param name="remoteName">Name of remote to check.</param>
        /// <returns>True if input remote exists and is disabled.</returns>
        bool DisabledRemoteExists(string remoteName);

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

        /// <summary>
        ///  Marks the remote as enabled or disabled in .git/config file.
        /// </summary>
        /// <param name="remoteName">The name of the remote.</param>
        void ToggleRemoteState(string remoteName, bool disabled);

        /// <summary>
        /// Retrieves disabled remotes from .git/config file
        /// </summary>
        IReadOnlyList<Remote> GetDisabledRemotes();

        /// <summary>
        /// Retrieves disabled remote names from the .git/config file.
        /// </summary>
        IReadOnlyList<string> GetDisabledRemoteNames();

        /// <summary>
        /// Retrieves enabled remote names
        /// </summary>
        IReadOnlyList<string> GetEnabledRemoteNames();

        /// <summary>
        /// Retrieves enabled remote names of remotes without branches (i.e. that require a fetch)
        /// </summary>
        IReadOnlyList<string> GetEnabledRemoteNamesWithoutBranches();
    }

    public class GitRemoteManager : IGitRemoteManager
    {
        internal static readonly string DisabledSectionPrefix = "-";
        internal static readonly string SectionRemote = "remote";
        private readonly Func<IGitModule> _getModule;

        public GitRemoteManager(Func<IGitModule> getModule)
        {
            _getModule = getModule;
        }

        // TODO: moved verbatim from FormRemotes.cs, perhaps needs refactoring
        [SuppressMessage("ReSharper", "RedundantArgumentDefaultValue")]
        public void ConfigureRemotes(string remoteName)
        {
            var module = GetModule();
            var localConfig = module.LocalConfigFile;
            var moduleRefs = module.GetRefs(tags: false, branches: true);

            foreach (var remoteHead in moduleRefs)
            {
                if (!remoteHead.IsRemote ||
                    !remoteHead.Name.Contains(remoteName, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                foreach (var localHead in moduleRefs)
                {
                    if (localHead.IsRemote ||
                        !string.IsNullOrEmpty(localHead.GetTrackingRemote(localConfig)) ||
                        !remoteHead.Name.Contains(localHead.Name, StringComparison.InvariantCultureIgnoreCase))
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
        /// <returns>The <see cref="GitRef.Name"/> if found, otherwise <see langword="null"/>.</returns>
        // TODO: moved verbatim from FormPush.cs, perhaps needs refactoring
        [CanBeNull]
        public string GetDefaultPushRemote(GitRemote remote, string branch)
        {
            if (remote == null)
            {
                throw new ArgumentNullException(nameof(remote));
            }

            var module = GetModule();
            bool IsSettingForBranch(string setting, string branchName)
            {
                var head = new GitRef(module, null, setting);
                return head.IsHead && head.Name.Equals(branchName, StringComparison.OrdinalIgnoreCase);
            }

            var remoteHead = remote.Push
                                   .Select(s => s.Split(':'))
                                   .Where(t => t.Length == 2)
                                   .Where(t => IsSettingForBranch(t[0], branch))
                                   .Select(t => new GitRef(module, null, t[1]))
                                   .FirstOrDefault(h => h.IsHead);

            return remoteHead?.Name;
        }

        /// <summary>
        /// Retrieves disabled remotes from .git/config file
        /// </summary>
        public IReadOnlyList<Remote> GetDisabledRemotes()
        {
            var disabledRemotes = GetModule().LocalConfigFile.GetConfigSections()
                .Where(s => s.SectionName == $"{DisabledSectionPrefix}remote")
                .Select(s => new Remote(s.SubSection, s.GetValue("pushurl", s.GetValue("url")), s.GetValue("url")))
                .ToList();

            return disabledRemotes;
        }

        /// <summary>
        /// Retrieves disabled remote names from the .git/config file.
        /// </summary>
        public IReadOnlyList<string> GetDisabledRemoteNames()
        {
            var module = GetModule();
            return module.LocalConfigFile.GetConfigSections()
                .Where(s => s.SectionName == $"{DisabledSectionPrefix}remote")
                .Select(s => s.SubSection)
                .ToList();
        }

        /// <summary>
        /// Retrieves enabled remote names
        /// </summary>
        public IReadOnlyList<string> GetEnabledRemoteNames()
        {
            return GetModule().GetRemoteNames();
        }

        /// <summary>
        /// Retrieves enabled remote names of remotes without branches (i.e. that require a fetch)
        /// </summary>
        public IReadOnlyList<string> GetEnabledRemoteNamesWithoutBranches()
        {
            HashSet<string> remotesWithBranches = GetModule().GetRefs()
                .Where(branch => branch.IsRemote && !branch.IsTag)
                .Select(branch => branch.Name.SubstringUntil('/'))
                .ToHashSet();

            HashSet<string> allRemotes = GetEnabledRemoteNames().ToHashSet();

            return allRemotes.Except(remotesWithBranches).ToList();
        }

        /// <summary>
        /// Loads the list of remotes configured in .git/config file.
        /// </summary>
        // TODO: candidate for Async implementations
        public IEnumerable<GitRemote> LoadRemotes(bool loadDisabled)
        {
            var remotes = new List<GitRemote>();
            var module = _getModule();
            if (module == null)
            {
                return remotes;
            }

            PopulateRemotes(remotes, true);
            if (loadDisabled)
            {
                PopulateRemotes(remotes, false);
            }

            return remotes.OrderBy(x => x.Name);
        }

        /// <summary>
        /// Removes the specified remote from .git/config file.
        /// </summary>
        /// <param name="remote">Remote to remove.</param>
        /// <returns>Output of <see cref="IGitModule.RemoveRemote"/> operation, if the remote is active; otherwise <see cref="string.Empty"/>.</returns>
        public string RemoveRemote(GitRemote remote)
        {
            if (remote == null)
            {
                throw new ArgumentNullException(nameof(remote));
            }

            var module = GetModule();
            if (!remote.Disabled)
            {
                return module.RemoveRemote(remote.Name);
            }

            var sectionName = $"{DisabledSectionPrefix}{SectionRemote}.{remote.Name}";
            module.LocalConfigFile.RemoveConfigSection(sectionName, true);
            return string.Empty;
        }

        /// <summary>
        /// Returns true if input remote exists and is enabled.
        /// </summary>
        /// <param name="remoteName">Name of remote to check.</param>
        /// <returns>True if input remote exists and is enabled.</returns>
        public bool EnabledRemoteExists(string remoteName)
        {
            return GetEnabledRemoteNames().FirstOrDefault(r => r == remoteName) != null;
        }

        /// <summary>
        /// Returns true if input remote exists and is disabled.
        /// </summary>
        /// <param name="remoteName">Name of remote to check.</param>
        /// <returns>True if input remote exists and is disabled.</returns>
        public bool DisabledRemoteExists(string remoteName)
        {
            return GetDisabledRemoteNames().FirstOrDefault(r => r == remoteName) != null;
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
                throw new ArgumentNullException(nameof(remoteName));
            }

            remoteName = remoteName.Trim();

            // if create a new remote or updated the url - we may need to perform "update remote"
            bool updateRemoteRequired = false;

            // if operation return anything back, relay that to the user
            var output = string.Empty;

            var module = GetModule();
            bool creatingNew = remote == null;
            bool remoteDisabled = false;
            if (creatingNew)
            {
                output = module.AddRemote(remoteName, remoteUrl);

                // If output was returned, something went wrong
                if (!string.IsNullOrWhiteSpace(output))
                {
                    return new GitRemoteSaveResult(output, false);
                }

                updateRemoteRequired = true;
            }
            else
            {
                if (remote.Disabled)
                {
                    // disabled branches can't updated as it poses to many problems, i.e.
                    // - verify that the branch name is valid, and
                    // - it does not duplicate an active branch name etc.
                    return new GitRemoteSaveResult(null, false);
                }

                remoteDisabled = remote.Disabled;
                if (!string.Equals(remote.Name, remoteName, StringComparison.OrdinalIgnoreCase))
                {
                    // the name of the remote changed - perform rename
                    output = module.RenameRemote(remote.Name, remoteName);
                }

                if (!string.Equals(remote.Url, remoteUrl, StringComparison.OrdinalIgnoreCase))
                {
                    // the remote url changed - we may need to update remote
                    updateRemoteRequired = true;
                }
            }

            UpdateSettings(module, remoteName, remoteDisabled, SettingKeyString.RemoteUrl, remoteUrl);
            UpdateSettings(module, remoteName, remoteDisabled, SettingKeyString.RemotePushUrl, remotePushUrl);
            UpdateSettings(module, remoteName, remoteDisabled, SettingKeyString.RemotePuttySshKey, remotePuttySshKey);

            return new GitRemoteSaveResult(output, updateRemoteRequired);
        }

        /// <summary>
        ///  Marks the remote as enabled or disabled in .git/config file.
        /// </summary>
        /// <param name="remoteName">An existing remote instance.</param>
        /// <param name="disabled">The new state of the remote. <see langword="true"/> to disable the remote; otherwise <see langword="false"/>.</param>
        public void ToggleRemoteState(string remoteName, bool disabled)
        {
            if (string.IsNullOrWhiteSpace(remoteName))
            {
                throw new ArgumentNullException(nameof(remoteName));
            }

            // disabled is the new state, so if the new state is 'false' (=enabled), then the existing state is 'true' (=disabled, i.e. '-remote')
            var sectionName = (disabled ? "" : DisabledSectionPrefix) + SectionRemote;

            var module = GetModule();
            var sections = module.LocalConfigFile.GetConfigSections();
            var section = sections.FirstOrDefault(s => s.SectionName == sectionName && s.SubSection == remoteName);
            if (section == null)
            {
                // we didn't find it, nothing we can do
                return;
            }

            if (disabled)
            {
                module.RemoveRemote(remoteName);
            }
            else
            {
                module.LocalConfigFile.RemoveConfigSection($"{sectionName}.{remoteName}");
            }

            var newSectionName = (disabled ? DisabledSectionPrefix : "") + SectionRemote;

            // ensure that the section with the same name doesn't already exist
            // use case:
            // - a user has added a remote,
            // - then deactivated the remote via GE
            // - then added a remote with the same name from a command line or via UI
            // - then attempted to deactivate the new remote
            var dupSection = sections.FirstOrDefault(s => s.SectionName == newSectionName && s.SubSection == remoteName);
            if (dupSection != null)
            {
                module.LocalConfigFile.RemoveConfigSection($"{newSectionName}.{remoteName}");
            }

            // rename the remote
            section.SectionName = newSectionName;

            module.LocalConfigFile.AddConfigSection(section);
            module.LocalConfigFile.Save();
        }

        // pass the list in to minimise allocations
        private void PopulateRemotes(List<GitRemote> allRemotes, bool enabled)
        {
            var module = GetModule();

            Func<IReadOnlyList<string>> func;
            if (enabled)
            {
                func = () => module.GetRemoteNames();
            }
            else
            {
                func = GetDisabledRemoteNames;
            }

            var gitRemotes = func().Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            if (gitRemotes.Any())
            {
                allRemotes.AddRange(gitRemotes.Select(remote => new GitRemote
                {
                    Disabled = !enabled,
                    Name = remote,
                    Url = module.GetSetting(GetSettingKey(SettingKeyString.RemoteUrl, remote, enabled)),
                    Push = module.GetSettings(GetSettingKey(SettingKeyString.RemotePush, remote, enabled)).ToList(),
                    PushUrl = module.GetSetting(GetSettingKey(SettingKeyString.RemotePushUrl, remote, enabled)),
                    PuttySshKey = module.GetSetting(GetSettingKey(SettingKeyString.RemotePuttySshKey, remote, enabled)),
                }));
            }
        }

        private IGitModule GetModule()
        {
            var module = _getModule();
            if (module == null)
            {
                throw new ArgumentException($"Require a valid instance of {nameof(IGitModule)}");
            }

            return module;
        }

        private static string GetSettingKey(string settingKey, string remoteName, bool remoteEnabled)
        {
            var key = string.Format(settingKey, remoteName);
            return remoteEnabled ? key : DisabledSectionPrefix + key;
        }

        private static void UpdateSettings(IGitModule module, string remoteName, bool remoteDisabled, string settingName, string value)
        {
            var prefix = remoteDisabled ? DisabledSectionPrefix : string.Empty;
            var fullSettingName = prefix + string.Format(settingName, remoteName);

            if (!string.IsNullOrWhiteSpace(value))
            {
                module.SetSetting(fullSettingName, value);
            }
            else
            {
                module.UnsetSetting(fullSettingName);
            }
        }
    }
}
