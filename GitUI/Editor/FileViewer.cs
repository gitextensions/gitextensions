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
using GitUI.CommandsDialogs.SettingsDialog.Pages;
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
            NumberOfVisibleLines = AppSettings.NumberOfContextLines;
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
                    if (!IsDisposed)
                    {
                        ResetForText(null);
                        _internalFileViewer.SetText("Unsupported file: \n\n" + args.Exception.ToString());
                        if (TextLoaded != null)
                            TextLoaded(this, null);
                    }
                };

            IgnoreWhitespaceChanges = AppSettings.IgnoreWhitespaceChanges;
            ignoreWhiteSpaces.Checked = IgnoreWhitespaceChanges;
            ignoreWhiteSpaces.Image = GitUI.Properties.Resources.ignore_whitespaces;
            ignoreWhitespaceChangesToolStripMenuItem.Checked = IgnoreWhitespaceChanges;
            ignoreWhitespaceChangesToolStripMenuItem.Image = ignoreWhiteSpaces.Image;

            ignoreAllWhitespaces.Checked = AppSettings.IgnoreAllWhitespaceChanges;
            ignoreAllWhitespaces.Image = GitUI.Properties.Resources.ignore_all_whitespaces;
            ignoreAllWhitespaceChangesToolStripMenuItem.Checked = ignoreAllWhitespaces.Checked;
            ignoreAllWhitespaceChangesToolStripMenuItem.Image = ignoreAllWhitespaces.Image;

            ShowEntireFile = AppSettings.ShowEntireFile;
            showEntireFileButton.Checked = ShowEntireFile;
            showEntireFileToolStripMenuItem.Checked = ShowEntireFile;

            showNonPrintChars.Checked = AppSettings.ShowNonPrintingChars;
            showNonprintableCharactersToolStripMenuItem.Checked = AppSettings.ShowNonPrintingChars;
            ToggleNonPrintingChars(AppSettings.ShowNonPrintingChars);

            IsReadOnly = true;
            var encodingList = AppSettings.AvailableEncodings.Values.Select(e => e.EncodingName).ToList();
            var defaultEncodingName = Encoding.Default.EncodingName;

            for (int i = 0; i < encodingList.Count; i++)
            {
                if (string.Compare(encodingList[i], defaultEncodingName, true) == 0)
                {
                    encodingList[i] = "Default (" + Encoding.Default.HeaderName + ")";
                    break;
                }
            }
            encodingToolStripComboBox.Items.AddRange(encodingList.ToArray());

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
            UICommandsSource.GitUICommandsChanged += UICommandsSourceChanged;
            UICommandsSourceChanged(UICommandsSource, null);
        }

        protected override void DisposeUICommandsSource()
        {
            UICommandsSource.GitUICommandsChanged -= UICommandsSourceChanged;
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
        public byte[] FilePreamble { get; private set; }

        private void UICommandsSourceChanged(object sender, GitUICommandsChangedEventArgs e)
        {
            if(e?.OldCommands != null)
                e.OldCommands.PostSettings -= UICommands_PostSettings;

            var commandSource = sender as IGitUICommandsSource;
            if( commandSource?.UICommands != null)
                commandSource.UICommands.PostSettings += UICommands_PostSettings;

            this.Encoding = null;
        }

        private void UICommands_PostSettings( object sender, GitUIPluginInterfaces.GitUIPostActionEventArgs e )
        {
            _internalFileViewer.VRulerPosition = AppSettings.DiffVerticalRulerPosition;
        }

        protected override void OnRuntimeLoad(EventArgs e)
        {
            ReloadHotkeys();
            Font = AppSettings.DiffFont;
        }

        public void ReloadHotkeys()
        {
            this.Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
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
            if (Encoding != null)
                encodingToolStripComboBox.Text = Encoding.EncodingName;
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
            //TODO The following should not be enabled if this is afile and the file does not exist
            cherrypickSelectedLinesToolStripMenuItem.Visible = visible && !isStaging_diff && !Module.IsBareRepository();
            revertSelectedLinesToolStripMenuItem.Visible = visible && !isStaging_diff && !Module.IsBareRepository();
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

            if (ignoreAllWhitespaces.Checked)
            {
                diffArguments.Append(" --ignore-all-space");
            }
            else
            {
                if (IgnoreWhitespaceChanges)
                    diffArguments.Append(" --ignore-space-change");
            }

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
            if (guid == GitRevision.UnstagedGuid)
            {
                //No blob exists for unstaged, present contents from file system
                ViewFile(fileName);
            }
            else
            {
                //Retrieve blob, same as GitItemStatus.TreeGuid
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
            FilePreamble = null;

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
            //Check binary from extension/attributes (a secondary check for file contents before display)
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
                // StreamReader disposes of 'fileStream'.
                // see: https://msdn.microsoft.com/library/ms182334.aspx
                var fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                try
                {
                    using (var reader = new StreamReader(fileStream, Module.FilesEncoding))
                    {
                        fileStream = null;
                        var content = reader.ReadToEnd();
                        FilePreamble = reader.CurrentEncoding.GetPreamble();
                        return content;
                    }
                }
                finally
                {
                    fileStream?.Dispose();
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
            AppSettings.NumberOfContextLines = NumberOfVisibleLines;
            OnExtraDiffArgumentsChanged();
        }

        private void DescreaseNumberOfLinesToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (NumberOfVisibleLines > 0)
                NumberOfVisibleLines--;
            else
                NumberOfVisibleLines = 0;
            AppSettings.NumberOfContextLines = NumberOfVisibleLines;
            OnExtraDiffArgumentsChanged();
        }

        private void ShowEntireFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            ShowEntireFile = !ShowEntireFile;
            showEntireFileButton.Checked = ShowEntireFile;
            showEntireFileToolStripMenuItem.Checked = ShowEntireFile;
            AppSettings.ShowEntireFile = ShowEntireFile;
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
            var specials = isCombinedDiff ? new[] { "  ", "++", "+ ", " +", "--", "- ", " -" }
                : new[] { " ", "-", "+" };

            if (string.IsNullOrWhiteSpace(line))
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
            return lineContent.StartsWithAny(isCombinedDiff ? new[] { "+", "-", " +", " -" }
                : new[] { "+", "-" });
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

            ToggleNonPrintingChars(show: showNonprintableCharactersToolStripMenuItem.Checked);
            AppSettings.ShowNonPrintingChars = showNonPrintChars.Checked;
        }

        private void ToggleNonPrintingChars(bool show)
        {
            _internalFileViewer.ShowEOLMarkers = show;
            _internalFileViewer.ShowSpaces = show;
            _internalFileViewer.ShowTabs = show;
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

        public static readonly string HotkeySettingsName = "FileViewer";

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

        public void SetFileLoader(Func<bool, Tuple<int, string>> fileLoader)
        {
            _internalFileViewer.SetFileLoader(fileLoader);
        }

        private void encodingToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Encoding encod = null;

            if (string.IsNullOrEmpty(encodingToolStripComboBox.Text))
                encod = Module.FilesEncoding;
            else if (encodingToolStripComboBox.Text.StartsWith("Default", StringComparison.CurrentCultureIgnoreCase))
                encod = Encoding.Default;
            else
                encod = AppSettings.AvailableEncodings.Values
                      .Where(en => en.EncodingName == encodingToolStripComboBox.Text)
                      .FirstOrDefault() ?? Module.FilesEncoding;
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
            if (Module.EffectiveConfigFile.core.autocrlf.ValueOrDefault == AutoCRLFType.@true)
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

        private void applySelectedLines(int selectionStart, int selectionLength, bool reverse)
        {
            // Prepare git command
            string args = "apply --3way --whitespace=nowarn";

            byte[] patch;

            if (reverse)
            {
                patch = PatchManager.GetResetUnstagedLinesAsPatch(Module, GetText(),
                selectionStart, selectionLength,
                false, Encoding);
            }
            else
            {
                patch = PatchManager.GetSelectedLinesAsPatch(Module, GetText(),
                selectionStart, selectionLength,
                false, Encoding, false);
            }
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
            applySelectedLines(GetSelectionPosition(), GetSelectionLength(), reverse: false);
        }

        public void CherryPickAllChanges()
        {
            if (GetText().Length > 0)
            {
                applySelectedLines(0, GetText().Length, reverse: false);
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
                if (IsUICommandsInitialized)
                {
                    UICommands.PostSettings -= UICommands_PostSettings;
                }
            }
            base.Dispose(disposing);
        }

        private void settingsButton_Click(object sender, EventArgs e)
        {
            UICommands.StartSettingsDialog(this.ParentForm, DiffViewerSettingsPage.GetPageReference());
        }

        private void revertSelectedLinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            applySelectedLines(GetSelectionPosition(), GetSelectionLength(), reverse: true);
        }

        private void ignoreAllWhitespaceChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newIgnoreValue = !ignoreAllWhitespaces.Checked;
            ignoreAllWhitespaces.Checked = newIgnoreValue;
            ignoreAllWhitespaceChangesToolStripMenuItem.Checked = newIgnoreValue;
            AppSettings.IgnoreAllWhitespaceChanges = newIgnoreValue;
            OnExtraDiffArgumentsChanged();
        }
    }
}
