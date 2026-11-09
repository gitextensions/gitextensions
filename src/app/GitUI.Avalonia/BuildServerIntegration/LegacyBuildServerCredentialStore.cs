using System.Security.Cryptography;
using System.Text;
using GitCommands.Config;
using GitExtensions.Extensibility.Configurations;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace GitUI.BuildServerIntegration;

// Framework constraint: the Avalonia apphost has a different isolated-storage identity from the original application.
internal static class LegacyBuildServerCredentialStore
{
    private static readonly ExternalIO DefaultIO = new(
        () => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IsolatedStorage"),
        Unprotect,
        OperatingSystem.IsWindows);

    // parity-scaffolding: isolate only external platform, filesystem-root and decryption I/O in the current test flow.
    private static readonly AsyncLocal<ExternalIO?> TestIO = new();

    internal static BuildServerCredentials? Get(string buildServerUniqueKey)
    {
        ExternalIO io = TestIO.Value ?? DefaultIO;
        if (!io.IsSupported())
        {
            return null;
        }

        string root = io.Root();
        if (!Directory.Exists(root))
        {
            return null;
        }

        string fileName = string.Format("BuildServer-{0}.options", Convert.ToBase64String(Encoding.UTF8.GetBytes(buildServerUniqueKey)));
        BuildServerCredentials? result = null;
        try
        {
            foreach (string randomRoot in Directory.EnumerateDirectories(root).Where(IsRandomDirectory))
            {
                foreach (string randomDirectory in Directory.EnumerateDirectories(randomRoot).Where(IsRandomDirectory))
                {
                    foreach (string identity in Directory.EnumerateDirectories(randomDirectory).Where(IsIdentityDirectory))
                    {
                        string name = Path.GetFileName(identity);
                        string files = Path.Combine(identity, name, "Files");
                        string path = Path.Combine(files, fileName);
                        if (!File.Exists(path))
                        {
                            continue;
                        }

                        BuildServerCredentials? credentials = Read(path, io.Unprotect);
                        if (credentials is null || (result is not null && !Matches(result, credentials)))
                        {
                            return null;
                        }

                        result = credentials;
                    }
                }
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return null;
        }

        return result;
    }

    private static BuildServerCredentials? Read(string path, Func<byte[], byte[]> unprotect)
    {
        byte[] protectedData = File.ReadAllBytes(path);
        if (protectedData.Length == 0)
        {
            return null;
        }

        try
        {
            byte[] unprotectedData = unprotect(protectedData);
            using MemoryStream memoryStream = new(unprotectedData);
            ConfigFile credentialsConfig = new(fileName: "");
            using (StreamReader textReader = new(memoryStream, Encoding.UTF8))
            {
                credentialsConfig.LoadFromString(textReader.ReadToEnd());
            }

            IConfigSection? section = credentialsConfig.FindConfigSection("Credentials");
            if (section is null)
            {
                return null;
            }

            BuildServerCredentialsType credentialType;
            string? configuredType = section.GetValue("BuildServerCredentialsType");
            if (!string.IsNullOrWhiteSpace(configuredType))
            {
                if (!Enum.TryParse(configuredType, ignoreCase: true, out credentialType))
                {
                    credentialType = BuildServerCredentialsType.Guest;
                }
            }
            else
            {
                credentialType = section.GetValueAsBool("UseGuestAccess", true)
                    ? BuildServerCredentialsType.Guest
                    : BuildServerCredentialsType.UsernameAndPassword;
            }

            return new BuildServerCredentials
            {
                BuildServerCredentialsType = credentialType,
                Username = section.GetValue("Username"),
                Password = section.GetValue("Password"),
                BearerToken = section.GetValue("BearerToken"),
            };
        }
        catch (CryptographicException)
        {
            // As per MSDN, the ProtectedData.Unprotect method is per user,
            // it will throw the CryptographicException if the current user
            // is not the one who protected the data.

            // Return no stored credentials so the user can reset the credentials.
            return null;
        }
    }

    private static bool Matches(BuildServerCredentials left, BuildServerCredentials right)
        => left.BuildServerCredentialsType == right.BuildServerCredentialsType
            && left.Username == right.Username && left.Password == right.Password && left.BearerToken == right.BearerToken;

    private static bool IsRandomDirectory(string path)
        => Path.GetFileName(path).Length == 12 && !IsLink(path);

    private static bool IsIdentityDirectory(string path)
    {
        string name = Path.GetFileName(path);
        return (name.StartsWith("Url.", StringComparison.Ordinal) || name.StartsWith("StrongName.", StringComparison.Ordinal))
            && !IsLink(path) && !IsLink(Path.Combine(path, name));
    }

    private static bool IsLink(string path)
        => Directory.Exists(path) && (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;

    private static byte[] Unprotect(byte[] protectedData)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Original encrypted credentials require Windows data protection.");
        }

        return ProtectedData.Unprotect(protectedData, null, DataProtectionScope.CurrentUser);
    }

    private sealed record ExternalIO(Func<string> Root, Func<byte[], byte[]> Unprotect, Func<bool> IsSupported);

    // parity-scaffolding: replace only external I/O for the real legacy-reader and watcher path.
    internal readonly struct TestAccessor
    {
        internal static IDisposable UseExternalIO(Func<string> root, Func<byte[], byte[]> unprotect, Func<bool> isSupported)
            => new ExternalIOScope(new ExternalIO(root, unprotect, isSupported));
    }

    // parity-scaffolding: restore the exact prior external I/O context after each regression.
    private sealed class ExternalIOScope : IDisposable
    {
        private readonly ExternalIO? _previous;

        internal ExternalIOScope(ExternalIO current)
        {
            _previous = TestIO.Value;
            TestIO.Value = current;
        }

        public void Dispose() => TestIO.Value = _previous;
    }
}
