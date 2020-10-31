using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using AdysTech.CredentialManager;
using GitExtensions.Core.Settings;
using JetBrains.Annotations;

namespace GitExtensions.Extensibility.Settings
{
    public interface ICredentialsManager
    {
        void Save();
    }

    public class CredentialsManager : ICredentialsManager
    {
        private static ConcurrentDictionary<string, NetworkCredential> Credentials { get; } = new ConcurrentDictionary<string, NetworkCredential>();
        private readonly Func<string> _getWorkingDir;

        public CredentialsManager()
        {
        }

        protected CredentialsManager(Func<string> getWorkingDir)
        {
            _getWorkingDir = getWorkingDir;
        }

        public void Save()
        {
            var credentials = Credentials.ToList();
            if (credentials.Count < 1)
            {
                return;
            }

            Credentials.Clear();

            foreach (var networkCredentials in credentials.Where(c => c.Value != null))
            {
                AdysTechCredentialManagerWrapper.UpdateCredentials(networkCredentials.Key,
                    networkCredentials.Value.UserName,
                    networkCredentials.Value.Password);
            }
        }

        protected NetworkCredential GetCredentialOrDefault(SettingLevel settingLevel, [NotNull] string name, NetworkCredential defaultValue)
        {
            var targetName = GetWindowsCredentialsTarget(name, settingLevel);
            if (string.IsNullOrWhiteSpace(targetName))
            {
                return defaultValue;
            }

            if (Credentials.TryGetValue(targetName, out var result) || AdysTechCredentialManagerWrapper.TryGetCredentials(targetName, out result))
            {
                return result ?? defaultValue;
            }

            return defaultValue;
        }

        protected void SetCredentials(SettingLevel settingLevel, [NotNull] string name, [CanBeNull] NetworkCredential value)
        {
            var targetName = GetWindowsCredentialsTarget(name, settingLevel);
            Credentials.AddOrUpdate(targetName, value, (s, credential) => value);
        }

        private string GetWindowsCredentialsTarget(string name, SettingLevel settingLevel)
        {
            if (settingLevel == SettingLevel.Global)
            {
                return $"{name}";
            }

            var suffix = _getWorkingDir();
            return string.IsNullOrWhiteSpace(suffix) ? null : $"{name}_{suffix}";
        }

        private static class AdysTechCredentialManagerWrapper
        {
            private const string TargetPrefix = "GitExtensions_";

            private static string GetTarget(string rawTarget)
            {
                if (string.IsNullOrWhiteSpace(rawTarget))
                {
                    throw new ArgumentNullException(nameof(rawTarget));
                }

                return $"{TargetPrefix}{rawTarget}";
            }

            public static bool TryGetCredentials(string target, out NetworkCredential credentials)
            {
                credentials = CredentialManager.GetCredentials(GetTarget(target));
                return credentials != null;
            }

            public static bool SaveCredentials(string target, string userName, string password)
            {
                if (string.IsNullOrWhiteSpace(target))
                {
                    return false;
                }

                return CredentialManager.SaveCredentials(GetTarget(target), new NetworkCredential(userName.Trim(), password));
            }

            private static bool RemoveCredentials(string target)
            {
                if (string.IsNullOrWhiteSpace(target) || CredentialManager.GetCredentials(GetTarget(target)) == null)
                {
                    return false;
                }

                return CredentialManager.RemoveCredentials(GetTarget(target));
            }

            public static bool UpdateCredentials(string target, string userName, string password)
            {
                if (string.IsNullOrWhiteSpace(userName))
                {
                    return RemoveCredentials(target);
                }

                return SaveCredentials(target, userName, password);
            }
        }
    }
}
