using System;
using System.Text;
using System.Windows.Forms;
using BugReporter.Serialization;
using GitUI;
using Microsoft.VisualStudio.Threading;

namespace BugReporter
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // This form created to obtain UI synchronization context only
            using (new Form())
            {
                // Store the shared JoinableTaskContext
                ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
            }

            // If an error happens before we had a chance to init the environment information
            // the call to GetInformation() from BugReporter.ShowNBug() will fail.
            // There's no perf hit calling Initialise() multiple times.
            UserEnvironmentInformation.Initialise(ThisAssembly.Git.Sha, ThisAssembly.Git.IsDirty);

            SerializableException? exception = null;

            string[] args = Environment.GetCommandLineArgs();
            foreach (string arg in args)
            {
                string xml;
                try
                {
                    xml = Base64Decode(arg);
                }
                catch (Exception ex)
                {
                    exception = new(new Exception($"Failed to decode the error payload\r\n{arg}", ex));
                    continue;
                }

                try
                {
                    exception = SerializableException.FromXmlString(xml);
                }
                catch (Exception ex)
                {
                    exception = new(new Exception($"Failed to decode/parse error payload\r\n{xml}", ex));
                }
            }

            exception ??= new(new Exception("Missing error payload"));

            new BugReportForm().ShowDialog(owner: null, exception, UserEnvironmentInformation.GetInformation(), canIgnore: true, showIgnore: false, focusDetails: false);
        }

        private static string Base64Decode(string base64EncodedData)
        {
            byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
