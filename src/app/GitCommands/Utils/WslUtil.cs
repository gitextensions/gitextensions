using System.Text.RegularExpressions;

namespace GitCommands.Utils;

public static partial class WslUtil
{
    [GeneratedRegex(@"\$(?:LOCAL|REMOTE|BASE|MERGED)", RegexOptions.ExplicitCapture)]
    private static partial Regex DiffMergePlaceholderRegex { get; }

    /// <summary>
    /// Adapts a Windows diff/merge tool command to be run by WSL git:
    /// the leading drive is mapped to /mnt and the placeholders are converted by wslpath.
    /// </summary>
    /// <param name="command">The Windows command.</param>
    /// <returns>The command for WSL.</returns>
    public static string AdaptDiffMergeToolCommandToWsl(string command)
    {
        if (string.IsNullOrEmpty(command))
        {
            return command;
        }

        // Replace "D:" with "/mnt/d"
        int colonIndex = command.IndexOf(':');
        if (colonIndex == (command[0] == '"' ? 2 : 1))
        {
            int windowsDriveIndex = colonIndex - 1;
            command = $"{command[..windowsDriveIndex]}/mnt/{char.ToLower(command[windowsDriveIndex])}{command[(colonIndex + 1)..]}";
        }

        return DiffMergePlaceholderRegex.Replace(command, @"$(wslpath -aw $&)").ToPosixPath()!;
    }

    /// <summary>
    /// Tells Windows to forward <paramref name="envVarNames"/> to WSL by means of also setting WSLENV
    /// if <paramref name="workingDir"/> is a WSL path.
    /// </summary>
    /// <param name="envVariables">The current set of environment variables to be adapted.</param>
    /// <param name="workingDir">The path of the affected repo in order to check whether it is WSL.</param>
    /// <param name="envVarNames">A list of environment variables to be forwarded to WSL.</param>
    public static void ForwardEnvironmentVariableToWsl(this Dictionary<string, string> envVariables, string workingDir, params string[] envVarNames)
    {
        if (!PathUtil.IsWslPath(workingDir))
        {
            return;
        }

        const string envVarNameWslEnvVarControl = "WSLENV";
        const char separator = ':';
        string wslEnvControlValue = string.Join(separator, envVarNames);
        if (envVariables.Remove(envVarNameWslEnvVarControl, out string? existingWslEnvValue))
        {
            wslEnvControlValue = $"{existingWslEnvValue}{separator}{wslEnvControlValue}";
        }

        envVariables.Add(envVarNameWslEnvVarControl, wslEnvControlValue);
    }
}
