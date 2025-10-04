namespace GitCommands.Config
{
    /// <summary>
    /// Defines the strings to access certain git config settings.
    /// Goal is to eliminate duplicate string constants in the code.
    /// </summary>
    public static class SettingKeyString
    {
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
        /// "remote.{0}.color"
        /// </summary>
        public static string RemoteColor = "remote.{0}.color";

        /// <summary>
        /// "remote.{0}.push"
        /// </summary>
        public static readonly string RemotePush = "remote.{0}.push";

        /// <summary>
        /// "remote.{0}.pushurl"
        /// </summary>
        public static readonly string RemotePushUrl = "remote.{0}.pushurl";

        /// <summary>
        /// "remote.{0}.url"
        /// </summary>
        public static readonly string RemoteUrl = "remote.{0}.url";

        /// <summary>
        /// "remote.{0}.puttykeyfile"
        /// </summary>
        public static readonly string RemotePuttySshKey = "remote.{0}.puttykeyfile";

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

        /// <summary>
        /// core.commentchar
        /// </summary>
        /// <remarks>
        /// Default is '#'
        /// Alias: core.commentstring
        /// https://git-scm.com/docs/git-config#Documentation/git-config.txt-corecommentchar
        /// Canonical setting is core.commentstring after git 2.45.0
        /// </remarks>
        public static readonly string CommentChar = "core.commentchar";

        /// <summary>
        /// core.commentstring
        /// </summary>
        /// <remarks>
        /// Alias: core.commentchar
        /// https://git-scm.com/docs/git-config#Documentation/git-config.txt-corecommentString
        /// </remarks>
        public static readonly string CommentString = "core.commentstring";
    }
}
