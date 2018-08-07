using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormApplyPatch : GitModuleForm
    {
        #region Translation

        private readonly TranslationString _conflictResolvedText =
            new TranslationString("Conflicts resolved");
        private readonly TranslationString _conflictMergetoolText =
            new TranslationString("Solve conflicts");

        private readonly TranslationString _selectPatchFileFilter =
            new TranslationString("Patch file (*.Patch)");
        private readonly TranslationString _selectPatchFileCaption =
            new TranslationString("Select patch file");

        private readonly TranslationString _noFileSelectedText =
            new TranslationString("Please select a patch to apply");

        private readonly TranslationString _applyPatchMsgBox =
            new TranslationString("Apply patch");

        #endregion

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormApplyPatch()
        {
            InitializeComponent();
        }

        public FormApplyPatch(GitUICommands commands)
            : base(commands)
        {
            InitializeComponent();
            InitializeComplete();
            EnableButtons();
        }

        public void SetPatchFile(string name)
        {
            PatchFileMode.Checked = true;
            PatchFile.Text = name;
        }

        public void SetPatchDir(string name)
        {
            PatchDirMode.Checked = true;
            PatchDir.Text = name;
        }

        private void EnableButtons()
        {
            if (Module.InTheMiddleOfPatch())
            {
                Apply.Enabled = false;
                IgnoreWhitespace.Enabled = false;
                PatchFileMode.Enabled = false;
                PatchDirMode.Enabled = false;
                AddFiles.Enabled = true;
                Resolved.Enabled = !Module.InTheMiddleOfConflictedMerge();
                Mergetool.Enabled = Module.InTheMiddleOfConflictedMerge();
                Skip.Enabled = true;
                Abort.Enabled = true;

                PatchFile.Enabled = false;
                PatchFile.ReadOnly = false;
                BrowsePatch.Enabled = false;
                PatchDir.Enabled = false;
                PatchDir.ReadOnly = false;
                BrowseDir.Enabled = false;
            }
            else
            {
                PatchFile.Enabled = PatchFileMode.Checked;
                PatchFile.ReadOnly = !PatchFileMode.Checked;
                BrowsePatch.Enabled = PatchFileMode.Checked;
                PatchDir.Enabled = PatchDirMode.Checked;
                PatchDir.ReadOnly = !PatchDirMode.Checked;
                BrowseDir.Enabled = PatchDirMode.Checked;

                Apply.Enabled = true;
                IgnoreWhitespace.Enabled = true;
                PatchFileMode.Enabled = true;
                PatchDirMode.Enabled = true;
                AddFiles.Enabled = false;
                Resolved.Enabled = false;
                Mergetool.Enabled = false;
                Skip.Enabled = false;
                Abort.Enabled = false;
            }

            patchGrid1.Initialize();

            SolveMergeConflicts.Visible = Module.InTheMiddleOfConflictedMerge();

            Resolved.Text = _conflictResolvedText.Text;
            Mergetool.Text = _conflictMergetoolText.Text;
            ContinuePanel.BackColor = Color.Transparent;
            MergeToolPanel.BackColor = Color.Transparent;

            if (Module.InTheMiddleOfConflictedMerge())
            {
                Mergetool.Text = ">" + _conflictMergetoolText.Text + "<";
                Mergetool.Focus();
                AcceptButton = Mergetool;
                MergeToolPanel.BackColor = Color.Black;
            }
            else if (Module.InTheMiddleOfPatch())
            {
                Resolved.Text = ">" + _conflictResolvedText.Text + "<";
                Resolved.Focus();
                AcceptButton = Resolved;
                ContinuePanel.BackColor = Color.Black;
            }
        }

        private string SelectPatchFile(string initialDirectory)
        {
            using (var dialog = new OpenFileDialog
            {
                Filter = _selectPatchFileFilter.Text + "|*.patch",
                InitialDirectory = initialDirectory,
                Title = _selectPatchFileCaption.Text
            })
            {
                return (dialog.ShowDialog(this) == DialogResult.OK) ? dialog.FileName : PatchFile.Text;
            }
        }

        private void BrowsePatch_Click(object sender, EventArgs e)
        {
            PatchFile.Text = SelectPatchFile(@".");
        }

        private void Apply_Click(object sender, EventArgs e)
        {
            var patchFile = PatchFile.Text;
            var dirText = PatchDir.Text;
            var ignoreWhiteSpace = IgnoreWhitespace.Checked;

            if (string.IsNullOrEmpty(patchFile) && string.IsNullOrEmpty(dirText))
            {
                MessageBox.Show(this, _noFileSelectedText.Text);
                return;
            }

            using (WaitCursorScope.Enter())
            {
                if (PatchFileMode.Checked)
                {
                    var arguments = IsDiffFile(patchFile)
                        ? GitCommandHelpers.ApplyDiffPatchCmd(ignoreWhiteSpace, patchFile)
                        : GitCommandHelpers.ApplyMailboxPatchCmd(ignoreWhiteSpace, patchFile);

                    FormProcess.ShowDialog(this, arguments);
                }
                else
                {
                    var arguments = GitCommandHelpers.ApplyMailboxPatchCmd(ignoreWhiteSpace);

                    Module.ApplyPatch(dirText, arguments);
                }

                UICommands.RepoChangedNotifier.Notify();

                EnableButtons();

                if (!Module.InTheMiddleOfAction() && !Module.InTheMiddleOfPatch())
                {
                    Close();
                }
            }
        }

        // look into patch file and try to figure out if it's a raw diff (i.e from git diff -p)
        // only looks at start, as all we want is to tell from automail format
        // returns false on any problem, never throws
        private static bool IsDiffFile(string path)
        {
            try
            {
                using (var sr = new StreamReader(path))
                {
                    string line = sr.ReadLine();

                    return line != null && (line.StartsWith("diff ") || line.StartsWith("Index: "));
                }
            }
            catch
            {
                return false;
            }
        }

        private void Mergetool_Click(object sender, EventArgs e)
        {
            UICommands.StartResolveConflictsDialog(this);
            EnableButtons();
        }

        private void Skip_Click(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                FormProcess.ShowDialog(this, GitCommandHelpers.SkipCmd());
                EnableButtons();
            }
        }

        private void Resolved_Click(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                FormProcess.ShowDialog(this, GitCommandHelpers.ResolvedCmd());
                EnableButtons();
            }
        }

        private void Abort_Click(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                FormProcess.ShowDialog(this, GitCommandHelpers.AbortCmd());
                EnableButtons();
            }
        }

        private void AddFiles_Click(object sender, EventArgs e)
        {
            UICommands.StartAddFilesDialog(this);
        }

        private void MergePatch_Load(object sender, EventArgs e)
        {
            PatchFile.Select();

            Text = _applyPatchMsgBox.Text + " (" + Module.WorkingDir + ")";
            IgnoreWhitespace.Checked = AppSettings.ApplyPatchIgnoreWhitespace;
        }

        private void BrowseDir_Click(object sender, EventArgs e)
        {
            var userSelectedPath = OsShellUtil.PickFolder(this);

            if (userSelectedPath != null)
            {
                PatchDir.Text = userSelectedPath;
            }
        }

        private void PatchFileMode_CheckedChanged(object sender, EventArgs e)
        {
            EnableButtons();
        }

        private void SolveMergeConflicts_Click(object sender, EventArgs e)
        {
            Mergetool_Click(sender, e);
        }

        private void IgnoreWhitespace_CheckedChanged(object sender, EventArgs e)
        {
            AppSettings.ApplyPatchIgnoreWhitespace = IgnoreWhitespace.Checked;
        }
    }
}
