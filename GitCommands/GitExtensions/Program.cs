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

            try
            {
                if (Application.UserAppDataRegistry.GetValue("dir8") != null) RepositoryHistory.MostRecentRepositories.Add(Application.UserAppDataRegistry.GetValue("dir8").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir7") != null) RepositoryHistory.MostRecentRepositories.Add(Application.UserAppDataRegistry.GetValue("dir7").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir6") != null) RepositoryHistory.MostRecentRepositories.Add(Application.UserAppDataRegistry.GetValue("dir6").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir5") != null) RepositoryHistory.MostRecentRepositories.Add(Application.UserAppDataRegistry.GetValue("dir5").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir4") != null) RepositoryHistory.MostRecentRepositories.Add(Application.UserAppDataRegistry.GetValue("dir4").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir3") != null) RepositoryHistory.MostRecentRepositories.Add(Application.UserAppDataRegistry.GetValue("dir3").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir2") != null) RepositoryHistory.MostRecentRepositories.Add(Application.UserAppDataRegistry.GetValue("dir2").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir1") != null) RepositoryHistory.MostRecentRepositories.Add(Application.UserAppDataRegistry.GetValue("dir1").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir0") != null) RepositoryHistory.MostRecentRepositories.Add(Application.UserAppDataRegistry.GetValue("dir0").ToString());
            }
            catch
            {
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
            
            if (args.Length <= 1)
            {
                Application.Run(new FormBrowse());

            }else

            if (args.Length > 1 && args[1] == "browse")
            {
                Application.Run(new FormBrowse());
                
            }else

            if (args.Length > 1 && args[1] == "addfiles")
            {
                Application.Run(new FormAddFiles());
                
            }else

            if (args.Length > 1 && args[1] == "applypatch")
            {
                Application.Run(new MergePatch());
                
            }else

            if (args.Length > 1 && args[1] == "branch")
            {
                Application.Run(new FormBranch());
                
            }else

            if (args.Length > 1 && args[1] == "checkoutbranch")
            {
                Application.Run(new FormCheckoutBranck());
                
            }else
            if (args.Length > 1 && args[1] == "checkoutrevision")
            {
                Application.Run(new FormCheckout());
                
            }else
            if (args.Length > 1 && args[1] == "clone")
            {
                Application.Run(new FormClone());
                
            }else
            if (args.Length > 1 && args[1] == "commit")
            {
                Application.Run(new FormCommit());
                
            }else
            if (args.Length > 1 && args[1] == "filehistory")
            {
                if (args.Length > 2)
                    Application.Run(new FormFileHistory(args[2]));
                else
                    MessageBox.Show("No file selected");
                
            } else
            if (args.Length > 1 && args[1] == "formatpatch")
            {
                Application.Run(new FormFormatPath());
                
            }else
            if (args.Length > 1 && args[1] == "pull")
            {
                Application.Run(new FormPull());
                
            }else
            if (args.Length > 1 && args[1] == "push")
            {
                Application.Run(new FormPush());
            }else
            if (args.Length > 1 && args[1] == "settings")
            {
                Application.Run(new FormSettigns());
            } else
            if (args.Length > 1 && args[1] == "viewdiff")
            {
                Application.Run(new FormDiff());
            }

            try
            {
                for (int n = 0; n < RepositoryHistory.MostRecentRepositories.Count; n++)
                {
                    Application.UserAppDataRegistry.SetValue("dir" + n.ToString(), RepositoryHistory.MostRecentRepositories[n]);
                }
            }
            catch
            {
            }
        }
    }
}
