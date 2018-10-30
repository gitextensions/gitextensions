using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public sealed class UserEnvironmentInformation
    {
        public static void CopyInformation()
        {
            Clipboard.SetText(GetInformation());
        }

        public static string GetInformation()
        {
            string gitVer;
            try
            {
                gitVer = GitVersion.Current?.Full ?? "";
            }
            catch (Exception)
            {
                gitVer = "";
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("- Git Extensions {0}{1}", AppSettings.ProductVersion, Environment.NewLine);
            sb.AppendFormat("- {0} {1}{2}", ThisAssembly.Git.Sha, ThisAssembly.Git.IsDirty ? " (Dirty)" : "", Environment.NewLine);
            sb.AppendFormat("- Git {0}{1}", gitVer, Environment.NewLine);
            sb.AppendFormat("- {0}{1}", Environment.OSVersion, Environment.NewLine);
            sb.AppendFormat("- {0}", RuntimeInformation.FrameworkDescription);
            return sb.ToString();
        }
    }
}