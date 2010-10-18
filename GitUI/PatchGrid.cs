using System;
using System.Windows.Forms;
using GitCommands;
using PatchApply;

namespace GitUI
{
    public partial class PatchGrid : GitExtensionsControl
    {
        public PatchGrid()
        {
            InitializeComponent(); Translate();
            Patches.CellPainting += new DataGridViewCellPaintingEventHandler(Patches_CellPainting);
        }

        void Patches_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void PatchGrid_Load(object sender, EventArgs e)
        {
            Initialize();
        }

        public void Initialize()
        {
            Patches.DataSource = GitCommandHelpers.GetRebasePatchFiles();
        }

        private void Patches_DoubleClick(object sender, EventArgs e)
        {
            if (Patches.SelectedRows.Count != 1) return;

            PatchFile patchFile = (PatchFile)Patches.SelectedRows[0].DataBoundItem;

            ViewPatch viewPatch = new ViewPatch();
            viewPatch.LoadPatch(patchFile.FullName);
            viewPatch.ShowDialog();
        }
    }
}
