using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using ICSharpCode.TextEditor.Util;

namespace GitUI.Editor
{

    public partial class FileViewer : GitExtensionsControl
    {
        private readonly AsyncLoader _async;
        private int _currentScrollPos = -1;
        private bool _currentViewIsPatch;
        private IFileViewer _internalFileViewer;

        public FileViewer()
        {
            TreatAllFilesAsText = false;
            ShowEntireFile = false;
            NumberOfVisibleLines = 3;
            InitializeComponent();
            Translate();

            if (GitCommands.Settings.RunningOnWindows())
                _internalFileViewer = new FileViewerWindows();
            else
                _internalFileViewer = new FileViewerMono();

            Control internalFileViewerControl = (Control)_internalFileViewer;
            internalFileViewerControl.Dock = DockStyle.Fill;
            Controls.Add(internalFileViewerControl);

            _async = new AsyncLoader();
            _async.LoadingError +=
                (sender, args) =>
                {
                    ResetForText(null);
                    _internalFileViewer.SetText("Unsupported file");

                };

            IgnoreWhitespaceChanges = false;

            IsReadOnly = true;

            _internalFileViewer.MouseMove += TextAreaMouseMove;
            _internalFileViewer.MouseLeave += TextAreaMouseLeave;
            _internalFileViewer.TextChanged += TextEditor_TextChanged;
            _internalFileViewer.ScrollPosChanged += new EventHandler(_internalFileViewer_ScrollPosChanged);
            _internalFileViewer.SelectedLineChanged += new SelectedLineChangedEventHandler(_internalFileViewer_SelectedLineChanged);
            _internalFileViewer.DoubleClick += (sender, args) => OnRequestDiffView(EventArgs.Empty);
        }

        void _internalFileViewer_SelectedLineChanged(object sender, int selectedLine)
        {
            if (SelectedLineChanged != null)
                SelectedLineChanged(sender, selectedLine);
        }

        public event SelectedLineChangedEventHandler SelectedLineChanged;

        public event EventHandler ScrollPosChanged;
        public event EventHandler RequestDiffView;

        protected virtual void OnRequestDiffView(EventArgs args)
        {
            var handler = RequestDiffView;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        void _internalFileViewer_ScrollPosChanged(object sender, EventArgs e)
        {
            if (ScrollPosChanged != null)
                ScrollPosChanged(sender, e);
        }

        public void EnableScrollBars(bool enable)
        {
            _internalFileViewer.EnableScrollBars(enable);
        }

        public bool ShowLineNumbers
        {
            get { return _internalFileViewer.ShowLineNumbers; }
            set { _internalFileViewer.ShowLineNumbers = value; }
        }


        void TextEditor_TextChanged(object sender, EventArgs e)
        {
            if (patchHighlighting)
                _internalFileViewer.AddPatchHighlighting();
        }

        public bool IsReadOnly
        {
            get { return _internalFileViewer.IsReadOnly; }
            set { _internalFileViewer.IsReadOnly = value; }
        }

        public bool IgnoreWhitespaceChanges { get; set; }
        public int NumberOfVisibleLines { get; set; }
        public bool ShowEntireFile { get; set; }
        public bool TreatAllFilesAsText { get; set; }

        public int ScrollPos
        {
            get { return _internalFileViewer.ScrollPos; }
            set { _internalFileViewer.ScrollPos = value; }
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
                fileviewerToolbar.BringToFront();
            }
        }

        private void TextAreaMouseLeave(object sender, EventArgs e)
        {
            if (GetChildAtPoint(PointToClient(MousePosition)) != fileviewerToolbar &&
                fileviewerToolbar != null)
                fileviewerToolbar.Visible = false;
        }


        public void SaveCurrentScrollPos()
        {
            _currentScrollPos = ScrollPos;
        }

        private void RestoreCurrentScrollPos()
        {
            if (_currentScrollPos < 0)
                return;
            ScrollPos = _currentScrollPos;
            _currentScrollPos = 0;
        }


        public void ViewFile(string fileName)
        {
            ViewItem(fileName, () => GetImage(fileName), () => GetFileText(fileName));
        }

        public string GetText()
        {
            return _internalFileViewer.GetText();
        }

        public string GetSelectedLinesAsPatch(bool staged)
        {
            // Ported from the git-gui tcl code to C#
            // see lib/diff.tcl

            // Get currently displayed diff
            string text = _internalFileViewer.GetText();

            // Devide diff into header and patch
            int patch_pos = text.IndexOf("@@");
            string header = text.Substring(0, patch_pos);
            string diff = text.Substring(patch_pos);

            // Get selection position and length
            int first = _internalFileViewer.GetSelectionPosition() - patch_pos;
            int last = _internalFileViewer.GetSelectionPosition() - patch_pos + _internalFileViewer.GetSelectionLength();

            // Round selection to previous and next line breaks to select the whole lines
            int first_l = diff.LastIndexOf("\n", first, first) + 1;
            int last_l = diff.IndexOf("\n", last);

            // Are we looking at a diff from the working dir or the staging area
            char to_context = staged ? '+' : '-';

            // this will hold the entire patch at the end
            string wholepatch = "";

            // loop until $first_l has reached $last_l
            // ($first_l is modified inside the loop!)
            while (first_l < last_l)
            {
                // search from $first_l backwards for lines starting with @@
                int i_l = diff.LastIndexOf("\n@@", first_l, first_l);
                if (i_l == -1 && diff.Substring(0, 2) != "@@")
                {
                    // if there's not a @@ above, then the selected range
                    // must have come before the first @@
                    i_l = diff.IndexOf("\n@@", first_l, last_l - first_l);
                    if (i_l == -1)
                    {
                        // if the @@ is not even in the selected range then
                        // any further action is useless because there is no
                        // selected data that can be applied
                        return null;
                    }
                }
                i_l++;
                // $i_l is now at the beginning of the first @@ line in 
                // front of first_l

                // pick start line number from hunk header
                // example: hh = "@@ -604,58 +604,105 @@ foo bar"
                string hh = diff.Substring(i_l, diff.IndexOf("\n", i_l) - i_l);
                // example: hh = "@@ -604"
                hh = hh.Split(',')[0];
                // example: hlh = "604"
                string hln = hh.Split('-')[1];

                // There is a special situation to take care of. Consider this
                // hunk:
                //
                //    @@ -10,4 +10,4 @@
                //     context before
                //    -old 1
                //    -old 2
                //    +new 1
                //    +new 2
                //     context after
                //
                // We used to keep the context lines in the order they appear in
                // the hunk. But then it is not possible to correctly stage only
                // "-old 1" and "+new 1" - it would result in this staged text:
                //
                //    context before
                //    old 2
                //    new 1
                //    context after
                //
                // (By symmetry it is not possible to *un*stage "old 2" and "new
                // 2".)
                //
                // We resolve the problem by introducing an asymmetry, namely,
                // when a "+" line is *staged*, it is moved in front of the
                // context lines that are generated from the "-" lines that are
                // immediately before the "+" block. That is, we construct this
                // patch:
                //
                //    @@ -10,4 +10,5 @@
                //     context before
                //    +new 1
                //     old 1
                //     old 2
                //     context after
                //
                // But we do *not* treat "-" lines that are *un*staged in a
                // special way.
                //
                // With this asymmetry it is possible to stage the change "old
                // 1" -> "new 1" directly, and to stage the change "old 2" ->
                // "new 2" by first staging the entire hunk and then unstaging
                // the change "old 1" -> "new 1".
                //
                // Applying multiple lines adds complexity to the special
                // situation.  The pre_context must be moved after the entire
                // first block of consecutive staged "+" lines, so that
                // staging both additions gives the following patch:
                //
                //    @@ -10,4 +10,6 @@
                //     context before
                //    +new 1
                //    +new 2
                //     old 1
                //     old 2
                //     context after

                // This is non-empty if and only if we are _staging_ changes;
                // then it accumulates the consecutive "-" lines (after
                // converting them to context lines) in order to be moved after
                // "+" change lines.
                string pre_context = "";

                int n = 0;
                int m = 0;
                // move $i_l to the first line after the @@ line $i_l pointed at
                i_l = diff.IndexOf("\n", i_l) + 1;
                string patch = "";

                // while $i_l is not at the end of the file and not 
                // at the next @@ line
                while (i_l < diff.Length - 1 && diff.Substring(i_l, 2) != "@@")
                {
                    // set $next_l to the beginning of the next 
                    // line after $i_l
                    int next_l = diff.IndexOf("\n", i_l) + 1;

                    // get character at $i_l 
                    char c1 = diff[i_l];

                    // if $i_l is in selected range and the line starts 
                    // with either - or + this is a line to stage/unstage
                    if (first_l <= i_l && i_l < last_l && (c1 == '-' || c1 == '+'))
                    {
                        // set $ln to the content of the line at $i_l
                        string ln = diff.Substring(i_l, next_l - i_l);
                        // if line starts with -
                        if (c1 == '-')
                        {
                            // increase n counter by one
                            n++;
                            // update $patch
                            patch += pre_context + ln;
                            // reset $pre_context
                            pre_context = "";

                            // if line starts with +
                        }
                        else
                        {
                            // increase m counter by one
                            m++;
                            // update $patch
                            patch += ln;
                        }

                        // if the line doesn't start with either - or +
                        // this is a context line 
                    }
                    else if (c1 != '-' && c1 != '+')
                    {
                        // set $ln to the content of the line at $i_l
                        string ln = diff.Substring(i_l, next_l - i_l);
                        // update $patch
                        patch += pre_context + ln;
                        // increase counters by one each
                        n++;
                        m++;
                        // reset $pre_context
                        pre_context = "";

                        // if the line starts with $to_context (see earlier)
                        // the sign at the beginning should be stripped
                    }
                    else if (c1 == to_context)
                    {
                        // turn change line into context line
                        string ln = diff.Substring(i_l + 1, next_l - i_l - 1);
                        if (c1 == '-')
                            pre_context += " " + ln;
                        else
                            patch += " " + ln;
                        // increase counters by one each
                        n++;
                        m++;

                        // if the line starts with the opposite sign of
                        // $to_context this line should be removed
                    }
                    else
                    {
                        // a change in the opposite direction of
                        // to_context which is outside the range of
                        // lines to apply.
                        patch += pre_context;
                        pre_context = "";
                    }
                    // set $i_l to the next line
                    i_l = next_l;
                }
                // finished current hunk (reached @@ or file/diff end)

                // update $patch (makes sure $pre_context gets appended)
                patch += pre_context;
                // update $wholepatch with the current hunk
                wholepatch += "@@ -" + hln + "," + n.ToString() + " +" + hln + "," + m + " @@\n" + patch;

                // set $first_l to first line after the next @@ line
                first_l = diff.IndexOf("\n", i_l) + 1;
                if (first_l == 0)
                    break;
            }
            // we are almost done, $wholepatch should no contain all the 
            // (modified) hunks

            return header + wholepatch;
        }

        public void ViewCurrentChanges(string fileName, bool staged)
        {
            _async.Load(() => GitCommandHelpers.GetCurrentChanges(fileName, staged, GetExtraDiffArguments()), ViewPatch);
        }

        public void ViewPatch(string text)
        {
            ResetForDiff();
            _internalFileViewer.SetText(text);
            RestoreCurrentScrollPos();
        }

        public void ViewPatch(Func<string> loadPatchText)
        {
            _async.Load(loadPatchText, ViewPatch);
        }

        public void ViewText(string fileName, string text)
        {
            ResetForText(fileName);

            //Check for binary file.
            if (!string.IsNullOrEmpty(text))
            {
                int nullCount = 0;
                foreach (char c in text)
                {
                    if (c == '\0')
                        nullCount++;
                    if (nullCount > 5) break;
                }


                if (nullCount > 5)
                {
                    _internalFileViewer.SetText("Binary file: " + fileName + " (Detected)");
                    return;
                }
            }

            _internalFileViewer.SetText(text);

            RestoreCurrentScrollPos();
        }

        public void ViewGitItemRevision(string fileName, string guid)
        {
            ViewItem(fileName, () => GetImage(fileName, guid), () => GitCommandHelpers.GetFileRevisionText(fileName, guid));
        }

        public void ViewGitItem(string fileName, string guid)
        {
            ViewItem(fileName, () => GetImage(fileName, guid), () => GitCommandHelpers.GetFileText(guid));
        }

        private void ViewItem(string fileName, Func<Image> getImage, Func<string> getFileText)
        {
            if (IsImage(fileName))
            {
                _async.Load(getImage,
                            image =>
                            {
                                ResetForImage();
                                if (image != null)
                                {
                                    if (image.Size.Height > PictureBox.Size.Height ||
                                        image.Size.Width > PictureBox.Size.Width)
                                    {
                                        PictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                                    }
                                    else
                                    {
                                        PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
                                    }

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
            try
            {
                using (var stream = GitCommandHelpers.GetFileStream(guid))
                {
                    return CreateImage(fileName, stream);
                }
            }
            catch
            {
                return null;
            }
        }

        private static Image GetImage(string fileName)
        {
            try
            {
                using (Stream stream = File.OpenRead(GitCommands.Settings.WorkingDir + fileName))
                {
                    return CreateImage(fileName, stream);
                }
            }
            catch
            {
                return null;
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
            return new MemoryStream(new BinaryReader(stream).ReadBytes((int)stream.Length));
        }

        private static string GetFileText(string fileName)
        {
            string path;
            if (File.Exists(fileName))
                path = fileName;
            else
                path = GitCommands.Settings.WorkingDir + fileName;

            return !File.Exists(path) ? null : FileReader.ReadFileContent(path, GitCommands.Settings.Encoding);
        }

        private void ResetForImage()
        {
            Reset(false, false);
            _internalFileViewer.SetHighlighting("Default");
        }

        public void SetSyntax(string fileName)
        {
            EditorOptions.SetSyntax(_internalFileViewer, fileName);
        }

        private void ResetForText(string fileName)
        {
            Reset(false, true);

            if (fileName == null)
                _internalFileViewer.SetHighlighting("Default");
            else
                EditorOptions.SetSyntax(_internalFileViewer, fileName);

            if (!string.IsNullOrEmpty(fileName) &&
                (fileName.EndsWith(".diff", StringComparison.OrdinalIgnoreCase) ||
                 fileName.EndsWith(".patch", StringComparison.OrdinalIgnoreCase)))
            {
                ResetForDiff();
            }
        }
        private bool patchHighlighting = false;
        private void ResetForDiff()
        {
            Reset(true, true);
            _internalFileViewer.SetHighlighting("Patch");
            patchHighlighting = true;
        }

        private void Reset(bool diff, bool text)
        {
            patchHighlighting = diff;
            EnableDiffContextMenu(diff);
            ClearImage();
            PictureBox.Visible = !text;
            _internalFileViewer.Visible = text;
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
            if (string.IsNullOrEmpty(_internalFileViewer.GetSelectedText()))
                return;

            string code;
            if (_currentViewIsPatch)
            {
                code = _internalFileViewer.GetSelectedText();

                if (code.Contains("\n") && (code[0].Equals(' ') || code[0].Equals('+') || code[0].Equals('-')))
                    code = code.Substring(1);

                code = code.Replace("\n+", "\n").Replace("\n-", "\n").Replace("\n ", "\n");
            }
            else
                code = _internalFileViewer.GetSelectedText();

            Clipboard.SetText(code);
        }

        private void CopyPatchToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_internalFileViewer.GetSelectedText()))
            {
                Clipboard.SetText(_internalFileViewer.GetSelectedText());
            }
            else
            {
                Clipboard.SetText(_internalFileViewer.GetText());
            }
        }

        private void NextChangeButtonClick(object sender, EventArgs e)
        {
            var firstVisibleLine = _internalFileViewer.FirstVisibleLine;
            var totalNumberOfLines = _internalFileViewer.TotalNumberOfLines;
            var emptyLineCheck = false;

            for (var line = firstVisibleLine + 1; line < totalNumberOfLines; line++)
            {
                var lineContent = _internalFileViewer.GetLineText(line);
                if (lineContent.StartsWith("+") || lineContent.StartsWith("-"))
                {
                    if (emptyLineCheck)
                    {
                        _internalFileViewer.FirstVisibleLine = Math.Max(line - 1, 0);
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
            var firstVisibleLine = _internalFileViewer.FirstVisibleLine;
            var emptyLineCheck = false;

            for (var line = firstVisibleLine - 1; line > 0; line--)
            {
                var lineContent = _internalFileViewer.GetLineText(line);
                if (lineContent.StartsWith("+") || lineContent.StartsWith("-"))
                {
                    emptyLineCheck = true;
                }
                else
                {
                    if (emptyLineCheck)
                    {
                        _internalFileViewer.FirstVisibleLine = line;
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

            _internalFileViewer.ShowEOLMarkers = showNonprintableCharactersToolStripMenuItem.Checked;
            _internalFileViewer.ShowSpaces = showNonprintableCharactersToolStripMenuItem.Checked;
            _internalFileViewer.ShowTabs = showNonprintableCharactersToolStripMenuItem.Checked;
        }

        private void FindToolStripMenuItemClick(object sender, EventArgs e)
        {
            _internalFileViewer.Find();
        }

        private void ignoreWhiteSpaces_Click(object sender, EventArgs e)
        {
            IgnoreWhitespaceChangesToolStripMenuItemClick(null, null);
        }
    }
}