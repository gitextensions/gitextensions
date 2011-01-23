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
            Patches.CellPainting += Patches_CellPainting;
        }

        static void Patches_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {

        }

        private static void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
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

            var patchFile = (PatchFile)Patches.SelectedRows[0].DataBoundItem;

            var viewPatch = new ViewPatch();
            viewPatch.LoadPatch(patchFile.FullName);
            viewPatch.ShowDialog();
        }
    }
}
