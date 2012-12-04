using System;
using GitUIPluginInterfaces;

namespace GitCommands
{
    public sealed class GitSubmodule : IGitSubmodule
    {
        private readonly GitModule Module;
        public string LocalPath { get; set; }

        public GitSubmodule(GitModule aModule)
        {
            Module = aModule;
        }

        public string Name
        {
            get
            {
                return Module.GetSubmoduleNameByPath(LocalPath);
            }
            set
            {
            }
        }

        public string RemotePath
        {
            get
            {
                return Module.GetSubmoduleRemotePath(Name);
            }
            set
            {
            }
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

        public static bool operator ==(GitSubmodule a, GitSubmodule b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
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

        public static bool operator !=(GitSubmodule a, GitSubmodule b)
        {
            return !(a == b);
        }

        public override bool Equals(Object obj)
        {
            return obj is GitSubmodule && this == (GitSubmodule)obj;
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
