using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitFlow.Properties;
using GitUIPluginInterfaces;

namespace GitFlow
{
    public partial class GitFlowForm : Form
    {
        GitUIBaseEventArgs m_gitUiCommands;

        Dictionary<string,List<string>> Branches { get; set; }

        readonly AsyncLoader _task = new AsyncLoader();

        public bool IsRefreshNeeded { get; set; }
        private const string refHeads = "refs/heads/";

        private string CurrentBranch { get; set; }

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
            get { return !string.IsNullOrWhiteSpace(m_gitUiCommands.GitModule.RunGitCmd("config --get gitflow.branch.master")); }
        }

        public GitFlowForm(GitUIBaseEventArgs gitUiCommands)
        {
            InitializeComponent();

            m_gitUiCommands = gitUiCommands;

            Branches = new Dictionary<string, List<string>>();

            lblPrefixManage.Text = string.Empty;
            ttGitFlow.SetToolTip(lnkGitFlow, "A good branch model for your project with Git...");

            Init();
        }

        private void Init()
        {
            var isGitFlowInited = IsGitFlowInited;

            btnInit.Visible = !isGitFlowInited;
            gbStart.Enabled = isGitFlowInited;
            gbManage.Enabled = isGitFlowInited;
            lblCaptionHead.Visible = isGitFlowInited;
            lblHead.Visible = isGitFlowInited;

            if (isGitFlowInited)
            {
                var remotes = m_gitUiCommands.GitModule.GetRemotes(true).Where(r => !string.IsNullOrWhiteSpace(r)).ToList();
                cbRemote.DataSource = remotes;
                btnPull.Enabled = btnPublish.Enabled = remotes.Any();

                cbType.DataSource = BranchTypes;
                var types = new List<string> { string.Empty };
                types.AddRange(BranchTypes);
                cbManageType.DataSource = types;

                cbBasedOn.Checked = false;
                cbBaseBranch.Enabled  = false;
                LoadBaseBranches();

                DisplayHead();
            }
        }

        private bool TryExtractBranchFromHead(string currentRef, out string branchType, out string branchName)
        {
            foreach (Branch branch in Enum.GetValues(typeof(Branch)))
            {
                var startRef = branch.ToString("G") + "/";
                if (currentRef.StartsWith(startRef))
                {
                    branchType = branch.ToString("G");
                    branchName = currentRef.Substring(startRef.Length);
                    return true;
                }
            }
            branchType = null;
            branchName = null;
            return false;
        }

        #region Loading Branches
        private void LoadBranches(string branchType)
        {
            cbManageType.Enabled = false;
            cbBranches.DataSource = new List<string> {"Loading..."};
            if (!Branches.ContainsKey(branchType))
                _task.Load(() => GetBranches(branchType), (branches) => { Branches.Add(branchType, branches); DisplayBranchDatas(); }); 
            else
                DisplayBranchDatas();
        }

        private List<string> GetBranches(string typeBranch)
        {
            string[] references = m_gitUiCommands.GitModule.RunGitCmd("flow " + typeBranch)
                                                 .Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);

            if (references.Length == 0 || references.Any(l=>l.StartsWith("No " + typeBranch + " branches exist.")))
                return new List<string>();

            return references.Select(e => e.Trim('*', ' ', '\n', '\r')).ToList();
        }

        private List<string> GetLocalBranches()
        {
            string[] references = m_gitUiCommands.GitModule.RunGitCmd("branch")
                                                 .Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);

            return references.Select(e => e.Trim('*', ' ', '\n', '\r')).ToList();
        }

        private void DisplayBranchDatas()
        {
            var branchType = cbManageType.SelectedValue.ToString();
            var branches = Branches[branchType];
            var isThereABranch = branches.Any();

            cbManageType.Enabled = true;
            cbBranches.DataSource = isThereABranch ? branches : new List<string> { "No " + branchType + " branches exist." };
            cbBranches.Enabled = isThereABranch;
            if (isThereABranch && CurrentBranch != null)
            {
                cbBranches.SelectedItem = CurrentBranch;
                CurrentBranch = null;
            }

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
            return " " + cbBaseBranch.SelectedValue;
        }

        private void btnPublish_Click(object sender, EventArgs e)
        {
            RunCommand("flow feature publish " + cbBranches.SelectedValue);
        }

        private void btnPull_Click(object sender, EventArgs e)
        {
            RunCommand("flow feature pull " + cbRemote.SelectedValue + " " + cbBranches.SelectedValue);
        }

        private void btnFinish_Click(object sender, EventArgs e)
        {
            RunCommand("flow " + cbManageType.SelectedValue + " finish " + cbBranches.SelectedValue);
        }

        private bool RunCommand(string commandText)
        {
            int exitCode;
            pbResultCommand.Image = Resource.StatusHourglass;
            ShowToolTip(pbResultCommand, "running command : git " + commandText);
            ForceRefresh(pbResultCommand);
            lblRunCommand.Text = "git " + commandText;
            ForceRefresh(lblRunCommand);
            txtResult.Text = "running...";
            ForceRefresh(txtResult);

            var result = m_gitUiCommands.GitModule.RunGitCmd(commandText, out exitCode).Trim().Replace("\n", Environment.NewLine);

            IsRefreshNeeded = true;

            ttDebug.RemoveAll();
            ttDebug.SetToolTip(lblDebug, "cmd: git " + commandText + "\n" + "exit code:" + exitCode);

            if (exitCode == 0)
            {
                pbResultCommand.Image = Resource.success;
                ShowToolTip(pbResultCommand, result);
                DisplayHead();
                txtResult.Text = result;
            }
            else
            {
                pbResultCommand.Image = Resource.error;
                ShowToolTip(pbResultCommand, "error: " + result);
                txtResult.Text = result;
            }
            return exitCode == 0;
        }
        #endregion

        #region GUI interactions
        private void ForceRefresh(Control c)
        {
            c.Invalidate();
            c.Update();
            c.Refresh();
        }

        private void ShowToolTip(Control c, string msg)
        {
            ttCommandResult.RemoveAll();
            ttCommandResult.SetToolTip(c, msg);
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

        private void DisplayHead()
        {
            var head = m_gitUiCommands.GitModule.RunGitCmd("symbolic-ref HEAD").Trim('*', ' ', '\n', '\r');
            lblHead.Text = head;
            var currentRef = head.StartsWith(refHeads) ? head.Substring(refHeads.Length) : head;

            string branchTypes;
            string branchName;
            if (TryExtractBranchFromHead(currentRef, out branchTypes, out branchName))
            {
                cbManageType.SelectedItem = branchTypes;
                CurrentBranch = branchName;
            }
        }
    }
}
