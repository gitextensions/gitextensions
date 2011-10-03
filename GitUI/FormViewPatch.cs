﻿using System;
using System.Windows.Forms;
using GitUI;
using ResourceManager.Translation;

namespace PatchApply
{
    public partial class ViewPatch : GitExtensionsForm
    {
        private readonly TranslationString _patchFileFilterString =
            new TranslationString("Patch file (*.Patch)");

        private readonly TranslationString _patchFileFilterTitle =
            new TranslationString("Select patch file");

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
                                 Filter = _patchFileFilterString.Text + "|*.Patch",
                                 InitialDirectory = initialDirectory,
                                 Title = _patchFileFilterTitle.Text
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

                GridChangedFiles.DataSource = PatchManager.Patches;
            }
            catch
            {
            }
        }

        private void PatchFileNameEdit_TextChanged(object sender, EventArgs e)
        {

        }

        private void ViewPatch_Load(object sender, EventArgs e)
        {

        }
    }
}
