using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitExtensions.Core.Commands;
using GitExtensions.Core.Utils.UI;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;

namespace DeleteUnusedBranches
{
    // Extract from GitCommands.ExecutableExtensions
    internal static class ExecutableExtensions
    {
        private static readonly Regex _ansiCodePattern = new Regex(@"\u001B[\u0040-\u005F].*?[\u0040-\u007E]", RegexOptions.Compiled);

        /// <summary>
        /// Launches a process for the executable and returns an object detailing exit code, standard output and standard error values.
        /// </summary>
        /// <remarks>
        /// This method uses <see cref="JoinableTaskFactory"/> to allow the calling thread to
        /// do useful work while waiting for the process to exit. Internally, this method delegates to
        /// <see cref="ExecuteAsync"/>.
        /// </remarks>
        /// <param name="executable">The executable from which to launch a process.</param>
        /// <param name="arguments">The arguments to pass to the executable</param>
        /// <param name="writeInput">A callback that writes bytes to the process's standard input stream, or <c>null</c> if no input is required.</param>
        /// <param name="outputEncoding">The text encoding to use when decoding bytes read from the process's standard output and standard error streams, or <c>null</c> if the default encoding is to be used.</param>
        /// <param name="stripAnsiEscapeCodes">A flag indicating whether ANSI escape codes should be removed from output strings.</param>
        /// <returns>An <see cref="ExecutionResult"/> object that gives access to exit code, standard output and standard error values.</returns>
        [MustUseReturnValue("If execution result is not required, use RunCommand instead")]
        public static ExecutionResult Execute(
            this IExecutable executable,
            ArgumentString arguments,
            Encoding outputEncoding,
            Action<StreamWriter> writeInput = null,
            bool stripAnsiEscapeCodes = true)
        {
            return ThreadHelper.JoinableTaskFactory.Run(
                () => executable.ExecuteAsync(arguments, outputEncoding, writeInput, stripAnsiEscapeCodes));
        }

        /// <summary>
        /// Launches a process for the executable and returns an object detailing exit code, standard output and standard error values.
        /// </summary>
        /// <param name="executable">The executable from which to launch a process.</param>
        /// <param name="arguments">The arguments to pass to the executable</param>
        /// <param name="writeInput">A callback that writes bytes to the process's standard input stream, or <c>null</c> if no input is required.</param>
        /// <param name="outputEncoding">The text encoding to use when decoding bytes read from the process's standard output and standard error streams, or <c>null</c> if the default encoding is to be used.</param>
        /// <param name="stripAnsiEscapeCodes">A flag indicating whether ANSI escape codes should be removed from output strings.</param>
        /// <returns>A task that yields an <see cref="ExecutionResult"/> object that gives access to exit code, standard output and standard error values.</returns>
        public static async Task<ExecutionResult> ExecuteAsync(
            this IExecutable executable,
            ArgumentString arguments,
            Encoding outputEncoding,
            Action<StreamWriter> writeInput = null,
            bool stripAnsiEscapeCodes = true)
        {
            using (var process = executable.Start(arguments, createWindow: false, redirectInput: writeInput != null, redirectOutput: true, outputEncoding))
            {
                var outputBuffer = new MemoryStream();
                var errorBuffer = new MemoryStream();
                var outputTask = process.StandardOutput.BaseStream.CopyToAsync(outputBuffer);
                var errorTask = process.StandardError.BaseStream.CopyToAsync(errorBuffer);

                if (writeInput != null)
                {
                    // TODO do we want to make this async?
                    writeInput(process.StandardInput);
                    process.StandardInput.Close();
                }

                var exitTask = process.WaitForExitAsync();

                await Task.WhenAll(outputTask, errorTask, exitTask);

                var output = outputEncoding.GetString(outputBuffer.GetBuffer(), 0, (int)outputBuffer.Length);
                var error = outputEncoding.GetString(errorBuffer.GetBuffer(), 0, (int)errorBuffer.Length);

                var exitCode = await process.WaitForExitAsync();

                return new ExecutionResult(
                    CleanString(stripAnsiEscapeCodes, output),
                    CleanString(stripAnsiEscapeCodes, error),
                    exitCode);
            }
        }

        [Pure]
        [NotNull]
        private static string CleanString(bool stripAnsiEscapeCodes, [NotNull] string s)
        {
            // NOTE Regex returns the original string if no ANSI codes are found (no allocation)
            return stripAnsiEscapeCodes
                ? _ansiCodePattern.Replace(s, "")
                : s;
        }
    }
}
