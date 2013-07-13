using System;
using GitUIPluginInterfaces;

namespace GitCommands
{
    public sealed class GitSubmoduleInfo : IGitSubmoduleInfo
    {
        private readonly GitModule _module;
        public string LocalPath { get; set; }

        public GitSubmoduleInfo(GitModule module)
        {
            _module = module;
        }

        public string Name
        {
            get { return _module.GetSubmoduleNameByPath(LocalPath); }
        }

        public string RemotePath
        {
            get { return _module.GetSubmoduleRemotePath(Name); }
        }

        public string CurrentCommitGuid { get; set; }
        public string Branch { get; set; }

        public bool Initialized { get; set; }
        public bool UpToDate { get; set; }

        public string Status
        {
            get
            {
                if (!Initialized)
                    return "Not initialized";
                if (!UpToDate)
                    return "Modified";

                return "Up-to-date";
            }
        }

        public static bool operator ==(GitSubmoduleInfo a, GitSubmoduleInfo b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.LocalPath == b.LocalPath;
        }

        public static bool operator !=(GitSubmoduleInfo a, GitSubmoduleInfo b)
        {
            return !(a == b);
        }

        public override bool Equals(Object obj)
        {
            return obj is GitSubmoduleInfo && this == (GitSubmoduleInfo)obj;
        }

        public override int GetHashCode()
        {
            return LocalPath != null ? LocalPath.GetHashCode() : 0;
        }

        public override string ToString()
        {
            if (String.IsNullOrEmpty(Branch))
                return LocalPath;
            return LocalPath + " [" + Branch + "]";
        }
    }
}
