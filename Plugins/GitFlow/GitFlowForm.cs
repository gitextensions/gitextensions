using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;
using GitFlow.Properties;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitFlow
{
    public partial class GitFlowForm : GitExtensionsFormBase
    {
        private readonly TranslationString _gitFlowTooltip = new TranslationString("A good branch model for your project with Git...");
        private readonly TranslationString _loading = new TranslationString("Loading...");
        private readonly TranslationString _noBranchExist = new TranslationString("No {0} branches exist.");

        private readonly GitUIEventArgs _gitUiCommands;

        private Dictionary<string, IReadOnlyList<string>> Branches { get; } = new Dictionary<string, IReadOnlyList<string>>();

        private readonly AsyncLoader _task = new AsyncLoader();

        public bool IsRefreshNeeded { get; set; }
        private const string RefHeads = "refs/heads/";

        private string CurrentBranch { get; set; }

        private enum Branch
        {
            feature,
            bugfix,
            hotfix,
            release,
            support
        }

        private static List<string> BranchTypes
        {
            get { return Enum.GetValues(typeof(Branch)).Cast<object>().Select(e => e.ToString()).ToList(); }
        }

        private bool IsGitFlowInited => !string.IsNullOrWhiteSpace(_gitUiCommands.GitModule.RunGitCmd("config --get gitflow.branch.master"));

        public GitFlowForm(GitUIEventArgs gitUiCommands)
        {
            InitializeComponent();
            Translate();

            _gitUiCommands = gitUiCommands;

            lblPrefixManage.Text = string.Empty;
            ttGitFlow.SetToolTip(lnkGitFlow, _gitFlowTooltip.Text);

            if (_gitUiCommands != null)
            {
                Init();
            }
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
                var remotes = _gitUiCommands.GitModule.GetRemotes(true).Where(r => !string.IsNullOrWhiteSpace(r)).ToList();
                cbRemote.DataSource = remotes;
                btnPull.Enabled = btnPublish.Enabled = remotes.Any();

                cbType.DataSource = BranchTypes;
                var types = new List<string> { string.Empty };
                types.AddRange(BranchTypes);
                cbManageType.DataSource = types;

                cbBasedOn.Checked = false;
                cbBaseBranch.Enabled = false;
                LoadBaseBranches();

                DisplayHead();
            }
        }

        private static bool TryExtractBranchFromHead(string currentRef, out string branchType, out string branchName)
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
            cbBranches.DataSource = new List<string> { _loading.Text };
            if (!Branches.ContainsKey(branchType))
            {
                _task.LoadAsync(() => GetBranches(branchType), branches =>
                {
                    Branches.Add(branchType, branches);
                    DisplayBranchDatas();
                });
            }
            else
            {
                DisplayBranchDatas();
            }
        }

        private IReadOnlyList<string> GetBranches(string typeBranch)
        {
            var result = _gitUiCommands.GitModule.RunGitCmdResult("flow " + typeBranch);

            if (result.ExitCode != 0)
            {
                return Array.Empty<string>();
            }

            string[] references = result.StdOutput.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            return references.Select(e => e.Trim('*', ' ', '\n', '\r')).ToList();
        }

        private List<string> GetLocalBranches()
        {
            string[] references = _gitUiCommands.GitModule.RunGitCmd("branch")
                                                 .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            return references.Select(e => e.Trim('*', ' ', '\n', '\r')).ToList();
        }

        private void DisplayBranchDatas()
        {
            var branchType = cbManageType.SelectedValue.ToString();
            var branches = Branches[branchType];
            var isThereABranch = branches.Any();

            cbManageType.Enabled = true;
            cbBranches.DataSource = isThereABranch ? branches : new[] { string.Format(_noBranchExist.Text, branchType) };
            cbBranches.Enabled = isThereABranch;
            if (isThereABranch && CurrentBranch != null)
            {
                cbBranches.SelectedItem = CurrentBranch;
                CurrentBranch = null;
            }

            btnFinish.Enabled = isThereABranch && (branchType != Branch.support.ToString("G"));
            btnPublish.Enabled = isThereABranch && (branchType != Branch.support.ToString("G"));
            btnPull.Enabled = isThereABranch && (branchType != Branch.support.ToString("G"));
            pnlPull.Enabled = branchType != Branch.support.ToString("G");
        }

        private void LoadBaseBranches()
        {
            var branchType = cbType.SelectedValue.ToString();
            var manageBaseBranch = branchType != Branch.release.ToString("G");
            pnlBasedOn.Visible = manageBaseBranch;

            if (manageBaseBranch)
            {
                cbBaseBranch.DataSource = GetLocalBranches();
            }
        }
        #endregion

        #region Run GitFlow commands
        private void btnInit_Click(object sender, EventArgs e)
        {
            if (RunCommand("flow init -d"))
            {
                Init();
            }
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
                {
                    Branches.Remove(branchType);
                }
            }
        }

        private string GetBaseBranch()
        {
            var branchType = cbType.SelectedValue.ToString();
            if (branchType == Branch.release.ToString("G"))
            {
                return string.Empty;
            }

            if (branchType == Branch.support.ToString("G"))
            {
                return " HEAD"; // Hoping that's a revision on master (How to get the sha of the selected line in GitExtension?)
            }

            if (!cbBasedOn.Checked)
            {
                return string.Empty;
            }

            return " " + cbBaseBranch.SelectedValue;
        }

        private void btnPublish_Click(object sender, EventArgs e)
        {
            var branchType = cbManageType.SelectedValue.ToString();
            RunCommand(string.Format("flow {0} publish {1}", branchType, cbBranches.SelectedValue));
        }

        private void btnPull_Click(object sender, EventArgs e)
        {
            var branchType = cbManageType.SelectedValue.ToString();
            RunCommand(string.Format("flow {0} pull {1} {2}", branchType, cbRemote.SelectedValue, cbBranches.SelectedValue));
        }

        private void btnFinish_Click(object sender, EventArgs e)
        {
            RunCommand(string.Format("flow {0} finish {1}", cbManageType.SelectedValue, cbBranches.SelectedValue));
        }

        private bool RunCommand(string commandText)
        {
            pbResultCommand.Image = DpiUtil.Scale(Resource.StatusHourglass);
            ShowToolTip(pbResultCommand, "running command : git " + commandText);
            ForceRefresh(pbResultCommand);
            lblRunCommand.Text = "git " + commandText;
            ForceRefresh(lblRunCommand);
            txtResult.Text = "running...";
            ForceRefresh(txtResult);

            var result = _gitUiCommands.GitModule.RunGitCmdResult(commandText);

            IsRefreshNeeded = true;

            ttDebug.RemoveAll();
            ttDebug.SetToolTip(lblDebug, "cmd: git " + commandText + "\n" + "exit code:" + result.ExitCode);

            var resultText = Regex.Replace(result.GetString(), @"\r\n?|\n", Environment.NewLine);
            if (result.ExitCode == 0)
            {
                pbResultCommand.Image = DpiUtil.Scale(Resource.success);
                ShowToolTip(pbResultCommand, resultText);
                DisplayHead();
                txtResult.Text = resultText;
            }
            else
            {
                pbResultCommand.Image = DpiUtil.Scale(Resource.error);
                ShowToolTip(pbResultCommand, "error: " + resultText);
                txtResult.Text = resultText;
            }

            return result.ExitCode == 0;
        }
        #endregion

        #region GUI interactions

        private static void ForceRefresh(Control c)
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
            var head = _gitUiCommands.GitModule.RunGitCmd("symbolic-ref HEAD").Trim('*', ' ', '\n', '\r');
            lblHead.Text = head;
            var currentRef = head.StartsWith(RefHeads) ? head.Substring(RefHeads.Length) : head;

            if (TryExtractBranchFromHead(currentRef, out var branchTypes, out var branchName))
            {
                cbManageType.SelectedItem = branchTypes;
                CurrentBranch = branchName;
            }
        }
    }
}
