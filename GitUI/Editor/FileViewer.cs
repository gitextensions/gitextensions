using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Util;

namespace GitUI.Editor
{
    public partial class FileViewer : GitExtensionsControl
    {
        private readonly AsyncLoader _async;
        private readonly FindAndReplaceForm _findAndReplaceForm = new FindAndReplaceForm();
        private int _currentScrollPos = -1;
        private bool _currentViewIsPatch;

        public FileViewer()
        {
            TreatAllFilesAsText = false;
            ShowEntireFile = false;
            NumberOfVisibleLines = 3;
            InitializeComponent();
            Translate();
            TextEditor.ActiveTextAreaControl.TextArea.KeyDown += TextAreaKeyUp;
            TextEditor.Encoding = Settings.Encoding;
            IgnoreWhitespaceChanges = false;

            _async = new AsyncLoader();
            _async.LoadingError +=
                (sender, args) =>
                    {
                        ResetForText(null);
                        TextEditor.Text = "Unsupported file";
                        TextEditor.Refresh();
                    };


            TextEditor.ActiveTextAreaControl.TextArea.MouseMove += TextAreaMouseMove;
            TextEditor.ActiveTextAreaControl.TextArea.MouseLeave += TextAreaMouseLeave;

            TextEditor.ShowVRuler = false;
            IsReadOnly = true;
            TextEditor.TextChanged += new EventHandler(TextEditor_TextChanged);
        }

        void TextEditor_TextChanged(object sender, EventArgs e)
        {
            if (patchHighlighting)
                AddPatchHighlighting();
        }

        public bool IsReadOnly
        {
            get { return TextEditor.IsReadOnly;  }
            set { TextEditor.IsReadOnly = value; }
        }

        public bool IgnoreWhitespaceChanges { get; set; }
        public int NumberOfVisibleLines { get; set; }
        public bool ShowEntireFile { get; set; }
        public bool TreatAllFilesAsText { get; set; }

        public int ScrollPos
        {
            get { return TextEditor.ActiveTextAreaControl.VScrollBar.Value; }
            set
            {
                var scrollBar = TextEditor.ActiveTextAreaControl.VScrollBar;
                scrollBar.Value = scrollBar.Maximum > value ? value : scrollBar.Maximum;
            }
        }

        public event EventHandler<EventArgs> ExtraDiffArgumentsChanged;

        public void EnableDiffContextMenu(bool enable)
        {
            _currentViewIsPatch = enable;
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
            var diffArguments = new StringBuilder();
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

        private void TextAreaMouseMove(object sender, MouseEventArgs e)
        {
            if (_currentViewIsPatch && !fileviewerToolbar.Visible)
            {
                fileviewerToolbar.Visible = true;
                fileviewerToolbar.Location = new Point(this.Width - fileviewerToolbar.Width - 40, 0);
            }
        }

        private void TextAreaMouseLeave(object sender, EventArgs e)
        {
            if (GetChildAtPoint(PointToClient(MousePosition)) != fileviewerToolbar &&
                fileviewerToolbar != null)
                fileviewerToolbar.Visible = false;
        }

        private void TextAreaKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.F)
                Find();

            if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.F3)
                _findAndReplaceForm.FindNext(true, true, "Text not found");
            else if (e.KeyCode == Keys.F3)
                _findAndReplaceForm.FindNext(true, false, "Text not found");
        }

        public void SaveCurrentScrollPos()
        {
            _currentScrollPos = TextEditor.ActiveTextAreaControl.VScrollBar.Value;
        }

        private void RestoreCurrentScrollPos()
        {
            if (_currentScrollPos < 0)
                return;
            ScrollPos = _currentScrollPos;
            _currentScrollPos = 0;
        }

        public void Find()
        {
            _findAndReplaceForm.ShowFor(TextEditor, false);
        }

        public void ViewFile(string fileName)
        {
            ViewItem(fileName, () => GetImage(fileName), () => GetFileText(fileName));
        }

        public string GetText()
        {
            return TextEditor.Text;
        }

        public void ViewCurrentChanges(string fileName, bool staged)
        {
            _async.Load(() => GitCommands.GitCommands.GetCurrentChanges(fileName, staged, GetExtraDiffArguments()), ViewPatch);
        }

        public void ViewPatch(string text)
        {
            ResetForDiff();
            TextEditor.Text = text;
            TextEditor.Refresh();
            RestoreCurrentScrollPos();
        }

        public void ViewPatch(Func<string> loadPatchText)
        {
            _async.Load(loadPatchText, ViewPatch);
        }

        private void AddExtraPatchHighlighting()
        {
            var document = TextEditor.Document;
            var markerStrategy = document.MarkerStrategy;

            for (var line = 0; line + 3 < document.TotalNumberOfLines; line++)
            {
                var lineSegment1 = document.GetLineSegment(line);
                var lineSegment2 = document.GetLineSegment(line + 1);
                var lineSegment3 = document.GetLineSegment(line + 2);
                var lineSegment4 = document.GetLineSegment(line + 3);

                if (document.GetCharAt(lineSegment1.Offset) != ' ' ||
                    document.GetCharAt(lineSegment2.Offset) != '-' ||
                    document.GetCharAt(lineSegment3.Offset) != '+' ||
                    document.GetCharAt(lineSegment4.Offset) != ' ')
                    continue;

                var beginOffset = 0;
                var endOffset = lineSegment3.Length;
                var reverseOffset = 0;

                for (; beginOffset < endOffset; beginOffset++)
                {
                    if (!document.GetCharAt(lineSegment3.Offset + beginOffset).Equals('+') &&
                        !document.GetCharAt(lineSegment2.Offset + beginOffset).Equals('-') &&
                        !document.GetCharAt(lineSegment3.Offset + beginOffset).Equals(
                            document.GetCharAt(lineSegment2.Offset + beginOffset)))
                        break;
                }

                for (; endOffset > beginOffset; endOffset--)
                {
                    reverseOffset = lineSegment3.Length - endOffset;

                    if (!document.GetCharAt(lineSegment3.Offset + lineSegment3.Length - 1 - reverseOffset)
                             .Equals('+') &&
                        !document.GetCharAt(lineSegment2.Offset + lineSegment2.Length - 1 - reverseOffset)
                             .Equals('-') &&
                        !document.GetCharAt(lineSegment3.Offset + lineSegment3.Length - 1 - reverseOffset).
                             Equals(document.GetCharAt(lineSegment2.Offset + lineSegment2.Length - 1 -
                                                       reverseOffset)))
                        break;
                }

                Color color;
                if (lineSegment3.Length - beginOffset - reverseOffset > 0)
                {
                    color = Settings.DiffAddedExtraColor;
                    markerStrategy.AddMarker(new TextMarker(lineSegment3.Offset + beginOffset,
                                                            lineSegment3.Length - beginOffset - reverseOffset,
                                                            TextMarkerType.SolidBlock, color,
                                                            ColorHelper.GetForeColorForBackColor(color)));
                }

                if (lineSegment2.Length - beginOffset - reverseOffset > 0)
                {
                    color = Settings.DiffRemovedExtraColor;
                    markerStrategy.AddMarker(new TextMarker(lineSegment2.Offset + beginOffset,
                                                            lineSegment2.Length - beginOffset - reverseOffset,
                                                            TextMarkerType.SolidBlock, color,
                                                            ColorHelper.GetForeColorForBackColor(color)));
                }
            }
        }

        private void AddPatchHighlighting()
        {
            var document = TextEditor.Document;
            var markerStrategy = document.MarkerStrategy;
            markerStrategy.RemoveAll(m => true);

            AddExtraPatchHighlighting();

            for (var line = 0; line < document.TotalNumberOfLines; line++)
            {
                var lineSegment = document.GetLineSegment(line);

                if (lineSegment.TotalLength == 0)
                    continue;

                if (document.GetCharAt(lineSegment.Offset) == '+')
                {
                    var color = Settings.DiffAddedColor;
                    var endLine = document.GetLineSegment(line);

                    for (; line < document.TotalNumberOfLines && document.GetCharAt(endLine.Offset) == '+'; line++)
                    {
                        endLine = document.GetLineSegment(line);
                    }
                    line--;
                    line--;
                    endLine = document.GetLineSegment(line);

                    markerStrategy.AddMarker(new TextMarker(lineSegment.Offset,
                                                            (endLine.Offset + endLine.TotalLength) -
                                                            lineSegment.Offset, TextMarkerType.SolidBlock, color,
                                                            ColorHelper.GetForeColorForBackColor(color)));
                }
                if (document.GetCharAt(lineSegment.Offset) == '-')
                {
                    var color = Settings.DiffRemovedColor;
                    var endLine = document.GetLineSegment(line);

                    for (; line < document.TotalNumberOfLines && document.GetCharAt(endLine.Offset) == '-'; line++)
                    {
                        endLine = document.GetLineSegment(line);
                    }
                    line--;
                    line--;
                    endLine = document.GetLineSegment(line);

                    markerStrategy.AddMarker(new TextMarker(lineSegment.Offset,
                                                            (endLine.Offset + endLine.TotalLength) -
                                                            lineSegment.Offset, TextMarkerType.SolidBlock, color,
                                                            ColorHelper.GetForeColorForBackColor(color)));
                }
                if (document.GetCharAt(lineSegment.Offset) == '@')
                {
                    var color = Settings.DiffSectionColor;
                    var endLine = document.GetLineSegment(line);

                    for (; line < document.TotalNumberOfLines && document.GetCharAt(endLine.Offset) == '@'; line++)
                    {
                        endLine = document.GetLineSegment(line);
                    }
                    line--;
                    line--;
                    endLine = document.GetLineSegment(line);

                    markerStrategy.AddMarker(new TextMarker(lineSegment.Offset,
                                                            (endLine.Offset + endLine.TotalLength) -
                                                            lineSegment.Offset, TextMarkerType.SolidBlock, color,
                                                            ColorHelper.GetForeColorForBackColor(color)));
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
            ViewItem(fileName, () => GetImage(fileName, guid), () => GitCommands.GitCommands.GetFileRevisionText(fileName, guid));
        }

        public void ViewGitItem(string fileName, string guid)
        {
            ViewItem(fileName, () => GetImage(fileName, guid), () => GitCommands.GitCommands.GetFileText(guid));
        }

        private void ViewItem(string fileName, Func<Image> getImage, Func<string> getFileText)
        {
            if (IsImage(fileName))
            {
                _async.Load(getImage,
                            image =>
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
                _async.Load(getFileText, text => ViewText(fileName, text));
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
            using (var stream = GitCommands.GitCommands.GetFileStream(guid))
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
                using (var icon = new Icon(stream))
                {
                    return icon.ToBitmap();
                }
            }

            return new Bitmap(CreateCopy(stream));
        }

        private static MemoryStream CreateCopy(Stream stream)
        {
            return new MemoryStream(new BinaryReader(stream).ReadBytes((int) stream.Length));
        }

        private static string GetFileText(string fileName)
        {
            string path; 
            if (File.Exists(fileName))
                path = fileName;
            else
                path = Settings.WorkingDir + fileName;

            return !File.Exists(path) ? null : FileReader.ReadFileContent(path, Settings.Encoding);
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

            if (fileName.EndsWith(".diff", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(".patch", StringComparison.OrdinalIgnoreCase))
            {
                ResetForDiff();
            }
        }
        private bool patchHighlighting = false;
        private void ResetForDiff()
        {
            Reset(true, true);
            TextEditor.SetHighlighting("Patch");
            patchHighlighting = true;
        }

        private void Reset(bool diff, bool text)
        {
            patchHighlighting = diff;
            EnableDiffContextMenu(diff);
            ClearImage();
            PictureBox.Visible = !text;
            TextEditor.Visible = text;
        }

        private void ClearImage()
        {
            PictureBox.ImageLocation = "";

            if (PictureBox.Image == null) return;
            PictureBox.Image.Dispose();
            PictureBox.Image = null;
        }

        private void IgnoreWhitespaceChangesToolStripMenuItemClick(object sender, EventArgs e)
        {
            IgnoreWhitespaceChanges = !IgnoreWhitespaceChanges;
            ignoreWhiteSpaces.Checked = IgnoreWhitespaceChanges;
            ignoreWhitespaceChangesToolStripMenuItem.Checked = IgnoreWhitespaceChanges;
            OnExtraDiffArgumentsChanged();
        }

        private void IncreaseNumberOfLinesToolStripMenuItemClick(object sender, EventArgs e)
        {
            NumberOfVisibleLines++;
            OnExtraDiffArgumentsChanged();
        }

        private void DescreaseNumberOfLinesToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (NumberOfVisibleLines > 0)
                NumberOfVisibleLines--;
            else
                NumberOfVisibleLines = 0;
            OnExtraDiffArgumentsChanged();
        }

        private void ShowEntireFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            showEntireFileToolStripMenuItem.Checked = !showEntireFileToolStripMenuItem.Checked;
            showEntireFileButton.Checked = showEntireFileToolStripMenuItem.Checked;

            ShowEntireFile = showEntireFileToolStripMenuItem.Checked;
            OnExtraDiffArgumentsChanged();
        }

        private void TreatAllFilesAsTextToolStripMenuItemClick(object sender, EventArgs e)
        {
            treatAllFilesAsTextToolStripMenuItem.Checked = !treatAllFilesAsTextToolStripMenuItem.Checked;
            TreatAllFilesAsText = treatAllFilesAsTextToolStripMenuItem.Checked;
            OnExtraDiffArgumentsChanged();
        }

        private void CopyToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TextEditor.ActiveTextAreaControl.SelectionManager.SelectedText))
                return;

            string code;
            if (_currentViewIsPatch)
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

        private void CopyPatchToolStripMenuItemClick(object sender, EventArgs e)
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

        private void NextChangeButtonClick(object sender, EventArgs e)
        {
            var firstVisibleLine = TextEditor.ActiveTextAreaControl.TextArea.TextView.FirstVisibleLine;
            var totalNumberOfLines = TextEditor.Document.TotalNumberOfLines;
            var emptyLineCheck = false;

            for (var line = firstVisibleLine + 1; line < totalNumberOfLines; line++)
            {
                var lineContent = TextEditor.Document.GetText(TextEditor.Document.GetLineSegment(line));
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

        private void PreviousChangeButtonClick(object sender, EventArgs e)
        {
            var firstVisibleLine = TextEditor.ActiveTextAreaControl.TextArea.TextView.FirstVisibleLine;
            var emptyLineCheck = false;

            for (var line = firstVisibleLine - 1; line > 0; line--)
            {
                var lineContent = TextEditor.Document.GetText(TextEditor.Document.GetLineSegment(line));
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

        private void IncreaseNumberOfLinesClick(object sender, EventArgs e)
        {
            IncreaseNumberOfLinesToolStripMenuItemClick(null, null);
        }

        private void DecreaseNumberOfLinesClick(object sender, EventArgs e)
        {
            DescreaseNumberOfLinesToolStripMenuItemClick(null, null);
        }

        private void ShowEntireFileButtonClick(object sender, EventArgs e)
        {
            ShowEntireFileToolStripMenuItemClick(null, null);
        }

        private void ShowNonPrintCharsClick(object sender, EventArgs e)
        {
            ShowNonprintableCharactersToolStripMenuItemClick(null, null);
        }

        private void ShowNonprintableCharactersToolStripMenuItemClick(object sender, EventArgs e)
        {
            showNonprintableCharactersToolStripMenuItem.Checked = !showNonprintableCharactersToolStripMenuItem.Checked;
            showNonPrintChars.Checked = showNonprintableCharactersToolStripMenuItem.Checked;

            TextEditor.ShowEOLMarkers = showNonprintableCharactersToolStripMenuItem.Checked;
            TextEditor.ShowSpaces = showNonprintableCharactersToolStripMenuItem.Checked;
            TextEditor.ShowTabs = showNonprintableCharactersToolStripMenuItem.Checked;
        }

        private void FindToolStripMenuItemClick(object sender, EventArgs e)
        {
            Find();
        }

        private void ignoreWhiteSpaces_Click(object sender, EventArgs e)
        {
            IgnoreWhitespaceChangesToolStripMenuItemClick(null, null);
        }
    }
}