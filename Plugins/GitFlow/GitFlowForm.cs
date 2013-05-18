using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace GitFlow
{
    public partial class GitFlowForm : Form
    {
        GitUIBaseEventArgs m_gitUiCommands;

        Dictionary<string,List<string>> Branches { get; set; }
        readonly ToolTip _toolTipCommandResult = new ToolTip();
        readonly ToolTip _toolTipDebug = new ToolTip();

        readonly BackgroundWorker _bw = new BackgroundWorker();
        public bool IsRefreshNeeded { get; set; }

        enum Branch
        {
            feature,
            hotfix,
            release,
            support
        }

        private List<string> BranchTypes
        {
            get { return Enum.GetValues(typeof (Branch)).Cast<object>().Select(e => e.ToString()).ToList(); }
        }

        private bool IsGitFlowInited
        {
            get { return !string.IsNullOrWhiteSpace(m_gitUiCommands.GitModule.RunGit("config --get gitflow.branch.master")); }
        }

        public GitFlowForm(GitUIBaseEventArgs gitUiCommands)
        {
            InitializeComponent();

            m_gitUiCommands = gitUiCommands;

            Branches = new Dictionary<string, List<string>>();

            lblPrefixManage.Text = string.Empty;
            lblCommandResult.Text = string.Empty;

            Init();
        }

        private void Init()
        {
            if (IsGitFlowInited)
            {
                btnInit.Visible = false;
                gbStart.Enabled = true;
                gbManage.Enabled = true;
                var remotes = m_gitUiCommands.GitModule.GetRemotes(true).Where(r => !string.IsNullOrWhiteSpace(r)).ToList();
                cbRemote.DataSource = remotes;
                btnPull.Enabled = btnPublish.Enabled = remotes.Any();
                _bw.WorkerSupportsCancellation = true;
                _bw.DoWork += bw_DoWork;
                _bw.RunWorkerCompleted += bw_RunWorkerCompleted;

                _toolTipCommandResult.AutoPopDelay = 0;
                _toolTipCommandResult.InitialDelay = 0;
                _toolTipCommandResult.ReshowDelay = 0;
                _toolTipCommandResult.ShowAlways = true;

                _toolTipDebug.AutoPopDelay = 0;
                _toolTipDebug.InitialDelay = 0;
                _toolTipDebug.ReshowDelay = 0;
                _toolTipDebug.ShowAlways = true;

                cbType.DataSource = BranchTypes;
                var types = new List<string> { string.Empty };
                types.AddRange(BranchTypes);
                cbManageType.DataSource = types;

                cbBasedOn.Checked = false;
                cbBaseBranch.Enabled  = false;
                LoadBaseBranches();
            }
            else
            {
                gbStart.Enabled = false;
                gbManage.Enabled = false;
            }
        }

        #region Loading Branches
        private void LoadBranches(string branchType)
        {
            cbManageType.Enabled = false;
            cbBranches.DataSource = new List<string> {"Loading..."};
            while (_bw.IsBusy)
            {
                Thread.Sleep(100);
            }
            if (!Branches.ContainsKey(branchType))
                _bw.RunWorkerAsync(branchType);
            else
                DisplayBranchDatas();
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var branchType = (string)e.Argument;
            Branches.Add(branchType, GetBranches(branchType));
        }

        private List<string> GetBranches(string typeBranch)
        {
            string[] references = m_gitUiCommands.GitModule.RunGit("flow " + typeBranch)
                                                 .Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);

            if (references.Length == 0 || references.Any(l=>l.StartsWith("No " + typeBranch + " branches exist.")))
                return new List<string>();

            return references.Select(e => e.Trim('*', ' ', '\n', '\r')).ToList();
        }

        private List<string> GetLocalBranches()
        {
            string[] references = m_gitUiCommands.GitModule.RunGit("branch")
                                                 .Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);

            return references.Select(e => e.Trim('*', ' ', '\n', '\r')).ToList();
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DisplayBranchDatas();
        }

        private void DisplayBranchDatas()
        {
            var branchType = cbManageType.SelectedValue.ToString();
            var branches = Branches[branchType];
            var isThereABranch = branches.Any();

            cbManageType.Enabled = true;
            cbBranches.DataSource = isThereABranch ? branches : new List<string> { "No " + branchType + " branches exist." };
            cbBranches.Enabled = isThereABranch;
            btnFinish.Enabled = isThereABranch &&(branchType != Branch.support.ToString("G"));
            btnPublish.Enabled = isThereABranch;
            btnPull.Enabled = isThereABranch;
            pnlPull.Enabled = (branchType == Branch.feature.ToString("G"));
        }

        private void LoadBaseBranches()
        {
            var branchType = cbType.SelectedValue.ToString();
            var manageBaseBranch = (branchType == Branch.feature.ToString("G") || branchType == Branch.hotfix.ToString("G"));
            pnlBasedOn.Visible = manageBaseBranch;

            if (manageBaseBranch)
                cbBaseBranch.DataSource = GetLocalBranches();
        }
        #endregion

        #region Run GitFlow commands
        private void btnInit_Click(object sender, EventArgs e)
        {
            if(RunCommand("flow init -d"))
                Init();
        }

        private void btnStartBranch_Click(object sender, EventArgs e)
        {
            var branchType = cbType.SelectedValue.ToString();
            if (RunCommand("flow " + branchType + " start " + txtBranchName.Text + GetBaseBranch()))
            {
                txtBranchName.Text = string.Empty;
                if (cbManageType.SelectedValue.ToString() == branchType)
                {
                    Branches.Remove(branchType);
                    LoadBranches(branchType);
                }
                else
                    Branches.Remove(branchType);
            }
        }

        private string GetBaseBranch()
        {
            var branchType = cbType.SelectedValue.ToString();
            if (branchType == Branch.release.ToString("G"))
                return string.Empty;
            if (branchType == Branch.support.ToString("G"))
                return " HEAD"; //Hoping that's a revision on master (How to get the sha of the selected line in GitExtension?)
            if(!cbBasedOn.Checked)
                return string.Empty;
            return " " + cbBaseBranch.SelectedValue.ToString();
        }

        private void btnPublish_Click(object sender, EventArgs e)
        {
            RunCommand("flow feature publish " + cbBranches.SelectedValue);
        }

        private void btnPull_Click(object sender, EventArgs e)
        {
            RunCommand("flow feature pull " + cbBranches.SelectedValue + " " + cbRemote.SelectedValue);
        }

        private void btnFinish_Click(object sender, EventArgs e)
        {
            RunCommand("flow " + cbManageType.SelectedValue + " finish " + cbBranches.SelectedValue);
        }

        private bool RunCommand(string commandText, bool displayResult = true)
        {
            lblCommandResult.Text = string.Empty;
            int exitCode;
            var result = m_gitUiCommands.GitModule.RunGit(commandText, out exitCode);

            IsRefreshNeeded = true;

            _toolTipDebug.RemoveAll();
            _toolTipDebug.SetToolTip(lblDebug, "cmd: git " + commandText+"\n" + "exit code:"+exitCode);

            if (result.Length != 0 && displayResult)
            {
                if (exitCode == 0)
                {
                    lblCommandResult.Text = "Command Succeed! (see tooltip for details...)";
                    lblCommandResult.ForeColor = Color.Green;
                    ShowToolTip(lblCommandResult, result);

                }
                else
                {
                    lblCommandResult.Text = "Command failed! (see tooltip for error...)";
                    lblCommandResult.ForeColor = Color.Red;
                    ShowToolTip(lblCommandResult, "error: " + result);
                }
            }
            return exitCode == 0;
        }
        #endregion

        #region GUI interactions
        private void ShowToolTip(Control c, string msg)
        {
            _toolTipCommandResult.RemoveAll();
            _toolTipCommandResult.SetToolTip(c, msg);
        }

        private void cbType_SelectedValueChanged(object sender, EventArgs e)
        {
            lblPrefixName.Text = cbType.SelectedValue + "/";
            LoadBaseBranches();
        }

        private void cbManageType_SelectedValueChanged(object sender, EventArgs e)
        {
            var branchType = cbManageType.SelectedValue.ToString();
            lblPrefixManage.Text = branchType + "/";
            if (!string.IsNullOrWhiteSpace(branchType))
            {
                pnlManageBranch.Enabled = true;
                LoadBranches(branchType);
            }
            else
            {
                pnlManageBranch.Enabled = false;
            }
        }
        #endregion

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void cbBasedOn_CheckedChanged(object sender, EventArgs e)
        {
            cbBaseBranch.Enabled = cbBasedOn.Checked;
        }

        private void lnkGitFlow_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/nvie/gitflow");
        }
    }
}
