using System;
using JetBrains.Annotations;

namespace GitCommands.Config
{
    public class GitConfigurationException : Exception
    {
        public GitConfigurationException([NotNull] string configPath, [CanBeNull] Exception innerException)
            : this(configPath, message: null, innerException)
        {
        }

        public GitConfigurationException([NotNull] string configPath, [CanBeNull] string message, [CanBeNull] Exception innerException)
            : base(message, innerException)
        {
            if (string.IsNullOrWhiteSpace(configPath))
            {
                throw new ArgumentException("ConfigPath is required", nameof(configPath));
            }

            ConfigPath = configPath;
        }

        public string ConfigPath { get; }
    }
}
