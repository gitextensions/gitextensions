using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace GitUI.ConsoleEmulation.Mintty;

internal static partial class MinttyConsoleRuntime
{
    internal readonly record struct CommandLaunchParams(
        string BashBootstrapCommand,
        Dictionary<string, string> EnvironmentVariables);

    internal static CommandLaunchParams CreateLaunchParams(MinttyStartInfo startInfo)
    {
        string script = BuildWrapperScript(startInfo.ConsoleProcessCommandLine);
        string scriptBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(script));

        const string bashBootstrap = @"GITEX_D=$(printf '%s' \""$GITEX_SCRIPT\"" | base64 -d) && eval \""$GITEX_D\""";

        Dictionary<string, string> envVars = [];
        foreach ((string key, string value) in startInfo.EnvironmentVariables)
        {
            envVars[key] = value;
        }

        envVars["GITEX_SCRIPT"] = scriptBase64;
        envVars["GITEX_CMD_DISPLAY"] = startInfo.ConsoleProcessCommandLine;

        // MSYS would otherwise rewrite POSIX-looking arguments (e.g. /home/user/...)
        // into Windows paths prefixed with the MSYS root before passing them to native
        // Windows binaries like wsl.exe. The C# side already produces arguments in their
        // target form — both vars are needed to cover MSYS and MSYS2 builds of bash.
        envVars["MSYS_NO_PATHCONV"] = "1";
        envVars["MSYS2_ARG_CONV_EXCL"] = "*";

        return new CommandLaunchParams(bashBootstrap, envVars);
    }

    internal static void StartOutputReader(
        Process minttyProcess,
        Action<string>? lineCallback,
        Action<int>? exitCallback)
    {
        ThreadHelper.FileAndForget(() => ReadLogStreamAsync(minttyProcess, lineCallback, exitCallback));
    }

    private static async Task ReadLogStreamAsync(
        Process minttyProcess,
        Action<string>? lineCallback,
        Action<int>? exitCallback)
    {
        char[] charBuf = new char[4096];
        StringBuilder pendingLine = new();
        Decoder decoder = Encoding.UTF8.GetDecoder();

        try
        {
            Stream stdout = minttyProcess.StandardOutput.BaseStream;
            byte[] buffer = new byte[4096];

            while (true)
            {
                int read = await stdout.ReadAsync(buffer.AsMemory());
                if (read == 0)
                {
                    break;
                }

                DecodeAndEmitLines(buffer.AsSpan(0, read), flush: false);
            }

            DecodeAndEmitLines(ReadOnlySpan<byte>.Empty, flush: true);

            if (pendingLine.Length > 0)
            {
                EmitLine(pendingLine.ToString());
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Failed to read mintty output: {ex}");
        }

        return;

        void DecodeAndEmitLines(ReadOnlySpan<byte> bytes, bool flush)
        {
            // flush: false mid-stream so a multi-byte UTF-8 sequence split across reads
            // is carried into the next call instead of being emitted as U+FFFD.
            int count = decoder.GetChars(bytes, charBuf, flush);
            for (int i = 0; i < count; i++)
            {
                char c = charBuf[i];
                pendingLine.Append(c);
                if (c == '\n')
                {
                    EmitLine(pendingLine.ToString());
                    pendingLine.Clear();
                }
            }
        }

        void EmitLine(string text)
        {
            Match match = ExitSentinelRegex().Match(text);
            if (match.Success && int.TryParse(match.Groups[1].ValueSpan, out int exitCode))
            {
                exitCallback?.Invoke(exitCode);
            }
            else
            {
                lineCallback?.Invoke(text);
            }
        }
    }

    [GeneratedRegex(@"\x1b\[8mGITEX_EXIT:(\d+)\x1b\[0m")]
    private static partial Regex ExitSentinelRegex();

    private static string BuildWrapperScript(string commandLine)
    {
        string bashCommandLine = ConvertCommandLineToBash(commandLine);

        return
            $$"""
              #!/bin/bash
              printf '\x1b[0;38;5;5;49m%s\x1b[0m\n' "$GITEX_CMD_DISPLAY"
              {{bashCommandLine}}
              GITEX_RC=$?
              printf '\x1b[8mGITEX_EXIT:%d\x1b[0m' "$GITEX_RC"
              echo
              if [ "$GITEX_RC" -ne 0 ]; then
                  printf '\x1b[0;38;5;160;49mProcess exited with code %d\x1b[0m\n' "$GITEX_RC"
              fi
              printf '\x1b[0;38;5;243;49mDone\x1b[0m\n'
              printf '\x1b[0;38;5;243;49mPress Enter or Esc to exit...\x1b[0m\n'
              echo
              read -n 1
              exit 0
              """;
    }

    /// <summary>
    /// Tokenizes the Windows-style command line and re-emits each token wrapped in bash
    /// single quotes. Single quotes preserve everything literally, so backslashes and
    /// dollar signs (e.g. WSL UNC paths like <c>\\wsl$\distro\...</c>) survive bash
    /// parsing unmodified — splicing the raw command line into the script would let
    /// bash collapse <c>\\</c> to <c>\</c> and break the path.
    /// </summary>
    internal static string ConvertCommandLineToBash(string commandLine)
    {
        List<string> tokens = TokenizeWindowsCommandLine(commandLine);
        if (tokens.Count == 0)
        {
            return string.Empty;
        }

        // Backslash → forward slash for the executable path so MSYS resolves
        // Windows paths consistently (e.g. C:/Program Files/Git/bin/git.exe).
        tokens[0] = tokens[0].Replace('\\', '/');

        return string.Join(' ', tokens.Select(BashSingleQuote));
    }

    private static string BashSingleQuote(string token)
    {
        return $"'{token.Replace("'", "'\\''")}'";
    }

    private static List<string> TokenizeWindowsCommandLine(string commandLine)
    {
        List<string> tokens = [];
        StringBuilder current = new();
        bool inQuotes = false;
        bool hasContent = false;

        for (int i = 0; i < commandLine.Length; i++)
        {
            char c = commandLine[i];

            if (c == '\\')
            {
                // Apply the Windows CommandLineToArgvW backslash rules so a run of
                // backslashes before a quote is interpreted correctly:
                //   2n   backslashes + '"' => n literal backslashes, the quote toggles
                //   2n+1 backslashes + '"' => n literal backslashes + a literal '"'
                //   backslashes not before a '"' are all literal.
                // Without this, a quoted path ending in '\' (escaped as `\\"`) loses its
                // closing quote and swallows every following token.
                int backslashes = 0;
                while (i < commandLine.Length && commandLine[i] == '\\')
                {
                    backslashes++;
                    i++;
                }

                if (i < commandLine.Length && commandLine[i] == '"')
                {
                    current.Append('\\', backslashes / 2);
                    if (backslashes % 2 == 0)
                    {
                        inQuotes = !inQuotes;
                    }
                    else
                    {
                        current.Append('"');
                    }
                }
                else
                {
                    current.Append('\\', backslashes);

                    // Reprocess the non-backslash character (or stop at end of string).
                    i--;
                }

                hasContent = true;
            }
            else if (c == '"')
            {
                inQuotes = !inQuotes;
                hasContent = true;
            }
            else if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (hasContent)
                {
                    tokens.Add(current.ToString());
                    current.Clear();
                    hasContent = false;
                }
            }
            else
            {
                current.Append(c);
                hasContent = true;
            }
        }

        if (hasContent)
        {
            tokens.Add(current.ToString());
        }

        return tokens;
    }
}
