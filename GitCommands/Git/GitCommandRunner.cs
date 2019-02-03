using System;
using System.Text;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands
{
    public sealed class GitCommandRunner : IGitCommandRunner
    {
        private readonly IExecutable _gitExecutable;
        private readonly Func<Encoding> _defaultEncoding;

        public GitCommandRunner(IExecutable gitExecutable, Func<Encoding> defaultEncoding)
        {
            _gitExecutable = gitExecutable;
            _defaultEncoding = defaultEncoding;
        }

        [NotNull]
        public IProcess RunDetached(
            ArgumentString arguments = default,
            bool createWindow = false,
            bool redirectInput = false,
            bool redirectOutput = false,
            Encoding outputEncoding = null)
        {
            if (outputEncoding == null && redirectOutput)
            {
                outputEncoding = _defaultEncoding();
            }

            return _gitExecutable.Start(arguments, createWindow, redirectInput, redirectOutput, outputEncoding);
        }
    }
}