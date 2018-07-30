using System;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands
{
    public sealed class GitSubmoduleInfo : IGitSubmoduleInfo
    {
        [NotNull] public string Name { get; }
        [NotNull] public string LocalPath { get; }
        [NotNull] public string RemotePath { get; }
        [NotNull] public ObjectId CurrentCommitId { get; }
        [NotNull] public string Branch { get; }
        public bool IsInitialized { get; }
        public bool IsUpToDate { get; }

        public GitSubmoduleInfo([NotNull] string name, [NotNull] string localPath, [NotNull] string remotePath, [NotNull] ObjectId currentCommitGuid, [NotNull] string branch, bool isInitialized, bool isUpToDate)
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
