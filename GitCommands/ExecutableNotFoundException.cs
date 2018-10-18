using System;
using JetBrains.Annotations;

namespace GitCommands
{
    public class ExecutableNotFoundException : Exception
    {
        public ExecutableNotFoundException([NotNull]string fileName)
            : base("Executable not found")
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException(nameof(fileName));
            }

            FileName = fileName;
        }

        /// <summary>
        /// Gets the name of the executable file which could not be found.
        /// </summary>
        public string FileName { get; }
    }
}