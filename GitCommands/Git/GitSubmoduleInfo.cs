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

        public string Name => _module.GetSubmoduleNameByPath(LocalPath);

        public string RemotePath => _module.GetSubmoduleRemotePath(Name);

        public string CurrentCommitGuid { get; set; }
        public string Branch { get; set; }

        public bool Initialized { get; set; }
        public bool UpToDate { get; set; }

        public string Status
        {
            get
            {
                if (!Initialized)
                {
                    return "Not initialized";
                }

                if (!UpToDate)
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
