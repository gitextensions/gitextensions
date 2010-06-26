using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using GitCommands;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Util;
using Git = GitCommands.GitCommands;

namespace GitUI
{
    public partial class FileViewer : GitExtensionsControl
    {
        public bool IgnoreWhitespaceChanges { get; set; }
        public int NumberOfVisibleLines { get; set; }
        public bool ShowEntireFile { get; set; }
        public bool TreatAllFilesAsText { get; set; }
        private bool currentViewIsPatch = false;

        public event EventHandler<EventArgs> ExtraDiffArgumentsChanged;

        private readonly AsyncLoader async;

        private void EnableDiffContextMenu(bool enable)
        {
            currentViewIsPatch = enable;
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
            InitializeComponent(); Translate();
            TextEditor.ActiveTextAreaControl.TextArea.KeyDown += TextArea_KeyUp;
            IgnoreWhitespaceChanges = false;

            async = new AsyncLoader();
            async.LoadingError += delegate
            {
                ResetForText(null);
                TextEditor.Text = "Unsupported file";
                TextEditor.Refresh();
            };


            TextEditor.ActiveTextAreaControl.TextArea.MouseEnter += new EventHandler(TextArea_MouseEnter);
            TextEditor.ActiveTextAreaControl.TextArea.MouseLeave += new EventHandler(TextArea_MouseLeave);

            TextEditor.ShowVRuler = false;
        }

        void TextArea_MouseLeave(object sender, EventArgs e)
        {
            if (GetChildAtPoint(PointToClient(MousePosition)) != fileviewerToolbar)
                fileviewerToolbar.Visible = false;
        }

        void TextArea_MouseEnter(object sender, EventArgs e)
        {
            if (currentViewIsPatch)
                fileviewerToolbar.Visible = true;
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

        private int currentScrollPos = -1;
        public void SaveCurrentScrollPos()
        {
            currentScrollPos = TextEditor.ActiveTextAreaControl.VScrollBar.Value;
        }

        private void RestoreCurrentScrollPos()
        {
            if (currentScrollPos >= 0)
            {
                ScrollPos = currentScrollPos;
                currentScrollPos = 0;
            }
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

        FindAndReplaceForm findAndReplaceForm = new FindAndReplaceForm();

        public void Find()
        {
            findAndReplaceForm.ShowFor(TextEditor, false);
        }

        public void ViewFile(string fileName)
        {
            ViewItem(fileName, () => GetImage(fileName), () => GetFileText(fileName));
        }

        public void ViewCurrentChanges(string fileName, bool staged)
        {
            async.Load(() => Git.GetCurrentChanges(fileName, staged, GetExtraDiffArguments()), ViewPatch);
        }

        public void ViewPatch(string text)
        {
            ResetForDiff();
            TextEditor.Text = text;
            AddPatchHighlighting();
            TextEditor.Refresh();
            RestoreCurrentScrollPos();
        }

        public void ViewPatch(Func<string> loadPatchText)
        {
            async.Load(loadPatchText, ViewPatch);
        }

        private void AddExtraPatchHighlighting()
        {
            IDocument document = TextEditor.Document;
            MarkerStrategy markerStrategy = document.MarkerStrategy;

            Color color;

            for (int line = 0; line + 3 < document.TotalNumberOfLines; line++)
            {
                LineSegment lineSegment1 = document.GetLineSegment(line);
                LineSegment lineSegment2 = document.GetLineSegment(line + 1);
                LineSegment lineSegment3 = document.GetLineSegment(line + 2);
                LineSegment lineSegment4 = document.GetLineSegment(line + 3);

                if (document.GetCharAt(lineSegment1.Offset) == ' ' &&
                    document.GetCharAt(lineSegment2.Offset) == '-' &&
                    document.GetCharAt(lineSegment3.Offset) == '+' &&
                    document.GetCharAt(lineSegment4.Offset) == ' ')
                {
                    int beginOffset = 0;
                    int endOffset = lineSegment3.Length;
                    int reverseOffset = 0;

                    for (; beginOffset < endOffset; beginOffset++)
                    {
                        if (!document.GetCharAt(lineSegment3.Offset + beginOffset).Equals('+'))
                            if (!document.GetCharAt(lineSegment2.Offset + beginOffset).Equals('-'))
                                if (!document.GetCharAt(lineSegment3.Offset + beginOffset).Equals(document.GetCharAt(lineSegment2.Offset + beginOffset)))
                                    break;
                    }

                    for (; endOffset > beginOffset; endOffset--)
                    {
                        reverseOffset = lineSegment3.Length - endOffset;

                        if (!document.GetCharAt(lineSegment3.Offset + lineSegment3.Length - 1 - reverseOffset).Equals('+'))
                            if (!document.GetCharAt(lineSegment2.Offset + lineSegment2.Length - 1 - reverseOffset).Equals('-'))
                                if (!document.GetCharAt(lineSegment3.Offset + lineSegment3.Length - 1 - reverseOffset).Equals(document.GetCharAt(lineSegment2.Offset + lineSegment2.Length - 1 - reverseOffset)))
                                    break;
                    }

                    if (lineSegment3.Length - beginOffset - reverseOffset > 0)
                    {
                        color = Settings.DiffAddedExtraColor;
                        markerStrategy.AddMarker(new TextMarker(lineSegment3.Offset + beginOffset, lineSegment3.Length - beginOffset - reverseOffset, TextMarkerType.SolidBlock, color, ColorHelper.GetForeColorForBackColor(color)));
                    }

                    if (lineSegment2.Length - beginOffset - reverseOffset > 0)
                    {
                        color = Settings.DiffRemovedExtraColor;
                        markerStrategy.AddMarker(new TextMarker(lineSegment2.Offset + beginOffset, lineSegment2.Length - beginOffset - reverseOffset, TextMarkerType.SolidBlock, color, ColorHelper.GetForeColorForBackColor(color)));
                    }
                }
            }
        }

        private void AddPatchHighlighting()
        {
            AddExtraPatchHighlighting();

            IDocument document = TextEditor.Document;
            MarkerStrategy markerStrategy = document.MarkerStrategy;

            for (int line = 0; line < document.TotalNumberOfLines; line++)
            {
                LineSegment lineSegment = document.GetLineSegment(line);

                if (lineSegment.TotalLength != 0)
                {
                    if (document.GetCharAt(lineSegment.Offset) == '+')
                    {
                        Color color = Settings.DiffAddedColor;
                        LineSegment endLine = document.GetLineSegment(line);

                        for (; line < document.TotalNumberOfLines && document.GetCharAt(endLine.Offset) == '+'; line++)
                        {
                            endLine = document.GetLineSegment(line);

                        }
                        line--;
                        line--;
                        endLine = document.GetLineSegment(line);

                        markerStrategy.AddMarker(new TextMarker(lineSegment.Offset, (endLine.Offset + endLine.TotalLength) - lineSegment.Offset, TextMarkerType.SolidBlock, color, ColorHelper.GetForeColorForBackColor(color)));
                    }
                    if (document.GetCharAt(lineSegment.Offset) == '-')
                    {
                        Color color = Settings.DiffRemovedColor;
                        LineSegment endLine = document.GetLineSegment(line);

                        for (; line < document.TotalNumberOfLines && document.GetCharAt(endLine.Offset) == '-'; line++)
                        {
                            endLine = document.GetLineSegment(line);

                        }
                        line--;
                        line--;
                        endLine = document.GetLineSegment(line);

                        markerStrategy.AddMarker(new TextMarker(lineSegment.Offset, (endLine.Offset + endLine.TotalLength) - lineSegment.Offset, TextMarkerType.SolidBlock, color, ColorHelper.GetForeColorForBackColor(color)));
                    }
                    if (document.GetCharAt(lineSegment.Offset) == '@')
                    {
                        Color color = Settings.DiffSectionColor;
                        LineSegment endLine = document.GetLineSegment(line);

                        for (; line < document.TotalNumberOfLines && document.GetCharAt(endLine.Offset) == '@'; line++)
                        {
                            endLine = document.GetLineSegment(line);

                        }
                        line--;
                        line--;
                        endLine = document.GetLineSegment(line);

                        markerStrategy.AddMarker(new TextMarker(lineSegment.Offset, (endLine.Offset + endLine.TotalLength) - lineSegment.Offset, TextMarkerType.SolidBlock, color, ColorHelper.GetForeColorForBackColor(color)));
                    }
                }
            }
        }

        public void ViewText(string fileName, string text)
        {
            ResetForText(fileName);
            TextEditor.Text = text;
            TextEditor.Refresh();
            RestoreCurrentScrollPos();
        }

        public void ViewGitItemRevision(string fileName, string guid)
        {
            ViewItem(fileName, () => GetImage(fileName, guid), () => Git.GetFileRevisionText(fileName, guid));
        }

        public void ViewGitItem(string fileName, string guid)
        {
            ViewItem(fileName, () => GetImage(fileName, guid), () => Git.GetFileText(guid));
        }

        private void ViewItem(string fileName, Func<Image> getImage, Func<string> getFileText)
        {
            if (IsImage(fileName))
            {
                async.Load(getImage, image =>
                {
                    ResetForImage();

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
                });
            }
            else if (IsBinaryFile(fileName))
            {
                ViewText(null, "Binary file: " + fileName);
            }
            else
            {
                async.Load(getFileText, text => ViewText(fileName, text));
            }
        }

        private static bool IsBinaryFile(string fileName)
        {
            return FileHelper.IsBinaryFile(fileName);
        }

        private static bool IsImage(string fileName)
        {
            return FileHelper.IsImage(fileName);
        }

        private static bool IsIcon(string fileName)
        {
            return fileName.EndsWith(".ico", StringComparison.CurrentCultureIgnoreCase);
        }

        private static Image GetImage(string fileName, string guid)
        {
            using (Stream stream = Git.GetFileStream(guid))
            {
                return CreateImage(fileName, stream);
            }
        }

        private static Image GetImage(string fileName)
        {
            using (Stream stream = File.OpenRead(Settings.WorkingDir + fileName))
            {
                return CreateImage(fileName, stream);
            }
        }

        private static Image CreateImage(string fileName, Stream stream)
        {
            if (IsIcon(fileName))
            {
                using (Icon icon = new Icon(stream))
                {
                    return icon.ToBitmap();
                }
            }

            return new Bitmap(CreateCopy(stream));
        }

        private static MemoryStream CreateCopy(Stream stream)
        {
            return new MemoryStream(new BinaryReader(stream).ReadBytes((int)stream.Length));
        }

        private static string GetFileText(string fileName)
        {
            string path = Settings.WorkingDir + fileName;

            if (!File.Exists(path))
            {
                return null;
            }

            return FileReader.ReadFileContent(path, Encoding.ASCII);
        }

        private void ResetForImage()
        {
            Reset(false, false);
            TextEditor.SetHighlighting("Default");
        }

        private void ResetForText(string fileName)
        {
            Reset(false, true);

            if (fileName == null)
                TextEditor.SetHighlighting("Default");
            else
                EditorOptions.SetSyntax(TextEditor, fileName);
        }

        private void ResetForDiff()
        {
            Reset(true, true);
            TextEditor.SetHighlighting("Patch");
        }

        private void Reset(bool diff, bool text)
        {
            EnableDiffContextMenu(diff);
            ClearImage();
            PictureBox.Visible = !text;
            TextEditor.Visible = text;
        }

        private void ClearImage()
        {
            PictureBox.ImageLocation = "";

            if (PictureBox.Image != null)
            {
                PictureBox.Image.Dispose();
                PictureBox.Image = null;
            }
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
            showEntireFileButton.Checked = showEntireFileToolStripMenuItem.Checked;

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
            {
                string code;
                if (currentViewIsPatch)
                {
                    code = TextEditor.ActiveTextAreaControl.SelectionManager.SelectedText;

                    if (code.Contains("\n") && (code[0].Equals(' ') || code[0].Equals('+') || code[0].Equals('-')))
                        code = code.Substring(1);

                    code = code.Replace("\n+", "\n").Replace("\n-", "\n").Replace("\n ", "\n");
                }
                else
                    code = TextEditor.ActiveTextAreaControl.SelectionManager.SelectedText;

                Clipboard.SetText(code);
            }
        }

        private void copyCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(TextEditor.ActiveTextAreaControl.SelectionManager.SelectedText))
                Clipboard.SetText(TextEditor.ActiveTextAreaControl.SelectionManager.SelectedText);
        }

        private void copyPatchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(TextEditor.ActiveTextAreaControl.SelectionManager.SelectedText))
            {
                Clipboard.SetText(TextEditor.ActiveTextAreaControl.SelectionManager.SelectedText);
            }
            else
            {
                Clipboard.SetText(TextEditor.Text);
            }
        }

        private void nextChangeButton_Click(object sender, EventArgs e)
        {
            int firstVisibleLine = TextEditor.ActiveTextAreaControl.TextArea.TextView.FirstVisibleLine;
            int totalNumberOfLines = TextEditor.Document.TotalNumberOfLines;
            bool emptyLineCheck = false;

            for (int line = firstVisibleLine + 1; line < totalNumberOfLines; line++)
            {
                string lineContent = TextEditor.Document.GetText(TextEditor.Document.GetLineSegment(line));
                if (lineContent.StartsWith("+") || lineContent.StartsWith("-"))
                {
                    if (emptyLineCheck)
                    {
                        TextEditor.ActiveTextAreaControl.TextArea.TextView.FirstVisibleLine = Math.Max(line - 1, 0);
                        return;
                    }
                }
                else
                {
                    emptyLineCheck = true;
                }
            }

            //Do not go to the end of the file if no change is found
            //TextEditor.ActiveTextAreaControl.TextArea.TextView.FirstVisibleLine = totalNumberOfLines - TextEditor.ActiveTextAreaControl.TextArea.TextView.VisibleLineCount;
        }

        private void previousChangeButton_Click(object sender, EventArgs e)
        {
            int firstVisibleLine = TextEditor.ActiveTextAreaControl.TextArea.TextView.FirstVisibleLine;
            int totalNumberOfLines = TextEditor.Document.TotalNumberOfLines;
            bool emptyLineCheck = false;

            for (int line = firstVisibleLine - 1; line > 0; line--)
            {
                string lineContent = TextEditor.Document.GetText(TextEditor.Document.GetLineSegment(line));
                if (lineContent.StartsWith("+") || lineContent.StartsWith("-"))
                {
                    emptyLineCheck = true;
                }
                else
                {
                    if (emptyLineCheck)
                    {
                        TextEditor.ActiveTextAreaControl.TextArea.TextView.FirstVisibleLine = line;
                        return;
                    }
                }
            }

            //Do not go to the start of the file if no change is found
            //TextEditor.ActiveTextAreaControl.TextArea.TextView.FirstVisibleLine = 0;
        }

        private void increaseNumberOfLines_Click(object sender, EventArgs e)
        {
            increaseNumberOfLinesToolStripMenuItem_Click(null, null);
        }

        private void DecreaseNumberOfLines_Click(object sender, EventArgs e)
        {
            descreaseNumberOfLinesToolStripMenuItem_Click(null, null);
        }

        private void showEntireFileButton_Click(object sender, EventArgs e)
        {
            showEntireFileToolStripMenuItem_Click(null, null);
        }

        private void showNonPrintChars_Click(object sender, EventArgs e)
        {
            showNonprintableCharactersToolStripMenuItem_Click(null, null);
        }

        private void showNonprintableCharactersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showNonprintableCharactersToolStripMenuItem.Checked = !showNonprintableCharactersToolStripMenuItem.Checked;
            showNonPrintChars.Checked = showNonprintableCharactersToolStripMenuItem.Checked;

            TextEditor.ShowEOLMarkers = showNonprintableCharactersToolStripMenuItem.Checked;
            TextEditor.ShowSpaces = showNonprintableCharactersToolStripMenuItem.Checked;
            TextEditor.ShowTabs = showNonprintableCharactersToolStripMenuItem.Checked;
        }
    }
}