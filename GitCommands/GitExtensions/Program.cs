using System;
using System.Collections.Generic;

using System.Windows.Forms;
using GitUI;
using System.IO;

namespace GitExtensions
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string[] args = Environment.GetCommandLineArgs();

            if (args.Length <= 1)
            {
                Application.Run(new FormBrowse());
                return;
            }

            if (args.Length == 3)
            {
                if (Directory.Exists(args[2]))
                    GitCommands.Settings.WorkingDir = args[2];

                if (string.IsNullOrEmpty(GitCommands.Settings.WorkingDir))
                    GitCommands.Settings.WorkingDir = args[2].Substring(0, args[2].LastIndexOf('\\'));

                if (string.IsNullOrEmpty(GitCommands.Settings.WorkingDir))
                    GitCommands.Settings.WorkingDir = Directory.GetCurrentDirectory();
            }

            if (args.Length > 1 && args[1] == "browse")
            {
                Application.Run(new FormBrowse());
                return;
            }

            if (args.Length > 1 && args[1] == "addfiles")
            {
                Application.Run(new FormAddFiles());
                return;
            }

            if (args.Length > 1 && args[1] == "applypatch")
            {
                Application.Run(new MergePatch());
                return;
            }

            if (args.Length > 1 && args[1] == "branch")
            {
                Application.Run(new FormBranch());
                return;
            }

            if (args.Length > 1 && args[1] == "checkoutbranch")
            {
                Application.Run(new FormCheckoutBranck());
                return;
            }
            if (args.Length > 1 && args[1] == "checkoutrevision")
            {
                Application.Run(new FormCheckout());
                return;
            }
            if (args.Length > 1 && args[1] == "clone")
            {
                Application.Run(new FormClone());
                return;
            }
            if (args.Length > 1 && args[1] == "commit")
            {
                Application.Run(new FormCommit());
                return;
            }
            if (args.Length > 1 && args[1] == "filehistory")
            {
                if (args.Length > 2)
                    Application.Run(new FormFileHistory(args[2]));
                else
                    MessageBox.Show("No file selected");
                return;
            } 
            if (args.Length > 1 && args[1] == "formatpatch")
            {
                Application.Run(new FormFormatPath());
                return;
            }
            if (args.Length > 1 && args[1] == "pull")
            {
                Application.Run(new FormPull());
                return;
            }
            if (args.Length > 1 && args[1] == "push")
            {
                Application.Run(new FormPush());
                return;
            }
            if (args.Length > 1 && args[1] == "settings")
            {
                Application.Run(new FormSettigns());
                return;
            }
            if (args.Length > 1 && args[1] == "viewdiff")
            {
                Application.Run(new FormDiff());
                return;
            }
        }
    }
}
