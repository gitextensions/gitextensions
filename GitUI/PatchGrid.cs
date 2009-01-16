using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using PatchApply;

namespace GitUI
{
    public partial class PatchGrid : UserControl
    {
        public PatchGrid()
        {
            InitializeComponent();
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
            Patches.DataSource = GitCommands.GitCommands.GetRebasePatchFiles();
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
