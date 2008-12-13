using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitUI;

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

            if (args.Count() <= 1)
            {
                Application.Run(new FormBrowse());
                return;
            }

            if (args.Count() > 1 && args[1] == "addfiles")
            {
                Application.Run(new FormAddFiles());
                return;
            }

            if (args.Count() > 1 && args[1] == "applypatch")
            {
                Application.Run(new MergePatch());
                return;
            }

            if (args.Count() > 1 && args[1] == "branch")
            {
                Application.Run(new FormBranch());
                return;
            }

            if (args.Count() > 1 && args[1] == "chechoutbranch")
            {
                Application.Run(new FormCheckoutBranck());
                return;
            }
            if (args.Count() > 1 && args[1] == "chechoutrevision")
            {
                Application.Run(new FormCheckout());
                return;
            }
            if (args.Count() > 1 && args[1] == "clone")
            {
                Application.Run(new FormClone());
                return;
            }
            if (args.Count() > 1 && args[1] == "commit")
            {
                Application.Run(new FormCommit());
                return;
            }
            if (args.Count() > 1 && args[1] == "formatpatch")
            {
                Application.Run(new FormFormatPath());
                return;
            }
            if (args.Count() > 1 && args[1] == "pull")
            {
                Application.Run(new FormPull());
                return;
            }
            if (args.Count() > 1 && args[1] == "push")
            {
                Application.Run(new FormPush());
                return;
            }
            if (args.Count() > 1 && args[1] == "settings")
            {
                Application.Run(new FormSettigns());
                return;
            }
            if (args.Count() > 1 && args[1] == "viewdiff")
            {
                Application.Run(new FormDiff());
                return;
            }
        }
    }
}
