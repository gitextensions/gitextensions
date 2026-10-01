namespace GitCommands.Config;

/// <summary>
/// Defines the strings to access certain git config settings.
/// Goal is to eliminate duplicate string constants in the code.
/// </summary>
public static class SettingKeyString
{
    /// <summary>
    ///  "branch.{0}.merge"
    /// </summary>
    public static readonly string BranchMerge = "branch.{0}.merge";

    /// <summary>
    /// "branch.{0}.remote"
    /// </summary>
    public static readonly string BranchRemote = "branch.{0}.remote";

    /// <summary>
    /// "credential.helper"
    /// </summary>
    public static readonly string CredentialHelper = "credential.helper";

    /// <summary>
    /// "i18n.filesencoding"
    /// </summary>
    public static readonly string FilesEncoding = "i18n.filesencoding";

    /// <summary>
    /// "remote."
    /// </summary>
    public static string RemoteKeyPrefix = "remote.";

    /// <summary>
    /// "remote.{0}.color"
    /// </summary>
    public static string RemoteColor = RemoteKeyPrefix + "{0}.color";

    /// <summary>
    /// "remote.{0}.prefix"
    /// </summary>
    public static readonly string RemotePrefix = RemoteKeyPrefix + "{0}.prefix";

    /// <summary>
    /// "remote.{0}.push"
    /// </summary>
    public static readonly string RemotePush = RemoteKeyPrefix + "{0}.push";

    /// <summary>
    /// "remote.{0}.pushurl"
    /// </summary>
    public static readonly string RemotePushUrl = RemoteKeyPrefix + "{0}.pushurl";

    /// <summary>
    /// ".url"
    /// </summary>
    public static readonly string RemoteUrlSuffix = ".url";

    /// <summary>
    /// "remote.{0}.url"
    /// </summary>
    public static readonly string RemoteUrl = RemoteKeyPrefix + "{0}" + RemoteUrlSuffix;

    /// <summary>
    /// "remote.{0}.puttykeyfile"
    /// </summary>
    public static readonly string RemotePuttySshKey = RemoteKeyPrefix + "{0}.puttykeyfile";

    /// <summary>
    /// user.name
    /// </summary>
    public static readonly string UserName = "user.name";

    /// <summary>
    /// user.email
    /// </summary>
    public static readonly string UserEmail = "user.email";

    /// <summary>
    /// diff.guitool
    /// </summary>
    public static readonly string DiffToolKey = "diff.guitool";

    /// <summary>
    /// merge.guitool, requires Git 2.20.0
    /// </summary>
    public static readonly string MergeToolKey = "merge.guitool";

    /// <summary>
    /// merge.tool
    /// </summary>
    public static readonly string MergeToolNoGuiKey = "merge.tool";

    /// <summary>
    /// protocol.file.allow
    /// </summary>
    public static readonly string AllowFileProtocol = "protocol.file.allow";
}
