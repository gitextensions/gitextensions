using System.Collections.Generic;

namespace GitUI.Script
{
    internal static class ScriptOptions
    {
        public const string SHashes = "sHashes";
        public const string STag = "sTag";
        public const string SBranch = "sBranch";
        public const string SLocalBranch = "sLocalBranch";
        public const string SRemoteBranch = "sRemoteBranch";
        public const string SRemote = "sRemote";
        public const string SRemoteUrl = "sRemoteUrl";
        public const string SRemotePathFromUrl = "sRemotePathFromUrl";
        public const string SHash = "sHash";
        public const string SMessage = "sMessage";
        public const string SAuthor = "sAuthor";
        public const string SCommitter = "sCommitter";
        public const string SAuthorDate = "sAuthorDate";
        public const string SCommitDate = "sCommitDate";
        public const string CTag = "cTag";
        public const string CBranch = "cBranch";
        public const string CLocalBranch = "cLocalBranch";
        public const string CRemoteBranch = "cRemoteBranch";
        public const string CHash = "cHash";
        public const string CMessage = "cMessage";
        public const string CAuthor = "cAuthor";
        public const string CCommitter = "cCommitter";
        public const string CAuthorDate = "cAuthorDate";
        public const string CCommitDate = "cCommitDate";
        public const string CDefaultRemote = "cDefaultRemote";
        public const string CDefaultRemoteUrl = "cDefaultRemoteUrl";
        public const string CDefaultRemotePathFromUrl = "cDefaultRemotePathFromUrl";
        public const string UserInput = "UserInput";
        public const string UserFiles = "UserFiles";
        public const string WorkingDir = "WorkingDir";

        public static IReadOnlyList<string> List => new[]
        {
            SHashes,
            STag,
            SBranch,
            SLocalBranch,
            SRemoteBranch,
            SRemote,
            SRemoteUrl,
            SRemotePathFromUrl,
            SHash,
            SMessage,
            SAuthor,
            SCommitter,
            SAuthorDate,
            SCommitDate,
            CTag,
            CBranch,
            CLocalBranch,
            CRemoteBranch,
            CHash,
            CMessage,
            CAuthor,
            CCommitter,
            CAuthorDate,
            CCommitDate,
            CDefaultRemote,
            CDefaultRemoteUrl,
            CDefaultRemotePathFromUrl,
            UserInput,
            UserFiles,
            WorkingDir
        };
    }
}
