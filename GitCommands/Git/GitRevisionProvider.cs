using System;
using GitUIPluginInterfaces;

namespace GitCommands.Git
{
    public interface IGitRevisionProvider
    {
        GitRevision Get(string sha1);
    }

    public sealed class GitRevisionProvider : IGitRevisionProvider
    {
        private readonly Func<IGitModule> _getModule;


        public GitRevisionProvider(Func<IGitModule> getModule)
        {
            _getModule = getModule;
        }


        public GitRevision Get(string sha1)
        {
            if (sha1.IsNullOrWhiteSpace() || sha1.Length >= 40)
            {
                return new GitRevision(sha1);
            }

            var module = GetModule();
            if (module.IsExistingCommitHash(sha1, out var fullSha1))
            {
                sha1 = fullSha1;
            }

            return new GitRevision(sha1);
        }

        private IGitModule GetModule()
        {
            var module = _getModule();
            if (module == null)
            {
                throw new ArgumentException($"Require a valid instance of {nameof(IGitModule)}");
            }
            return module;
        }
    }
}