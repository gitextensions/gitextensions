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
        }

        private void PatchGrid_Load(object sender, EventArgs e)
        {
            Initialize();
        }

        public void Initialize()
        {
            Patches.DataSource = Settings.Module.GetRebasePatchFiles();
        }

        private void Patches_DoubleClick(object sender, EventArgs e)
        {
            if (Patches.SelectedRows.Count != 1) return;

            var patchFile = (PatchFile)Patches.SelectedRows[0].DataBoundItem;

            var viewPatch = new ViewPatch();
            viewPatch.LoadPatch(patchFile.FullName);
            viewPatch.ShowDialog(this);
        }
    }
}
