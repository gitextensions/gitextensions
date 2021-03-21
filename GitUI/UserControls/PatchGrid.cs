using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands.Patches;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using Microsoft;
using ResourceManager;

namespace GitUI
{
    public partial class PatchGrid : GitModuleControl
    {
        private readonly TranslationString _unableToShowPatchDetails = new("Unable to show details of patch file.");

        private List<PatchFile>? _skipped;
        private bool _isManagingRebase;

        public IReadOnlyList<PatchFile>? PatchFiles { get; private set; }

        public PatchGrid()
        {
            InitializeComponent();
            InitializeComplete();
            FileName.DataPropertyName = nameof(PatchFile.Name);
            subjectDataGridViewTextBoxColumn.DataPropertyName = nameof(PatchFile.Subject);
            authorDataGridViewTextBoxColumn.DataPropertyName = nameof(PatchFile.Author);
            dateDataGridViewTextBoxColumn.DataPropertyName = nameof(PatchFile.Date);
            Status.DataPropertyName = nameof(PatchFile.Status);

            Status.Width = DpiUtil.Scale(70);
            FileName.Width = DpiUtil.Scale(50);
            CommitHash.Width = DpiUtil.Scale(55);
            authorDataGridViewTextBoxColumn.Width = DpiUtil.Scale(140);
            Patches.RowTemplate.MinimumHeight = Patches.ColumnHeadersHeight;
            UpdateState(IsManagingRebase);
        }

        private void UpdateState(bool isManagingRebase)
        {
            Action.Visible = isManagingRebase;
            FileName.Visible = !isManagingRebase;
            CommitHash.Visible = isManagingRebase;
            dateDataGridViewTextBoxColumn.Width = isManagingRebase ? DpiUtil.Scale(110) : DpiUtil.Scale(160);
        }

        [Category("Behavior"), Description("Should it be used to display commit to rebase (otherwise patches)."), DefaultValue(true)]
        public bool IsManagingRebase
        {
            get => _isManagingRebase;
            set
            {
                _isManagingRebase = value;
                UpdateState(value);
            }
        }

        protected override void OnRuntimeLoad()
        {
            Initialize();
        }

        public void RefreshGrid()
        {
            Validates.NotNull(PatchFiles);

            var updatedPatches = GetPatches();

            for (int i = 0; i < updatedPatches.Count; i++)
            {
                updatedPatches[i].IsSkipped = PatchFiles[i].IsSkipped;
            }

            DisplayPatches(updatedPatches);
        }

        private IReadOnlyList<PatchFile> GetPatches()
        {
            var patches = Module.InTheMiddleOfInteractiveRebase()
                            ? Module.GetInteractiveRebasePatchFiles()
                            : Module.GetRebasePatchFiles();

            if (!_skipped.Any())
            {
                return patches;
            }

            // Select commits with `ObjectId` and patches with `Name`
            var skippedPatches = patches
                .TakeWhile(p => !p.IsNext)
                .Where(p => _skipped.Any(s => p.ObjectId == s.ObjectId && p.Name == s.Name));
            foreach (var patchFile in skippedPatches)
            {
                patchFile.IsSkipped = true;
            }

            return patches;
        }

        public void Initialize()
        {
            if (DesignMode)
            {
                return;
            }

            DisplayPatches(GetPatches());
        }

        private void DisplayPatches(IReadOnlyList<PatchFile> patchFiles)
        {
            PatchFiles = patchFiles;
            Patches.DataSource = patchFiles;

            if (patchFiles.Any())
            {
                int rowsInView = Patches.DisplayedRowCount(false);
                int currentPatchFileIndex = patchFiles.TakeWhile(pf => !pf.IsNext).Count() - 1;
                Patches.FirstDisplayedScrollingRowIndex = Math.Max(0, currentPatchFileIndex - (rowsInView / 2));
            }

            SelectCurrentlyApplyingPatch();
        }

        private void Patches_DoubleClick(object sender, EventArgs e)
        {
            if (Patches.SelectedRows.Count != 1)
            {
                return;
            }

            var patchFile = (PatchFile)Patches.SelectedRows[0].DataBoundItem;

            if (patchFile.ObjectId is not null && !patchFile.ObjectId.IsArtificial)
            {
                UICommands.StartFormCommitDiff(patchFile.ObjectId);
                return;
            }

            if (string.IsNullOrEmpty(patchFile.FullName))
            {
                MessageBox.Show(_unableToShowPatchDetails.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            UICommands.StartViewPatchDialog(patchFile.FullName);
        }

        public void SelectCurrentlyApplyingPatch()
        {
            if (PatchFiles is null || !PatchFiles.Any())
            {
                return;
            }

            var shouldSelectIndex = PatchFiles.IndexOf(p => p.IsNext);

            if (shouldSelectIndex >= 0)
            {
                Patches.ClearSelection();
                DataGridViewRow dataGridViewRow = Patches.Rows[shouldSelectIndex];
                dataGridViewRow.DefaultCellStyle.ForeColor = Color.OrangeRed.AdaptTextColor();
                dataGridViewRow.Selected = true;
            }
        }

        public void SetSkipped(List<PatchFile> skipped)
        {
            _skipped = skipped;
        }
    }
}
