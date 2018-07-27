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
using GitExtUtils.GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.Editor.Diff;
using GitUI.Hotkey;
using GitUI.Properties;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.Editor
{
    [DefaultEvent("SelectedLineChanged")]
    public partial class FileViewer : GitModuleControl
    {
        public event EventHandler<SelectedLineEventArgs> SelectedLineChanged;
        public event EventHandler ScrollPosChanged;
        public event EventHandler RequestDiffView;
        public new event EventHandler TextChanged;
        public event EventHandler TextLoaded;
        public event CancelEventHandler ContextMenuOpening;
        public event EventHandler<EventArgs> ExtraDiffArgumentsChanged;

        private readonly AsyncLoader _async;
        private readonly IFileViewer _internalFileViewer;
        private readonly IFullPathResolver _fullPathResolver;
        private int _currentScrollPos = -1;
        private bool _currentViewIsPatch;
        private bool _patchHighlighting;
        private Encoding _encoding;

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
        [Browsable(false)]
        public byte[] FilePreamble { get; private set; }

        public FileViewer()
        {
            TreatAllFilesAsText = false;
            ShowEntireFile = false;
            NumberOfVisibleLines = AppSettings.NumberOfContextLines;
            InitializeComponent();
            InitializeComplete();

            UICommandsSourceSet += FileViewer_UICommandsSourceSet;

            _internalFileViewer = new FileViewerInternal(() => Module);
            _internalFileViewer.MouseEnter += (_, e) => OnMouseEnter(e);
            _internalFileViewer.MouseLeave += (_, e) => OnMouseLeave(e);
            _internalFileViewer.MouseMove += (_, e) => OnMouseMove(e);
            _internalFileViewer.KeyUp += (_, e) => OnKeyUp(e);

            var internalFileViewerControl = (Control)_internalFileViewer;
            internalFileViewerControl.Dock = DockStyle.Fill;
            Controls.Add(internalFileViewerControl);

            _async = new AsyncLoader();
            _async.LoadingError +=
                (_, e) =>
                {
                    if (!IsDisposed)
                    {
                        ResetForText(null);
                        _internalFileViewer.SetText("Unsupported file: \n\n" + e.Exception.ToString(), openWithDifftool: null /* not applicable */);
                        TextLoaded?.Invoke(this, null);
                    }
                };

            IgnoreWhitespaceChanges = AppSettings.IgnoreWhitespaceChanges;
            ignoreWhiteSpaces.Checked = IgnoreWhitespaceChanges;
            ignoreWhiteSpaces.Image = Images.WhitespaceIgnore;
            ignoreWhitespaceChangesToolStripMenuItem.Checked = IgnoreWhitespaceChanges;
            ignoreWhitespaceChangesToolStripMenuItem.Image = ignoreWhiteSpaces.Image;

            ignoreAllWhitespaces.Checked = AppSettings.IgnoreAllWhitespaceChanges;
            ignoreAllWhitespaces.Image = Images.WhitespaceIgnoreAll;
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
                if (string.Equals(encodingList[i], defaultEncodingName, StringComparison.OrdinalIgnoreCase))
                {
                    encodingList[i] = "Default (" + Encoding.Default.HeaderName + ")";
                    break;
                }
            }

            encodingToolStripComboBox.Items.AddRange(encodingList.ToArray<object>());

            _internalFileViewer.MouseMove += (_, e) =>
            {
                if (_currentViewIsPatch && !fileviewerToolbar.Visible)
                {
                    fileviewerToolbar.Visible = true;
                    fileviewerToolbar.Location = new Point(Width - fileviewerToolbar.Width - 40, 0);
                    fileviewerToolbar.BringToFront();
                }
            };
            _internalFileViewer.MouseLeave += (_, e) =>
            {
                if (GetChildAtPoint(PointToClient(MousePosition)) != fileviewerToolbar &&
                    fileviewerToolbar != null)
                {
                    fileviewerToolbar.Visible = false;
                }
            };
            _internalFileViewer.TextChanged += (sender, e) =>
            {
                if (_patchHighlighting)
                {
                    _internalFileViewer.AddPatchHighlighting();
                }

                TextChanged?.Invoke(sender, e);
            };
            _internalFileViewer.ScrollPosChanged += (sender, e) => ScrollPosChanged?.Invoke(sender, e);
            _internalFileViewer.SelectedLineChanged += (sender, e) => SelectedLineChanged?.Invoke(sender, e);
            _internalFileViewer.DoubleClick += (_, args) => RequestDiffView?.Invoke(this, EventArgs.Empty);

            HotkeysEnabled = true;

            if (!IsDesignModeActive && ContextMenuStrip == null)
            {
                ContextMenuStrip = contextMenu;
            }

            contextMenu.Opening += (sender, e) =>
            {
                copyToolStripMenuItem.Enabled = _internalFileViewer.GetSelectionLength() > 0;
                ContextMenuOpening?.Invoke(sender, e);
            };

            _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
        }

        private void FileViewer_UICommandsSourceSet(object sender, GitUICommandsSourceEventArgs e)
        {
            UICommandsSource.UICommandsChanged += UICommandsSourceChanged;
            UICommandsSourceChanged(UICommandsSource, null);
        }

        protected override void DisposeUICommandsSource()
        {
            UICommandsSource.UICommandsChanged -= UICommandsSourceChanged;
            base.DisposeUICommandsSource();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public new Font Font
        {
            get => _internalFileViewer.Font;
            set => _internalFileViewer.Font = value;
        }

        public Action OpenWithDifftool => _internalFileViewer.OpenWithDifftool;

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
            set { _encoding = value; }
        }

        [DefaultValue(0)]
        [Browsable(false)]
        public int ScrollPos
        {
            get => _internalFileViewer.ScrollPos;
            set => _internalFileViewer.ScrollPos = value;
        }

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

        private void UICommands_PostSettings(object sender, GitUIPostActionEventArgs e)
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

        public ToolStripSeparator AddContextMenuSeparator()
        {
            var separator = new ToolStripSeparator();
            contextMenu.Items.Add(separator);
            return separator;
        }

        public ToolStripMenuItem AddContextMenuEntry(string text, EventHandler toolStripItem_Click)
        {
            var toolStripItem = new ToolStripMenuItem(text);
            contextMenu.Items.Add(toolStripItem);
            toolStripItem.Click += toolStripItem_Click;
            return toolStripItem;
        }

        public void EnableScrollBars(bool enable)
        {
            _internalFileViewer.EnableScrollBars(enable);
        }

        public void SetVisibilityDiffContextMenu(bool visible, bool isStagingDiff)
        {
            _currentViewIsPatch = visible;
            ignoreWhitespaceChangesToolStripMenuItem.Visible = visible;
            increaseNumberOfLinesToolStripMenuItem.Visible = visible;
            decreaseNumberOfLinesToolStripMenuItem.Visible = visible;
            showEntireFileToolStripMenuItem.Visible = visible;
            toolStripSeparator2.Visible = visible;
            treatAllFilesAsTextToolStripMenuItem.Visible = visible;
            copyNewVersionToolStripMenuItem.Visible = visible;
            copyOldVersionToolStripMenuItem.Visible = visible;

            // TODO The following should not be enabled if this is a file and the file does not exist
            cherrypickSelectedLinesToolStripMenuItem.Visible = visible && !isStagingDiff && !Module.IsBareRepository();
            revertSelectedLinesToolStripMenuItem.Visible = visible && !isStagingDiff && !Module.IsBareRepository();
            copyPatchToolStripMenuItem.Visible = visible;
        }

        private void OnExtraDiffArgumentsChanged()
        {
            ExtraDiffArgumentsChanged?.Invoke(this, EventArgs.Empty);
        }

        [NotNull]
        public string GetExtraDiffArguments()
        {
            var args = new ArgumentBuilder
            {
                { ignoreAllWhitespaces.Checked, "--ignore-all-space" },
                { !ignoreAllWhitespaces.Checked && IgnoreWhitespaceChanges, "--ignore-space-change" },
                { ShowEntireFile, "--inter-hunk-context=9000 --unified=9000", $"--unified={NumberOfVisibleLines}" },
                { TreatAllFilesAsText, "--text" }
            };

            return args.ToString();
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
            return ViewItemAsync(
                fileName,
                getImage: GetImage,
                getFileText: GetFileText,
                getSubmoduleText: () => LocalizationHelpers.GetSubmoduleText(Module, fileName.TrimEnd('/'), ""),
                openWithDifftool: null /* not implemented */);

            Image GetImage()
            {
                try
                {
                    var path = _fullPathResolver.Resolve(fileName);

                    if (!File.Exists(path))
                    {
                        return null;
                    }

                    using (var stream = File.OpenRead(path))
                    {
                        return CreateImage(fileName, stream);
                    }
                }
                catch
                {
                    return null;
                }
            }

            string GetFileText()
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
                    #pragma warning disable VSTHRD103 // Call async methods when in an async method
                    var content = reader.ReadToEnd();
                    #pragma warning restore VSTHRD103 // Call async methods when in an async method
                    FilePreamble = reader.CurrentEncoding.GetPreamble();
                    return content;
                }
            }
        }

        public string GetText()
        {
            return _internalFileViewer.GetText();
        }

        public void ViewCurrentChanges(GitItemStatus item)
        {
            ViewCurrentChanges(item.Name, item.OldName, item.Staged == StagedStatus.Index, item.IsSubmodule, item.GetSubmoduleStatusAsync, null /* not implemented */);
        }

        public void ViewCurrentChanges(GitItemStatus item, bool isStaged, [CanBeNull] Action openWithDifftool)
        {
            ViewCurrentChanges(item.Name, item.OldName, isStaged, item.IsSubmodule, item.GetSubmoduleStatusAsync, openWithDifftool);
        }

        public void ViewCurrentChanges(string fileName, string oldFileName, bool staged,
            bool isSubmodule, Func<Task<GitSubmoduleStatus>> getStatusAsync, [CanBeNull] Action openWithDifftool)
        {
            // BUG why do we call getStatusAsync() twice

            if (!isSubmodule)
            {
                _async.LoadAsync(
                    () => (patch: Module.GetCurrentChanges(fileName, oldFileName, staged, GetExtraDiffArguments(), Encoding), openWithDifftool),
                    patch => ViewStagingPatch(patch.patch, patch.openWithDifftool));
            }
            else if (getStatusAsync() != null)
            {
                ViewPatchAsync(() =>
                    {
                        var status = ThreadHelper.JoinableTaskFactory.Run(() => getStatusAsync());
                        if (status == null)
                        {
                            return (text: $"Submodule \"{fileName}\" has unresolved conflicts",
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

        public async Task ViewTextAsync([NotNull] string fileName, [NotNull] string text, [CanBeNull] Action openWithDifftool = null)
        {
            ResetForText(fileName);

            // Check for binary file.
            if (FileHelper.IsBinaryFileAccordingToContent(text))
            {
                try
                {
                    var summary = await SummariseBinaryFileAsync();

                    _internalFileViewer.SetText(summary, openWithDifftool);
                }
                catch
                {
                    _internalFileViewer.SetText($"Binary file: {fileName} (Detected)", openWithDifftool);
                }

                TextLoaded?.Invoke(this, null);
            }
            else
            {
                _internalFileViewer.SetText(text, openWithDifftool);
                TextLoaded?.Invoke(this, null);

                RestoreCurrentScrollPos();
            }

            async Task<string> SummariseBinaryFileAsync()
            {
                var fullPath = Path.GetFullPath(_fullPathResolver.Resolve(fileName));
                var fileInfo = new FileInfo(fullPath);

                var str = new StringBuilder()
                    .AppendLine("Binary file:")
                    .AppendLine()
                    .AppendLine(fileName)
                    .AppendLine()
                    .AppendLine($"{fileInfo.Length:N0} bytes:")
                    .AppendLine();

                try
                {
                    const int maxLength = 1024 * 4;

                    var displayByteCount = (int)Math.Min(fileInfo.Length, maxLength);
                    var bytes = new byte[displayByteCount];

                    using (var stream = File.OpenRead(fullPath))
                    {
                        var offset = 0;
                        while (offset < displayByteCount)
                        {
                            offset += await stream.ReadAsync(bytes, offset, displayByteCount - offset);
                        }
                    }

                    ToHexDump(bytes, str);

                    if (fileInfo.Length > maxLength)
                    {
                        str.AppendLine()
                           .AppendLine()
                           .Append($"TRUNCATED: Shown first {maxLength:N0} of {fileInfo.Length:N0} bytes");
                    }
                }
                catch
                {
                    // oops
                }

                return str.ToString();
            }
        }

        public static string ToHexDump(byte[] bytes, StringBuilder str, int columnWidth = 8, int columnCount = 2)
        {
            if (bytes.Length == 0)
            {
                return "";
            }

            var i = 0;

            while (i < bytes.Length)
            {
                var baseIndex = i;

                if (i != 0)
                {
                    str.AppendLine();
                }

                // OFFSET
                str.Append($"{baseIndex:X4}    ");

                // BYTES
                for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    // space between columns
                    if (columnIndex != 0)
                    {
                        str.Append("  ");
                    }

                    for (var j = 0; j < columnWidth; j++)
                    {
                        if (j != 0)
                        {
                            str.Append(' ');
                        }

                        str.Append(i < bytes.Length
                            ? bytes[i].ToString("X2")
                            : "  ");
                        i++;
                    }
                }

                str.Append("    ");

                // ASCII
                i = baseIndex;
                for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    // space between columns
                    if (columnIndex != 0)
                    {
                        str.Append(' ');
                    }

                    for (var j = 0; j < columnWidth; j++)
                    {
                        if (i < bytes.Length)
                        {
                            var c = (char)bytes[i];
                            str.Append(char.IsControl(c) ? '.' : c);
                        }
                        else
                        {
                            str.Append(' ');
                        }

                        i++;
                    }
                }
            }

            return str.ToString();
        }

        public Task ViewGitItemRevisionAsync(string fileName, ObjectId objectId)
        {
            if (objectId == ObjectId.WorkTreeId)
            {
                // No blob exists for worktree, present contents from file system
                return ViewFileAsync(fileName);
            }
            else
            {
                // Retrieve blob, same as GitItemStatus.TreeGuid
                var blob = Module.GetFileBlobHash(fileName, objectId);
                return ViewGitItemAsync(fileName, blob);
            }
        }

        public Task ViewGitItemAsync(string fileName, [CanBeNull] ObjectId objectId)
        {
            var sha = objectId?.ToString();

            return ViewItemAsync(
                fileName,
                getImage: GetImage,
                getFileText: GetFileTextIfBlobExists,
                getSubmoduleText: () => LocalizationHelpers.GetSubmoduleText(Module, fileName.TrimEnd('/'), sha),
                openWithDifftool: () => Module.OpenWithDifftool(fileName, firstRevision: sha));

            string GetFileTextIfBlobExists() => sha != null ? Module.GetFileText(sha, Encoding) : "";

            Image GetImage()
            {
                try
                {
                    using (var stream = Module.GetFileStream(sha))
                    {
                        return CreateImage(fileName, stream);
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

        private Task ViewItemAsync(string fileName, Func<Image> getImage, Func<string> getFileText, Func<string> getSubmoduleText, [CanBeNull] Action openWithDifftool)
        {
            FilePreamble = null;

            string fullPath = Path.GetFullPath(_fullPathResolver.Resolve(fileName));

            if (fileName.EndsWith("/") || Directory.Exists(fullPath))
            {
                if (GitModule.IsValidGitWorkingDir(fullPath))
                {
                    return _async.LoadAsync(
                        getSubmoduleText,
                        text => ThreadHelper.JoinableTaskFactory.Run(
                            () => ViewTextAsync(fileName, text, openWithDifftool)));
                }
                else
                {
                    return ViewTextAsync(fileName, "Directory: " + fileName, openWithDifftool: null /* not applicable */);
                }
            }
            else if (FileHelper.IsImage(fileName))
            {
                return _async.LoadAsync(getImage,
                            image =>
                            {
                                ResetForImage();
                                if (image != null)
                                {
                                    var size = DpiUtil.Scale(image.Size);
                                    if (size.Height > PictureBox.Size.Height || size.Width > PictureBox.Size.Width)
                                    {
                                        PictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                                    }
                                    else
                                    {
                                        PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
                                    }
                                }

                                PictureBox.Image = image == null ? null : DpiUtil.Scale(image);
                                _internalFileViewer.SetText("", () => { });
                            });
            }

            // Check binary from extension/attributes (a secondary check for file contents before display)
            else if (FileHelper.IsBinaryFileName(Module, fileName))
            {
                return ViewTextAsync(fileName, $"Binary file: {fileName}", openWithDifftool);
            }
            else
            {
                return _async.LoadAsync(
                    getFileText,
                    text => ThreadHelper.JoinableTaskFactory.Run(
                        () => ViewTextAsync(fileName, text, openWithDifftool)));
            }
        }

        [NotNull]
        private static Image CreateImage([NotNull] string fileName, [NotNull] Stream stream)
        {
            if (IsIcon())
            {
                using (var icon = new Icon(stream))
                {
                    return icon.ToBitmap();
                }
            }

            return new Bitmap(CopyStream());

            bool IsIcon()
            {
                return fileName.EndsWith(".ico", StringComparison.CurrentCultureIgnoreCase);
            }

            MemoryStream CopyStream()
            {
                var copy = new MemoryStream();
                stream.CopyTo(copy);
                return copy;
            }
        }

        private void ResetForImage()
        {
            Reset(false, false);
            _internalFileViewer.SetHighlighting("Default");
        }

        private void ResetForText([CanBeNull] string fileName)
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

        private void ResetForDiff()
        {
            Reset(true, true);
            _internalFileViewer.SetHighlighting("");
            _patchHighlighting = true;
        }

        private void Reset(bool diff, bool text, bool isStagingDiff = false)
        {
            _patchHighlighting = diff;
            SetVisibilityDiffContextMenu(diff, isStagingDiff);
            ClearImage();
            PictureBox.Visible = !text;
            _internalFileViewer.Visible = text;

            return;

            void ClearImage()
            {
                PictureBox.ImageLocation = "";

                if (PictureBox.Image == null)
                {
                    return;
                }

                PictureBox.Image.Dispose();
                PictureBox.Image = null;
            }
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

        private void DecreaseNumberOfLinesToolStripMenuItemClick(object sender, EventArgs e)
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
                // add artificial space if selected text is not starting from line beginning, it will be removed later
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

            return;

            string RemovePrefix(string line)
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

            return;

            bool IsDiffLine(string wholeText, string lineContent)
            {
                var isCombinedDiff = DiffHighlightService.IsCombinedDiff(wholeText);
                return lineContent.StartsWithAny(isCombinedDiff ? new[] { "+", "-", " +", " -" }
                    : new[] { "+", "-" });
            }
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
            var command = (Commands)cmd;

            switch (command)
            {
                case Commands.Find: _internalFileViewer.Find(); break;
                case Commands.FindNextOrOpenWithDifftool: _internalFileViewer.FindNextAsync(searchForwardOrOpenWithDifftool: true); break;
                case Commands.FindPrevious: _internalFileViewer.FindNextAsync(searchForwardOrOpenWithDifftool: false); break;
                case Commands.GoToLine: goToLineToolStripMenuItem_Click(null, null); break;
                case Commands.IncreaseNumberOfVisibleLines: IncreaseNumberOfLinesToolStripMenuItemClick(null, null); break;
                case Commands.DecreaseNumberOfVisibleLines: DecreaseNumberOfLinesToolStripMenuItemClick(null, null); break;
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
            ThreadHelper.JoinableTaskFactory.Run(() => ViewTextAsync("", ""));
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
                if (Encoding != null)
                {
                    encodingToolStripComboBox.Text = Encoding.EncodingName;
                }
            }
        }

        private void goToLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_internalFileViewer.IsGotoLineUIApplicable())
            {
                return;
            }

            using (var formGoToLine = new FormGoToLine())
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
                // add artificial space if selected text is not starting from line beginning, it will be removed later
                int pos = noSelection ? 0 : _internalFileViewer.GetSelectionPosition();
                string fileText = _internalFileViewer.GetText();

                if (pos > 0 && fileText[pos - 1] != '\n')
                {
                    code = " " + code;
                }

                var lines = code.Split('\n')
                    .Where(s => s.Length == 0 || s[0] != startChar || (s.Length > 2 && s[1] == s[0] && s[2] == s[0]));
                var hpos = fileText.IndexOf("\n@@");

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
                patch = PatchManager.GetResetWorkTreeLinesAsPatch(
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
                UICommandsSourceSet -= FileViewer_UICommandsSourceSet;
                _async.Dispose();
                components?.Dispose();

                if (TryGetUICommands(out var uiCommands))
                {
                    uiCommands.PostSettings -= UICommands_PostSettings;
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
