using System;
using System.Net;
using Adys = AdysTech.CredentialManager;

namespace GitExtUtils
{
    public static class CredentialManager
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
            credentials = Adys.CredentialManager.GetCredentials(GetTarget(target));
            return credentials != null;
        }

        public static bool SaveCredentials(string target, string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                return false;
            }

            return Adys.CredentialManager.SaveCredentials(GetTarget(target), new NetworkCredential(userName.Trim(), password));
        }

        public static bool RemoveCredentials(string target)
        {
            if (string.IsNullOrWhiteSpace(target) || Adys.CredentialManager.GetCredentials(GetTarget(target)) == null)
            {
                return false;
            }

            return Adys.CredentialManager.RemoveCredentials(GetTarget(target));
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
