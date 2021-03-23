using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace GitCommands.Settings
{
    internal static class GitExtensionConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddGitExtensionConfiguration(this IConfigurationBuilder configurationBuilder, string filePath)
        {
            var provider = new PhysicalFileProvider(Path.GetDirectoryName(filePath));
            var path = Path.GetFileName(filePath);
            var source = new GitExtensionConfigurationSource
            {
                FileProvider = provider,
                Path = path,
                ReloadOnChange = true,
                Optional = true
            };

            return configurationBuilder.Add(source);
        }
    }
}
