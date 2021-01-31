using System;

namespace GitCommands.Config
{
    public class GitConfigurationException : Exception
    {
        public GitConfigurationException(string configPath, Exception? innerException)
            : this(configPath, message: null, innerException)
        {
        }

        public GitConfigurationException(string configPath, string? message, Exception? innerException)
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
