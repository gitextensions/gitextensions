using Microsoft.Extensions.Configuration;

namespace GitCommands.Settings
{
    internal sealed class GitExtensionConfigurationSource : FileConfigurationSource
    {
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
            => new GitExtensionConfigurationProvider(this);
    }
}
