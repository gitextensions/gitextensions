using GitCommands.Config;

namespace GitCommands.Remotes;

public class ConfigFileRemote
{
    /// <summary>
    ///  Gets or sets value stored in .git/config via <see cref="SettingKeyString.RemoteColor"/> key.
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    ///  Gets or sets value indicating whether the remote is enabled or not.
    ///  If remote section is [remote branch] then it is considered enabled, if it is [-remote branch] then it is disabled.
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    ///  Gets or sets the name of the remote branch.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///  Gets or sets value stored in .git/config via <see cref="SettingKeyString.RemotePush"/> key.
    /// </summary>
    public IReadOnlyList<string>? Push { get; set; }

    /// <summary>
    ///  Gets or sets an optional prefix stored in <c>.git/config</c> via the
    ///  <see cref="SettingKeyString.RemotePrefix"/> key (<c>remote.&lt;name&gt;.prefix</c>).
    /// </summary>
    /// <remarks>
    ///  When a push has no explicit tracking branch or default push remote configured,
    ///  this value is prepended to the local branch name to form the remote branch name
    ///  (e.g. a prefix of <c>"user/"</c> turns local branch <c>feature</c> into remote branch
    ///  <c>user/feature</c>).
    /// </remarks>
    public string? Prefix { get; set; }

    /// <summary>
    ///  Gets or sets the last pushurl stored in .git/config via <see cref="SettingKeyString.RemotePushUrl"/> key.
    /// </summary>
    public string? PushUrl { get; set; }

    /// <summary>
    ///  Gets or sets value stored in .git/config via <see cref="SettingKeyString.RemotePuttySshKey"/> key.
    /// </summary>
    public string? PuttySshKey { get; set; }

    /// <summary>
    ///  Gets or sets value stored in .git/config via <see cref="SettingKeyString.RemoteUrl"/> key.
    /// </summary>
    public string? Url { get; set; }
}
