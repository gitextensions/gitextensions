using System;
using JetBrains.Annotations;

namespace GitCommands
{
    public interface IEnvironmentAbstraction
    {
        /// <inheritdoc cref="Environment.Exit" />
        void Exit(int exitCode);

        /// <inheritdoc cref="Environment.GetCommandLineArgs" />
        string[] GetCommandLineArgs();

        /// <inheritdoc cref="Environment.GetEnvironmentVariable(string)" />
        [CanBeNull]
        string GetEnvironmentVariable(string variable);

        /// <inheritdoc cref="Environment.GetEnvironmentVariable(string,EnvironmentVariableTarget)" />
        [CanBeNull]
        string GetEnvironmentVariable(string variable, EnvironmentVariableTarget target);

        /// <inheritdoc cref="Environment.GetFolderPath(Environment.SpecialFolder)" />
        string GetFolderPath(Environment.SpecialFolder folder);

        /// <inheritdoc cref="Environment.SetEnvironmentVariable(string,string)" />
        void SetEnvironmentVariable(string variable, string value);
    }

    public sealed class EnvironmentAbstraction : IEnvironmentAbstraction
    {
        /// <inheritdoc />
        public void Exit(int exitCode)
        {
            Environment.Exit(exitCode);
        }

        /// <inheritdoc />
        public string[] GetCommandLineArgs()
        {
            return Environment.GetCommandLineArgs();
        }

        /// <inheritdoc />
        public string GetEnvironmentVariable(string variable)
        {
            return Environment.GetEnvironmentVariable(variable);
        }

        /// <inheritdoc />
        public string GetEnvironmentVariable(string variable, EnvironmentVariableTarget target)
        {
            return Environment.GetEnvironmentVariable(variable, target);
        }

        /// <inheritdoc />
        public string GetFolderPath(Environment.SpecialFolder folder)
        {
            return Environment.GetFolderPath(folder);
        }

        /// <inheritdoc />
        public void SetEnvironmentVariable(string variable, string value)
        {
            Environment.SetEnvironmentVariable(variable, value);
        }
    }
}