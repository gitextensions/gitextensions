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
using GitExtUtils;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
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
        /// <summary>
        /// Raised when the Escape key is pressed (and only when no selection exists, as the default behaviour of escape is to clear the selection).
        /// </summary>
        public event Action EscapePressed;

        private readonly TranslationString _largeFileSizeWarning = new TranslationString("This file is {0:N1} MB. Showing large files can be slow. Click to show anyway.");
        private readonly TranslationString _cannotViewImage = new TranslationString("Cannot view image {0}");

        public event EventHandler<SelectedLineEventArgs> SelectedLineChanged;
        public event EventHandler HScrollPositionChanged;
        public event EventHandler VScrollPositionChanged;
        public event EventHandler BottomScrollReached;
        public event EventHandler TopScrollReached;
        public event EventHandler RequestDiffView;
        public new event EventHandler TextChanged;
        public event EventHandler TextLoaded;
        public event CancelEventHandler ContextMenuOpening;
        public event EventHandler<EventArgs> ExtraDiffArgumentsChanged;

        private readonly AsyncLoader _async;
        private readonly IFullPathResolver _fullPathResolver;
        private bool _currentViewIsPatch;
        private bool _patchHighlighting;
        private Encoding _encoding;
        private Func<Task> _deferShowFunc;
        private readonly ContinuousScrollEventManager _continuousScrollEventManager;

        public FileViewer()
        {
            TreatAllFilesAsText = false;
            ShowEntireFile = false;
            NumberOfContextLines = AppSettings.NumberOfContextLines;
            InitializeComponent();
            InitializeComplete();

            UICommandsSourceSet += OnUICommandsSourceSet;

            internalFileViewer.MouseEnter += (_, e) => OnMouseEnter(e);
            internalFileViewer.MouseLeave += (_, e) => OnMouseLeave(e);
            internalFileViewer.MouseMove += (_, e) => OnMouseMove(e);
            internalFileViewer.KeyUp += (_, e) => OnKeyUp(e);
            internalFileViewer.EscapePressed += () => EscapePressed?.Invoke();

            _continuousScrollEventManager = new ContinuousScrollEventManager();
            _continuousScrollEventManager.BottomScrollReached += _continuousScrollEventManager_BottomScrollReached;
            _continuousScrollEventManager.TopScrollReached += _continuousScrollEventManager_TopScrollReached;

            PictureBox.MouseWheel += PictureBox_MouseWheel;
            internalFileViewer.SetContinuousScrollManager(_continuousScrollEventManager);

            _async = new AsyncLoader();
            _async.LoadingError +=
                (_, e) =>
                {
                    if (!IsDisposed)
                    {
                        ResetForText(null);
                        internalFileViewer.SetText("Unsupported file: \n\n" + e.Exception.ToString(), openWithDifftool: null /* not applicable */);
                        TextLoaded?.Invoke(this, null);
                    }
                };

            IgnoreWhitespace = AppSettings.IgnoreWhitespaceKind;
            OnIgnoreWhitespaceChanged();

            ignoreWhitespaceAtEol.Image = Images.WhitespaceIgnoreEol.AdaptLightness();
            ignoreWhitespaceAtEolToolStripMenuItem.Image = ignoreWhitespaceAtEol.Image;

            ignoreWhiteSpaces.Image = Images.WhitespaceIgnore.AdaptLightness();
            ignoreWhitespaceChangesToolStripMenuItem.Image = ignoreWhiteSpaces.Image;

            ignoreAllWhitespaces.Image = Images.WhitespaceIgnoreAll.AdaptLightness();
            ignoreAllWhitespaceChangesToolStripMenuItem.Image = ignoreAllWhitespaces.Image;

            ShowEntireFile = AppSettings.ShowEntireFile;
            showEntireFileButton.Checked = ShowEntireFile;
            showEntireFileToolStripMenuItem.Checked = ShowEntireFile;
            SetStateOfContextLinesButtons();

            automaticContinuousScrollToolStripMenuItem.Image = Images.UiScrollBar.AdaptLightness();
            automaticContinuousScrollToolStripMenuItem.Checked = AppSettings.AutomaticContinuousScroll;

            showNonPrintChars.Image = Images.ShowWhitespace.AdaptLightness();
            showNonprintableCharactersToolStripMenuItem.Image = showNonPrintChars.Image;
            showNonPrintChars.Checked = AppSettings.ShowNonPrintingChars;
            showNonprintableCharactersToolStripMenuItem.Checked = AppSettings.ShowNonPrintingChars;
            ToggleNonPrintingChars(AppSettings.ShowNonPrintingChars);

            ShowSyntaxHighlightingInDiff = AppSettings.ShowSyntaxHighlightingInDiff;
            showSyntaxHighlighting.Image = Resources.SyntaxHighlighting.AdaptLightness();
            showSyntaxHighlighting.Checked = ShowSyntaxHighlightingInDiff;
            automaticContinuousScrollToolStripMenuItem.Text = Strings.ContScrollToNextFileOnlyWithAlt;

            IsReadOnly = true;

            internalFileViewer.MouseMove += (_, e) =>
            {
                if (_currentViewIsPatch && !fileviewerToolbar.Visible)
                {
                    fileviewerToolbar.Visible = true;
                    fileviewerToolbar.Location = new Point(Width - fileviewerToolbar.Width - 40, 0);
                    fileviewerToolbar.BringToFront();
                }
            };
            internalFileViewer.MouseLeave += (_, e) =>
            {
                if (GetChildAtPoint(PointToClient(MousePosition)) != fileviewerToolbar &&
                    fileviewerToolbar != null)
                {
                    fileviewerToolbar.Visible = false;
                }
            };
            internalFileViewer.TextChanged += (sender, e) =>
            {
                if (_patchHighlighting)
                {
                    internalFileViewer.AddPatchHighlighting();
                }

                TextChanged?.Invoke(sender, e);
            };
            internalFileViewer.HScrollPositionChanged += (sender, e) => HScrollPositionChanged?.Invoke(sender, e);
            internalFileViewer.VScrollPositionChanged += (sender, e) => VScrollPositionChanged?.Invoke(sender, e);
            internalFileViewer.SelectedLineChanged += (sender, e) => SelectedLineChanged?.Invoke(sender, e);
            internalFileViewer.DoubleClick += (_, args) => RequestDiffView?.Invoke(this, EventArgs.Empty);

            HotkeysEnabled = true;

            if (!IsDesignModeActive && ContextMenuStrip == null)
            {
                ContextMenuStrip = contextMenu;
            }

            contextMenu.Opening += (sender, e) =>
            {
                copyToolStripMenuItem.Enabled = internalFileViewer.GetSelectionLength() > 0;
                ContextMenuOpening?.Invoke(sender, e);
            };

            _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
        }

        // Public properties

        [Browsable(false)]
        public byte[] FilePreamble { get; private set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public new Font Font
        {
            get => internalFileViewer.Font;
            set => internalFileViewer.Font = value;
        }

        [DefaultValue(true)]
        [Category("Behavior")]
        public bool IsReadOnly
        {
            get => internalFileViewer.IsReadOnly;
            set => internalFileViewer.IsReadOnly = value;
        }

        [DefaultValue(null)]
        [Description("If true line numbers are shown in the textarea")]
        [Category("Appearance")]
        public bool? ShowLineNumbers
        {
            get => internalFileViewer.ShowLineNumbers;
            set => internalFileViewer.ShowLineNumbers = value;
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
            set
            {
                _encoding = value;

                this.InvokeAsync(() =>
                {
                    if (_encoding != null)
                    {
                        encodingToolStripComboBox.Text = _encoding.EncodingName;
                    }
                    else
                    {
                        encodingToolStripComboBox.SelectedIndex = -1;
                    }
                }).FileAndForget();
            }
        }

        public void ScrollToTop()
        {
            internalFileViewer.ScrollToTop();
        }

        public void ScrollToBottom()
        {
            internalFileViewer.ScrollToBottom();
        }

        [DefaultValue(0)]
        [Browsable(false)]
        public int HScrollPosition
        {
            get => internalFileViewer.HScrollPosition;
            set => internalFileViewer.HScrollPosition = value;
        }

        [DefaultValue(0)]
        [Browsable(false)]
        public int VScrollPosition
        {
            get => internalFileViewer.VScrollPosition;
            set => internalFileViewer.VScrollPosition = value;
        }

        // Private properties

        [Description("Sets what kind of whitespace changes shall be ignored in diffs")]
        [DefaultValue(IgnoreWhitespaceKind.None)]
        private IgnoreWhitespaceKind IgnoreWhitespace { get; set; }

        [Description("Show diffs with <n> lines of context.")]
        [DefaultValue(3)]
        private int NumberOfContextLines { get; set; }

        [Description("Show diffs with entire file.")]
        [DefaultValue(false)]
        private bool ShowEntireFile { get; set; }

        [Description("Treat all files as text.")]
        [DefaultValue(false)]
        private bool TreatAllFilesAsText { get; set; }

        [Description("Show syntax highlighting in diffs.")]
        [DefaultValue(true)]
        private bool ShowSyntaxHighlightingInDiff { get; set; }

        // Public methods

        public void SetGitBlameGutter(IEnumerable<GitBlameEntry> gitBlameEntries)
        {
            internalFileViewer.ShowGutterAvatars = AppSettings.BlameShowAuthorAvatar;

            if (AppSettings.BlameShowAuthorAvatar)
            {
                internalFileViewer.SetGitBlameGutter(gitBlameEntries);
            }
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
            internalFileViewer.EnableScrollBars(enable);
        }

        public ArgumentString GetExtraDiffArguments()
        {
            return new ArgumentBuilder
            {
                { IgnoreWhitespace == IgnoreWhitespaceKind.AllSpace, "--ignore-all-space" },
                { IgnoreWhitespace == IgnoreWhitespaceKind.Change, "--ignore-space-change" },
                { IgnoreWhitespace == IgnoreWhitespaceKind.Eol, "--ignore-space-at-eol" },
                { ShowEntireFile, "--inter-hunk-context=9000 --unified=9000", $"--unified={NumberOfContextLines}" },
                { TreatAllFilesAsText, "--text" }
            };
        }

        public string GetSelectedText()
        {
            return internalFileViewer.GetSelectedText();
        }

        public int GetSelectionPosition()
        {
            return internalFileViewer.GetSelectionPosition();
        }

        public int GetSelectionLength()
        {
            return internalFileViewer.GetSelectionLength();
        }

        public void GoToLine(int line)
        {
            internalFileViewer.GoToLine(line);
        }

        public int GetLineFromVisualPosY(int visualPosY)
        {
            return internalFileViewer.GetLineFromVisualPosY(visualPosY);
        }

        public void HighlightLines(int startLine, int endLine, Color color)
        {
            internalFileViewer.HighlightLines(startLine, endLine, color);
        }

        public void ClearHighlighting()
        {
            internalFileViewer.ClearHighlighting();
        }

        public string GetText() => internalFileViewer.GetText();

        public void ViewCurrentChanges(GitItemStatus item, bool isStaged, [CanBeNull] Action openWithDifftool)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(
                async () =>
                {
                    if (item?.IsStatusOnly ?? false)
                    {
                        // Present error (e.g. parsing Git)
                        await ViewTextAsync(item.Name, item.ErrorMessage);
                        return;
                    }

                    if (item.IsSubmodule)
                    {
                        var getStatusTask = item.GetSubmoduleStatusAsync();
                        if (getStatusTask != null)
                        {
                            var status = await getStatusTask;
                            if (status == null)
                            {
                                await ViewTextAsync(item.Name, $"Submodule \"{item.Name}\" has unresolved conflicts");
                                return;
                            }

                            await ViewTextAsync(item.Name, LocalizationHelpers.ProcessSubmoduleStatus(Module, status));
                            return;
                        }

                        var changes = await Module.GetCurrentChangesAsync(item.Name, item.OldName, isStaged,
                            GetExtraDiffArguments(), Encoding);
                        var text = LocalizationHelpers.ProcessSubmodulePatch(Module, item.Name, changes);
                        await ViewTextAsync(item.Name, text);
                        return;
                    }

                    if (!item.IsTracked || item.IsNew)
                    {
                        var id = isStaged ? ObjectId.IndexId : ObjectId.WorkTreeId;
                        await ViewGitItemRevisionAsync(item, id, openWithDifftool);
                    }
                    else
                    {
                        var patch = await Module.GetCurrentChangesAsync(
                            item.Name, item.OldName, isStaged, GetExtraDiffArguments(), Encoding);
                        await ViewPatchAsync(item.Name, patch?.Text ?? "", openWithDifftool);
                    }

                    SetVisibilityDiffContextMenuStaging();
                });
        }

        /// <summary>
        /// Present the text as a patch in the file viewer
        /// </summary>
        /// <param name="fileName">The fileName to present</param>
        /// <param name="text">The patch text</param>
        /// <param name="openWithDifftool">The action to open the difftool</param>
        public void ViewPatch([CanBeNull] string fileName,
            [NotNull] string text,
            [CanBeNull] Action openWithDifftool = null)
        {
            ThreadHelper.JoinableTaskFactory.Run(
                () => ViewPatchAsync(fileName, text, openWithDifftool));
        }

        public async Task ViewPatchAsync(string fileName, string text, Action openWithDifftool)
        {
            await ShowOrDeferAsync(
                text.Length,
                () =>
                {
                    ResetForDiff(fileName);
                    internalFileViewer.SetText(text, openWithDifftool, isDiff: true);

                    TextLoaded?.Invoke(this, null);
                    return Task.CompletedTask;
                });
        }

        public void ViewText([CanBeNull] string fileName,
            [NotNull] string text,
            [CanBeNull] Action openWithDifftool = null)
        {
            ThreadHelper.JoinableTaskFactory.Run(
                () => ViewTextAsync(fileName, text, openWithDifftool));
        }

        public async Task ViewTextAsync([CanBeNull] string fileName, [NotNull] string text,
            [CanBeNull] Action openWithDifftool = null, bool checkGitAttributes = false)
        {
            await ShowOrDeferAsync(
                text.Length,
                () =>
                {
                    ResetForText(fileName);

                    // Check for binary file. Using gitattributes could be misleading for a changed file,
                    // but not much other can be done
                    bool isBinary = (checkGitAttributes && FileHelper.IsBinaryFileName(Module, fileName))
                                    || FileHelper.IsBinaryFileAccordingToContent(text);

                    if (isBinary)
                    {
                        try
                        {
                            var summary = new StringBuilder()
                                .AppendLine("Binary file:")
                                .AppendLine()
                                .AppendLine(fileName)
                                .AppendLine()
                                .AppendLine($"{text.Length:N0} bytes:")
                                .AppendLine();
                            internalFileViewer.SetText(summary.ToString(), openWithDifftool);

                            ToHexDump(Encoding.ASCII.GetBytes(text), summary);
                            internalFileViewer.SetText(summary.ToString(), openWithDifftool);
                        }
                        catch
                        {
                            internalFileViewer.SetText($"Binary file: {fileName} (Detected)", openWithDifftool);
                        }
                    }
                    else
                    {
                        internalFileViewer.SetText(text, openWithDifftool);
                    }

                    TextLoaded?.Invoke(this, null);
                    return Task.CompletedTask;
                });
        }

        public Task ViewGitItemRevisionAsync(GitItemStatus file, ObjectId revision, [CanBeNull] Action openWithDifftool = null)
        {
            if (revision == ObjectId.WorkTreeId)
            {
                // No blob exists for worktree, present contents from file system
                return ViewFileAsync(file.Name, file.IsSubmodule, openWithDifftool);
            }

            if (file.TreeGuid == null)
            {
                file.TreeGuid = Module.GetFileBlobHash(file.Name, revision);
            }

            return ViewGitItemAsync(file, openWithDifftool);
        }

        /// <summary>
        /// View the git item with the TreeGuid
        /// </summary>
        /// <param name="file">GitItem file, with TreeGuid</param>
        /// <param name="openWithDifftool">difftool command</param>
        /// <returns>Task to view the item</returns>
        public Task ViewGitItemAsync(GitItemStatus file, [CanBeNull] Action openWithDifftool = null)
        {
            var sha = file.TreeGuid?.ToString();
            var isSubmodule = file.IsSubmodule;

            if (!isSubmodule && file.IsNew && file.Staged == StagedStatus.Index)
            {
                // File system access for other than Worktree,
                // to handle that git-status does not detect details for untracked (git-diff --no-index will not give info)
                var fullPath = Path.Combine(Module.WorkingDir, file.Name);
                if (Directory.Exists(fullPath) && GitModule.IsValidGitWorkingDir(fullPath))
                {
                    isSubmodule = true;
                }
            }

            return ViewItemAsync(
                file.Name,
                isSubmodule,
                getImage: GetImage,
                getFileText: GetFileTextIfBlobExists,
                getSubmoduleText: () => LocalizationHelpers.GetSubmoduleText(Module, file.Name.TrimEnd('/'), sha),
                openWithDifftool: openWithDifftool);

            string GetFileTextIfBlobExists()
            {
                FilePreamble = new byte[] { };
                return file.TreeGuid != null ? Module.GetFileText(file.TreeGuid, Encoding) : string.Empty;
            }

            Image GetImage()
            {
                try
                {
                    using (var stream = Module.GetFileStream(sha))
                    {
                        return CreateImage(file.Name, stream);
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Get contents in the file system async if not too big, otherwise ask user
        /// </summary>
        /// <param name="fileName">The file/submodule path</param>
        /// <param name="isSubmodule">If submodule</param>
        /// <param name="openWithDifftool">Diff action</param>
        /// <returns>Task</returns>
        public Task ViewFileAsync(string fileName, bool isSubmodule = false, [CanBeNull] Action openWithDifftool = null)
        {
            string fullPath = Path.GetFullPath(_fullPathResolver.Resolve(fileName));

            if (isSubmodule && !GitModule.IsValidGitWorkingDir(fullPath))
            {
                return ViewTextAsync(fileName, "Invalid submodule: " + fileName);
            }

            if (!isSubmodule && (fileName.EndsWith("/") || Directory.Exists(fullPath)))
            {
                if (!GitModule.IsValidGitWorkingDir(fullPath))
                {
                    return ViewTextAsync(fileName, "Directory: " + fileName);
                }

                isSubmodule = true;
            }

            return ShowOrDeferAsync(
                fileName,
                () => ViewItemAsync(
                    fileName,
                    isSubmodule,
                    getImage: GetImage,
                    getFileText: GetFileText,
                    getSubmoduleText: () => LocalizationHelpers.GetSubmoduleText(Module, fileName.TrimEnd('/'), ""),
                    openWithDifftool));

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
                    return $"File {path} does not exist";
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

        public void Clear()
        {
            ThreadHelper.JoinableTaskFactory.Run(() => ViewTextAsync("", ""));
        }

        public bool HasAnyPatches()
        {
            return internalFileViewer.GetText() != null && internalFileViewer.GetText().Contains("@@");
        }

        public void SetFileLoader(GetNextFileFnc fileLoader)
        {
            internalFileViewer.SetFileLoader(fileLoader);
        }

        public void CherryPickAllChanges()
        {
            if (GetText().Length > 0)
            {
                applySelectedLines(0, GetText().Length, reverse: false);
            }
        }

        // Protected

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UICommandsSourceSet -= OnUICommandsSourceSet;
                _async.Dispose();
                components?.Dispose();

                if (TryGetUICommands(out var uiCommands))
                {
                    uiCommands.PostSettings -= UICommands_PostSettings;
                }
            }

            base.Dispose(disposing);
        }

        protected override void DisposeUICommandsSource()
        {
            UICommandsSource.UICommandsChanged -= OnUICommandsChanged;
            base.DisposeUICommandsSource();
        }

        protected override void OnRuntimeLoad()
        {
            ReloadHotkeys();
            Font = AppSettings.FixedWidthFont;

            DetectDefaultEncoding();
            return;

            void DetectDefaultEncoding()
            {
                var encodings = AppSettings.AvailableEncodings.Values.Select(e => e.EncodingName).ToArray();
                encodingToolStripComboBox.Items.AddRange(encodings);
                encodingToolStripComboBox.ResizeDropDownWidth(50, 250);

                var defaultEncodingName = Encoding.Default.EncodingName;

                for (int i = 0; i < encodings.Length; i++)
                {
                    if (string.Equals(encodings[i], defaultEncodingName, StringComparison.OrdinalIgnoreCase))
                    {
                        encodingToolStripComboBox.Items[i] = "Default (" + Encoding.Default.HeaderName + ")";
                        break;
                    }
                }
            }
        }

        // Private methods

        private void CopyNotStartingWith(char startChar)
        {
            string code = internalFileViewer.GetSelectedText();
            bool noSelection = false;

            if (string.IsNullOrEmpty(code))
            {
                code = internalFileViewer.GetText();
                noSelection = true;
            }

            if (_currentViewIsPatch)
            {
                // add artificial space if selected text is not starting from line beginning, it will be removed later
                int pos = noSelection ? 0 : internalFileViewer.GetSelectionPosition();
                string fileText = internalFileViewer.GetText();

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

            ClipboardUtil.TrySetText(code.AdjustLineEndings(Module.EffectiveConfigFile.core.autocrlf.ValueOrDefault));
        }

        private void SetVisibilityDiffContextMenu(bool visibleTextFile, [CanBeNull] string fileName)
        {
            _currentViewIsPatch = visibleTextFile;
            ignoreWhitespaceAtEolToolStripMenuItem.Visible = visibleTextFile;
            ignoreWhitespaceChangesToolStripMenuItem.Visible = visibleTextFile;
            ignoreAllWhitespaceChangesToolStripMenuItem.Visible = visibleTextFile;
            increaseNumberOfLinesToolStripMenuItem.Visible = visibleTextFile;
            decreaseNumberOfLinesToolStripMenuItem.Visible = visibleTextFile;
            showEntireFileToolStripMenuItem.Visible = visibleTextFile;
            toolStripSeparator2.Visible = visibleTextFile;
            treatAllFilesAsTextToolStripMenuItem.Visible = visibleTextFile;
            copyNewVersionToolStripMenuItem.Visible = visibleTextFile;
            copyOldVersionToolStripMenuItem.Visible = visibleTextFile;

            bool fileExists = !string.IsNullOrWhiteSpace(fileName)
                              && File.Exists(_fullPathResolver.Resolve(fileName));

            cherrypickSelectedLinesToolStripMenuItem.Visible =
                revertSelectedLinesToolStripMenuItem.Visible =
                    visibleTextFile && fileExists && !Module.IsBareRepository();
            copyPatchToolStripMenuItem.Visible = visibleTextFile;
        }

        private void SetVisibilityDiffContextMenuStaging()
        {
            cherrypickSelectedLinesToolStripMenuItem.Visible = false;
            revertSelectedLinesToolStripMenuItem.Visible = false;
        }

        private void OnExtraDiffArgumentsChanged()
        {
            ExtraDiffArgumentsChanged?.Invoke(this, EventArgs.Empty);
        }

        private Task ShowOrDeferAsync(string fileName, Func<Task> showFunc)
        {
            return ShowOrDeferAsync(GetFileLength(), showFunc);

            long GetFileLength()
            {
                try
                {
                    var file = GetFileInfo(fileName);

                    if (file.Exists)
                    {
                        return file.Length;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"{ex.Message}{Environment.NewLine}{fileName}", Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // If the file does not exist, it doesn't matter what size we
                // return as nothing will be shown anyway.
                return 0;
            }
        }

        private Task ShowOrDeferAsync(long contentLength, Func<Task> showFunc)
        {
            const long maxLength = 5 * 1024 * 1024;

            if (contentLength > maxLength)
            {
                Clear();
                Refresh();
                _NO_TRANSLATE_lblShowPreview.Text = string.Format(_largeFileSizeWarning.Text, contentLength / (1024d * 1024));
                _NO_TRANSLATE_lblShowPreview.Show();
                _deferShowFunc = showFunc;
                return Task.CompletedTask;
            }
            else
            {
                _NO_TRANSLATE_lblShowPreview.Hide();
                _deferShowFunc = null;
                return showFunc();
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

        private void OnIgnoreWhitespaceChanged()
        {
            switch (IgnoreWhitespace)
            {
                case IgnoreWhitespaceKind.None:
                    ignoreWhitespaceAtEol.Checked = false;
                    ignoreWhitespaceAtEolToolStripMenuItem.Checked = ignoreWhitespaceAtEol.Checked;
                    ignoreWhiteSpaces.Checked = false;
                    ignoreWhitespaceChangesToolStripMenuItem.Checked = ignoreWhiteSpaces.Checked;
                    ignoreAllWhitespaces.Checked = false;
                    ignoreAllWhitespaceChangesToolStripMenuItem.Checked = ignoreAllWhitespaces.Checked;
                    break;
                case IgnoreWhitespaceKind.Eol:
                    ignoreWhitespaceAtEol.Checked = true;
                    ignoreWhitespaceAtEolToolStripMenuItem.Checked = ignoreWhitespaceAtEol.Checked;
                    ignoreWhiteSpaces.Checked = false;
                    ignoreWhitespaceChangesToolStripMenuItem.Checked = ignoreWhiteSpaces.Checked;
                    ignoreAllWhitespaces.Checked = false;
                    ignoreAllWhitespaceChangesToolStripMenuItem.Checked = ignoreAllWhitespaces.Checked;
                    break;
                case IgnoreWhitespaceKind.Change:
                    ignoreWhitespaceAtEol.Checked = true;
                    ignoreWhitespaceAtEolToolStripMenuItem.Checked = ignoreWhitespaceAtEol.Checked;
                    ignoreWhiteSpaces.Checked = true;
                    ignoreWhitespaceChangesToolStripMenuItem.Checked = ignoreWhiteSpaces.Checked;
                    ignoreAllWhitespaces.Checked = false;
                    ignoreAllWhitespaceChangesToolStripMenuItem.Checked = ignoreAllWhitespaces.Checked;
                    break;
                case IgnoreWhitespaceKind.AllSpace:
                    ignoreWhitespaceAtEol.Checked = true;
                    ignoreWhitespaceAtEolToolStripMenuItem.Checked = ignoreWhitespaceAtEol.Checked;
                    ignoreWhiteSpaces.Checked = true;
                    ignoreWhitespaceChangesToolStripMenuItem.Checked = ignoreWhiteSpaces.Checked;
                    ignoreAllWhitespaces.Checked = true;
                    ignoreAllWhitespaceChangesToolStripMenuItem.Checked = ignoreAllWhitespaces.Checked;
                    break;
                default:
                    throw new NotSupportedException("Unsupported value for IgnoreWhitespaceKind: " + IgnoreWhitespace);
            }

            AppSettings.IgnoreWhitespaceKind = IgnoreWhitespace;
        }

        private void ResetForImage([CanBeNull] string fileName)
        {
            Reset(false, false, fileName);
            internalFileViewer.SetHighlighting("Default");
        }

        private void ResetForText([CanBeNull] string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) &&
                (fileName.EndsWith(".diff", StringComparison.OrdinalIgnoreCase) ||
                 fileName.EndsWith(".patch", StringComparison.OrdinalIgnoreCase)))
            {
                ResetForDiff(fileName);
                return;
            }

            Reset(false, true, fileName);

            if (fileName == null)
            {
                internalFileViewer.SetHighlighting("Default");
            }
            else
            {
                internalFileViewer.SetHighlightingForFile(fileName);
            }
        }

        private void ResetForDiff([CanBeNull] string fileName)
        {
            Reset(true, true, fileName);

            if (ShowSyntaxHighlightingInDiff && fileName != null)
            {
                internalFileViewer.SetHighlightingForFile(fileName);
            }
            else
            {
                internalFileViewer.SetHighlighting("");
            }
        }

        private void Reset(bool isDiff, bool isText, [CanBeNull] string fileName)
        {
            _patchHighlighting = isDiff;
            SetVisibilityDiffContextMenu(isDiff, fileName);
            ClearImage();
            PictureBox.Visible = !isText;
            internalFileViewer.Visible = isText;

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

        private Task ViewItemAsync(string fileName, bool isSubmodule, Func<Image> getImage, Func<string> getFileText, Func<string> getSubmoduleText, [CanBeNull] Action openWithDifftool)
        {
            FilePreamble = null;

            if (isSubmodule)
            {
                return _async.LoadAsync(
                    getSubmoduleText,
                    text => ThreadHelper.JoinableTaskFactory.Run(
                        () => ViewTextAsync(fileName, text, openWithDifftool)));
            }
            else if (FileHelper.IsImage(fileName))
            {
                return _async.LoadAsync(getImage,
                            image =>
                            {
                                if (image == null)
                                {
                                    ResetForText(null);
                                    internalFileViewer.SetText(string.Format(_cannotViewImage.Text, fileName), openWithDifftool);
                                    return;
                                }

                                ResetForImage(fileName);
                                var size = DpiUtil.Scale(image.Size);
                                if (size.Height > PictureBox.Size.Height || size.Width > PictureBox.Size.Width)
                                {
                                    PictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                                }
                                else
                                {
                                    PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
                                }

                                PictureBox.Image = DpiUtil.Scale(image);
                                internalFileViewer.SetText("", openWithDifftool);
                            });
            }
            else
            {
                return _async.LoadAsync(
                    getFileText,
                    text => ThreadHelper.JoinableTaskFactory.Run(
                        () => ViewTextAsync(fileName, text, openWithDifftool, checkGitAttributes: true)));
            }
        }

        private FileInfo GetFileInfo(string fileName)
        {
            var resolvedPath = _fullPathResolver.Resolve(fileName);
            return new FileInfo(resolvedPath);
        }

        private static string ToHexDump(byte[] bytes, StringBuilder str, int columnWidth = 8, int columnCount = 2)
        {
            if (bytes.Length == 0)
            {
                return "";
            }

            // Do not freeze GE when selecting large binary files
            // Show only the header of the binary file to indicate contents and files incorrectly handled
            // Use a dedicated editor to view the complete file
            var limit = Math.Min(bytes.Length, columnWidth * columnCount * 256);
            var i = 0;

            while (i < limit)
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

            if (bytes.Length > limit)
            {
                str.AppendLine();
                str.Append("[Truncated]");
            }

            return str.ToString();
        }

        private void SetStateOfContextLinesButtons()
        {
            increaseNumberOfLines.Enabled = !ShowEntireFile;
            decreaseNumberOfLines.Enabled = !ShowEntireFile;
            increaseNumberOfLinesToolStripMenuItem.Enabled = !ShowEntireFile;
            decreaseNumberOfLinesToolStripMenuItem.Enabled = !ShowEntireFile;
        }

        private void ToggleNonPrintingChars(bool show)
        {
            internalFileViewer.ShowEOLMarkers = show;
            internalFileViewer.ShowSpaces = show;
            internalFileViewer.ShowTabs = show;
        }

        // Event handlers

        private void OnUICommandsChanged(object sender, [CanBeNull] GitUICommandsChangedEventArgs e)
        {
            if (e?.OldCommands != null)
            {
                e.OldCommands.PostSettings -= UICommands_PostSettings;
            }

            var commandSource = sender as IGitUICommandsSource;
            if (commandSource?.UICommands != null)
            {
                commandSource.UICommands.PostSettings += UICommands_PostSettings;
                UICommands_PostSettings(commandSource.UICommands, null);
            }

            Encoding = null;
        }

        private void UICommands_PostSettings(object sender, GitUIPostActionEventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await internalFileViewer.SwitchToMainThreadAsync();
                internalFileViewer.VRulerPosition = AppSettings.DiffVerticalRulerPosition;
            }).FileAndForget();
        }

        private void IgnoreWhitespaceAtEolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (IgnoreWhitespace == IgnoreWhitespaceKind.Eol)
            {
                IgnoreWhitespace = IgnoreWhitespaceKind.None;
            }
            else
            {
                IgnoreWhitespace = IgnoreWhitespaceKind.Eol;
            }

            OnIgnoreWhitespaceChanged();
            OnExtraDiffArgumentsChanged();
        }

        private void IgnoreWhitespaceChangesToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (IgnoreWhitespace == IgnoreWhitespaceKind.Change)
            {
                IgnoreWhitespace = IgnoreWhitespaceKind.None;
            }
            else
            {
                IgnoreWhitespace = IgnoreWhitespaceKind.Change;
            }

            OnIgnoreWhitespaceChanged();
            OnExtraDiffArgumentsChanged();
        }

        private void IncreaseNumberOfLinesToolStripMenuItemClick(object sender, EventArgs e)
        {
            NumberOfContextLines++;
            AppSettings.NumberOfContextLines = NumberOfContextLines;
            OnExtraDiffArgumentsChanged();
        }

        private void DecreaseNumberOfLinesToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (NumberOfContextLines > 0)
            {
                NumberOfContextLines--;
            }
            else
            {
                NumberOfContextLines = 0;
            }

            AppSettings.NumberOfContextLines = NumberOfContextLines;
            OnExtraDiffArgumentsChanged();
        }

        private void ShowSyntaxHighlighting_Click(object sender, System.EventArgs e)
        {
            ShowSyntaxHighlightingInDiff = !ShowSyntaxHighlightingInDiff;
            showSyntaxHighlighting.Checked = ShowSyntaxHighlightingInDiff;
            AppSettings.ShowSyntaxHighlightingInDiff = ShowSyntaxHighlightingInDiff;
            OnExtraDiffArgumentsChanged();
        }

        private void ShowEntireFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            ShowEntireFile = !ShowEntireFile;
            showEntireFileButton.Checked = ShowEntireFile;
            showEntireFileToolStripMenuItem.Checked = ShowEntireFile;
            SetStateOfContextLinesButtons();
            AppSettings.ShowEntireFile = ShowEntireFile;
            OnExtraDiffArgumentsChanged();
        }

        private void _continuousScrollEventManager_BottomScrollReached(object sender, EventArgs e)
            => BottomScrollReached?.Invoke(sender, e);

        private void _continuousScrollEventManager_TopScrollReached(object sender, EventArgs e)
            => TopScrollReached?.Invoke(sender, e);

        private void llShowPreview_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _NO_TRANSLATE_lblShowPreview.Hide();
            ThreadHelper.JoinableTaskFactory.Run(() => _deferShowFunc());
        }

        private void PictureBox_MouseWheel(object sender, MouseEventArgs e)
        {
            var isScrollingTowardTop = e.Delta > 0;
            var isScrollingTowardBottom = e.Delta < 0;

            if (isScrollingTowardTop)
            {
                _continuousScrollEventManager.RaiseTopScrollReached(sender, e);
            }

            if (isScrollingTowardBottom)
            {
                _continuousScrollEventManager.RaiseBottomScrollReached(sender, e);
            }
        }

        private void OnUICommandsSourceSet(object sender, GitUICommandsSourceEventArgs e)
        {
            UICommandsSource.UICommandsChanged += OnUICommandsChanged;
            OnUICommandsChanged(UICommandsSource, null);
        }

        private void TreatAllFilesAsTextToolStripMenuItemClick(object sender, EventArgs e)
        {
            treatAllFilesAsTextToolStripMenuItem.Checked = !treatAllFilesAsTextToolStripMenuItem.Checked;
            TreatAllFilesAsText = treatAllFilesAsTextToolStripMenuItem.Checked;
            OnExtraDiffArgumentsChanged();
        }

        private void CopyToolStripMenuItemClick(object sender, EventArgs e)
        {
            string code = internalFileViewer.GetSelectedText();

            if (string.IsNullOrEmpty(code))
            {
                return;
            }

            if (_currentViewIsPatch)
            {
                // add artificial space if selected text is not starting from line beginning, it will be removed later
                int pos = internalFileViewer.GetSelectionPosition();
                string fileText = internalFileViewer.GetText();
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

            ClipboardUtil.TrySetText(code.AdjustLineEndings(Module.EffectiveConfigFile.core.autocrlf.ValueOrDefault));

            return;

            string RemovePrefix(string line)
            {
                var isCombinedDiff = DiffHighlightService.IsCombinedDiff(internalFileViewer.GetText());
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
            var selectedText = internalFileViewer.GetSelectedText();

            if (!string.IsNullOrEmpty(selectedText))
            {
                ClipboardUtil.TrySetText(selectedText);
                return;
            }

            var text = internalFileViewer.GetText();

            if (!string.IsNullOrEmpty(text))
            {
                ClipboardUtil.TrySetText(text);
            }
        }

        private void NextChangeButtonClick(object sender, EventArgs e)
        {
            Focus();

            var currentVisibleLine = internalFileViewer.LineAtCaret;
            var totalNumberOfLines = internalFileViewer.TotalNumberOfLines;
            var emptyLineCheck = false;

            // skip the first pseudo-change containing the file names
            var startLine = Math.Max(4, currentVisibleLine + 1);
            for (var line = startLine; line < totalNumberOfLines; line++)
            {
                var lineContent = internalFileViewer.GetLineText(line);

                if (IsDiffLine(internalFileViewer.GetText(), lineContent))
                {
                    if (emptyLineCheck)
                    {
                        internalFileViewer.FirstVisibleLine = Math.Max(line - 4, 0);
                        internalFileViewer.LineAtCaret = line;
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

            var startLine = internalFileViewer.LineAtCaret;
            var emptyLineCheck = false;

            // go to the top of change block
            while (startLine > 0 &&
                internalFileViewer.GetLineText(startLine).StartsWithAny(new[] { "+", "-" }))
            {
                startLine--;
            }

            for (var line = startLine; line > 0; line--)
            {
                var lineContent = internalFileViewer.GetLineText(line);

                if (lineContent.StartsWithAny(new[] { "+", "-" })
                    && !lineContent.StartsWithAny(new[] { "++", "--" }))
                {
                    emptyLineCheck = true;
                }
                else
                {
                    if (emptyLineCheck)
                    {
                        internalFileViewer.FirstVisibleLine = Math.Max(0, line - 3);
                        internalFileViewer.LineAtCaret = line + 1;
                        return;
                    }
                }
            }

            // Do not go to the start of the file if no change is found
            ////TextEditor.ActiveTextAreaControl.TextArea.TextView.FirstVisibleLine = 0;
        }

        private void ContinuousScrollToolStripMenuItemClick(object sender, EventArgs e)
        {
            automaticContinuousScrollToolStripMenuItem.Checked = !automaticContinuousScrollToolStripMenuItem.Checked;
            AppSettings.AutomaticContinuousScroll = automaticContinuousScrollToolStripMenuItem.Checked;
        }

        private void ShowNonprintableCharactersToolStripMenuItemClick(object sender, EventArgs e)
        {
            showNonprintableCharactersToolStripMenuItem.Checked = !showNonprintableCharactersToolStripMenuItem.Checked;
            showNonPrintChars.Checked = showNonprintableCharactersToolStripMenuItem.Checked;

            ToggleNonPrintingChars(show: showNonprintableCharactersToolStripMenuItem.Checked);
            AppSettings.ShowNonPrintingChars = showNonPrintChars.Checked;
        }

        private void FindToolStripMenuItemClick(object sender, EventArgs e)
        {
            internalFileViewer.Find();
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
            using (var formGoToLine = new FormGoToLine())
            {
                formGoToLine.SetMaxLineNumber(internalFileViewer.MaxLineNumber);
                if (formGoToLine.ShowDialog(this) == DialogResult.OK)
                {
                    GoToLine(formGoToLine.GetLineNumber());
                }
            }
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
            var args = new GitArgumentBuilder("apply")
            {
                "--3way",
                "--whitespace=nowarn"
            };

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
                string output = Module.GitExecutable.GetOutput(args, patch);

                if (!string.IsNullOrEmpty(output))
                {
                    if (!MergeConflictHandler.HandleMergeConflicts(UICommands, this, false, false))
                    {
                        MessageBox.Show(this, output + "\n\n" + Encoding.GetString(patch), Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void cherrypickSelectedLinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            applySelectedLines(GetSelectionPosition(), GetSelectionLength(), reverse: false);
        }

        private void settingsButton_Click(object sender, EventArgs e)
        {
            UICommands.StartSettingsDialog(ParentForm, DiffViewerSettingsPage.GetPageReference());
        }

        private void revertSelectedLinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            applySelectedLines(GetSelectionPosition(), GetSelectionLength(), reverse: true);
        }

        private void IgnoreAllWhitespaceChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (IgnoreWhitespace == IgnoreWhitespaceKind.AllSpace)
            {
                IgnoreWhitespace = IgnoreWhitespaceKind.None;
            }
            else
            {
                IgnoreWhitespace = IgnoreWhitespaceKind.AllSpace;
            }

            OnIgnoreWhitespaceChanged();
            OnExtraDiffArgumentsChanged();
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
            PreviousChange = 7,
            NextOccurrence = 10,
            PreviousOccurrence = 11
        }

        protected override CommandStatus ExecuteCommand(int cmd)
        {
            var command = (Commands)cmd;

            switch (command)
            {
                case Commands.Find: internalFileViewer.Find(); break;
                case Commands.FindNextOrOpenWithDifftool: ThreadHelper.JoinableTaskFactory.RunAsync(() => internalFileViewer.FindNextAsync(searchForwardOrOpenWithDifftool: true)); break;
                case Commands.FindPrevious: ThreadHelper.JoinableTaskFactory.RunAsync(() => internalFileViewer.FindNextAsync(searchForwardOrOpenWithDifftool: false)); break;
                case Commands.GoToLine: goToLineToolStripMenuItem_Click(null, null); break;
                case Commands.IncreaseNumberOfVisibleLines: IncreaseNumberOfLinesToolStripMenuItemClick(null, null); break;
                case Commands.DecreaseNumberOfVisibleLines: DecreaseNumberOfLinesToolStripMenuItemClick(null, null); break;
                case Commands.ShowEntireFile: ShowEntireFileToolStripMenuItemClick(null, null); break;
                case Commands.TreatFileAsText: TreatAllFilesAsTextToolStripMenuItemClick(null, null); break;
                case Commands.NextChange: NextChangeButtonClick(null, null); break;
                case Commands.PreviousChange: PreviousChangeButtonClick(null, null); break;
                case Commands.NextOccurrence: internalFileViewer.GoToNextOccurrence(); break;
                case Commands.PreviousOccurrence: internalFileViewer.GoToPreviousOccurrence(); break;
                default: return base.ExecuteCommand(cmd);
            }

            return true;
        }

        #endregion

        internal TestAccessor GetTestAccessor() => new TestAccessor(this);

        internal readonly struct TestAccessor
        {
            private readonly FileViewer _fileViewer;

            public TestAccessor(FileViewer fileViewer)
            {
                _fileViewer = fileViewer;
            }

            public ToolStripMenuItem CopyToolStripMenuItem => _fileViewer.copyToolStripMenuItem;

            public FileViewerInternal FileViewerInternal => _fileViewer.internalFileViewer;

            public IgnoreWhitespaceKind IgnoreWhitespace
            {
                get => _fileViewer.IgnoreWhitespace;
                set => _fileViewer.IgnoreWhitespace = value;
            }

            public bool ShowSyntaxHighlightingInDiff
            {
                get => _fileViewer.ShowSyntaxHighlightingInDiff;
                set => _fileViewer.ShowSyntaxHighlightingInDiff = value;
            }

            public ToolStripButton IgnoreWhitespaceAtEolButton => _fileViewer.ignoreWhitespaceAtEol;
            public ToolStripMenuItem IgnoreWhitespaceAtEolMenuItem => _fileViewer.ignoreWhitespaceAtEolToolStripMenuItem;

            public ToolStripButton IgnoreWhiteSpacesButton => _fileViewer.ignoreWhiteSpaces;
            public ToolStripMenuItem IgnoreWhiteSpacesMenuItem => _fileViewer.ignoreWhitespaceChangesToolStripMenuItem;

            public ToolStripButton IgnoreAllWhitespacesButton => _fileViewer.ignoreAllWhitespaces;
            public ToolStripMenuItem IgnoreAllWhitespacesMenuItem => _fileViewer.ignoreAllWhitespaceChangesToolStripMenuItem;

            internal void IgnoreWhitespaceAtEolToolStripMenuItem_Click(object sender, EventArgs e) => _fileViewer.IgnoreWhitespaceAtEolToolStripMenuItem_Click(sender, e);
            internal void IgnoreWhitespaceChangesToolStripMenuItemClick(object sender, EventArgs e) => _fileViewer.IgnoreWhitespaceChangesToolStripMenuItemClick(sender, e);
            internal void IgnoreAllWhitespaceChangesToolStripMenuItem_Click(object sender, EventArgs e) => _fileViewer.IgnoreAllWhitespaceChangesToolStripMenuItem_Click(sender, e);
        }
    }
}
