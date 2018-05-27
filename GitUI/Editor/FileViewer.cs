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
using GitCommands.Patches;
using GitCommands.Settings;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.Editor.Diff;
using GitUI.Hotkey;
using JetBrains.Annotations;
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
        private readonly IFullPathResolver _fullPathResolver;

        public FileViewer()
        {
            TreatAllFilesAsText = false;
            ShowEntireFile = false;
            NumberOfVisibleLines = AppSettings.NumberOfContextLines;
            InitializeComponent();
            Translate();

            GitUICommandsSourceSet += FileViewer_GitUICommandsSourceSet;

            _internalFileViewer = new FileViewerInternal(() => Module);
            _internalFileViewer.MouseEnter += _internalFileViewer_MouseEnter;
            _internalFileViewer.MouseLeave += _internalFileViewer_MouseLeave;
            _internalFileViewer.MouseMove += _internalFileViewer_MouseMove;
            _internalFileViewer.KeyUp += _internalFileViewer_KeyUp;

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
                        _internalFileViewer.SetText("Unsupported file: \n\n" + args.Exception.ToString(), openWithDifftool: null /* not applicable */);
                        TextLoaded?.Invoke(this, null);
                    }
                };

            IgnoreWhitespaceChanges = AppSettings.IgnoreWhitespaceChanges;
            ignoreWhiteSpaces.Checked = IgnoreWhitespaceChanges;
            ignoreWhiteSpaces.Image = Properties.Resources.ignore_whitespaces;
            ignoreWhitespaceChangesToolStripMenuItem.Checked = IgnoreWhitespaceChanges;
            ignoreWhitespaceChangesToolStripMenuItem.Image = ignoreWhiteSpaces.Image;

            ignoreAllWhitespaces.Checked = AppSettings.IgnoreAllWhitespaceChanges;
            ignoreAllWhitespaces.Image = Properties.Resources.ignore_all_whitespaces;
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

            HotkeysEnabled = true;

            if (RunTime() && ContextMenuStrip == null)
            {
                ContextMenuStrip = contextMenu;
            }

            contextMenu.Opening += ContextMenu_Opening;
            _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
        }

        private void _internalFileViewer_KeyUp(object sender, KeyEventArgs e)
        {
            OnKeyUp(e);
        }

        private void FileViewer_GitUICommandsSourceSet(object sender, GitUICommandsSourceEventArgs e)
        {
            UICommandsSource.GitUICommandsChanged += UICommandsSourceChanged;
            UICommandsSourceChanged(UICommandsSource, null);
        }

        protected override void DisposeUICommandsSource()
        {
            UICommandsSource.GitUICommandsChanged -= UICommandsSourceChanged;
            base.DisposeUICommandsSource();
        }

        private static bool RunTime()
        {
            return System.Diagnostics.Process.GetCurrentProcess().ProcessName != "devenv";
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public new Font Font
        {
            get => _internalFileViewer.Font;
            set => _internalFileViewer.Font = value;
        }

        public Action OpenWithDifftool
        {
            get => _internalFileViewer.OpenWithDifftool;
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
            get => _internalFileViewer.IsReadOnly;
            set => _internalFileViewer.IsReadOnly = value;
        }

        [DefaultValue(true)]
        [Description("If true line numbers are shown in the textarea")]
        [Category("Appearance")]
        public bool ShowLineNumbers
        {
            get => _internalFileViewer.ShowLineNumbers;
            set => _internalFileViewer.ShowLineNumbers = value;
        }

        private Encoding _encoding;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Encoding Encoding
        {
            get
            {
                if (_encoding == null)
                {
                    _encoding = Module.FilesEncoding;
                }

                return _encoding;
            }

            set
            {
                _encoding = value;
            }
        }

        [DefaultValue(0)]
        [Browsable(false)]
        public int ScrollPos
        {
            get => _internalFileViewer.ScrollPos;
            set => _internalFileViewer.ScrollPos = value;
        }

        [Browsable(false)]
        public byte[] FilePreamble { get; private set; }

        private void UICommandsSourceChanged(object sender, GitUICommandsChangedEventArgs e)
        {
            if (e?.OldCommands != null)
            {
                e.OldCommands.PostSettings -= UICommands_PostSettings;
            }

            var commandSource = sender as IGitUICommandsSource;
            if (commandSource?.UICommands != null)
            {
                commandSource.UICommands.PostSettings += UICommands_PostSettings;
            }

            Encoding = null;
        }

        private void UICommands_PostSettings(object sender, GitUIPluginInterfaces.GitUIPostActionEventArgs e)
        {
            _internalFileViewer.VRulerPosition = AppSettings.DiffVerticalRulerPosition;
        }

        protected override void OnRuntimeLoad()
        {
            ReloadHotkeys();
            Font = AppSettings.DiffFont;
        }

        public void ReloadHotkeys()
        {
            Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
        }

        private void ContextMenu_Opening(object sender, CancelEventArgs e)
        {
            copyToolStripMenuItem.Enabled = _internalFileViewer.GetSelectionLength() > 0;
            ContextMenuOpening?.Invoke(sender, e);
        }

        private void _internalFileViewer_MouseMove(object sender, MouseEventArgs e)
        {
            OnMouseMove(e);
        }

        private void _internalFileViewer_MouseEnter(object sender, EventArgs e)
        {
            OnMouseEnter(e);
        }

        private void _internalFileViewer_MouseLeave(object sender, EventArgs e)
        {
            OnMouseLeave(e);
        }

        private void _internalFileViewer_SelectedLineChanged(object sender, SelectedLineEventArgs e)
        {
            SelectedLineChanged?.Invoke(sender, e);
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
            RequestDiffView?.Invoke(this, args);
        }

        private void _internalFileViewer_ScrollPosChanged(object sender, EventArgs e)
        {
            ScrollPosChanged?.Invoke(sender, e);
        }

        public void EnableScrollBars(bool enable)
        {
            _internalFileViewer.EnableScrollBars(enable);
        }

        private void TextEditor_TextChanged(object sender, EventArgs e)
        {
            if (_patchHighlighting)
            {
                _internalFileViewer.AddPatchHighlighting();
            }

            TextChanged?.Invoke(sender, e);
        }

        private void UpdateEncodingCombo()
        {
            if (Encoding != null)
            {
                encodingToolStripComboBox.Text = Encoding.EncodingName;
            }
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

            // TODO The following should not be enabled if this is afile and the file does not exist
            cherrypickSelectedLinesToolStripMenuItem.Visible = visible && !isStaging_diff && !Module.IsBareRepository();
            revertSelectedLinesToolStripMenuItem.Visible = visible && !isStaging_diff && !Module.IsBareRepository();
            copyPatchToolStripMenuItem.Visible = visible;
        }

        private void OnExtraDiffArgumentsChanged()
        {
            ExtraDiffArgumentsChanged?.Invoke(this, EventArgs.Empty);
        }

        [NotNull]
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
                {
                    diffArguments.Append(" --ignore-space-change");
                }
            }

            if (ShowEntireFile)
            {
                diffArguments.AppendFormat(" --inter-hunk-context=9000 --unified=9000");
            }
            else
            {
                diffArguments.AppendFormat(" --unified={0}", NumberOfVisibleLines);
            }

            if (TreatAllFilesAsText)
            {
                diffArguments.Append(" --text");
            }

            return diffArguments.ToString();
        }

        private void TextAreaMouseMove(object sender, MouseEventArgs e)
        {
            if (_currentViewIsPatch && !fileviewerToolbar.Visible)
            {
                fileviewerToolbar.Visible = true;
                fileviewerToolbar.Location = new Point(Width - fileviewerToolbar.Width - 40, 0);
                fileviewerToolbar.BringToFront();
            }
        }

        private void TextAreaMouseLeave(object sender, EventArgs e)
        {
            if (GetChildAtPoint(PointToClient(MousePosition)) != fileviewerToolbar &&
                fileviewerToolbar != null)
            {
                fileviewerToolbar.Visible = false;
            }
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
            {
                return;
            }

            ScrollPos = _currentScrollPos;
            ResetCurrentScrollPos();
        }

        public Task ViewFileAsync(string fileName)
        {
            return ViewItemAsync(fileName, getImage: () => GetImage(fileName), getFileText: () => GetFileText(fileName),
                getSubmoduleText: () => LocalizationHelpers.GetSubmoduleText(Module, fileName.TrimEnd('/'), ""),
                openWithDifftool: null /* not implemented */);
        }

        public string GetText()
        {
            return _internalFileViewer.GetText();
        }

        public void ViewCurrentChanges(GitItemStatus item)
        {
            ViewCurrentChanges(item.Name, item.OldName, item.IsStaged, item.IsSubmodule, item.GetSubmoduleStatusAsync, null /* not implemented */);
        }

        public void ViewCurrentChanges(GitItemStatus item, bool isStaged, [CanBeNull] Action openWithDifftool)
        {
            ViewCurrentChanges(item.Name, item.OldName, isStaged, item.IsSubmodule, item.GetSubmoduleStatusAsync, openWithDifftool);
        }

        public void ViewCurrentChanges(string fileName, string oldFileName, bool staged,
            bool isSubmodule, Func<Task<GitSubmoduleStatus>> getStatusAsync, [CanBeNull] Action openWithDifftool)
        {
            if (!isSubmodule)
            {
                _async.LoadAsync(() => { return (patch: Module.GetCurrentChanges(fileName, oldFileName, staged, GetExtraDiffArguments(), Encoding), openWithDifftool); },
                    (patch) => ViewStagingPatch(patch.patch, patch.openWithDifftool));
            }
            else if (getStatusAsync() != null)
            {
                ViewPatchAsync(() =>
                    {
                        var status = ThreadHelper.JoinableTaskFactory.Run(() => getStatusAsync());
                        if (status == null)
                        {
                            return (text: string.Format("Submodule \"{0}\" has unresolved conflicts", fileName),
                                    openWithDifftool: null /* not applicable */);
                        }

                        return (text: LocalizationHelpers.ProcessSubmoduleStatus(Module, status),
                                openWithDifftool: null /* not implemented */);
                    });
            }
            else
            {
                ViewPatchAsync(() =>
                    (text: LocalizationHelpers.ProcessSubmodulePatch(Module, fileName,
                               Module.GetCurrentChanges(fileName, oldFileName, staged, GetExtraDiffArguments(), Encoding)),
                     openWithDifftool: null /* not implemented */));
            }
        }

        public void ViewStagingPatch(Patch patch, [CanBeNull] Action openWithDifftool)
        {
            ViewPatch(patch, openWithDifftool);
            Reset(true, true, true);
        }

        public void ViewPatch([CanBeNull] Patch patch, [CanBeNull] Action openWithDifftool = null)
        {
            ViewPatch(patch?.Text ?? "", openWithDifftool);
        }

        public void ViewPatch([NotNull] string text, [CanBeNull] Action openWithDifftool)
        {
            ResetForDiff();
            _internalFileViewer.SetText(text, openWithDifftool, isDiff: true);
            TextLoaded?.Invoke(this, null);
            RestoreCurrentScrollPos();
        }

        public Task ViewPatchAsync(Func<(string text, Action openWithDifftool)> loadPatchText)
        {
            return _async.LoadAsync(loadPatchText, patchText => ViewPatch(patchText.text, patchText.openWithDifftool));
        }

        public Task ViewTextAsync([NotNull] string fileName, [NotNull] string text, [CanBeNull] Action openWithDifftool = null)
        {
            ResetForText(fileName);

            // Check for binary file.
            if (FileHelper.IsBinaryFileAccordingToContent(text))
            {
                _internalFileViewer.SetText("Binary file: " + fileName + " (Detected)", openWithDifftool);
                TextLoaded?.Invoke(this, null);
                return Task.CompletedTask;
            }

            _internalFileViewer.SetText(text, openWithDifftool);
            TextLoaded?.Invoke(this, null);

            RestoreCurrentScrollPos();
            return Task.CompletedTask;
        }

        public Task ViewGitItemRevisionAsync(string fileName, string guid)
        {
            if (guid == GitRevision.UnstagedGuid)
            {
                // No blob exists for unstaged, present contents from file system
                return ViewFileAsync(fileName);
            }
            else
            {
                // Retrieve blob, same as GitItemStatus.TreeGuid
                string blob = Module.GetFileBlobHash(fileName, guid);
                return ViewGitItemAsync(fileName, blob);
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

        public Task ViewGitItemAsync(string fileName, string guid)
        {
            return ViewItemAsync(fileName, getImage: () => GetImage(fileName, guid), getFileText: () => GetFileTextIfBlobExists(guid),
                getSubmoduleText: () => LocalizationHelpers.GetSubmoduleText(Module, fileName.TrimEnd('/'), guid),
                openWithDifftool: () => Module.OpenWithDifftool(fileName, firstRevision: guid));
        }

        private Task ViewItemAsync(string fileName, Func<Image> getImage, Func<string> getFileText, Func<string> getSubmoduleText, [CanBeNull] Action openWithDifftool)
        {
            FilePreamble = null;

            string fullPath = Path.GetFullPath(_fullPathResolver.Resolve(fileName));

            if (fileName.EndsWith("/") || Directory.Exists(fullPath))
            {
                if (GitModule.IsValidGitWorkingDir(fullPath))
                {
                    return _async.LoadAsync(getSubmoduleText, text => ViewTextAsync(fileName, text, openWithDifftool));
                }
                else
                {
                    return ViewTextAsync(fileName, "Directory: " + fileName, openWithDifftool: null /* not applicable */);
                }
            }
            else if (IsImage(fileName))
            {
                return _async.LoadAsync(getImage,
                            image =>
                            {
                                ResetForImage();
                                if (image != null)
                                {
                                    // TODO review whether we need DPI scaling here
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

            // Check binary from extension/attributes (a secondary check for file contents before display)
            else if (IsBinaryFile(fileName))
            {
                return ViewTextAsync(fileName, "Binary file: " + fileName, openWithDifftool);
            }
            else
            {
                return _async.LoadAsync(getFileText, text => ViewTextAsync(fileName, text, openWithDifftool));
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
                using (Stream stream = File.OpenRead(_fullPathResolver.Resolve(fileName)))
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
            var path = File.Exists(fileName)
                ? fileName
                : _fullPathResolver.Resolve(fileName);

            if (!File.Exists(path))
            {
                return null;
            }

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream, Module.FilesEncoding))
            {
                var content = reader.ReadToEnd();
                FilePreamble = reader.CurrentEncoding.GetPreamble();
                return content;
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

            if (fileName == null)
            {
                _internalFileViewer.SetHighlighting("Default");
            }
            else
            {
                _internalFileViewer.SetHighlightingForFile(fileName);
            }

            if (!string.IsNullOrEmpty(fileName) &&
                (fileName.EndsWith(".diff", StringComparison.OrdinalIgnoreCase) ||
                 fileName.EndsWith(".patch", StringComparison.OrdinalIgnoreCase)))
            {
                ResetForDiff();
            }
        }

        private bool _patchHighlighting;

        private void ResetForDiff()
        {
            Reset(true, true);
            _internalFileViewer.SetHighlighting("");
            _patchHighlighting = true;
        }

        private void Reset(bool diff, bool text, bool staging_diff = false)
        {
            _patchHighlighting = diff;
            SetVisibilityDiffContextMenu(diff, staging_diff);
            ClearImage();
            PictureBox.Visible = !text;
            _internalFileViewer.Visible = text;
        }

        private void ClearImage()
        {
            PictureBox.ImageLocation = "";

            if (PictureBox.Image == null)
            {
                return;
            }

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
            {
                NumberOfVisibleLines--;
            }
            else
            {
                NumberOfVisibleLines = 0;
            }

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
            {
                return;
            }

            if (_currentViewIsPatch)
            {
                // add artificail space if selected text is not starting from line begining, it will be removed later
                int pos = _internalFileViewer.GetSelectionPosition();
                string fileText = _internalFileViewer.GetText();
                int hpos = fileText.IndexOf("\n@@");

                // if header is selected then don't remove diff extra chars
                if (hpos <= pos)
                {
                    if (pos > 0)
                    {
                        if (fileText[pos - 1] != '\n')
                        {
                            code = " " + code;
                        }
                    }

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
            {
                Clipboard.SetText(text);
            }
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

            // Do not go to the end of the file if no change is found
            ////TextEditor.ActiveTextAreaControl.TextArea.TextView.FirstVisibleLine = totalNumberOfLines - TextEditor.ActiveTextAreaControl.TextArea.TextView.VisibleLineCount;
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

            // go to the top of change block
            while (firstVisibleLine > 0 &&
                _internalFileViewer.GetLineText(firstVisibleLine).StartsWithAny(new[] { "+", "-" }))
            {
                firstVisibleLine--;
            }

            for (var line = firstVisibleLine; line > 0; line--)
            {
                var lineContent = _internalFileViewer.GetLineText(line);

                if (lineContent.StartsWithAny(new[] { "+", "-" })
                    && !lineContent.StartsWithAny(new[] { "++", "--" }))
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

            // Do not go to the start of the file if no change is found
            ////TextEditor.ActiveTextAreaControl.TextArea.TextView.FirstVisibleLine = 0;
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
            Find = 0,
            FindNextOrOpenWithDifftool = 8,
            FindPrevious = 9,
            GoToLine = 1,
            IncreaseNumberOfVisibleLines = 2,
            DecreaseNumberOfVisibleLines = 3,
            ShowEntireFile = 4,
            TreatFileAsText = 5,
            NextChange = 6,
            PreviousChange = 7
        }

        protected override bool ExecuteCommand(int cmd)
        {
            Commands command = (Commands)cmd;

            switch (command)
            {
                case Commands.Find: _internalFileViewer.Find(); break;
                case Commands.FindNextOrOpenWithDifftool: _internalFileViewer.FindNextAsync(searchForwardOrOpenWithDifftool: true); break;
                case Commands.FindPrevious: _internalFileViewer.FindNextAsync(searchForwardOrOpenWithDifftool: false); break;
                case Commands.GoToLine: goToLineToolStripMenuItem_Click(null, null); break;
                case Commands.IncreaseNumberOfVisibleLines: IncreaseNumberOfLinesToolStripMenuItemClick(null, null); break;
                case Commands.DecreaseNumberOfVisibleLines: DescreaseNumberOfLinesToolStripMenuItemClick(null, null); break;
                case Commands.ShowEntireFile: ShowEntireFileToolStripMenuItemClick(null, null); break;
                case Commands.TreatFileAsText: TreatAllFilesAsTextToolStripMenuItemClick(null, null); break;
                case Commands.NextChange: NextChangeButtonClick(null, null); break;
                case Commands.PreviousChange: PreviousChangeButtonClick(null, null); break;
                default: return base.ExecuteCommand(cmd);
            }

            return true;
        }

        #endregion

        public void Clear()
        {
            ViewTextAsync("", "");
        }

        public bool HasAnyPatches()
        {
            return _internalFileViewer.GetText() != null && _internalFileViewer.GetText().Contains("@@");
        }

        public void SetFileLoader(GetNextFileFnc fileLoader)
        {
            _internalFileViewer.SetFileLoader(fileLoader);
        }

        private void encodingToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Encoding encod;
            if (string.IsNullOrEmpty(encodingToolStripComboBox.Text))
            {
                encod = Module.FilesEncoding;
            }
            else if (encodingToolStripComboBox.Text.StartsWith("Default", StringComparison.CurrentCultureIgnoreCase))
            {
                encod = Encoding.Default;
            }
            else
            {
                encod = AppSettings.AvailableEncodings.Values
                    .FirstOrDefault(en => en.EncodingName == encodingToolStripComboBox.Text)
                        ?? Module.FilesEncoding;
            }

            if (!encod.Equals(Encoding))
            {
                Encoding = encod;
                OnExtraDiffArgumentsChanged();
            }
        }

        private void fileviewerToolbar_VisibleChanged(object sender, EventArgs e)
        {
            if (fileviewerToolbar.Visible)
            {
                UpdateEncodingCombo();
            }
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
                {
                    GoToLine(formGoToLine.GetLineNumber() - 1);
                }
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
                // add artificail space if selected text is not starting from line begining, it will be removed later
                int pos = noSelection ? 0 : _internalFileViewer.GetSelectionPosition();
                string fileText = _internalFileViewer.GetText();

                if (pos > 0)
                {
                    if (fileText[pos - 1] != '\n')
                    {
                        code = " " + code;
                    }
                }

                IEnumerable<string> lines = code.Split('\n');
                lines = lines.Where(s => s.Length == 0 || s[0] != startChar || (s.Length > 2 && s[1] == s[0] && s[2] == s[0]));
                int hpos = fileText.IndexOf("\n@@");

                // if header is selected then don't remove diff extra chars
                if (hpos <= pos)
                {
                    char[] specials = { ' ', '-', '+' };
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
            const string args = "apply --3way --whitespace=nowarn";

            byte[] patch;

            if (reverse)
            {
                patch = PatchManager.GetResetUnstagedLinesAsPatch(
                    Module, GetText(),
                    selectionStart, selectionLength, Encoding);
            }
            else
            {
                patch = PatchManager.GetSelectedLinesAsPatch(
                    GetText(),
                    selectionStart, selectionLength,
                    false, Encoding, false);
            }

            if (patch != null && patch.Length > 0)
            {
                string output = Module.RunGitCmd(args, null, patch);

                if (!string.IsNullOrEmpty(output))
                {
                    if (!MergeConflictHandler.HandleMergeConflicts(UICommands, this, false, false))
                    {
                        MessageBox.Show(this, output + "\n\n" + Encoding.GetString(patch));
                    }
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
                components?.Dispose();
                if (IsUICommandsInitialized)
                {
                    UICommands.PostSettings -= UICommands_PostSettings;
                }
            }

            base.Dispose(disposing);
        }

        private void settingsButton_Click(object sender, EventArgs e)
        {
            UICommands.StartSettingsDialog(ParentForm, DiffViewerSettingsPage.GetPageReference());
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
