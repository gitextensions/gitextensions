using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GitCommands;
using JetBrains.Annotations;

namespace GitUI.UserControls
{
    /// <summary>
    ///     <para>Base control for executing a console process, as used by the <see cref="FormProcess" />.</para>
    ///     <para>Switches between the basic impl which redirects stdout and integration of a real interactive terminal window into the form, if available.</para>
    /// </summary>
    public abstract class ConsoleOutputControl : ContainerControl
    {
        public abstract int ExitCode { get; }

        /// <summary>
        /// Whether this output controls accurately renders all of the process output, so there's no need in printing select lines manually, or duping progress in the title.
        /// </summary>
        public abstract bool IsDisplayingFullProcessOutput { get; }

        public abstract void AppendMessageFreeThreaded([NotNull] string text);

        /// <summary>
        /// Creates the instance best fitting the current environment.
        /// </summary>
        [NotNull]
        public static ConsoleOutputControl CreateInstance()
        {
            if (ConsoleEmulatorOutputControl.IsSupportedInThisEnvironment && AppSettings.UseConsoleEmulatorForCommands)
            {
                return new ConsoleEmulatorOutputControl();
            }

            return new EditboxBasedConsoleOutputControl();
        }

        public abstract void KillProcess();

        public abstract void Reset();

        public abstract void StartProcess([NotNull] string command, string arguments, string workDir, Dictionary<string, string> envVariables);

        public event EventHandler<TextEventArgs> DataReceived;

        protected void FireDataReceived([NotNull] TextEventArgs args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            DataReceived?.Invoke(this, args);
        }

        protected void FireProcessExited()
        {
            ProcessExited?.Invoke(this, EventArgs.Empty);
        }

        protected void FireTerminated()
        {
            Terminated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Fires when the cmdline process exits.
        /// </summary>
        public event EventHandler ProcessExited;

        /// <summary>
        /// Fires when the output control terminates. This only applies to the console emulator control mode (an editbox won't terminate), and fires when the console emulator itself (not the command it were executing) is terminated as a process, and the control goes blank.
        /// </summary>
        public event EventHandler Terminated;
    }
}