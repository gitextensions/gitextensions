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

        return new CommandLaunchParams(bashBootstrap, envVars);
    }

    internal static void StartOutputReader(
        Process minttyProcess,
        Action<string>? lineCallback,
        Action<int>? exitCallback,
        CancellationToken ct)
    {
        _ = ReadLogStreamAsync(minttyProcess, lineCallback, exitCallback, ct);
    }

    private static async Task ReadLogStreamAsync(
        Process minttyProcess,
        Action<string>? lineCallback,
        Action<int>? exitCallback,
        CancellationToken ct)
    {
        try
        {
            Stream stdout = minttyProcess.StandardOutput.BaseStream;
            byte[] buffer = new byte[4096];
            char[] charBuf = new char[4096];
            Decoder decoder = Encoding.UTF8.GetDecoder();
            StringBuilder line = new();

            while (!ct.IsCancellationRequested)
            {
                int read = await stdout.ReadAsync(buffer.AsMemory(), ct);
                if (read == 0)
                {
                    break;
                }

                int chars = decoder.GetChars(buffer, 0, read, charBuf, 0);
                for (int i = 0; i < chars; i++)
                {
                    char c = charBuf[i];
                    line.Append(c);
                    if (c == '\n')
                    {
                        EmitLine(line.ToString());
                        line.Clear();
                    }
                }
            }

            if (line.Length > 0)
            {
                EmitLine(line.ToString());
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception)
        {
        }

        return;

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

    [GeneratedRegex(@"\x1b\[8mGITEX_EXIT:(\d+)\x1b\[0?m")]
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
              printf '\033[8mGITEX_EXIT:%d\033[0m' "$GITEX_RC"
              echo
              if [ "$GITEX_RC" -ne 0 ]; then
                  printf '\x1b[0;38;5;160;49mProcess exited with code %d\033[0m\n' "$GITEX_RC"
              fi
              printf '\x1b[0;38;5;243;49mDone\033[0m\n'
              printf '\x1b[0;38;5;243;49mPress Enter or Esc to exit...\033[0m\n'
              echo
              read -n 1
              exit 0
              """;
    }

    private static string ConvertCommandLineToBash(string commandLine)
    {
        if (commandLine.StartsWith('"'))
        {
            int closingQuote = commandLine.IndexOf('"', 1);
            if (closingQuote > 0)
            {
                string path = commandLine[1..closingQuote].Replace('\\', '/');
                string rest = commandLine[(closingQuote + 1)..];
                return $"'{path}'{rest}";
            }
        }

        int firstSpace = commandLine.IndexOf(' ');
        if (firstSpace > 0)
        {
            return commandLine[..firstSpace].Replace('\\', '/') + commandLine[firstSpace..];
        }

        return commandLine.Replace('\\', '/');
    }
}
