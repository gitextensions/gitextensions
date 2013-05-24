using System;
using System.Windows.Forms;
using PatchApply;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class PatchGrid : GitModuleControl
    {
        private TranslationString _unableToShowPatchDetails = new TranslationString("Unable to show details of patch file.");

        public PatchGrid()            
        {
            InitializeComponent(); Translate();
            Patches.CellPainting += Patches_CellPainting;
        }

        static void Patches_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {

        }

        private static void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        protected override void OnRuntimeLoad( EventArgs e)
        {
            Initialize();
        }

        public void Initialize()
        {
            if (Module.InTheMiddleOfInteractiveRebase())
                Patches.DataSource = Module.GetInteractiveRebasePatchFiles();
            else
                Patches.DataSource = Module.GetRebasePatchFiles();
        }

        private void Patches_DoubleClick(object sender, EventArgs e)
        {
            if (Patches.SelectedRows.Count != 1) return;

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
