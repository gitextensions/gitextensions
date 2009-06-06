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

            GitCommands.Settings.LoadSettings();

            try
            {
                if ((Application.UserAppDataRegistry.GetValue("checksettings") == null ||
                     Application.UserAppDataRegistry.GetValue("checksettings").ToString() == "true"))
                {

                    FormSettings settings = new FormSettings();
                    if (!settings.CheckSettings())
                    {
                        FormSettings.AutoSolveAllSettings();
                        Application.Run(settings);
                        //settings.ShowDialog();
                    }
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
                Application.Run(new FormSettings());
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

            GitCommands.Settings.SaveSettings();
        }

    }
}
