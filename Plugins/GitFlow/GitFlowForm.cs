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

        private bool IsGitFlowInitialised
        {
            get
            {
                var args = new GitArgumentBuilder("config")
                {
                    "--get",
                    "gitflow.branch.master"
                };
                return !string.IsNullOrWhiteSpace(_gitUiCommands.GitModule.GitExecutable.GetOutput(args));
            }
        }

        public GitFlowForm(GitUIEventArgs gitUiCommands)
        {
            InitializeComponent();
            InitializeComplete();

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
            var isInitialised = IsGitFlowInitialised;

            btnInit.Visible = !isInitialised;
            gbStart.Enabled = isInitialised;
            gbManage.Enabled = isInitialised;
            lblCaptionHead.Visible = isInitialised;
            lblHead.Visible = isInitialised;

            if (isInitialised)
            {
                var remotes = _gitUiCommands.GitModule.GetRemoteNames();
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
                var startRef = branch + "/";
                if (currentRef.StartsWith(startRef))
                {
                    branchType = branch.ToString();
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
                    DisplayBranchData();
                });
            }
            else
            {
                DisplayBranchData();
            }
        }

        private IReadOnlyList<string> GetBranches(string typeBranch)
        {
            var args = new GitArgumentBuilder("flow") { typeBranch };
            var result = _gitUiCommands.GitModule.GitExecutable.Execute(args);

            if (result.ExitCode != 0 || result.StandardOutput == null)
            {
                return Array.Empty<string>();
            }

            return result.StandardOutput
                .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim('*', ' ', '\n', '\r'))
                .ToList();
        }

        private void DisplayBranchData()
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
            cbPushAfterFinish.Enabled = isThereABranch && (branchType == Branch.hotfix.ToString("G") || branchType == Branch.release.ToString("G"));
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

            List<string> GetLocalBranches()
            {
                var args = new GitArgumentBuilder("branch");
                return _gitUiCommands.GitModule
                    .GitExecutable.GetOutput(args)
                    .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim('*', ' ', '\n', '\r'))
                    .ToList();
            }
        }

        #endregion

        #region Run GitFlow commands
        private void btnInit_Click(object sender, EventArgs e)
        {
            var args = new GitArgumentBuilder("flow")
            {
                "init",
                "-d"
            };
            if (RunCommand(args))
            {
                Init();
            }
        }

        private void btnStartBranch_Click(object sender, EventArgs e)
        {
            var branchType = cbType.SelectedValue.ToString();
            var args = new GitArgumentBuilder("flow")
            {
                branchType,
                "start",
                txtBranchName.Text,
                GetBaseBranch()
            };
            if (RunCommand(args))
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
            var args = new GitArgumentBuilder("flow")
            {
                cbManageType.SelectedValue.ToString(),
                "publish",
                cbBranches.SelectedValue.ToString()
            };
            RunCommand(args);
        }

        private void btnPull_Click(object sender, EventArgs e)
        {
            var args = new GitArgumentBuilder("flow")
            {
                cbManageType.SelectedValue.ToString(),
                "pull",
                cbRemote.SelectedValue.ToString(),
                cbBranches.SelectedValue.ToString()
            };
            RunCommand(args);
        }

        private void btnFinish_Click(object sender, EventArgs e)
        {
            var args = new GitArgumentBuilder("flow")
            {
                cbManageType.SelectedValue.ToString(),
                "finish",
                { cbPushAfterFinish.Checked, "-p" },
                cbBranches.SelectedValue.ToString()
            };
            RunCommand(args);
        }

        private bool RunCommand(ArgumentString commandText)
        {
            pbResultCommand.Image = DpiUtil.Scale(Resource.StatusHourglass);
            ShowToolTip(pbResultCommand, "running command : git " + commandText);
            ForceRefresh(pbResultCommand);
            lblRunCommand.Text = "git " + commandText;
            ForceRefresh(lblRunCommand);
            txtResult.Text = "running...";
            ForceRefresh(txtResult);

            var result = _gitUiCommands.GitModule.GitExecutable.Execute(commandText);

            IsRefreshNeeded = true;

            ttDebug.RemoveAll();
            ttDebug.SetToolTip(lblDebug, "cmd: git " + commandText + "\n" + "exit code:" + result.ExitCode);

            var resultText = Regex.Replace(result.AllOutput, @"\r\n?|\n", Environment.NewLine);

            if (result.ExitCode == 0)
            {
                pbResultCommand.Image = DpiUtil.Scale(Resource.success);
                ShowToolTip(pbResultCommand, resultText);
                DisplayHead();
            }
            else
            {
                pbResultCommand.Image = DpiUtil.Scale(Resource.error);
                ShowToolTip(pbResultCommand, "error: " + resultText);
            }

            txtResult.Text = resultText;

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
            var args = new GitArgumentBuilder("symbolic-ref") { "HEAD" };
            var head = _gitUiCommands.GitModule.GitExecutable.GetOutput(args).Trim('*', ' ', '\n', '\r');
            lblHead.Text = head;

            var currentRef = head.RemovePrefix(GitRefName.RefsHeadsPrefix);

            if (TryExtractBranchFromHead(currentRef, out var branchTypes, out var branchName))
            {
                cbManageType.SelectedItem = branchTypes;
                CurrentBranch = branchName;
            }
        }
    }
}
