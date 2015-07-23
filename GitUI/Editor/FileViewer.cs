using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitUI.CommandsDialogs;
using GitUI.Hotkey;
using PatchApply;
using GitCommands.Settings;
using GitUI.Editor.Diff;
using ResourceManager;

namespace GitUI.Editor
{
    [DefaultEvent("SelectedLineChanged")]
    public partial class FileViewer : GitModuleControl
    {
        private readonly AsyncLoader _async;
        private int _currentScrollPos = -1;
        private bool _currentViewIsPatch;
        private readonly IFileViewer _internalFileViewer;
        private string _highlightingSyntax;

        public FileViewer()
        {
            TreatAllFilesAsText = false;
            ShowEntireFile = false;
            NumberOfVisibleLines = 3;
            InitializeComponent();
            Translate();

            GitUICommandsSourceSet += FileViewer_GitUICommandsSourceSet;

            _internalFileViewer = new FileViewerInternal();
            _internalFileViewer.MouseEnter += _internalFileViewer_MouseEnter;
            _internalFileViewer.MouseLeave += _internalFileViewer_MouseLeave;
            _internalFileViewer.MouseMove += _internalFileViewer_MouseMove;

            var internalFileViewerControl = (Control)_internalFileViewer;
            internalFileViewerControl.Dock = DockStyle.Fill;
            Controls.Add(internalFileViewerControl);

            _async = new AsyncLoader();
            _async.LoadingError +=
                (sender, args) =>
                {
                    ResetForText(null);
                    _internalFileViewer.SetText("Unsupported file: \n\n" + args.Exception.Message);
                    if (TextLoaded != null)
                        TextLoaded(this, null);
                };

            IgnoreWhitespaceChanges = AppSettings.IgnoreWhitespaceChanges;
            ignoreWhiteSpaces.Checked = IgnoreWhitespaceChanges;

            IsReadOnly = true;

            this.encodingToolStripComboBox.Items.AddRange(new Object[]
                                                    {
                                                        "Default (" + Encoding.Default.HeaderName + ")","DOS852", "ASCII",
                                                        "Unicode", "UTF7", "UTF8", "UTF32"
                                                    });
            _internalFileViewer.MouseMove += TextAreaMouseMove;
            _internalFileViewer.MouseLeave += TextAreaMouseLeave;
            _internalFileViewer.TextChanged += TextEditor_TextChanged;
            _internalFileViewer.ScrollPosChanged += _internalFileViewer_ScrollPosChanged;
            _internalFileViewer.SelectedLineChanged += _internalFileViewer_SelectedLineChanged;
            _internalFileViewer.DoubleClick += (sender, args) => OnRequestDiffView(EventArgs.Empty);

            this.HotkeysEnabled = true;

            if (RunTime() && ContextMenuStrip == null)
                ContextMenuStrip = contextMenu;
            contextMenu.Opening += ContextMenu_Opening;
        }

        void FileViewer_GitUICommandsSourceSet(object sender, GitUICommandsSourceEventArgs e)
        {
            UICommandsSource.GitUICommandsChanged += WorkingDirChanged;
            WorkingDirChanged(UICommandsSource, null);
        }

        protected override void DisposeUICommandsSource()
        {
            UICommandsSource.GitUICommandsChanged -= WorkingDirChanged;
            base.DisposeUICommandsSource();
        }

        private bool RunTime()
        {
            return (System.Diagnostics.Process.GetCurrentProcess().ProcessName != "devenv");
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public new Font Font
        {
            get { return _internalFileViewer.Font; }
            set { _internalFileViewer.Font = value; }
        }

        [Description("Ignore changes in amount of whitespace. This ignores whitespace at line end, and considers all other sequences of one or more whitespace characters to be equivalent.")]
        [DefaultValue(false)]
        public bool IgnoreWhitespaceChanges { get; set; }
        [Description("Show diffs with <n> lines of context.")]
        [DefaultValue(3)]
        public int NumberOfVisibleLines { get; set; }
        [Description("Show diffs with entire file.")]
        [DefaultValue(false)]
        public bool ShowEntireFile { get; set; }
        [Description("Treat all files as text.")]
        [DefaultValue(false)]
        public bool TreatAllFilesAsText { get; set; }
        [DefaultValue(true)]
        [Category("Behavior")]
        public bool IsReadOnly
        {
            get { return _internalFileViewer.IsReadOnly; }
            set { _internalFileViewer.IsReadOnly = value; }
        }
        [DefaultValue(true)]
        [Description("If true line numbers are shown in the textarea")]
        [Category("Appearance")]
        public bool ShowLineNumbers
        {
            get { return _internalFileViewer.ShowLineNumbers; }
            set { _internalFileViewer.ShowLineNumbers = value; }
        }

        private Encoding _Encoding;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Encoding Encoding
        {
            get
            {
                if (_Encoding == null)
                    _Encoding = Module.FilesEncoding;

                return _Encoding;
            }

            set
            {
                _Encoding = value;
            }
        }

        [DefaultValue(0)]
        [Browsable(false)]
        public int ScrollPos
        {
            get { return _internalFileViewer.ScrollPos; }
            set { _internalFileViewer.ScrollPos = value; }
        }

        [Browsable(false)]
        public byte[] FilePreabmle { get; private set; }

        private void WorkingDirChanged(object sender, GitUICommandsChangedEventArgs e)
        {
            this.Encoding = null;
        }


        protected override void OnRuntimeLoad(EventArgs e)
        {
            this.Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
            Font = AppSettings.DiffFont;
        }

        void ContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            copyToolStripMenuItem.Enabled = (_internalFileViewer.GetSelectionLength() > 0);
            if (ContextMenuOpening != null)
                ContextMenuOpening(sender, e);
        }

        void _internalFileViewer_MouseMove(object sender, MouseEventArgs e)
        {
            this.OnMouseMove(e);
        }

        void _internalFileViewer_MouseEnter(object sender, EventArgs e)
        {
            this.OnMouseEnter(e);
        }

        void _internalFileViewer_MouseLeave(object sender, EventArgs e)
        {
            this.OnMouseLeave(e);
        }

        void _internalFileViewer_SelectedLineChanged(object sender, SelectedLineEventArgs e)
        {
            if (SelectedLineChanged != null)
                SelectedLineChanged(sender, e);
        }

        public event EventHandler<SelectedLineEventArgs> SelectedLineChanged;

        public event EventHandler ScrollPosChanged;
        public event EventHandler RequestDiffView;
        public new event EventHandler TextChanged;
        public event EventHandler TextLoaded;
        public event CancelEventHandler ContextMenuOpening;

        public ToolStripSeparator AddContextMenuSeparator()
        {
            ToolStripSeparator separator = new ToolStripSeparator();
            contextMenu.Items.Add(separator);
            return separator;
        }

        public ToolStripMenuItem AddContextMenuEntry(string text, EventHandler toolStripItem_Click)
        {
            ToolStripMenuItem toolStripItem = new ToolStripMenuItem(text);
            contextMenu.Items.Add(toolStripItem);
            toolStripItem.Click += toolStripItem_Click;
            return toolStripItem;
        }

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

        void TextEditor_TextChanged(object sender, EventArgs e)
        {
            if (patchHighlighting)
                _internalFileViewer.AddPatchHighlighting();

            if (TextChanged != null)
                TextChanged(sender, e);
        }

        private void UpdateEncodingCombo()
        {
            if (this.Encoding.GetType() == typeof(ASCIIEncoding))
                this.encodingToolStripComboBox.Text = "ASCII";
            else if (this.Encoding.GetType() == typeof(UnicodeEncoding))
                this.encodingToolStripComboBox.Text = "Unicode";
            else if (this.Encoding.GetType() == typeof(UTF7Encoding))
                this.encodingToolStripComboBox.Text = "UTF7";
            else if (this.Encoding.GetType() == typeof(UTF8Encoding))
                this.encodingToolStripComboBox.Text = "UTF8";
            else if (this.Encoding.GetType() == typeof(UTF32Encoding))
                this.encodingToolStripComboBox.Text = "UTF32";
            else if (this.Encoding == Encoding.Default)
                this.encodingToolStripComboBox.Text = "Default (" + Encoding.Default.HeaderName + ")";
            else if (this.Encoding == Encoding.GetEncoding("CP852"))
                this.encodingToolStripComboBox.Text = "DOS852";
        }

        public event EventHandler<EventArgs> ExtraDiffArgumentsChanged;

        public void SetVisibilityDiffContextMenu(bool visible, bool isStaging_diff)
        {
            _currentViewIsPatch = visible;
            ignoreWhitespaceChangesToolStripMenuItem.Visible = visible;
            increaseNumberOfLinesToolStripMenuItem.Visible = visible;
            descreaseNumberOfLinesToolStripMenuItem.Visible = visible;
            showEntireFileToolStripMenuItem.Visible = visible;
            toolStripSeparator2.Visible = visible;
            treatAllFilesAsTextToolStripMenuItem.Visible = visible;
            copyNewVersionToolStripMenuItem.Visible = visible;
            copyOldVersionToolStripMenuItem.Visible = visible;
            cherrypickSelectedLinesToolStripMenuItem.Visible = visible && !isStaging_diff;
            copyPatchToolStripMenuItem.Visible = visible;
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

        public void ResetCurrentScrollPos()
        {
            _currentScrollPos = 0;
        }

        private void RestoreCurrentScrollPos()
        {
            if (_currentScrollPos < 0)
                return;
            ScrollPos = _currentScrollPos;
            ResetCurrentScrollPos();
        }

        public void ViewFile(string fileName)
        {
            ViewItem(fileName, () => GetImage(fileName), () => GetFileText(fileName),
                () => LocalizationHelpers.GetSubmoduleText(Module, fileName.TrimEnd('/'), ""));
        }

        public string GetText()
        {
            return _internalFileViewer.GetText();
        }

        public void ViewCurrentChanges(GitItemStatus item)
        {
            ViewCurrentChanges(item.Name, item.OldName, item.IsStaged, item.IsSubmodule, item.SubmoduleStatus);
        }

        public void ViewCurrentChanges(GitItemStatus item, bool isStaged)
        {
            ViewCurrentChanges(item.Name, item.OldName, isStaged, item.IsSubmodule, item.SubmoduleStatus);
        }

        public void ViewCurrentChanges(string fileName, string oldFileName, bool staged,
            bool isSubmodule, Task<GitSubmoduleStatus> status)
        {
            if (!isSubmodule)
                _async.Load(() => Module.GetCurrentChanges(fileName, oldFileName, staged, GetExtraDiffArguments(), Encoding),
                    ViewStagingPatch);
            else if (status != null)
                _async.Load(() =>
                    {
                        if (status.Result == null)
                            return string.Format("Submodule \"{0}\" has unresolved conflicts", fileName);
                        return LocalizationHelpers.ProcessSubmoduleStatus(Module, status.Result);
                    }, ViewPatch);
            else
                _async.Load(() => LocalizationHelpers.ProcessSubmodulePatch(Module, fileName,
                    Module.GetCurrentChanges(fileName, oldFileName, staged, GetExtraDiffArguments(), Encoding)), ViewPatch);
        }

        public void ViewStagingPatch(Patch patch)
        {
            ViewPatch(patch);
            Reset(true, true, true);
        }

        public void ViewPatch(Patch patch)
        {
            string text = patch != null ? patch.Text : "";
            ViewPatch(text);
        }

        public void ViewPatch(string text)
        {
            ResetForDiff();
            _internalFileViewer.SetText(text, isDiff: true);
            if (TextLoaded != null)
                TextLoaded(this, null);
            RestoreCurrentScrollPos();
        }

        public void ViewStagingPatch(Func<string> loadPatchText)
        {
            ViewPatch(loadPatchText);
            Reset(true, true, true);
        }

        public void ViewPatch(Func<string> loadPatchText)
        {
            _async.Load(loadPatchText, ViewPatch);
        }

        public void SetHighlighting(string highlightingSyntax)
        {
            _highlightingSyntax = highlightingSyntax;
            _internalFileViewer.SetHighlighting(highlightingSyntax);
        }

        public void ViewText(string fileName, string text)
        {
            ResetForText(fileName);

            //Check for binary file.
            if (FileHelper.IsBinaryFileAccordingToContent(text))
            {
                _internalFileViewer.SetText("Binary file: " + fileName + " (Detected)");
                if (TextLoaded != null)
                    TextLoaded(this, null);
                return;
            }

            _internalFileViewer.SetText(text);
            if (TextLoaded != null)
                TextLoaded(this, null);

            RestoreCurrentScrollPos();
        }

        public void ViewGitItemRevision(string fileName, string guid)
        {
            if (guid == GitRevision.UnstagedGuid) //working directory changes
            {
                ViewFile(fileName);
            }
            else
            {
                string blob = Module.GetFileBlobHash(fileName, guid);
                ViewGitItem(fileName, blob);
            }
        }

        private string GetFileTextIfBlobExists(string guid)
        {
            if (guid != "")
            {
                return Module.GetFileText(guid, Encoding);
            }
            else
            {
                return "";
            }
        }

        public void ViewGitItem(string fileName, string guid)
        {
            ViewItem(fileName, () => GetImage(fileName, guid), () => GetFileTextIfBlobExists(guid),
                () => LocalizationHelpers.GetSubmoduleText(Module, fileName.TrimEnd('/'), guid));
        }

        private void ViewItem(string fileName, Func<Image> getImage, Func<string> getFileText, Func<string> getSubmoduleText)
        {
            FilePreabmle = null;
            string fullPath = Path.GetFullPath(Path.Combine(Module.WorkingDir, fileName));
            if (fileName.EndsWith("/") || Directory.Exists(fullPath))
            {
                if (GitModule.IsValidGitWorkingDir(fullPath))
                {
                    _async.Load(getSubmoduleText, text => ViewText(fileName, text));
                }
                else
                {
                    ViewText(null, "Directory: " + fileName);
                }
            }
            else if (IsImage(fileName))
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


        private bool IsBinaryFile(string fileName)
        {
            return FileHelper.IsBinaryFile(Module, fileName);
        }

        private static bool IsImage(string fileName)
        {
            return FileHelper.IsImage(fileName);
        }

        private static bool IsIcon(string fileName)
        {
            return fileName.EndsWith(".ico", StringComparison.CurrentCultureIgnoreCase);
        }

        private Image GetImage(string fileName, string guid)
        {
            try
            {
                using (var stream = Module.GetFileStream(guid))
                {
                    return CreateImage(fileName, stream);
                }
            }
            catch
            {
                return null;
            }
        }

        private Image GetImage(string fileName)
        {
            try
            {
                using (Stream stream = File.OpenRead(Path.Combine(Module.WorkingDir, fileName)))
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

        private string GetFileText(string fileName)
        {
            string path;
            if (File.Exists(fileName))
                path = fileName;
            else
                path = Path.Combine(Module.WorkingDir, fileName);

            if (File.Exists(path))
            {
                using (var reader = new StreamReader(path, Module.FilesEncoding))
                {
                    var content = reader.ReadToEnd();
                    FilePreabmle = reader.CurrentEncoding.GetPreamble();
                    return content;
                }
            }
            else
            {
                return null;
            }
        }

        private void ResetForImage()
        {
            Reset(false, false);
            _internalFileViewer.SetHighlighting("Default");
        }

        private void ResetForText(string fileName)
        {
            Reset(false, true);

            if (_highlightingSyntax != null)
                _internalFileViewer.SetHighlighting(_highlightingSyntax);
            else if (fileName == null)
                _internalFileViewer.SetHighlighting("Default");
            else
                _internalFileViewer.SetHighlightingForFile(fileName);

            if (!string.IsNullOrEmpty(fileName) &&
                (fileName.EndsWith(".diff", StringComparison.OrdinalIgnoreCase) ||
                 fileName.EndsWith(".patch", StringComparison.OrdinalIgnoreCase)))
            {
                ResetForDiff();
            }
        }

        private bool patchHighlighting;
        private void ResetForDiff()
        {
            Reset(true, true);
            _internalFileViewer.SetHighlighting("");
            patchHighlighting = true;
        }

        private void Reset(bool diff, bool text)
        {
            Reset(diff, text, false);
        }

        private void Reset(bool diff, bool text, bool staging_diff)
        {
            patchHighlighting = diff;
            SetVisibilityDiffContextMenu(diff, staging_diff);
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

            AppSettings.IgnoreWhitespaceChanges = IgnoreWhitespaceChanges;
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
            string code = _internalFileViewer.GetSelectedText();
            if (string.IsNullOrEmpty(code))
                return;

            if (_currentViewIsPatch)
            {
                //add artificail space if selected text is not starting from line begining, it will be removed later
                int pos = _internalFileViewer.GetSelectionPosition();
                string fileText = _internalFileViewer.GetText();
                int hpos = fileText.IndexOf("\n@@");
                //if header is selected then don't remove diff extra chars
                if (hpos <= pos)
                {
                    if (pos > 0)
                        if (fileText[pos - 1] != '\n')
                            code = " " + code;

                    string[] lines = code.Split('\n');
                    lines.Transform(RemovePrefix);
                    code = string.Join("\n", lines);
                }
            }

            Clipboard.SetText(DoAutoCRLF(code));
        }

        private string RemovePrefix(string line)
        {
            var isCombinedDiff = DiffHighlightService.IsCombinedDiff(_internalFileViewer.GetText());
            var specials = isCombinedDiff ? new[]{"  ", "++", "+ ", " +", "--", "- ", " -"}
                : new[]{ " ", "-", "+" };
            if(string.IsNullOrWhiteSpace(line))
            {
                return line;
            }
            foreach (var special in specials.Where(line.StartsWith))
            {
                return line.Substring(special.Length);
            }
            return line;
        }

        private void CopyPatchToolStripMenuItemClick(object sender, EventArgs e)
        {
            var selectedText = _internalFileViewer.GetSelectedText();
            if (!string.IsNullOrEmpty(selectedText))
            {
                Clipboard.SetText(selectedText);
                return;
            }

            var text = _internalFileViewer.GetText();
            if (!text.IsNullOrEmpty())
                Clipboard.SetText(text);
        }

        public int GetSelectionPosition()
        {
            return _internalFileViewer.GetSelectionPosition();
        }

        public int GetSelectionLength()
        {
            return _internalFileViewer.GetSelectionLength();
        }

        public void GoToLine(int line)
        {
            _internalFileViewer.GoToLine(line);
        }

        public int GetLineFromVisualPosY(int visualPosY)
        {
            return _internalFileViewer.GetLineFromVisualPosY(visualPosY);
        }

        public string GetLineText(int line)
        {
            return _internalFileViewer.GetLineText(line);
        }

        public void HighlightLine(int line, Color color)
        {
            _internalFileViewer.HighlightLine(line, color);
        }

        public void HighlightLines(int startLine, int endLine, Color color)
        {
            _internalFileViewer.HighlightLines(startLine, endLine, color);
        }

        public void ClearHighlighting()
        {
            _internalFileViewer.ClearHighlighting();
        }

        private void NextChangeButtonClick(object sender, EventArgs e)
        {
            Focus();
            var firstVisibleLine = _internalFileViewer.LineAtCaret;
            var totalNumberOfLines = _internalFileViewer.TotalNumberOfLines;
            var emptyLineCheck = false;

            for (var line = firstVisibleLine + 1; line < totalNumberOfLines; line++)
            {
                var lineContent = _internalFileViewer.GetLineText(line);
                if (IsDiffLine(_internalFileViewer.GetText(), lineContent))
                {
                    if (emptyLineCheck)
                    {
                        _internalFileViewer.FirstVisibleLine = Math.Max(line - 4, 0);
                        GoToLine(line);
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

        private static bool IsDiffLine(string wholeText, string lineContent)
        {
            var isCombinedDiff = DiffHighlightService.IsCombinedDiff(wholeText);
            return lineContent.StartsWithAny(isCombinedDiff ? new[] {"+", "-", " +", " -"}
                : new[] {"+", "-"});
        }

        private void PreviousChangeButtonClick(object sender, EventArgs e)
        {
            Focus();
            var firstVisibleLine = _internalFileViewer.LineAtCaret;
            var emptyLineCheck = false;
            //go to the top of change block
            while (firstVisibleLine > 0 &&
                _internalFileViewer.GetLineText(firstVisibleLine).StartsWithAny(new string[] { "+", "-" }))
                firstVisibleLine--;

            for (var line = firstVisibleLine; line > 0; line--)
            {
                var lineContent = _internalFileViewer.GetLineText(line);
                if (lineContent.StartsWithAny(new string[] { "+", "-" })
                    && !lineContent.StartsWithAny(new string[] { "++", "--" }))
                {
                    emptyLineCheck = true;
                }
                else
                {
                    if (emptyLineCheck)
                    {
                        _internalFileViewer.FirstVisibleLine = Math.Max(0, line - 3);
                        GoToLine(line + 1);
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

        #region Hotkey commands

        public const string HotkeySettingsName = "FileViewer";

        internal enum Commands
        {
            Find,
            GoToLine,
            IncreaseNumberOfVisibleLines,
            DecreaseNumberOfVisibleLines,
            ShowEntireFile,
            TreatFileAsText,
            NextChange,
            PreviousChange
        }

        protected override bool ExecuteCommand(int cmd)
        {
            Commands command = (Commands)cmd;

            switch (command)
            {
                case Commands.Find: this.FindToolStripMenuItemClick(null, null); break;
                case Commands.GoToLine: this.goToLineToolStripMenuItem_Click(null, null); break;
                case Commands.IncreaseNumberOfVisibleLines: this.IncreaseNumberOfLinesToolStripMenuItemClick(null, null); break;
                case Commands.DecreaseNumberOfVisibleLines: this.DescreaseNumberOfLinesToolStripMenuItemClick(null, null); break;
                case Commands.ShowEntireFile: this.ShowEntireFileToolStripMenuItemClick(null, null); break;
                case Commands.TreatFileAsText: this.TreatAllFilesAsTextToolStripMenuItemClick(null, null); break;
                case Commands.NextChange: this.NextChangeButtonClick(null, null); break;
                case Commands.PreviousChange: this.PreviousChangeButtonClick(null, null); break;
                default: return base.ExecuteCommand(cmd);
            }

            return true;
        }

        #endregion

        public void Clear()
        {
            ViewText("", "");
        }

        public bool HasAnyPatches()
        {
            return (_internalFileViewer.GetText() != null && _internalFileViewer.GetText().Contains("@@"));
        }

        public void SetFileLoader(Func<bool, Tuple<int, string>> fileLoader){
            _internalFileViewer.SetFileLoader(fileLoader);
        }

        private void encodingToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Encoding encod = null;
            if (string.IsNullOrEmpty(encodingToolStripComboBox.Text))
                encod = Module.FilesEncoding;
            else if (encodingToolStripComboBox.Text.StartsWith("Default", StringComparison.CurrentCultureIgnoreCase))
                encod = Encoding.Default;
            else if (encodingToolStripComboBox.Text.Equals("DOS852", StringComparison.CurrentCultureIgnoreCase))
                encod = Encoding.GetEncoding("CP852");
            else if (encodingToolStripComboBox.Text.Equals("ASCII", StringComparison.CurrentCultureIgnoreCase))
                encod = new ASCIIEncoding();
            else if (encodingToolStripComboBox.Text.Equals("Unicode", StringComparison.CurrentCultureIgnoreCase))
                encod = new UnicodeEncoding();
            else if (encodingToolStripComboBox.Text.Equals("UTF7", StringComparison.CurrentCultureIgnoreCase))
                encod = new UTF7Encoding();
            else if (encodingToolStripComboBox.Text.Equals("UTF8", StringComparison.CurrentCultureIgnoreCase))
                encod = new UTF8Encoding(false);
            else if (encodingToolStripComboBox.Text.Equals("UTF32", StringComparison.CurrentCultureIgnoreCase))
                encod = new UTF32Encoding(true, false);
            else
                encod = Module.FilesEncoding;
            if (!encod.Equals(this.Encoding))
            {
                this.Encoding = encod;
                this.OnExtraDiffArgumentsChanged();
            }
        }

        private void fileviewerToolbar_VisibleChanged(object sender, EventArgs e)
        {
            if (fileviewerToolbar.Visible)
                UpdateEncodingCombo();
        }

        private void goToLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_internalFileViewer.IsGotoLineUIApplicable())
            {
                return;
            }
            using (FormGoToLine formGoToLine = new FormGoToLine())
            {
                formGoToLine.SetMaxLineNumber(_internalFileViewer.TotalNumberOfLines);
                if (formGoToLine.ShowDialog(this) == DialogResult.OK)
                    GoToLine(formGoToLine.GetLineNumber() - 1);
            }
        }

        private void CopyNotStartingWith(char startChar)
        {
            string code = _internalFileViewer.GetSelectedText();
            bool noSelection = false;
            if (string.IsNullOrEmpty(code))
            {
                code = _internalFileViewer.GetText();
                noSelection = true;
            }

            if (_currentViewIsPatch)
            {
                //add artificail space if selected text is not starting from line begining, it will be removed later
                int pos = noSelection ? 0 : _internalFileViewer.GetSelectionPosition();
                string fileText = _internalFileViewer.GetText();
                if (pos > 0)
                    if (fileText[pos - 1] != '\n')
                        code = " " + code;
                IEnumerable<string> lines = code.Split('\n');
                lines = lines.Where(s => s.Length == 0 || s[0] != startChar || (s.Length > 2 && s[1] == s[0] && s[2] == s[0]));
                int hpos = fileText.IndexOf("\n@@");
                //if header is selected then don't remove diff extra chars
                if (hpos <= pos)
                {
                    char[] specials = new char[] { ' ', '-', '+' };
                    lines = lines.Select(s => s.Length > 0 && specials.Any(c => c == s[0]) ? s.Substring(1) : s);
                }

                code = string.Join("\n", lines);
            }

            Clipboard.SetText(DoAutoCRLF(code));
        }

        private string DoAutoCRLF(string s)
        {
            if (Module.EffectiveConfigFile.core.autocrlf.Value == AutoCRLFType.@true)
            {
                return s.Replace("\n", Environment.NewLine);
            }

            return s;
        }

        private void copyNewVersionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyNotStartingWith('-');
        }

        private void copyOldVersionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyNotStartingWith('+');
        }

        private void cherrypickSelectedLines(int selectionStart, int selectionLength)
        {
            // Prepare git command
            string args = "apply --3way --whitespace=nowarn";

            byte[] patch;
            patch = PatchManager.GetSelectedLinesAsPatch(Module, GetText(),
                selectionStart, selectionLength,
                false, Encoding, false);

            if (patch != null && patch.Length > 0)
            {
                string output = Module.RunGitCmd(args, null, patch);
                if (!string.IsNullOrEmpty(output))
                {
                    if (!MergeConflictHandler.HandleMergeConflicts(UICommands, this, false, false))
                        MessageBox.Show(this, output + "\n\n" + Encoding.GetString(patch));
                }
            }
        }

        private void cherrypickSelectedLinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cherrypickSelectedLines(GetSelectionPosition(), GetSelectionLength());
        }

        public void CherryPickAllChanges()
        {
            if (GetText().Length > 0)
            {
                cherrypickSelectedLines(0, GetText().Length);
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GitUICommandsSourceSet -= FileViewer_GitUICommandsSourceSet;
                _async.Dispose();
                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}