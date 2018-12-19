using System;
using System.Linq;
using System.Windows.Forms;
using GitCommands.Patches;
using GitExtUtils.GitUI;
using ResourceManager;

namespace GitUI
{
    public partial class PatchGrid : GitModuleControl
    {
        private readonly TranslationString _unableToShowPatchDetails = new TranslationString("Unable to show details of patch file.");

        public PatchGrid()
        {
            InitializeComponent();
            InitializeComplete();
            FileName.DataPropertyName = nameof(PatchFile.Name);
            subjectDataGridViewTextBoxColumn.DataPropertyName = nameof(PatchFile.Subject);
            authorDataGridViewTextBoxColumn.DataPropertyName = nameof(PatchFile.Author);
            dateDataGridViewTextBoxColumn.DataPropertyName = nameof(PatchFile.Date);
            Status.DataPropertyName = nameof(PatchFile.Status);

            FileName.Width = DpiUtil.Scale(50);
            authorDataGridViewTextBoxColumn.Width = DpiUtil.Scale(140);
            dateDataGridViewTextBoxColumn.Width = DpiUtil.Scale(160);
            Status.Width = DpiUtil.Scale(80);
        }

        protected override void OnRuntimeLoad()
        {
            Initialize();
        }

        public void Initialize()
        {
            var patchFiles = Module.InTheMiddleOfInteractiveRebase()
                ? Module.GetInteractiveRebasePatchFiles()
                : Module.GetRebasePatchFiles();

            Patches.DataSource = patchFiles;

            if (patchFiles.Any())
            {
                int rowsInView = Patches.DisplayedRowCount(false);
                int currentPatchFileIndex = patchFiles.TakeWhile(pf => !pf.IsNext).Count() - 1;
                Patches.FirstDisplayedScrollingRowIndex = Math.Max(0, currentPatchFileIndex - (rowsInView / 2));
            }
        }

        private void Patches_DoubleClick(object sender, EventArgs e)
        {
            if (Patches.SelectedRows.Count != 1)
            {
                return;
            }

            var patchFile = (PatchFile)Patches.SelectedRows[0].DataBoundItem;

            if (string.IsNullOrEmpty(patchFile.FullName))
            {
                MessageBox.Show(_unableToShowPatchDetails.Text);
                return;
            }

            UICommands.StartViewPatchDialog(patchFile.FullName);
        }
    }
}
