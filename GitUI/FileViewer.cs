using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;
using GitCommands;

namespace GitUI
{
    public partial class FileViewer : UserControl
    {
        public bool IgnoreWhitespaceChanges { get; set; }
        public int NumberOfVisibleLines { get; set; }
        public bool ShowEntireFile { get; set; }
        public bool TreatAllFilesAsText { get; set; }

        public event EventHandler<EventArgs> ExtraDiffArgumentsChanged;

        private void EnableDiffContextMenu(bool enable)
        {
            ignoreWhitespaceChangesToolStripMenuItem.Enabled = enable;
            increaseNumberOfLinesToolStripMenuItem.Enabled = enable;
            descreaseNumberOfLinesToolStripMenuItem.Enabled = enable;
            showEntireFileToolStripMenuItem.Enabled = enable;
            treatAllFilesAsTextToolStripMenuItem.Enabled = enable;
        }

        private void OnExtraDiffArgumentsChanged()
        {
            if (ExtraDiffArgumentsChanged != null)
                ExtraDiffArgumentsChanged(this, new EventArgs());
        }

        public string GetExtraDiffArguments()
        {
            StringBuilder diffArguments = new StringBuilder();
            if (IgnoreWhitespaceChanges)
                diffArguments.Append(" --ignore-space-change");

            if (ShowEntireFile)
                diffArguments.AppendFormat(" --inter-hunk-context=9000 --unified=9000");
            else
                diffArguments.AppendFormat(" --unified={0}", NumberOfVisibleLines);

            if (TreatAllFilesAsText)
                diffArguments.Append(" --text");

            return diffArguments.ToString();
        }

        public FileViewer()
        {
            TreatAllFilesAsText = false;
            ShowEntireFile = false;
            NumberOfVisibleLines = 3;
            InitializeComponent();
            TextEditor.ActiveTextAreaControl.TextArea.KeyDown += new KeyEventHandler(TextArea_KeyUp);
            IgnoreWhitespaceChanges = false;
        }

        void TextArea_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.F)
                Find();

            if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.F3)
                findAndReplaceForm.FindNext(true, true, "Text not found");
            else
                if (e.KeyCode == Keys.F3)
                    findAndReplaceForm.FindNext(true, false, "Text not found");

        }

        public int ScrollPos
        {
            get
            {
                return TextEditor.ActiveTextAreaControl.VScrollBar.Value;
            }
            set
            {
                if (TextEditor.ActiveTextAreaControl.VScrollBar.Maximum > value)
                    TextEditor.ActiveTextAreaControl.VScrollBar.Value = value;
                else
                    TextEditor.ActiveTextAreaControl.VScrollBar.Value = TextEditor.ActiveTextAreaControl.VScrollBar.Maximum;
            }
        }

        private void ClearImage()
        {
            PictureBox.ImageLocation = "";

            if (PictureBox.Image is Image)
            {
                PictureBox.Image.Dispose();
                PictureBox.Image = null;
            }
        }

        FindAndReplaceForm findAndReplaceForm = new FindAndReplaceForm();

        public void Find()
        {
            findAndReplaceForm.ShowFor(TextEditor, false);
        }

        public void ViewFile(string fileName)
        {
            EnableDiffContextMenu(false);
            try
            {
 
                if (fileName.EndsWith(".png", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".jpg", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".bmp", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".jpeg", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".ico", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".tif", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".tiff", StringComparison.CurrentCultureIgnoreCase)
                    )
                {
                    Image image;

                    if (fileName.EndsWith(".ico", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Icon icon = new Icon(GitCommands.Settings.WorkingDir + fileName);
                        image = icon.ToBitmap();
                        icon.Dispose();
                    }
                    else
                    {
                        Image fileImage = Image.FromFile(GitCommands.Settings.WorkingDir + fileName);
                        image = (Image)fileImage.Clone();
                        fileImage.Dispose();
                    }

                    PictureBox.Visible = true;
                    TextEditor.Visible = false;

                    if (image.Size.Height > PictureBox.Size.Height ||
                        image.Size.Width > PictureBox.Size.Width)
                    {
                        PictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                    else
                    {
                        PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
                    }
                    PictureBox.Image = image;


                }
                else
                {
                    ClearImage();
                    PictureBox.Visible = false;
                    if (IsBinaryFile(fileName))
                    {
                        TextEditor.Visible = true;
                        TextEditor.Text = "Binary file: " + fileName;
                        TextEditor.Refresh();
                    }
                    else
                    {
                        TextEditor.Visible = true;

                        if (File.Exists(GitCommands.Settings.WorkingDir + fileName))
                            TextEditor.LoadFile(GitCommands.Settings.WorkingDir + fileName);
                    }
                }
            }
            catch
            {
            }
        }


        public void ViewCurrentChanges(string fileName, string format, bool staged)
        {
            EnableDiffContextMenu(true);
            ClearImage();
            PictureBox.Visible = false;
            TextEditor.Visible = true;

            TextEditor.SetHighlighting("Patch");
            TextEditor.Text = GitCommands.GitCommands.GetCurrentChanges(fileName, staged, GetExtraDiffArguments());

            AddPatchHighlighting();

            TextEditor.Refresh();
        }

        public void ViewPatch(string text)
        {
            EnableDiffContextMenu(true);
            ClearImage();
            PictureBox.Visible = false;
            TextEditor.Visible = true;

            TextEditor.SetHighlighting("Patch");
            TextEditor.Text = text;

            AddPatchHighlighting();

            TextEditor.Refresh();
        }

        private void AddPatchHighlighting()
        {
            EnableDiffContextMenu(true);
            //DIFF HIGHLIGHTING!
            for (int line = 0; line < TextEditor.Document.TotalNumberOfLines; line++)
            {
                ICSharpCode.TextEditor.Document.LineSegment lineSegment = TextEditor.Document.GetLineSegment(line);

                if (lineSegment.TotalLength != 0)
                {
                    if (TextEditor.Document.GetCharAt(lineSegment.Offset) == '+')
                    {
                        Color color = Settings.DiffAddedColor;
                        ICSharpCode.TextEditor.Document.LineSegment endLine = TextEditor.Document.GetLineSegment(line);

                        for (; line < TextEditor.Document.TotalNumberOfLines && TextEditor.Document.GetCharAt(endLine.Offset) == '+'; line++)
                        {
                            endLine = TextEditor.Document.GetLineSegment(line);

                        }
                        line--;
                        line--;
                        endLine = TextEditor.Document.GetLineSegment(line);

                        TextEditor.Document.MarkerStrategy.AddMarker(new ICSharpCode.TextEditor.Document.TextMarker(lineSegment.Offset, (endLine.Offset + endLine.TotalLength) - lineSegment.Offset, ICSharpCode.TextEditor.Document.TextMarkerType.SolidBlock, color, ColorHelper.GetForeColorForBackColor(color)));
                    }
                    if (TextEditor.Document.GetCharAt(lineSegment.Offset) == '-')
                    {
                        Color color = Settings.DiffRemovedColor;
                        ICSharpCode.TextEditor.Document.LineSegment endLine = TextEditor.Document.GetLineSegment(line);

                        for (; line < TextEditor.Document.TotalNumberOfLines && TextEditor.Document.GetCharAt(endLine.Offset) == '-'; line++)
                        {
                            endLine = TextEditor.Document.GetLineSegment(line);

                        }
                        line--;
                        line--;
                        endLine = TextEditor.Document.GetLineSegment(line);

                        TextEditor.Document.MarkerStrategy.AddMarker(new ICSharpCode.TextEditor.Document.TextMarker(lineSegment.Offset, (endLine.Offset + endLine.TotalLength) - lineSegment.Offset, ICSharpCode.TextEditor.Document.TextMarkerType.SolidBlock, color, ColorHelper.GetForeColorForBackColor(color)));
                    }
                    if (TextEditor.Document.GetCharAt(lineSegment.Offset) == '@')
                    {
                        Color color = Settings.DiffSectionColor;
                        ICSharpCode.TextEditor.Document.LineSegment endLine = TextEditor.Document.GetLineSegment(line);

                        for (; line < TextEditor.Document.TotalNumberOfLines && TextEditor.Document.GetCharAt(endLine.Offset) == '@'; line++)
                        {
                            endLine = TextEditor.Document.GetLineSegment(line);

                        }
                        line--;
                        line--;
                        endLine = TextEditor.Document.GetLineSegment(line);

                        TextEditor.Document.MarkerStrategy.AddMarker(new ICSharpCode.TextEditor.Document.TextMarker(lineSegment.Offset, (endLine.Offset + endLine.TotalLength) - lineSegment.Offset, ICSharpCode.TextEditor.Document.TextMarkerType.SolidBlock, color, ColorHelper.GetForeColorForBackColor(color)));
                    }

                }
            }
            //END
        }

        public void ViewText(string fileName, string text)
        {
            EnableDiffContextMenu(false);
            ClearImage();
            PictureBox.Visible = false;
            TextEditor.Visible = true;

            EditorOptions.SetSyntax(TextEditor, fileName);

            TextEditor.Text = text;
            TextEditor.Refresh();
        }

        public bool IsBinaryFile(string fileName)
        {
            return FileHelper.IsBinaryFile(fileName);
        }

        public void ViewGitItemRevision(string fileName, string guid)
        {
            EnableDiffContextMenu(false);
            ClearImage();
            PictureBox.Visible = false;
            TextEditor.Visible = true;

            try
            {

                if (fileName.EndsWith(".png", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".jpg", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".bmp", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".jpeg", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".ico", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".tif", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".tiff", StringComparison.CurrentCultureIgnoreCase))
                {
                    Stream stream = GitCommands.GitCommands.GetFileStream(guid);
                    Image images;

                    if (fileName.EndsWith(".ico", StringComparison.CurrentCultureIgnoreCase))
                        images = new Icon(stream).ToBitmap();
                    else
                        images = Image.FromStream(stream);

                    stream.Close();

                    PictureBox.Visible = true;
                    TextEditor.Visible = false;

                    if (images.Size.Height > PictureBox.Size.Height ||
                        images.Size.Width > PictureBox.Size.Width)
                    {
                        PictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                    else
                    {
                        PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
                    }
                    PictureBox.Image = images;
                }
                else
                    if (IsBinaryFile(fileName))
                    {
                        ClearImage();
                        TextEditor.Text = "Binary file: " + fileName;
                    }
                    else
                    {
                        ClearImage();
                        EditorOptions.SetSyntax(TextEditor, fileName);

                        TextEditor.Text = GitCommands.GitCommands.GetFileRevisionText(fileName, guid);
                    }
            }
            catch
            {
                TextEditor.Text = "Unsupported file";
            }
            TextEditor.Refresh();
        }

        public void ViewGitItem(string fileName, string guid)
        {
            EnableDiffContextMenu(false);
            ClearImage();
            PictureBox.Visible = false;
            TextEditor.Visible = true;

            try
            {

                if (fileName.EndsWith(".png", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".jpg", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".bmp", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".jpeg", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".ico", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".tif", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".tiff", StringComparison.CurrentCultureIgnoreCase))
                {
                    Stream stream = GitCommands.GitCommands.GetFileStream(guid);
                    Image images;

                    if (fileName.EndsWith(".ico", StringComparison.CurrentCultureIgnoreCase))
                        images = new Icon(stream).ToBitmap();
                    else
                        images = Image.FromStream(stream);

                    stream.Close();

                    PictureBox.Visible = true;
                    TextEditor.Visible = false;

                    if (images.Size.Height > PictureBox.Size.Height ||
                        images.Size.Width > PictureBox.Size.Width)
                    {
                        PictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                    else
                    {
                        PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
                    }
                    PictureBox.Image = images;
                }
                else
                    if (IsBinaryFile(fileName))
                    {
                        ClearImage();
                        TextEditor.Text = "Binary file: " + fileName;
                    }
                    else
                    {
                        ClearImage();
                        EditorOptions.SetSyntax(TextEditor, fileName);

                        TextEditor.Text = GitCommands.GitCommands.GetFileText(guid);
                    }
            }
            catch
            {
                TextEditor.Text = "Unsupported file";
            }
            TextEditor.Refresh();
        }

        private void TextEditor_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void TextEditor_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Find();
        }

        private void TextEditor_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void ignoreWhitespaceChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IgnoreWhitespaceChanges = !IgnoreWhitespaceChanges;
            ignoreWhitespaceChangesToolStripMenuItem.Checked = IgnoreWhitespaceChanges;
            OnExtraDiffArgumentsChanged();
        }

        private void increaseNumberOfLinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NumberOfVisibleLines++;
            OnExtraDiffArgumentsChanged();
        }

        private void descreaseNumberOfLinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (NumberOfVisibleLines > 0)
                NumberOfVisibleLines--;
            else
                NumberOfVisibleLines = 0;
            OnExtraDiffArgumentsChanged();
        }

        private void showEntireFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showEntireFileToolStripMenuItem.Checked = !showEntireFileToolStripMenuItem.Checked;

            ShowEntireFile = showEntireFileToolStripMenuItem.Checked;
            OnExtraDiffArgumentsChanged();
        }

        private void treatAllFilesAsTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            treatAllFilesAsTextToolStripMenuItem.Checked = !treatAllFilesAsTextToolStripMenuItem.Checked;
            TreatAllFilesAsText = treatAllFilesAsTextToolStripMenuItem.Checked;
            OnExtraDiffArgumentsChanged();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(TextEditor.ActiveTextAreaControl.SelectionManager.SelectedText))
                Clipboard.SetText(TextEditor.ActiveTextAreaControl.SelectionManager.SelectedText);
        }
    }
}
