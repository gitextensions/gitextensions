using System;
using System.Windows.Forms;
using GitUI;

namespace PatchApply
{
    public partial class ViewPatch : GitExtensionsForm
    {
        public ViewPatch()
        {
            InitializeComponent(); Translate();

            PatchManager = new PatchManager();
        }

        public void LoadPatch(string patch)
        {
            PatchFileNameEdit.Text = patch;
            LoadButton_Click(null, null);
        }

        public PatchManager PatchManager { get; set; }
        public Patch CurrentPatch { get; set; }

        private void GridChangedFiles_SelectionChanged(object sender, EventArgs e)
        {
            if (GridChangedFiles.SelectedRows.Count == 0) return;

            var patch = (Patch)GridChangedFiles.SelectedRows[0].DataBoundItem;
            CurrentPatch = patch;

            if (patch == null) return;

            ChangesList.ViewPatch(patch.Text);
            /*
            ChangesList.Text = "";

            try
            {
                ChangesList.SetHighlighting("Patch");
                ChangesList.Refresh();
                ChangesList.IsReadOnly = true;
            }
            catch
            {
                ChangesList.Text = "";
                ChangesList.Refresh();
            }
            ChangesList.Text = patch.Text;
            //PatchedFileEdit.Text = changedFile.New;*/

        }

        private string SelectPatchFile(string initialDirectory)
        {
            var dialog = new OpenFileDialog
                             {
                                 Filter = "Patch file (*.Patch)|*.Patch",
                                 InitialDirectory = initialDirectory,
                                 Title = "Select patch file"
                             };
            return (dialog.ShowDialog() == DialogResult.OK) ? dialog.FileName : PatchFileNameEdit.Text;
        }

        private void BrowsePatch_Click(object sender, EventArgs e)
        {
            PatchFileNameEdit.Text = SelectPatchFile(@".");
            LoadButton_Click(sender, e);
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            try
            {
                PatchManager.PatchFileName = PatchFileNameEdit.Text;
                PatchManager.LoadPatchFile(false);

                GridChangedFiles.DataSource = PatchManager.patches;
            }
            catch
            {
            }
        }

        private static void PatchFileNameEdit_TextChanged(object sender, EventArgs e)
        {

        }

        private static void ViewPatch_Load(object sender, EventArgs e)
        {

        }
    }
}
