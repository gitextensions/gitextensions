using System;
using System.Windows.Forms;
using GitCommands;
using PatchApply;

namespace GitUI
{
    public partial class PatchGrid : GitModuleControl
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

        protected override void OnRuntimeLoad( EventArgs e)
        {
            Initialize();
        }

        public void Initialize()
        {
            Patches.DataSource = Module.GetRebasePatchFiles();
        }

        private void Patches_DoubleClick(object sender, EventArgs e)
        {
            if (Patches.SelectedRows.Count != 1) return;

            var patchFile = (PatchFile)Patches.SelectedRows[0].DataBoundItem;

            using (var viewPatch = new FormViewPatch(UICommands))
            {
                viewPatch.LoadPatch(patchFile.FullName);
                viewPatch.ShowDialog(this);
            }
        }
    }
}
