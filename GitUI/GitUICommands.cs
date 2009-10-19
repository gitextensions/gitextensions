using System;
using System.Collections.Generic;
using System.Text;
using PatchApply;
using GitCommands;
using System.IO;
using GitUIPluginInterfaces;
using System.Windows.Forms;

namespace GitUI
{
    public class GitUICommands : IGitUICommands
    {
        private static GitUICommands instance = null;

        public static GitUICommands Instance
        {
            get
            {
                if (instance == null)
                    instance = new GitUICommands();

                return instance;
            }
        }

        public event GitUIEventHandler PreBrowse;
        public event GitUIEventHandler PostBrowse;

        public event GitUIEventHandler PreDeleteBranch;
        public event GitUIEventHandler PostDeleteBranch;

        public event GitUIEventHandler PreCheckoutRevision;
        public event GitUIEventHandler PostCheckoutRevision;

        public event GitUIEventHandler PreCheckoutBranch;
        public event GitUIEventHandler PostCheckoutBranch;

        public event GitUIEventHandler PreFileHistory;
        public event GitUIEventHandler PostFileHistory;

        public event GitUIEventHandler PreCompareRevisions;
        public event GitUIEventHandler PostCompareRevisions;

        public event GitUIEventHandler PreAddFiles;
        public event GitUIEventHandler PostAddFiles;

        public event GitUIEventHandler PreCreateBranch;
        public event GitUIEventHandler PostCreateBranch;

        public event GitUIEventHandler PreClone;
        public event GitUIEventHandler PostClone;

        public event GitUIEventHandler PreCommit;
        public event GitUIEventHandler PostCommit;

        public event GitUIEventHandler PreInitialize;
        public event GitUIEventHandler PostInitialize;

        public event GitUIEventHandler PrePush;
        public event GitUIEventHandler PostPush;

        public event GitUIEventHandler PrePull;
        public event GitUIEventHandler PostPull;

        public event GitUIEventHandler PreViewPatch;
        public event GitUIEventHandler PostViewPatch;

        public event GitUIEventHandler PreApplyPatch;
        public event GitUIEventHandler PostApplyPatch;

        public event GitUIEventHandler PreFormatPatch;
        public event GitUIEventHandler PostFormatPatch;

        public event GitUIEventHandler PreStash;
        public event GitUIEventHandler PostStash;

        public event GitUIEventHandler PreResolveConflicts;
        public event GitUIEventHandler PostResolveConflicts;

        public event GitUIEventHandler PreCherryPick;
        public event GitUIEventHandler PostCherryPick;

        public event GitUIEventHandler PreMergeBranch;
        public event GitUIEventHandler PostMergeBranch;

        public event GitUIEventHandler PreCreateTag;
        public event GitUIEventHandler PostCreateTag;

        public event GitUIEventHandler PreDeleteTag;
        public event GitUIEventHandler PostDeleteTag;

        public event GitUIEventHandler PreEditGitIgnore;
        public event GitUIEventHandler PostEditGitIgnore;

        public event GitUIEventHandler PreSettings;
        public event GitUIEventHandler PostSettings;

        public event GitUIEventHandler PreArchive;
        public event GitUIEventHandler PostArchive;

        public event GitUIEventHandler PreMailMap;
        public event GitUIEventHandler PostMailMap;

        public event GitUIEventHandler PreVerifyDatabase;
        public event GitUIEventHandler PostVerifyDatabase;

        public event GitUIEventHandler PreRemotes;
        public event GitUIEventHandler PostRemotes;

        public event GitUIEventHandler PreRebase;
        public event GitUIEventHandler PostRebase;

        public event GitUIEventHandler PreSubmodulesEdit;
        public event GitUIEventHandler PostSubmodulesEdit;

        public event GitUIEventHandler PreUpdateSubmodules;
        public event GitUIEventHandler PostUpdateSubmodules;

        public event GitUIEventHandler PreUpdateSubmodulesRecursive;
        public event GitUIEventHandler PostUpdateSubmodulesRecursive;

        public string GitCommand(string arguments)
        {
            return GitCommands.GitCommands.RunCmd(Settings.GitDir + "git.cmd", arguments);
        }

        public string CommandLineCommand(string cmd, string arguments)
        {
            return GitCommands.GitCommands.RunCmd(cmd, arguments);
        }


        public bool StartCommandLineProcessDialog(string command, string arguments)
        {
            FormProcess process = new FormProcess(command, arguments);
            return true;
        }

        public bool StartGitCommandProcessDialog(string arguments)
        {
            FormProcess process = new FormProcess(arguments);
            return true;
        }

        public bool StartBrowseDialog()
        {
            if (!InvokeEvent(PreBrowse))
                return false;

            FormBrowse form = new FormBrowse();
            form.ShowDialog();

            InvokeEvent(PostBrowse);

            return true;
        }
        
        public bool StartDeleteBranchDialog()
        {
            if (!InvokeEvent(PreDeleteBranch))
                return false;

            FormDeleteBranch form = new FormDeleteBranch();
            form.ShowDialog();

            InvokeEvent(PostDeleteBranch);

            return true;
        }

        public bool StartCheckoutRevisionDialog()
        {
            if (!InvokeEvent(PreCheckoutRevision))
                return false;

            FormCheckout form = new FormCheckout();
            form.ShowDialog();

            InvokeEvent(PostCheckoutRevision);

            return true;
        }

        public bool StartCheckoutBranchDialog()
        {
            if (!InvokeEvent(PreCheckoutBranch))
                return false;

            FormCheckoutBranch form = new FormCheckoutBranch();
            form.ShowDialog();

            InvokeEvent(PostCheckoutBranch);

            return true;
        }

        public bool StartFileHistoryDialog(string fileName)
        {
            if (!InvokeEvent(PreFileHistory))
                return false;

            FormFileHistory form = new FormFileHistory(fileName);
            form.ShowDialog();

            InvokeEvent(PostFileHistory);

            return false;
        }

        public bool StartCompareRevisionsDialog()
        {
            if (!InvokeEvent(PreCompareRevisions))
                return false; 
            
            FormDiff form = new FormDiff();
            form.ShowDialog();

            InvokeEvent(PostCompareRevisions);

            return false;
        }

        public bool StartAddFilesDialog()
        {
            if (!InvokeEvent(PreAddFiles))
                return false; 

            FormAddFiles form = new FormAddFiles();
            form.ShowDialog();

            InvokeEvent(PostAddFiles);

            return false;
        }

        public bool StartCreateBranchDialog()
        {
            if (!InvokeEvent(PreCreateBranch))
                return false; 

            FormBranch form = new FormBranch();
            form.ShowDialog();

            InvokeEvent(PostCreateBranch);

            return true;
        }

        public bool StartCloneDialog()
        {
            if (!InvokeEvent(PreClone))
                return false;

            FormClone form = new FormClone();
            form.ShowDialog();

            InvokeEvent(PostClone);

            return true;
        }


        public bool StartCommitDialog()
        {
            if (!InvokeEvent(PreCommit))
                return true;

            FormCommit form = new FormCommit();
            form.ShowDialog();

            InvokeEvent(PostCommit);

            if (!form.NeedRefresh)
                return false;

            return true;
        }


        public bool StartInitializeDialog()
        {
            if (!InvokeEvent(PreInitialize))
                return true;

            if (!GitCommands.Settings.ValidWorkingDir())
                new FormInit(GitCommands.Settings.WorkingDir).ShowDialog();
            else
                new FormInit().ShowDialog();

            InvokeEvent(PostInitialize);

            return true;
        }

        public bool StartInitializeDialog(string dir)
        {
            if (!InvokeEvent(PreInitialize))
                return true;

            new FormInit(dir).ShowDialog();

            InvokeEvent(PostInitialize);

            return true;
        }

        public bool StartPushDialog()
        {
            if (!InvokeEvent(PrePush))
                return true;

            new FormPush().ShowDialog();

            InvokeEvent(PostPush);

            return true;
        }

        public bool StartPullDialog()
        {
            if (!InvokeEvent(PrePull))
                return true;

            new FormPull().ShowDialog();

            InvokeEvent(PostPull);

            return true;
        }

        public bool StartViewPatchDialog()
        {
            if (!InvokeEvent(PreViewPatch))
                return true;

            ViewPatch applyPatch = new ViewPatch();
            applyPatch.ShowDialog();

            InvokeEvent(PostViewPatch);

            return true;
        }

        public bool StartApplyPatchDialog()
        {
            if (!InvokeEvent(PreApplyPatch))
                return true; 
            
            MergePatch form = new MergePatch();
            form.ShowDialog();

            InvokeEvent(PostApplyPatch);

            return true;
        }

        public bool StartFormatPatchDialog()
        {
            if (!InvokeEvent(PreFormatPatch))
                return true; 

            FormFormatPath form = new FormFormatPath();
            form.ShowDialog();

            InvokeEvent(PostFormatPatch);

            return false;
        }

        public bool StartStashDialog()
        {
            if (!InvokeEvent(PreStash))
                return true; 

            FormStash form = new FormStash();
            form.ShowDialog();

            InvokeEvent(PostStash);

            return false;
        }

        public bool StartResolveConflictsDialog()
        {
            if (!InvokeEvent(PreResolveConflicts))
                return true; 
            
            FormResolveConflicts form = new FormResolveConflicts();
            form.ShowDialog();

            InvokeEvent(PostResolveConflicts);

            return false;
        }

        public bool StartCherryPickDialog()
        {
            if (!InvokeEvent(PreCherryPick))
                return true; 

            FormCherryPick form = new FormCherryPick();
            form.ShowDialog();

            InvokeEvent(PostCherryPick);

            return true;
        }

        public bool StartMergeBranchDialog()
        {
            if (!InvokeEvent(PreMergeBranch))
                return true; 
            
            FormMergeBranch form = new FormMergeBranch();
            form.ShowDialog();

            InvokeEvent(PostMergeBranch);

            return true;
        }

        public bool StartCreateTagDialog()
        {
            if (!InvokeEvent(PreCreateTag))
                return true; 

            FormTag form = new FormTag();
            form.ShowDialog();

            InvokeEvent(PostCreateTag);
            
            return true;
        }

        public bool StartDeleteTagDialog()
        {
            if (!InvokeEvent(PreDeleteTag))
                return true;

            FormDeleteTag form = new FormDeleteTag();
            form.ShowDialog();

            InvokeEvent(PostDeleteTag);

            return true;
        }

        public bool StartEditGitIgnoreDialog()
        {
            if (!InvokeEvent(PreEditGitIgnore))
                return true;

            FormGitIgnore form = new FormGitIgnore();
            form.ShowDialog();

            InvokeEvent(PostEditGitIgnore);

            return false;
        }

        public bool StartSettingsDialog()
        {
            if (!InvokeEvent(PreSettings))
                return true;

            FormSettings form = new FormSettings();
            form.ShowDialog();

            InvokeEvent(PostSettings);

            return false;
        }

        public bool StartArchiveDialog()
        {
            if (!InvokeEvent(PreArchive))
                return true;

            FormArchive form = new FormArchive();
            form.ShowDialog();

            InvokeEvent(PostArchive);

            return false;
        }

        public bool StartMailMapDialog()
        {
            if (!InvokeEvent(PreMailMap))
                return true;

            FormMailMap form = new FormMailMap();
            form.ShowDialog();

            InvokeEvent(PostMailMap);

            return true;
        }

        public bool StartVerifyDatabaseDialog()
        {
            if (!InvokeEvent(PreVerifyDatabase))
                return true; 
            
            FormVerify form = new FormVerify();
            form.ShowDialog();

            InvokeEvent(PostVerifyDatabase);

            return true;
        }

        public bool StartRemotesDialog()
        {
            if (!InvokeEvent(PreRemotes))
                return true;

            FormRemotes form = new FormRemotes();
            form.ShowDialog();

            InvokeEvent(PostRemotes);

            return true;
        }

        public bool StartRebaseDialog()
        {
            if (!InvokeEvent(PreRebase))
                return true;

            FormRebase form = new FormRebase();
            form.ShowDialog();

            InvokeEvent(PostRebase);

            return true;
        }

        public bool StartSubmodulesDialog()
        {
            if (!InvokeEvent(PreSubmodulesEdit))
                return true;

            FormSubmodules form = new FormSubmodules();
            form.ShowDialog();

            InvokeEvent(PostSubmodulesEdit);

            return true;
        }

        public bool StartUpdateSubmodulesDialog()
        {
            if (!InvokeEvent(PreUpdateSubmodules))
                return true;

            FormProcess process = new FormProcess(GitCommands.GitCommands.SubmoduleUpdateCmd(""));

            InvokeEvent(PostUpdateSubmodules);

            return true;
        }

        public bool StartUpdateSubmodulesRecursiveDialog()
        {
            if (!InvokeEvent(PreUpdateSubmodulesRecursive))
                return true;

            FormProcess process = new FormProcess(GitCommands.GitCommands.SubmoduleUpdateCmd(""));
            UpdateSubmodulesRecursive();

            InvokeEvent(PostUpdateSubmodulesRecursive);

            return true;
        }

        public bool StartPluginSettingsDialog()
        {
            new FormPluginSettings().ShowDialog();
            return true;
        }
        

        private static void UpdateSubmodulesRecursive()
        {
            string oldworkingdir = Settings.WorkingDir;

            foreach (GitSubmodule submodule in (new GitCommands.GitCommands()).GetSubmodules())
            {
                if (!string.IsNullOrEmpty(submodule.LocalPath))
                {
                    Settings.WorkingDir = oldworkingdir + submodule.LocalPath;

                    if (Settings.WorkingDir != oldworkingdir && File.Exists(GitCommands.Settings.WorkingDir + ".gitmodules"))
                    {
                        FormProcess process = new FormProcess(GitCommands.GitCommands.SubmoduleUpdateCmd(""));

                        UpdateSubmodulesRecursive();
                    }

                    Settings.WorkingDir = oldworkingdir;
                }
            }

            Settings.WorkingDir = oldworkingdir;
        }

        internal static bool InvokeEvent(GitUIEventHandler gitUIEventHandler)
        {
            try
            {
                GitUIEventArgs e = new GitUIEventArgs(GitUICommands.Instance);
                if (gitUIEventHandler != null)
                    gitUIEventHandler.Invoke(e);

                return !e.Cancel;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception");
            }
            return true;
        }

    }
}
