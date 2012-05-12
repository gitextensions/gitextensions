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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            var other = obj as GitSubmodule;
            if (ReferenceEquals(null, other))
                return false;
            return Equals(other.Name, Name) && Equals(other.LocalPath, LocalPath);
        }

        public override int GetHashCode()
        {
            return Name != null ? Name.GetHashCode() : 0;
        }

        public static bool operator ==(GitSubmodule a, GitSubmodule b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(GitSubmodule a, GitSubmodule b)
        {
            return !Equals(a, b);
        }

        public override bool Equals(Object obj)
        {
            return obj is GitSubmodule && this == (GitSubmodule)obj;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ LocalPath.GetHashCode();
        }
    }
}
