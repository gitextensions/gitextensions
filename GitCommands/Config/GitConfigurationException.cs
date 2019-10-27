using System;

namespace GitCommands.Config
{
    public class GitConfigurationException : Exception
    {
        public GitConfigurationException(string configPath, Exception inner) : base(null, inner)
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
