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
        const int byteBufferSize = 4096;

        // GetMaxCharCount accounts for a partial multi-byte sequence the decoder
        // buffered from the previous read; sizing by the byte count alone can fall
        // one char short and make GetChars throw, killing the reader.
        char[] charBuf = new char[Encoding.UTF8.GetMaxCharCount(byteBufferSize)];
        StringBuilder pendingLine = new();
        Decoder decoder = Encoding.UTF8.GetDecoder();

        try
        {
            Stream stdout = minttyProcess.StandardOutput.BaseStream;
            byte[] buffer = new byte[byteBufferSize];

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

                // A '\r' not followed by '\n' terminates a transient progress line
                // (e.g. git's "Receiving objects: 42%\r"). Emit it with the trailing
                // '\r' so consumers can tell it apart from a completed line; waiting
                // for the eventual '\n' would merge the whole progress stream into
                // one huge line. "\r\n" breaks (pty ONLCR) remain a single line.
                if (c != '\n' && pendingLine.Length > 0 && pendingLine[^1] == '\r')
                {
                    EmitLine(pendingLine.ToString());
                    pendingLine.Clear();
                }

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
            else if (!text.StartsWith(ScriptChromeMarker, StringComparison.Ordinal))
            {
                lineCallback?.Invoke(text);
            }
        }
    }

    [GeneratedRegex(@"\x1b\[8mGITEX_EXIT:(\d+)\x1b\[0m")]
    private static partial Regex ExitSentinelRegex();

    /// <summary>
    /// Zero-width SGR pair (conceal on/off) prefixed to every line the wrapper script
    /// generates itself (command echo, exit sentinel, "Done", exit prompts). Mintty
    /// renders the marker as nothing, while the log reader uses it to keep such chrome
    /// out of the command output — mirroring ConEmu, which filters the echoed command
    /// line. The exit sentinel is matched before the chrome filter (the regex scans the
    /// line, so the prefix does not interfere); its marker only ensures a malformed
    /// sentinel is dropped instead of leaking into the output.
    /// </summary>
    private const string ScriptChromeMarker = "\x1b[8m\x1b[28m";

    private static string BuildWrapperScript(string commandLine)
    {
        string bashCommandLine = ConvertCommandLineToBash(commandLine);

        return
            $$"""
              #!/bin/bash
              printf '{{ScriptChromeMarker}}\x1b[0;38;5;5;49m%s\x1b[0m\n' "$GITEX_CMD_DISPLAY"
              {{bashCommandLine}}
              GITEX_RC=$?
              printf '{{ScriptChromeMarker}}\x1b[8mGITEX_EXIT:%d\x1b[0m\n' "$GITEX_RC"
              if [ "$GITEX_RC" -ne 0 ]; then
                  printf '{{ScriptChromeMarker}}\x1b[0;38;5;160;49mProcess exited with code %d\x1b[0m\n' "$GITEX_RC"
              fi
              printf '{{ScriptChromeMarker}}\x1b[0;38;5;243;49mDone\x1b[0m\n'
              printf '{{ScriptChromeMarker}}\x1b[0;38;5;243;49mPress Enter or Esc to exit...\x1b[0m\n'
              printf '{{ScriptChromeMarker}}\n'
              read -s -n 1
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
        bool hasToken = false;
        int pendingBackslashes = 0;

        foreach (char c in commandLine)
        {
            if (c == '\\')
            {
                // Defer: per the Windows CommandLineToArgvW rules, the meaning of a
                // backslash run depends on whether a '"' follows it.
                pendingBackslashes++;
                hasToken = true;
                continue;
            }

            if (c == '"')
            {
                // Resolve the deferred backslash run:
                //   2n   backslashes + '"' => n literal backslashes, the quote toggles
                //   2n+1 backslashes + '"' => n literal backslashes + a literal '"'
                // Without this, a quoted path ending in '\' (escaped as `\\"`) loses its
                // closing quote and swallows every following token.
                current.Append('\\', pendingBackslashes / 2);
                if (pendingBackslashes % 2 == 0)
                {
                    inQuotes = !inQuotes;
                }
                else
                {
                    current.Append('"');
                }

                pendingBackslashes = 0;
                hasToken = true;
                continue;
            }

            // Backslashes not followed by a '"' are all literal.
            current.Append('\\', pendingBackslashes);
            pendingBackslashes = 0;

            if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (hasToken)
                {
                    tokens.Add(current.ToString());
                    current.Clear();
                    hasToken = false;
                }
            }
            else
            {
                current.Append(c);
                hasToken = true;
            }
        }

        current.Append('\\', pendingBackslashes);
        if (hasToken)
        {
            tokens.Add(current.ToString());
        }

        return tokens;
    }
}
