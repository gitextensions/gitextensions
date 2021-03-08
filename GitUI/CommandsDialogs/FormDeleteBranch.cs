using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Commands;
using GitExtUtils.GitUI;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormDeleteBranch : GitExtensionsDialog
    {
        private readonly TranslationString _deleteBranchCaption = new("Delete branches");
        private readonly TranslationString _cannotDeleteCurrentBranchMessage =
            new("Cannot delete the branch “{0}” which you are currently on.");
        private readonly TranslationString _deleteBranchConfirmTitle = new("Delete Confirmation");
        private readonly TranslationString _deleteBranchQuestion = new("Are you sure you want to delete selected branches?");
        private readonly TranslationString _useReflogHint = new("Did you know you can use reflog to restore deleted branches?");

        private readonly IEnumerable<string> _defaultBranches;
        private readonly HashSet<string> _mergedBranches = new();
        private string? _currentBranch;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private FormDeleteBranch()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            InitializeComponent();
        }

        public FormDeleteBranch(GitUICommands commands, IEnumerable<string> defaultBranches)
            : base(commands, enablePositionRestore: false)
        {
            _defaultBranches = defaultBranches;

            InitializeComponent();

            // work-around the designer bug that can't add controls to FlowLayoutPanel
            ControlsPanel.Controls.Add(Delete);
            AcceptButton = Delete;

            InitializeComplete();
        }

        protected override void OnRuntimeLoad(EventArgs e)
        {
            base.OnRuntimeLoad(e);

            Branches.BranchesToSelect = Module.GetRefs(RefsFilter.Heads).ToList();
            foreach (string branch in Module.GetMergedBranches())
            {
                if (!branch.StartsWith("* "))
                {
                    _mergedBranches.Add(branch.Trim());
                }
                else if (!branch.StartsWith("* ") || !DetachedHeadParser.IsDetachedHead(branch[2..]))
                {
                    _currentBranch = branch.Trim('*', ' ');
                }
            }

            if (_defaultBranches is not null)
            {
                Branches.SetSelectedText(_defaultBranches.Join(" "));
            }
        }

        protected override void OnShown(EventArgs e)
        {
            RecalculateSizeConstraints();
            base.OnShown(e);
            Branches.Focus();
        }

        private void RecalculateSizeConstraints()
        {
            SuspendLayout();
            MinimumSize = MaximumSize = Size.Empty;

            int height = ControlsPanel.Height + MainPanel.Padding.Top + MainPanel.Padding.Bottom
                       + tlpnlMain.Height + tlpnlMain.Margin.Top + tlpnlMain.Margin.Bottom + DpiUtil.Scale(42);

            MinimumSize = new Size(tlpnlMain.PreferredSize.Width + DpiUtil.Scale(70), height);
            MaximumSize = new Size(Screen.PrimaryScreen.Bounds.Width, height);
            Size = new Size(Width, height);
            ResumeLayout();
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            var selectedBranches = Branches.GetSelectedBranches().ToArray();
            if (!selectedBranches.Any())
            {
                return;
            }

            if (_currentBranch is not null && selectedBranches.Any(branch => branch.Name == _currentBranch))
            {
                MessageBox.Show(this, string.Format(_cannotDeleteCurrentBranchMessage.Text, _currentBranch), _deleteBranchCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!AppSettings.DontConfirmDeleteBranch)
            {
                TaskDialogPage page = new()
                {
                    Text = _deleteBranchQuestion.Text,
                    Caption = _deleteBranchConfirmTitle.Text,
                    Icon = TaskDialogIcon.Information,
                    Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
                    Footnote = _useReflogHint.Text,
                    Verification = new(TranslatedStrings.DontShowAgain),
                    SizeToContent = true
                };

                bool messageBoxResult = TaskDialog.ShowDialog(Handle, page) == TaskDialogButton.Yes;
                if (messageBoxResult)
                {
                    AppSettings.DontConfirmDeleteBranch = page.Verification.Checked;
                }
            }

            GitDeleteBranchCmd cmd = new(selectedBranches, force: true);
            bool success = UICommands.StartCommandLineProcessDialog(Owner, cmd);
            if (success)
            {
                Close();
            }
        }
    }
}
