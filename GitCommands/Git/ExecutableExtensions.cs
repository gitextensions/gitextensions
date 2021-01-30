using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitExtUtils;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands
{
    /// <summary>
    /// Provides extension methods for <see cref="IExecutable"/> that provider operations on executables
    /// at a higher level than <see cref="IExecutable.Start"/>.
    /// </summary>
    public static class ExecutableExtensions
    {
        private static readonly Regex _ansiCodePattern = new(@"\u001B[\u0040-\u005F].*?[\u0040-\u007E]", RegexOptions.Compiled);
        private static readonly Lazy<Encoding> _defaultOutputEncoding = new Lazy<Encoding>(() => GitModule.SystemEncoding, false);

        /// <summary>
        /// Launches a process for the executable and returns its output.
        /// </summary>
        /// <remarks>
        /// This method uses <see cref="GitUI.ThreadHelper.JoinableTaskFactory"/> to allow the calling thread to
        /// do useful work while waiting for the process to exit. Internally, this method delegates to
        /// <see cref="GetOutputAsync"/>.
        /// </remarks>
        /// <param name="executable">The executable from which to launch a process.</param>
        /// <param name="arguments">The arguments to pass to the executable.</param>
        /// <param name="input">Bytes to be written to the process's standard input stream, or <c>null</c> if no input is required.</param>
        /// <param name="outputEncoding">The text encoding to use when decoding bytes read from the process's standard output and standard error streams, or <c>null</c> if the default encoding is to be used.</param>
        /// <param name="cache">A <see cref="CommandCache"/> to use if command results may be cached, otherwise <c>null</c>.</param>
        /// <param name="stripAnsiEscapeCodes">A flag indicating whether ANSI escape codes should be removed from output strings.</param>
        /// <returns>The concatenation of standard output and standard error. To receive these outputs separately, use <see cref="Execute"/> instead.</returns>
        [MustUseReturnValue("If output text is not required, use " + nameof(RunCommand) + " instead")]
        public static string GetOutput(
            this IExecutable executable,
            ArgumentString arguments = default,
            byte[]? input = null,
            Encoding? outputEncoding = null,
            CommandCache? cache = null,
            bool stripAnsiEscapeCodes = true)
        {
            return GitUI.ThreadHelper.JoinableTaskFactory.Run(
                () => executable.GetOutputAsync(arguments, input, outputEncoding, cache, stripAnsiEscapeCodes));
        }

        /// <summary>
        /// Launches a process for the executable per batch item and returns its output.
        /// </summary>
        /// <remarks>
        /// This method uses <see cref="GetOutput"/> to get concatenated outputs of multiple commands in batch.
        /// </remarks>
        /// <param name="executable">The executable from which to launch processes.</param>
        /// <param name="batchArguments">The array of batch arguments to pass to the executable.</param>
        /// <param name="input">Bytes to be written to each process's standard input stream, or <c>null</c> if no input is required.</param>
        /// <param name="outputEncoding">The text encoding to use when decoding bytes read from each process's standard output and standard error streams, or <c>null</c> if the default encoding is to be used.</param>
        /// <param name="cache">A <see cref="CommandCache"/> to use if command results may be cached, otherwise <c>null</c>.</param>
        /// <param name="stripAnsiEscapeCodes">A flag indicating whether ANSI escape codes should be removed from output strings.</param>
        /// <returns>The concatenation of standard output and standard error. To receive these outputs separately, use <see cref="Execute"/> instead.</returns>
        [MustUseReturnValue("If output text is not required, use " + nameof(RunCommand) + " instead")]
        public static string GetBatchOutput(
            this IExecutable executable,
            ICollection<BatchArgumentItem> batchArguments,
            byte[]? input = null,
            Encoding? outputEncoding = null,
            CommandCache? cache = null,
            bool stripAnsiEscapeCodes = true)
        {
            var sb = new StringBuilder();
            foreach (var batch in batchArguments)
            {
                sb.Append(executable.GetOutput(batch.Argument, input, outputEncoding, cache, stripAnsiEscapeCodes));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Launches a process for the executable and returns its output.
        /// </summary>
        /// <param name="executable">The executable from which to launch a process.</param>
        /// <param name="arguments">The arguments to pass to the executable.</param>
        /// <param name="input">Bytes to be written to the process's standard input stream, or <c>null</c> if no input is required.</param>
        /// <param name="outputEncoding">The text encoding to use when decoding bytes read from the process's standard output and standard error streams, or <c>null</c> if the default encoding is to be used.</param>
        /// <param name="cache">A <see cref="CommandCache"/> to use if command results may be cached, otherwise <c>null</c>.</param>
        /// <param name="stripAnsiEscapeCodes">A flag indicating whether ANSI escape codes should be removed from output strings.</param>
        /// <returns>A task that yields the concatenation of standard output and standard error. To receive these outputs separately, use <see cref="ExecuteAsync"/> instead.</returns>
        public static async Task<string> GetOutputAsync(
            this IExecutable executable,
            ArgumentString arguments = default,
            byte[]? input = null,
            Encoding? outputEncoding = null,
            CommandCache? cache = null,
            bool stripAnsiEscapeCodes = true)
        {
            if (outputEncoding is null)
            {
                outputEncoding = _defaultOutputEncoding.Value;
            }

            if (cache is not null && cache.TryGet(arguments, out var output, out var error))
            {
                return ComposeOutput();
            }

            using var process = executable.Start(
                arguments,
                createWindow: false,
                redirectInput: input is not null,
                redirectOutput: true,
                outputEncoding);
            if (input is not null)
            {
                await process.StandardInput.BaseStream.WriteAsync(input, 0, input.Length);
                process.StandardInput.Close();
            }

            var outputBuffer = new MemoryStream();
            var errorBuffer = new MemoryStream();
            var outputTask = process.StandardOutput.BaseStream.CopyToAsync(outputBuffer);
            var errorTask = process.StandardError.BaseStream.CopyToAsync(errorBuffer);
            var exitTask = process.WaitForExitAsync();

            await Task.WhenAll(outputTask, errorTask, exitTask);

            output = outputBuffer.ToArray();
            error = errorBuffer.ToArray();

            if (cache is not null && await exitTask == 0)
            {
                cache.Add(arguments, output, error);
            }

            return ComposeOutput();

            string ComposeOutput()
            {
                return CleanString(
                    stripAnsiEscapeCodes,
                    EncodingHelper.DecodeString(output, error, ref outputEncoding));
            }
        }

        /// <summary>
        /// Launches a process for the executable and returns <c>true</c> if its exit code is zero.
        /// </summary>
        /// <remarks>
        /// This method uses <see cref="GitUI.ThreadHelper.JoinableTaskFactory"/> to allow the calling thread to
        /// do useful work while waiting for the process to exit. Internally, this method delegates to
        /// <see cref="RunCommandAsync"/>.
        /// </remarks>
        /// <param name="executable">The executable from which to launch a process.</param>
        /// <param name="arguments">The arguments to pass to the executable.</param>
        /// <param name="input">Bytes to be written to the process's standard input stream, or <c>null</c> if no input is required.</param>
        /// <param name="createWindow">A flag indicating whether a console window should be created and bound to the process.</param>
        /// <returns><c>true</c> if the process's exit code was zero, otherwise <c>false</c>.</returns>
        [MustUseReturnValue("Callers should verify that " + nameof(RunCommand) + " returned true")]
        public static bool RunCommand(
            this IExecutable executable,
            ArgumentString arguments = default,
            byte[]? input = null,
            bool createWindow = false)
        {
            return GitUI.ThreadHelper.JoinableTaskFactory.Run(
                () => executable.RunCommandAsync(arguments, input, createWindow));
        }

        /// <summary>
        /// Launches a process for the executable per batch item, and returns <c>true</c> if all process exit codes were zero.
        /// </summary>
        /// <remarks>
        /// This method uses <see cref="RunCommand"/> to execute multiple commands in batch, used in accordance with
        /// <see cref="ArgumentBuilderExtensions.BuildBatchArguments(ArgumentBuilder, IEnumerable{string}, int?, int)"/>
        /// to work around Windows command line length 32767 character limitation
        /// <see href="https://docs.microsoft.com/en-us/windows/desktop/api/processthreadsapi/nf-processthreadsapi-createprocessa"/>.
        /// </remarks>
        /// <param name="executable">The executable from which to launch processes.</param>
        /// <param name="batchArguments">The array of batch arguments to pass to the executable.</param>
        /// <param name="input">Bytes to be written to each process's standard input stream, or <c>null</c> if no input is required.</param>
        /// <param name="createWindow">A flag indicating whether a console window should be created and bound to each process.</param>
        /// <returns><c>true</c> if all process exit codes were zero, otherwise <c>false</c>.</returns>
        [MustUseReturnValue("Callers should verify that " + nameof(RunBatchCommand) + " returned true")]
        public static bool RunBatchCommand(
            this IExecutable executable,
            ICollection<BatchArgumentItem> batchArguments,
            Action<BatchProgressEventArgs>? action = null,
            byte[]? input = null,
            bool createWindow = false)
        {
            int total = batchArguments.Sum(item => item.BatchItemsCount);
            var result = true;

            foreach (var item in batchArguments)
            {
                result &= executable.RunCommand(item.Argument, input, createWindow);

                // Invoke batch progress callback
                action?.Invoke(new BatchProgressEventArgs(item.BatchItemsCount, result));
            }

            return result;
        }

        /// <summary>
        /// Launches a process for the executable and returns <c>true</c> if its exit code is zero.
        /// </summary>
        /// <param name="executable">The executable from which to launch a process.</param>
        /// <param name="arguments">The arguments to pass to the executable.</param>
        /// <param name="input">Bytes to be written to the process's standard input stream, or <c>null</c> if no input is required.</param>
        /// <param name="createWindow">A flag indicating whether a console window should be created and bound to the process.</param>
        /// <returns>A task that yields <c>true</c> if the process's exit code was zero, otherwise <c>false</c>.</returns>
        public static async Task<bool> RunCommandAsync(
            this IExecutable executable,
            ArgumentString arguments = default,
            byte[]? input = null,
            bool createWindow = false)
        {
            using var process = executable.Start(arguments, createWindow: createWindow, redirectInput: input is not null);
            if (input is not null)
            {
                await process.StandardInput.BaseStream.WriteAsync(input, 0, input.Length);
                process.StandardInput.Close();
            }

            return await process.WaitForExitAsync() == 0;
        }

        /// <summary>
        /// Launches a process for the executable and returns output lines as they become available.
        /// </summary>
        /// <param name="executable">The executable from which to launch a process.</param>
        /// <param name="arguments">The arguments to pass to the executable.</param>
        /// <param name="input">Bytes to be written to the process's standard input stream, or <c>null</c> if no input is required.</param>
        /// <param name="outputEncoding">The text encoding to use when decoding bytes read from the process's standard output and standard error streams, or <c>null</c> if the default encoding is to be used.</param>
        /// <param name="stripAnsiEscapeCodes">A flag indicating whether ANSI escape codes should be removed from output strings.</param>
        /// <returns>An enumerable sequence of lines that yields lines as they become available. Lines from standard output are returned first, followed by lines from standard error.</returns>
        [MustUseReturnValue("If output lines are not required, use " + nameof(RunCommand) + " instead")]
        public static IEnumerable<string> GetOutputLines(
            this IExecutable executable,
            ArgumentString arguments = default,
            byte[]? input = null,
            Encoding? outputEncoding = null,
            bool stripAnsiEscapeCodes = true)
        {
            if (outputEncoding is null)
            {
                outputEncoding = _defaultOutputEncoding.Value;
            }

            using var process = executable.Start(arguments, createWindow: false, redirectInput: input is not null, redirectOutput: true, outputEncoding);
            if (input is not null)
            {
                process.StandardInput.BaseStream.Write(input, 0, input.Length);
                process.StandardInput.Close();
            }

            while (true)
            {
                var line = process.StandardOutput.ReadLine();

                if (line is null)
                {
                    break;
                }

                yield return CleanString(stripAnsiEscapeCodes, line);
            }

            while (true)
            {
                var line = process.StandardError.ReadLine();

                if (line is null)
                {
                    break;
                }

                yield return CleanString(stripAnsiEscapeCodes, line);
            }

            process.WaitForExit();
        }

        /// <summary>
        /// Launches a process for the executable and returns output lines as they become available.
        /// </summary>
        /// <param name="executable">The executable from which to launch a process.</param>
        /// <param name="arguments">The arguments to pass to the executable.</param>
        /// <param name="writeInput">A callback that writes bytes to the process's standard input stream, or <c>null</c> if no input is required.</param>
        /// <param name="outputEncoding">The text encoding to use when decoding bytes read from the process's standard output and standard error streams, or <c>null</c> if the default encoding is to be used.</param>
        /// <param name="stripAnsiEscapeCodes">A flag indicating whether ANSI escape codes should be removed from output strings.</param>
        /// <returns>An enumerable sequence of lines that yields lines as they become available. Lines from standard output are returned first, followed by lines from standard error.</returns>
        [MustUseReturnValue("If output lines are not required, use " + nameof(RunCommand) + " instead")]
        public static async Task<IEnumerable<string>> GetOutputLinesAsync(
            this IExecutable executable,
            ArgumentString arguments = default,
            Action<StreamWriter>? writeInput = null,
            Encoding? outputEncoding = null,
            bool stripAnsiEscapeCodes = true)
        {
            var result = await executable.ExecuteAsync(arguments, writeInput, outputEncoding, stripAnsiEscapeCodes);
            return result.StandardOutput.SplitLines().Concat(result.StandardError.SplitLines());
        }

        /// <summary>
        /// Launches a process for the executable and returns an object detailing exit code, standard output and standard error values.
        /// </summary>
        /// <remarks>
        /// This method uses <see cref="GitUI.ThreadHelper.JoinableTaskFactory"/> to allow the calling thread to
        /// do useful work while waiting for the process to exit. Internally, this method delegates to
        /// <see cref="ExecuteAsync"/>.
        /// </remarks>
        /// <param name="executable">The executable from which to launch a process.</param>
        /// <param name="arguments">The arguments to pass to the executable.</param>
        /// <param name="writeInput">A callback that writes bytes to the process's standard input stream, or <c>null</c> if no input is required.</param>
        /// <param name="outputEncoding">The text encoding to use when decoding bytes read from the process's standard output and standard error streams, or <c>null</c> if the default encoding is to be used.</param>
        /// <param name="stripAnsiEscapeCodes">A flag indicating whether ANSI escape codes should be removed from output strings.</param>
        /// <returns>An <see cref="ExecutionResult"/> object that gives access to exit code, standard output and standard error values.</returns>
        [MustUseReturnValue("If execution result is not required, use " + nameof(RunCommand) + " instead")]
        public static ExecutionResult Execute(
            this IExecutable executable,
            ArgumentString arguments,
            Action<StreamWriter>? writeInput = null,
            Encoding? outputEncoding = null,
            bool stripAnsiEscapeCodes = true)
        {
            return GitUI.ThreadHelper.JoinableTaskFactory.Run(
                () => executable.ExecuteAsync(arguments, writeInput, outputEncoding, stripAnsiEscapeCodes));
        }

        /// <summary>
        /// Launches a process for the executable and returns an object detailing exit code, standard output and standard error values.
        /// </summary>
        /// <param name="executable">The executable from which to launch a process.</param>
        /// <param name="arguments">The arguments to pass to the executable.</param>
        /// <param name="writeInput">A callback that writes bytes to the process's standard input stream, or <c>null</c> if no input is required.</param>
        /// <param name="outputEncoding">The text encoding to use when decoding bytes read from the process's standard output and standard error streams, or <c>null</c> if the default encoding is to be used.</param>
        /// <param name="stripAnsiEscapeCodes">A flag indicating whether ANSI escape codes should be removed from output strings.</param>
        /// <returns>A task that yields an <see cref="ExecutionResult"/> object that gives access to exit code, standard output and standard error values.</returns>
        public static async Task<ExecutionResult> ExecuteAsync(
            this IExecutable executable,
            ArgumentString arguments,
            Action<StreamWriter>? writeInput = null,
            Encoding? outputEncoding = null,
            bool stripAnsiEscapeCodes = true)
        {
            if (outputEncoding is null)
            {
                outputEncoding = _defaultOutputEncoding.Value;
            }

            using var process = executable.Start(arguments, createWindow: false, redirectInput: writeInput is not null, redirectOutput: true, outputEncoding);
            var outputBuffer = new MemoryStream();
            var errorBuffer = new MemoryStream();
            var outputTask = process.StandardOutput.BaseStream.CopyToAsync(outputBuffer);
            var errorTask = process.StandardError.BaseStream.CopyToAsync(errorBuffer);

            if (writeInput is not null)
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

        [Pure]
        private static string CleanString(bool stripAnsiEscapeCodes, string s)
        {
            // NOTE Regex returns the original string if no ANSI codes are found (no allocation)
            return stripAnsiEscapeCodes
                ? _ansiCodePattern.Replace(s, "")
                : s;
        }
    }
}
