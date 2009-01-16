using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;
using System.IO;
using GitCommands;
using GitUI;

namespace PatchApply
{
    public partial class ViewPatch : Form
    {
        public ViewPatch()
        {
            InitializeComponent();

            patchManager = new PatchManager();
        }

        public void LoadPatch(string patch)
        {
            PatchFileNameEdit.Text = patch;
            LoadButton_Click(null, null);
        }

        public PatchManager patchManager { get; set; }
        public Patch CurrentPatch { get; set; }

        private void GridChangedFiles_SelectionChanged(object sender, EventArgs e)
        {
            if (GridChangedFiles.SelectedRows.Count == 0) return;

            Patch patch = (Patch)GridChangedFiles.SelectedRows[0].DataBoundItem;
            CurrentPatch = patch;

            if (patch == null) return;

            ChangesList.Text = "";

            try
            {
                string syntax = "XML";
                if (patch.FileNameB.LastIndexOf('.') > 0)
                {
                    string extension = patch.FileNameB.Substring(patch.FileNameB.LastIndexOf('.') + 1).ToUpper();

                    switch (extension)
                    {
                        case "BAS":
                        case "VBS":
                        case "VB":
                            syntax = "VBNET";
                            break;
                        case "CS":
                            syntax = "C#";
                            break;
                        case "CMD":
                        case "BAT":
                            syntax = "BAT";
                            break;
                        case "C":
                        case "RC":
                        case "IDL":
                        case "H":
                        case "CPP":
                            syntax = "C#";
                            break;
                        default:
                            break;
                    }
                }

                ChangesList.SetHighlighting(syntax);
                ChangesList.Refresh();
                ChangesList.IsReadOnly = true;
            }
            catch
            {
                ChangesList.Text = "";
                ChangesList.Refresh();
            }
            ChangesList.Text = patch.Text;
            //PatchedFileEdit.Text = changedFile.New;

        }

        private string SelectPatchFile(string initialDirectory)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter =
               "Patch file (*.Patch)|*.Patch";
            dialog.InitialDirectory = initialDirectory;
            dialog.Title = "Select patch file";
            return (dialog.ShowDialog() == DialogResult.OK) ? dialog.FileName : PatchFileNameEdit.Text;
        }

        private void BrowsePatch_Click(object sender, EventArgs e)
        {
            PatchFileNameEdit.Text = SelectPatchFile(@".");
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            try
            {
                patchManager.PatchFileName = PatchFileNameEdit.Text;
                patchManager.LoadPatchFile(false);

                GridChangedFiles.DataSource = patchManager.patches;
            }
            catch
            {
            }
        }

        private void PatchFileNameEdit_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
