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
                if (Application.UserAppDataRegistry.GetValue("maxcommits") != null)
                {
                    int result;
                    if (int.TryParse(Application.UserAppDataRegistry.GetValue("maxcommits").ToString(), out result) == true)
                    {
                        GitCommands.Settings.MaxCommits = result;
                    }
                }

                if (Application.UserAppDataRegistry.GetValue("gitssh") != null) GitCommands.GitCommands.SetSsh( Application.UserAppDataRegistry.GetValue("gitssh").ToString() );
                if (Application.UserAppDataRegistry.GetValue("plink") != null) GitCommands.Settings.Plink = Application.UserAppDataRegistry.GetValue("plink").ToString();
                if (Application.UserAppDataRegistry.GetValue("puttygen") != null) GitCommands.Settings.Puttygen = Application.UserAppDataRegistry.GetValue("puttygen").ToString();
                if (Application.UserAppDataRegistry.GetValue("pageant") != null) GitCommands.Settings.Pageant = Application.UserAppDataRegistry.GetValue("pageant").ToString();

                if (Application.UserAppDataRegistry.GetValue("showrevisiongraph") != null) GitCommands.Settings.ShowRevisionGraph = Application.UserAppDataRegistry.GetValue("showrevisiongraph").ToString() == "True";
                if (Application.UserAppDataRegistry.GetValue("closeprocessdialog") != null) GitCommands.Settings.CloseProcessDialog = Application.UserAppDataRegistry.GetValue("closeprocessdialog").ToString() == "True";
                if (Application.UserAppDataRegistry.GetValue("showallbranches") != null) GitCommands.Settings.ShowAllBranches = Application.UserAppDataRegistry.GetValue("showallbranches").ToString() == "True";
                if (Application.UserAppDataRegistry.GetValue("gitdir") != null) GitCommands.Settings.GitDir = Application.UserAppDataRegistry.GetValue("gitdir").ToString();
                if (Application.UserAppDataRegistry.GetValue("gitbindir") != null) GitCommands.Settings.GitBinDir = Application.UserAppDataRegistry.GetValue("gitbindir").ToString();
                if (Application.UserAppDataRegistry.GetValue("dir13") != null) RepositoryHistory.AddMostRecentRepository(Application.UserAppDataRegistry.GetValue("dir13").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir12") != null) RepositoryHistory.AddMostRecentRepository(Application.UserAppDataRegistry.GetValue("dir12").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir11") != null) RepositoryHistory.AddMostRecentRepository(Application.UserAppDataRegistry.GetValue("dir11").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir10") != null) RepositoryHistory.AddMostRecentRepository(Application.UserAppDataRegistry.GetValue("dir10").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir9") != null) RepositoryHistory.AddMostRecentRepository(Application.UserAppDataRegistry.GetValue("dir9").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir8") != null) RepositoryHistory.AddMostRecentRepository(Application.UserAppDataRegistry.GetValue("dir8").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir7") != null) RepositoryHistory.AddMostRecentRepository(Application.UserAppDataRegistry.GetValue("dir7").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir6") != null) RepositoryHistory.AddMostRecentRepository(Application.UserAppDataRegistry.GetValue("dir6").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir5") != null) RepositoryHistory.AddMostRecentRepository(Application.UserAppDataRegistry.GetValue("dir5").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir4") != null) RepositoryHistory.AddMostRecentRepository(Application.UserAppDataRegistry.GetValue("dir4").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir3") != null) RepositoryHistory.AddMostRecentRepository(Application.UserAppDataRegistry.GetValue("dir3").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir2") != null) RepositoryHistory.AddMostRecentRepository(Application.UserAppDataRegistry.GetValue("dir2").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir1") != null) RepositoryHistory.AddMostRecentRepository(Application.UserAppDataRegistry.GetValue("dir1").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir0") != null) RepositoryHistory.AddMostRecentRepository(Application.UserAppDataRegistry.GetValue("dir0").ToString());

                if ((Application.UserAppDataRegistry.GetValue("checksettings") == null ||
                     Application.UserAppDataRegistry.GetValue("checksettings").ToString() == "true"))
                {
                    
                    FormSettigns settings = new FormSettigns();
                    if (!settings.CheckSettings())
                        Application.Run(settings);
                        //settings.ShowDialog();
                }
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
            if (args.Length > 1 && args[1] == "mergeconflicts")
            {
                Application.Run(new FormResolveConflicts());

            }
            else
            if (args.Length > 1 && args[1] == "gitbash")
            {
                GitCommands.GitCommands.RunBash();
            }
            else
            if (args.Length > 1 && args[1] == "gitignore")
            {
                Application.Run(new FormGitIgnore());
            }
            else
            if (args.Length > 1 && args[1] == "remotes")
            {
                Application.Run(new FormRemotes());
            }
            else 
            if (args.Length > 1 && args[1] == "browse")
            {
                Application.Run(new FormBrowse());
                
            }else
            if (args.Length > 1 && (args[1] == "addfiles" || args[1] == "add"))
            {
                Application.Run(new FormAddFiles());
                
            }else

            if (args.Length > 1 && (args[1] == "applypatch" || args[1] == "apply"))
            {
                Application.Run(new MergePatch());
                
            }else

            if (args.Length > 1 && args[1] == "branch")
            {
                Application.Run(new FormBranch());
                
            }else

            if (args.Length > 1 && (args[1] == "checkoutbranch" || args[1] == "checkout"))
            {
                Application.Run(new FormCheckoutBranck());
                
            }else
            if (args.Length > 1 && args[1] == "checkoutrevision")
            {
                Application.Run(new FormCheckout());
                
            }else
            if (args.Length > 1 && args[1] == "init")
            {
                FormInit frm = new FormInit(args[2]);
                Application.Run(frm);

            }
            else 
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
            } else
            if (args.Length > 1 && args[1] == "rebase")
            {
                Application.Run(new FormRebase());
            }
            else
            if (args.Length > 1 && args[1] == "merge")
            {
                Application.Run(new FormMergeBranch());
            } else
            if (args.Length > 1 && args[1] == "cherry")
            {
                Application.Run(new FormCherryPick());
            } else
            if (args.Length > 1 && args[1] == "revert")
            {
                Application.Run(new FormRevert(args[2]));
            }
            else
            if (args.Length > 1 && args[1] == "tag")
            {
                Application.Run(new FormTag());
            } else
            if (args.Length > 1 && args[1] == "about")
            {
                Application.Run(new AboutBox());
            }
            else
                if (args.Length > 1 && args[1] == "stash")
                {
                    Application.Run(new FormStash());
                }
                else
                    if (args.Length > 1)
                    {
                        Application.Run(new FormCommandlineHelp());
                    }

            try
            {
                for (int n = 0; n < RepositoryHistory.MostRecentRepositories.Count; n++)
                {
                    Application.UserAppDataRegistry.SetValue("dir" + n.ToString(), RepositoryHistory.MostRecentRepositories[n]);
                }
                Application.UserAppDataRegistry.SetValue("maxcommits", GitCommands.Settings.MaxCommits);
                Application.UserAppDataRegistry.SetValue("gitdir", GitCommands.Settings.GitDir);
                Application.UserAppDataRegistry.SetValue("gitbindir", GitCommands.Settings.GitBinDir);
                Application.UserAppDataRegistry.SetValue("showallbranches", GitCommands.Settings.ShowAllBranches);
                Application.UserAppDataRegistry.SetValue("closeprocessdialog", GitCommands.Settings.CloseProcessDialog);
                Application.UserAppDataRegistry.SetValue("showrevisiongraph", GitCommands.Settings.ShowRevisionGraph);
                
                Application.UserAppDataRegistry.SetValue("gitssh", GitCommands.GitCommands.GetSsh());

                Application.UserAppDataRegistry.SetValue("plink", GitCommands.Settings.Plink);
                Application.UserAppDataRegistry.SetValue("puttygen", GitCommands.Settings.Puttygen);
                Application.UserAppDataRegistry.SetValue("pageant", GitCommands.Settings.Pageant);
            }
            catch
            {
            }
        }
    }
}
