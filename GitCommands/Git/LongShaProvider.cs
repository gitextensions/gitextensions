using System;
using GitUIPluginInterfaces;

namespace GitCommands.Git
{
    public interface ILongShaProvider
    {
        string Get(string sha1);
    }

    public sealed class LongShaProvider : ILongShaProvider
    {
        private readonly Func<IGitModule> _getModule;

        public LongShaProvider(Func<IGitModule> getModule)
        {
            _getModule = getModule;
        }

        public string Get(string sha1)
        {
            if (sha1.IsNullOrWhiteSpace() || sha1.Length >= 40)
            {
                return sha1;
            }

            var module = GetModule();
            if (module.IsExistingCommitHash(sha1, out var fullSha1))
            {
                sha1 = fullSha1;
            }

            return sha1;
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