using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace PatchApply
{
    public partial class ApplyPatch : Form
    {
        public ApplyPatch()
        {
            InitializeComponent();

            patchManager = new PatchManager();
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
            FileToPatchEdit.Text = "";
            PatchedFileEdit.Text = "";

            try
            {
                PatchedFileEdit.Text = patch.FileTextB;

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
                PatchedFileEdit.SetHighlighting(syntax);
                PatchedFileEdit.IsIconBarVisible = true;

                PatchedFileEdit.Document.BookmarkManager.Clear();
                foreach (int n in patch.BookMarks)
                {
                    ICSharpCode.TextEditor.Document.Bookmark b = new ICSharpCode.TextEditor.Document.Bookmark(PatchedFileEdit.Document, new ICSharpCode.TextEditor.TextLocation(0, n));
                    PatchedFileEdit.Document.BookmarkManager.AddMark(b);
                }
                PatchedFileEdit.Refresh();

                FileToPatchEdit.SetHighlighting(syntax);
                FileToPatchEdit.LoadFile(patchManager.DirToPatch + patch.FileNameA, true, true);
                FileToPatchEdit.SetHighlighting(syntax);
                FileToPatchEdit.Refresh();
                FileToPatchEdit.IsReadOnly = true;
            }
            catch
            {
                FileToPatchEdit.Text = "";
                FileToPatchEdit.Refresh();
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
                patchManager.DirToPatch = ApplyToDirEdit.Text;

                if (patchManager.DirToPatch[patchManager.DirToPatch.Length-1] != '\\')
                {
                    patchManager.DirToPatch += @"\";
                }

                patchManager.LoadPatchFile();

                GridChangedFiles.DataSource = patchManager.patches;
            }
            catch
            {
            }
        }

        private void Clear()
        {
            CurrentPatch = null;

            patchManager = new PatchManager();

            GridChangedFiles.DataSource = null;

            PatchedFileEdit.Text = "";
            PatchedFileEdit.Refresh();
            
            FileToPatchEdit.Text = "";
            FileToPatchEdit.Refresh();

            ChangesList.Text = "";
        }

        private void PatchedFileEdit_Leave(object sender, EventArgs e)
        {
            if (CurrentPatch == null) return;
            CurrentPatch.FileTextB = PatchedFileEdit.Text;

        }

        private void PatchedFileEdit_Scroll(object sender, ScrollEventArgs e)
        {
        }

        private void PatchedFileEdit_MouseMove(object sender, MouseEventArgs e)
        {


        }

        private void PatchedFileEdit_Click(object sender, EventArgs e)
        {

        }

        private void ApplyToDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
                ApplyToDirEdit.Text = dialog.SelectedPath;
        }

        private void ApplyToDirEdit_TextChanged(object sender, EventArgs e)
        {

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (GridChangedFiles.SelectedRows.Count == 0) return;

            for (int n = GridChangedFiles.SelectedRows[0].Index+1; n < GridChangedFiles.RowCount; n++)
            {
                if (GridChangedFiles.Rows[n].DataBoundItem is Patch)
                {
                    Patch patch = (Patch)GridChangedFiles.Rows[n].DataBoundItem;
                    if (patch.Rate < 100)
                    {
                        GridChangedFiles.SelectedRows[0].Selected = false;
                        GridChangedFiles.Rows[n].Selected = true;
                        return;
                    }
                }
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (GridChangedFiles.SelectedRows.Count == 0) return;

            for (int n = GridChangedFiles.SelectedRows[0].Index - 1; n >= 0; n--)
            {
                if (GridChangedFiles.Rows[n].DataBoundItem is Patch)
                {
                    Patch patch = (Patch)GridChangedFiles.Rows[n].DataBoundItem;
                    if (patch.Rate < 100)
                    {
                        GridChangedFiles.SelectedRows[0].Selected = false;
                        GridChangedFiles.Rows[n].Selected = true;
                        return;
                    }
                }
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to apply this patch?", "Save", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                patchManager.SavePatch();
                MessageBox.Show("Patch applied!");
                Clear();
            }
        }

        private void ApplyPatch_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (patchManager.patches.Count > 0)
            {
                DialogResult result = MessageBox.Show("There is a patch loaded. Do you want to apply this patch before exit?", "Close", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Yes)
                {
                    patchManager.SavePatch();
                    MessageBox.Show("Patch applied!");
                    Clear();
                }
                else
                    if (result == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                    }
            }
        }
    }
}
