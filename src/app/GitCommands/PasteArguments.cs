// The argument escaping in this file is adapted from the .NET runtime (BCL),
// specifically System.Private.CoreLib's PasteArguments.AppendArgument
// (https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/PasteArguments.cs).
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses that file to you under the MIT license.

using System.Text;

namespace GitCommands;

internal static class PasteArguments
{
    /// <summary>
    ///  Escapes an argument for a Windows command line, following the same rules the BCL
    ///  <see cref="System.Diagnostics.ProcessStartInfo"/> uses when it pastes arguments
    ///  (see https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/PasteArguments.cs).
    ///  Unlike the BCL <c>AppendArgument</c>, this method does not add the surrounding
    ///  quotes: the quoting is performed by the caller (<c>ScriptOptionsParser.ReplaceOption</c>),
    ///  which also doubles a trailing backslash. Trailing backslashes are therefore left
    ///  as-is here, so that the two escaping steps stay consistent.
    /// </summary>
    internal static string EscapeArgument(string argument)
    {
        StringBuilder escapedArgument = new(argument.Length);
        int idx = 0;
        while (idx < argument.Length)
        {
            char c = argument[idx++];
            if (c == Backslash)
            {
                int numBackSlash = 1;
                while (idx < argument.Length && argument[idx] == Backslash)
                {
                    idx++;
                    numBackSlash++;
                }

                if (idx < argument.Length && argument[idx] == Quote)
                {
                    // Backslashes followed by a quote must be doubled so that the quote
                    // is parsed as a literal quote rather than as the end of the quoted region.
                    escapedArgument.Append(Backslash, (numBackSlash * 2) + 1);
                    escapedArgument.Append(Quote);
                    idx++;
                }
                else
                {
                    // A backslash not followed by a quote is a normal character. Trailing
                    // backslashes are left as-is: the caller's quoting step doubles them.
                    escapedArgument.Append(Backslash, numBackSlash);
                }

                continue;
            }

            if (c == Quote)
            {
                // Escape the quote so it appears as a literal.
                escapedArgument.Append(Backslash);
                escapedArgument.Append(Quote);
                continue;
            }

            escapedArgument.Append(c);
        }

        return escapedArgument.ToString();
    }

    private const char Quote = '\"';
    private const char Backslash = '\\';
}
