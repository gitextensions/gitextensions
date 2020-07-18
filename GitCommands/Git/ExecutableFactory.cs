using System;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands
{
    public class ExecutableFactory
    {
        /// <summary>
        /// Singleton accessor.
        /// </summary>
        public static ExecutableFactory Default { get; } = new ExecutableFactory();

        public IExecutable Create([NotNull] Func<string> fileNameProvider,
            [NotNull] string workingDir = "",
            ExternalOperationExceptionFactory.Handling exceptionHandling = ExternalOperationExceptionFactory.Handling.OptionalBugReport,
            [CanBeNull] string context = null)
            => new Executable(fileNameProvider, workingDir, exceptionHandling, context);

        public IExecutable Create([NotNull] string fileName,
            [NotNull] string workingDir = "",
            ExternalOperationExceptionFactory.Handling exceptionHandling = ExternalOperationExceptionFactory.Handling.OptionalBugReport,
            [CanBeNull] string context = null)
            => Create(() => fileName, workingDir, exceptionHandling, context);

        public IProcess Spawn(string arguments, [NotNull] string workingDir = "")
            => Create(Application.ExecutablePath, workingDir, ExternalOperationExceptionFactory.Handling.OptionalBugReport).Start(arguments);
    }
}
