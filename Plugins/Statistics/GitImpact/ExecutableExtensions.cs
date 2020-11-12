using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using GitExtensions.Core.Commands;
using JetBrains.Annotations;

namespace GitImpact
{
    // Extract from GitCommands.ExecutableExtensions
    internal static class ExecutableExtensions
    {
        private static readonly Regex _ansiCodePattern = new Regex(@"\u001B[\u0040-\u005F].*?[\u0040-\u007E]", RegexOptions.Compiled);

        /// <summary>
        /// Launches a process for the executable and returns output lines as they become available.
        /// </summary>
        /// <param name="executable">The executable from which to launch a process.</param>
        /// <param name="arguments">The arguments to pass to the executable</param>
        /// <param name="input">Bytes to be written to the process's standard input stream, or <c>null</c> if no input is required.</param>
        /// <param name="outputEncoding">The text encoding to use when decoding bytes read from the process's standard output and standard error streams, or <c>null</c> if the default encoding is to be used.</param>
        /// <param name="stripAnsiEscapeCodes">A flag indicating whether ANSI escape codes should be removed from output strings.</param>
        /// <returns>An enumerable sequence of lines that yields lines as they become available. Lines from standard output are returned first, followed by lines from standard error.</returns>
        [MustUseReturnValue("If output lines are not required, use RunCommand instead")]
        public static IEnumerable<string> GetOutputLines(
            this IExecutable executable,
            Encoding outputEncoding,
            ArgumentString arguments = default,
            byte[] input = null,
            bool stripAnsiEscapeCodes = true)
        {
            using (var process = executable.Start(arguments, createWindow: false, redirectInput: input != null, redirectOutput: true, outputEncoding))
            {
                if (input != null)
                {
                    process.StandardInput.BaseStream.Write(input, 0, input.Length);
                    process.StandardInput.Close();
                }

                while (true)
                {
                    var line = process.StandardOutput.ReadLine();

                    if (line == null)
                    {
                        break;
                    }

                    yield return CleanString(stripAnsiEscapeCodes, line);
                }

                while (true)
                {
                    var line = process.StandardError.ReadLine();

                    if (line == null)
                    {
                        break;
                    }

                    yield return CleanString(stripAnsiEscapeCodes, line);
                }

                process.WaitForExit();
            }
        }

        [Pure]
        [NotNull]
        private static string CleanString(bool stripAnsiEscapeCodes, string s)
        {
            // NOTE Regex returns the original string if no ANSI codes are found (no allocation)
            return stripAnsiEscapeCodes
                ? _ansiCodePattern.Replace(s, string.Empty)
                : s;
        }
    }
}
