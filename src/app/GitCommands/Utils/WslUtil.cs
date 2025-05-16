#nullable enable

namespace GitCommands.Utils;

public static class WslUtil
{
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
