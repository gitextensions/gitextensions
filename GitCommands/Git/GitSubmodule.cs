using System;
using GitUIPluginInterfaces;

namespace GitCommands
{
    public sealed class GitSubmodule : IGitSubmodule
    {
        public string Name { get; set; }
        public string RemotePath
        {
            get
            {
                return Settings.Module.GetSubmoduleRemotePath(Name);
            }
            set
            {
            }
        }

        public string LocalPath
        {
            get
            {
                return Settings.Module.GetSubmoduleLocalPath(Name);
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
            return a.Name == b.Name && a.LocalPath == b.LocalPath;
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
            return (Name != null ? Name.GetHashCode() : 0) &
                (LocalPath != null ? LocalPath.GetHashCode() : 0);
        }
    }
}
