using GitCommands.Config;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Configurations;
using GitExtensions.Extensibility.Git;
using Microsoft;

namespace GitCommands.Remotes;

public interface IConfigFileRemoteSettingsManager
{
    void ConfigureRemotes(string remoteName);

    /// <summary>
    /// Returns the default remote for push operation.
    /// </summary>
    /// <returns>The <see cref="GitRef.Name"/> if found; otherwise, <see langword="null"/>.</returns>
    string? GetDefaultPushRemote(ConfigFileRemote remote, string branch);

    /// <summary>
    /// Loads the list of remotes configured in .git/config file.
    /// </summary>
    IEnumerable<ConfigFileRemote> LoadRemotes(bool loadDisabled);

    /// <summary>
    /// Removes the specified remote from .git/config file.
    /// </summary>
    /// <param name="remote">Remote to remove.</param>
    /// <returns>Output of the operation.</returns>
    string RemoveRemote(ConfigFileRemote remote);

    /// <summary>
    /// Returns true if input remote exists and is enabled.
    /// </summary>
    /// <param name="remoteName">Name of remote to check.</param>
    /// <returns><see langword="true"/> if input remote exists and is enabled; otherwise, <see langword="false"/>.</returns>
    bool EnabledRemoteExists(string remoteName);

    /// <summary>
    /// Returns true if input remote exists and is disabled.
    /// </summary>
    /// <param name="remoteName">Name of remote to check.</param>
    /// <returns><see langword="true"/> if input remote exists and is disabled; otherwise, <see langword="false"/>.</returns>
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
    /// <param name="remoteColor">An optional color for the remote.</param>
    /// <returns>Result of the operation.</returns>
    ConfigFileRemoteSaveResult SaveRemote(ConfigFileRemote? remote, string remoteName, string remoteUrl, string? remotePushUrl, string remotePuttySshKey, string? remoteColor);

    /// <summary>
    ///  Marks the remote as enabled or disabled in .git/config file.
    /// </summary>
    /// <param name="remoteName">An existing remote instance.</param>
    /// <param name="disabled">The new state of the remote. <see langword="true"/> to disable the remote; otherwise <see langword="false"/>.</param>
    void ToggleRemoteState(string remoteName, bool disabled);

    /// <summary>
    /// Retrieves disabled remotes from .git/config file.
    /// </summary>
    IReadOnlyList<Remote> GetDisabledRemotes();

    /// <summary>
    /// Retrieves disabled remote names from the .git/config file.
    /// </summary>
    IReadOnlyList<string> GetDisabledRemoteNames();

    /// <summary>
    /// Retrieves enabled remote names.
    /// </summary>
    IReadOnlyList<string> GetEnabledRemoteNames();
}

public class ConfigFileRemoteSettingsManager : IConfigFileRemoteSettingsManager
{
    internal static readonly string DisabledSectionPrefix = "-";
    internal static readonly string SectionRemote = "remote";
    internal static readonly string SectionRemoteDisabled = $"{DisabledSectionPrefix}{SectionRemote}";
    private readonly Func<IGitModule?> _getModule;

    public ConfigFileRemoteSettingsManager(Func<IGitModule?> getModule)
    {
        _getModule = getModule;
    }

    // TODO: moved verbatim from FormRemotes.cs, perhaps needs refactoring
    public void ConfigureRemotes(string remoteName)
    {
        IGitModule module = GetModule();
        IReadOnlyList<IGitRef> moduleRefs = module.GetRefs(RefsFilter.Heads);

        foreach (IGitRef remoteHead in moduleRefs)
        {
            if (!remoteHead.IsRemote ||
                !remoteHead.Name.Contains(remoteName, StringComparison.InvariantCultureIgnoreCase))
            {
                continue;
            }

            foreach (IGitRef localHead in moduleRefs)
            {
                if (localHead.IsRemote ||
                    !string.IsNullOrEmpty(localHead.TrackingRemote) ||
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
    public string? GetDefaultPushRemote(ConfigFileRemote remote, string branch)
    {
        ArgumentNullException.ThrowIfNull(remote);

        IGitModule module = GetModule();

        GitRef remoteHead = remote.Push
                               .Select(s => s.Split(Delimiters.Colon))
                               .Where(t => t.Length == 2)
                               .Where(t => IsSettingForBranch(t[0], branch))
                               .Select(t => new GitRef(module, objectId: null, t[1]))
                               .FirstOrDefault(h => h.IsHead);

        if (remoteHead is not null)
        {
            return remoteHead.Name;
        }

        GitRef remoteWildcardHead = remote.Push
                               .Select(s => s.Split(Delimiters.Colon))
                               .Where(t => t.Length == 2)
                               .Where(t => IsSettingForWildcardBranch(t[0]))
                               .Select(t => new GitRef(module, objectId: null, t[1].Replace("*", branch)))
                               .FirstOrDefault(h => h.IsHead);

        return remoteWildcardHead?.Name;

        bool IsSettingForBranch(string setting, string branchName)
        {
            GitRef head = new(module, objectId: null, setting);
            return head.IsHead && head.Name.Equals(branchName, StringComparison.OrdinalIgnoreCase);
        }

        bool IsSettingForWildcardBranch(string setting)
        {
            GitRef head = new(module, objectId: null, setting);
            return head.IsHead && head.Name == "*";
        }
    }

    /// <summary>
    /// Retrieves disabled remotes from .git/config file.
    /// </summary>
    public IReadOnlyList<Remote> GetDisabledRemotes()
    {
        IGitModule module = GetModule();

        return GetDisabledRemoteNames()
            .Select(remoteName =>
            {
                string settingPrefix = $"{SectionRemoteDisabled}.{remoteName}.";
                string url = module.GetEffectiveSetting($"{settingPrefix}url");
                string firstPushUrl = module.GetSettings($"{settingPrefix}pushurl").FirstOrDefault(defaultValue: url);
                return new Remote(name: remoteName, fetchUrl: url, firstPushUrl);
            })
            .ToList();
    }

    public IReadOnlyList<string> GetDisabledRemoteNames()
    {
        IGitModule module = GetModule();

        return EnumerateDisabledRemoteNames()
            .WhereNotNull()
            .Distinct()
            .ToList();

        IEnumerable<string> EnumerateDisabledRemoteNames()
        {
            foreach ((string setting, string _) in module.GetAllLocalSettings())
            {
                (string section, string? subsection, string _) = IGitConfigSettingsGetter.SplitSetting(setting);
                if (section == SectionRemoteDisabled)
                {
                    yield return subsection;
                }
            }
        }
    }

    public IReadOnlyList<string> GetEnabledRemoteNames()
    {
        return GetModule().GetRemoteNames();
    }

    // TODO: candidate for Async implementations
    public IEnumerable<ConfigFileRemote> LoadRemotes(bool loadDisabled)
    {
        List<ConfigFileRemote> remotes = [];
        IGitModule module = _getModule();
        if (module is null)
        {
            return remotes;
        }

        PopulateRemotes(module, remotes, true);
        if (loadDisabled)
        {
            PopulateRemotes(module, remotes, false);
        }

        return remotes.OrderBy(x => x.Name);

        // pass the list in to minimise allocations
        void PopulateRemotes(IGitModule module, List<ConfigFileRemote> allRemotes, bool enabled)
        {
            Func<IReadOnlyList<string>> func;
            if (enabled)
            {
                func = module.GetRemoteNames;
            }
            else
            {
                func = GetDisabledRemoteNames;
            }

            List<string> gitRemotes = [.. func().Where(x => !string.IsNullOrWhiteSpace(x))];
            if (gitRemotes.Count == 0)
            {
                return;
            }

            allRemotes.AddRange(gitRemotes.Select(remote => new ConfigFileRemote
            {
                Disabled = !enabled,
                Name = remote,
                Url = module.GetSetting(GetSettingKey(SettingKeyString.RemoteUrl, remote, enabled)),
                Push = module.GetSettings(GetSettingKey(SettingKeyString.RemotePush, remote, enabled)).ToList(),
                Color = module.GetSetting(GetSettingKey(SettingKeyString.RemoteColor, remote, enabled)),

                // Note: This only gets the last pushurl
                PushUrl = module.GetSetting(GetSettingKey(SettingKeyString.RemotePushUrl, remote, enabled)),
                PuttySshKey = module.GetSetting(GetSettingKey(SettingKeyString.RemotePuttySshKey, remote, enabled))
            }));
        }
    }

    /// <summary>
    /// Removes the specified remote from .git/config file.
    /// </summary>
    /// <param name="remote">Remote to remove.</param>
    /// <returns>Output of <see cref="IGitModule.RemoveRemote"/> operation, if the remote is active; otherwise <see cref="string.Empty"/>.</returns>
    public string RemoveRemote(ConfigFileRemote remote)
    {
        ArgumentNullException.ThrowIfNull(remote);

        IGitModule module = GetModule();
        if (!remote.Disabled)
        {
            Validates.NotNull(remote.Name);
            return module.RemoveRemote(remote.Name);
        }

        module.RemoveConfigSection(SectionRemoteDisabled, subsection: remote.Name);
        return string.Empty;
    }

    public bool EnabledRemoteExists(string remoteName)
    {
        return GetEnabledRemoteNames().Any(r => r == remoteName);
    }

    public bool DisabledRemoteExists(string remoteName)
    {
        return GetDisabledRemoteNames().Any(r => r == remoteName);
    }

    public ConfigFileRemoteSaveResult SaveRemote(ConfigFileRemote? remote, string remoteName, string remoteUrl, string? remotePushUrl, string remotePuttySshKey, string? remoteColor)
    {
        if (string.IsNullOrWhiteSpace(remoteName))
        {
            throw new ArgumentNullException(nameof(remoteName));
        }

        remoteName = remoteName.Trim();

        // if create a new remote or updated the url - we may need to perform "update remote"
        bool updateRemoteRequired = false;

        // if operation return anything back, relay that to the user
        string output = string.Empty;

        IGitModule module = GetModule();
        bool remoteDisabled = false;
        if (remote is null)
        {
            output = module.AddRemote(remoteName, remoteUrl);

            // If output was returned, something went wrong
            if (!string.IsNullOrWhiteSpace(output))
            {
                return new ConfigFileRemoteSaveResult(output, false);
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
                return new ConfigFileRemoteSaveResult(null, false);
            }

            remoteDisabled = remote.Disabled;
            if (!string.Equals(remote.Name, remoteName, StringComparison.Ordinal))
            {
                Validates.NotNull(remote.Name);

                // the name of the remote changed - perform rename
                output = module.RenameRemote(remote.Name, remoteName);
            }

            if (!string.Equals(remote.Url, remoteUrl, StringComparison.Ordinal))
            {
                // the remote url changed - we may need to update remote
                updateRemoteRequired = true;
            }
        }

        // If URL is in a Windows path, it may need to be converted to WSL Git path
        if (remoteUrl is not null && !Uri.IsWellFormedUriString(remoteUrl, UriKind.RelativeOrAbsolute))
        {
            remoteUrl = module.GetPathForGitExecution(remoteUrl);
        }

        UpdateSettings(module, remoteName, remoteDisabled, SettingKeyString.RemoteUrl, remoteUrl);
        if (remotePushUrl is not null && !Uri.IsWellFormedUriString(remotePushUrl, UriKind.RelativeOrAbsolute))
        {
            remotePushUrl = module.GetPathForGitExecution(remotePushUrl);
        }

        UpdateSettings(module, remoteName, remoteDisabled, SettingKeyString.RemotePushUrl, remotePushUrl);
        UpdateSettings(module, remoteName, remoteDisabled, SettingKeyString.RemotePuttySshKey, remotePuttySshKey);
        UpdateSettings(module, remoteName, remoteDisabled, SettingKeyString.RemoteColor, remoteColor);

        return new ConfigFileRemoteSaveResult(output, updateRemoteRequired);
    }

    public void ToggleRemoteState(string remoteName, bool disabled)
    {
        if (string.IsNullOrWhiteSpace(remoteName))
        {
            throw new ArgumentNullException(nameof(remoteName));
        }

        // disabled is the new state, so if the new state is 'false' (=enabled), then the existing state is 'true' (=disabled, i.e. '-remote')
        string sectionName = disabled ? SectionRemote : SectionRemoteDisabled;
        string newSectionName = disabled ? SectionRemoteDisabled : SectionRemote;

        IGitModule module = GetModule();

        List<(string, string)> newSectionValues = [];
        bool newSectionAlreadyExists = false;
        foreach ((string setting, string value) in module.GetAllLocalSettings())
        {
            (string section, string? subsection, string name) = IGitConfigSettingsGetter.SplitSetting(setting);
            if (subsection != remoteName)
            {
                continue;
            }

            if (section == newSectionName)
            {
                newSectionAlreadyExists = true;
                continue;
            }

            if (section != sectionName)
            {
                continue;
            }

            string newSetting = $"{newSectionName}.{remoteName}.{name}";
            newSectionValues.Add((newSetting, value));
        }

        if (newSectionValues.Count == 0)
        {
            // we didn't find the section, nothing we can do
            return;
        }

        if (disabled)
        {
            module.RemoveRemote(remoteName);
        }
        else
        {
            module.RemoveConfigSection(sectionName, subsection: remoteName);
        }

        // ensure that the section with the same name doesn't already exist
        // use case:
        // - a user has added a remote,
        // - then deactivated the remote via GE
        // - then added a remote with the same name from a command line or via UI
        // - then attempted to deactivate the new remote
        if (newSectionAlreadyExists)
        {
            module.RemoveConfigSection(newSectionName, subsection: remoteName);
        }

        // add the renamed remote settings
        foreach ((string setting, string value) in newSectionValues)
        {
            module.SetSetting(setting, value, append: true);
        }
    }

    private IGitModule GetModule()
    {
        IGitModule module = _getModule();
        if (module is null)
        {
            throw new ArgumentException($"Require a valid instance of {nameof(IGitModule)}");
        }

        return module;
    }

    private static string GetSettingKey(string settingKey, string remoteName, bool remoteEnabled)
    {
        string key = string.Format(settingKey, remoteName);
        return remoteEnabled ? key : DisabledSectionPrefix + key;
    }

    private static void UpdateSettings(IGitModule module, string remoteName, bool remoteDisabled, string settingName, string? value)
    {
        string prefix = remoteDisabled ? DisabledSectionPrefix : string.Empty;
        string fullSettingName = prefix + string.Format(settingName, remoteName);

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
