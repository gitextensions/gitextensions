using GitUIPluginInterfaces;

namespace GitCommands
{
    public sealed class GitSubmoduleInfo : IGitSubmoduleInfo
    {
        public string Name { get; }
        public string LocalPath { get; }
        public string RemotePath { get; }
        public ObjectId CurrentCommitId { get; }
        public string Branch { get; }
        public bool IsInitialized { get; }
        public bool IsUpToDate { get; }

        public GitSubmoduleInfo(string name, string localPath, string remotePath, ObjectId currentCommitGuid, string branch, bool isInitialized, bool isUpToDate)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            LocalPath = localPath ?? throw new ArgumentNullException(nameof(localPath));
            RemotePath = remotePath ?? throw new ArgumentNullException(nameof(remotePath));
            CurrentCommitId = currentCommitGuid ?? throw new ArgumentNullException(nameof(currentCommitGuid));
            Branch = branch ?? throw new ArgumentNullException(nameof(branch));
            IsInitialized = isInitialized;
            IsUpToDate = isUpToDate;
        }

        public string Status
        {
            get
            {
                if (!IsInitialized)
                {
                    return "Not initialized";
                }

                if (!IsUpToDate)
                {
                    return "Modified";
                }

                return "Up-to-date";
            }
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Branch))
            {
                return LocalPath;
            }

            return LocalPath + " [" + Branch + "]";
        }
    }
}
