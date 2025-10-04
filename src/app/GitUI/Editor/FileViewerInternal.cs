using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI.Editor.Diff;
using GitUI.GitComments;
using GitUI.Theming;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor
{
    public partial class FileViewerInternal : GitModuleControl, IFileViewer
    {
        /// <summary>
        /// Raised when the Escape key is pressed (and only when no selection exists, as the default behaviour of escape is to clear the selection).
        /// </summary>
        public event Action? EscapePressed;

        public event EventHandler<SelectedLineEventArgs>? SelectedLineChanged;
        public new event MouseEventHandler? MouseMove;
        public new event EventHandler? MouseEnter;
        public new event EventHandler? MouseLeave;
        public new event System.Windows.Forms.KeyEventHandler? KeyUp;
        public new event EventHandler? DoubleClick;

        private readonly FindAndReplaceForm _findAndReplaceForm = new();
        private readonly CurrentViewPositionCache _currentViewPositionCache;
        private DiffViewerLineNumberControl _lineNumbersControl;
        private ITextHighlightService _textHighlightService = TextHighlightService.Instance;
        private bool _shouldScrollToTop = false;
        private bool _shouldScrollToBottom = false;
        private readonly int _bottomBlankHeight = DpiUtil.Scale(300);
        private ContinuousScrollEventManager? _continuousScrollEventManager;
        private BlameAuthorMargin? _authorsAvatarMargin;
        private bool _showGutterAvatars;

        public FileViewerInternal()
        {
            InitializeComponent();
            InitializeComplete();

            Disposed += (sender, e) =>
            {
                //// _textHighlightService not disposable
                //// _lineNumbersControl not disposable
                //// _currentViewPositionCache not disposable
                _findAndReplaceForm.Dispose();
            };

            FirstVisibleLine = 0;
            _currentViewPositionCache = new CurrentViewPositionCache(this);
            TextEditor.ActiveTextAreaControl.TextArea.SelectionManager.SelectionChanged += SelectionManagerSelectionChanged;

            TextEditor.ActiveTextAreaControl.TextArea.PreviewKeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape && !TextEditor.ActiveTextAreaControl.SelectionManager.HasSomethingSelected)
                {
                    EscapePressed?.Invoke();
                }
            };

            TextEditor.TextChanged += (s, e) => TextChanged?.Invoke(s, e);
            TextEditor.ActiveTextAreaControl.HScrollBar.ValueChanged += (s, e) => OnHScrollPositionChanged(EventArgs.Empty);
            TextEditor.ActiveTextAreaControl.VScrollBar.ValueChanged += (s, e) => OnVScrollPositionChanged(EventArgs.Empty);
            TextEditor.ActiveTextAreaControl.TextArea.KeyUp += (s, e) => KeyUp?.Invoke(s, e);
            TextEditor.ActiveTextAreaControl.TextArea.DoubleClick += (s, e) => DoubleClick?.Invoke(s, e);
            TextEditor.ActiveTextAreaControl.TextArea.MouseMove += (s, e) => MouseMove?.Invoke(s, e);
            TextEditor.ActiveTextAreaControl.TextArea.MouseEnter += (s, e) => MouseEnter?.Invoke(s, e);
            TextEditor.ActiveTextAreaControl.TextArea.MouseLeave += (s, e) => MouseLeave?.Invoke(s, e);
            TextEditor.ActiveTextAreaControl.TextArea.MouseDown += (s, e) =>
            {
                SelectedLineChanged?.Invoke(
                    this,
                    new SelectedLineEventArgs(
                        TextEditor.ActiveTextAreaControl.TextArea.TextView.GetLogicalLine(e.Y)));
            };
            TextEditor.ActiveTextAreaControl.TextArea.MouseWheel += TextArea_MouseWheel;

            TextEditor.ActiveTextAreaControl.TextEditorProperties.EnableFolding = false;
            _lineNumbersControl = new DiffViewerLineNumberControl(TextEditor.ActiveTextAreaControl.TextArea);
            VRulerPosition = AppSettings.DiffVerticalRulerPosition;
            TextEditor.ActiveTextAreaControl.Caret.PositionChanged += GutterSelectedLineChanged;
        }

        public void DontMarkGutterSelectedLine()
        {
            TextEditor.ActiveTextAreaControl.Caret.PositionChanged -= GutterSelectedLineChanged;
            TextEditor.ActiveTextAreaControl.TextArea.GutterMargin.MarkSelectedLine = false;
        }

        public void SetContinuousScrollManager(ContinuousScrollEventManager continuousScrollEventManager)
        {
            _continuousScrollEventManager = continuousScrollEventManager;
        }

        internal void GutterSelectedLineChanged(object sender, EventArgs e)
        {
            GutterSelectedLineChanged(TextEditor.ActiveTextAreaControl.Caret.Line);
        }

        internal void GutterSelectedLineChanged(int lineNo)
        {
            _lineNumbersControl.SelectedLineChanged(lineNo);
            TextEditor.ActiveTextAreaControl.TextArea.GutterMargin.SelectedLineChanged(lineNo);
        }

        private void SelectionManagerSelectionChanged(object sender, EventArgs e)
        {
            string text = TextEditor.ActiveTextAreaControl.TextArea.SelectionManager.SelectedText;
            TextEditor.Document.MarkerStrategy.RemoveAll(m => true);

            IList<TextMarker> selectionMarkers = GetTextMarkersMatchingWord(text);
            TextEditor.Document.MarkerStrategy.AddMarkers(selectionMarkers);

            _textHighlightService.AddTextHighlighting(TextEditor.Document);
            TextEditor.ActiveTextAreaControl.TextArea.Invalidate();
        }

        /// <summary>
        /// Create a list of TextMarker instances in the Document that match the given text.
        /// </summary>
        /// <param name="word">The text to match.</param>
        private IList<TextMarker> GetTextMarkersMatchingWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return Array.Empty<TextMarker>();
            }

            List<TextMarker> selectionMarkers = [];

            string textContent = TextEditor.Document.TextContent;
            int indexMatch = -1;
            do
            {
                indexMatch = textContent.IndexOf(word, indexMatch + 1, StringComparison.OrdinalIgnoreCase);
                if (indexMatch >= 0)
                {
                    Color highlightColor = AppColor.HighlightAllOccurences.GetThemeColor();

                    TextMarker textMarker = new(indexMatch,
                        word.Length, TextMarkerType.SolidBlock, highlightColor,
                        ColorHelper.GetForeColorForBackColor(highlightColor));

                    selectionMarkers.Add(textMarker);
                }
            }
            while (indexMatch >= 0 && indexMatch < textContent.Length - 1);

            return selectionMarkers;
        }

        public new Font Font
        {
            get => TextEditor.Font;
            set => TextEditor.Font = value;
        }

        public Action? OpenWithDifftool { get; private set; }

        /// <summary>
        /// Move the file viewer cursor position to the next TextMarker found in the document that matches the AppColor.HighlightAllOccurences.
        /// </summary>
        public void GoToNextOccurrence()
        {
            int offset = TextEditor.ActiveTextAreaControl.TextArea.Caret.Offset;

            List<TextMarker> markers = TextEditor.Document.MarkerStrategy.GetMarkers(offset,
                TextEditor.Document.TextLength - offset);

            TextMarker marker =
                markers.FirstOrDefault(x => x.Offset > offset && x.Color == AppColor.HighlightAllOccurences.GetThemeColor());
            if (marker is not null)
            {
                TextLocation position = TextEditor.ActiveTextAreaControl.TextArea.Document.OffsetToPosition(marker.Offset);
                TextEditor.ActiveTextAreaControl.Caret.Position = position;
            }
        }

        /// <summary>
        /// Move the file viewer cursor position to the previous TextMarker found in the document that matches the AppColor.HighlightAllOccurences.
        /// </summary>
        public void GoToPreviousOccurrence()
        {
            int offset = TextEditor.ActiveTextAreaControl.TextArea.Caret.Offset;

            List<TextMarker> markers = TextEditor.Document.MarkerStrategy.GetMarkers(0, offset);

            TextMarker marker =
                markers.LastOrDefault(x => x.Offset < offset && x.Color == AppColor.HighlightAllOccurences.GetThemeColor());
            if (marker is not null)
            {
                TextLocation position = TextEditor.ActiveTextAreaControl.TextArea.Document.OffsetToPosition(marker.Offset);
                TextEditor.ActiveTextAreaControl.Caret.Position = position;
            }
        }

        public void Find(bool replace)
        {
            _findAndReplaceForm.ShowFor(TextEditor, replace && !IsReadOnly);
            OnVScrollPositionChanged(EventArgs.Empty);
        }

        public async Task FindNextAsync(bool searchForwardOrOpenWithDifftool)
        {
            if (searchForwardOrOpenWithDifftool && OpenWithDifftool is not null && string.IsNullOrEmpty(_findAndReplaceForm.LookFor))
            {
                OpenWithDifftool();
                return;
            }

            await _findAndReplaceForm.FindNextAsync(viaF3: true, !searchForwardOrOpenWithDifftool, "Text not found");
            OnVScrollPositionChanged(EventArgs.Empty);
        }

        #region IFileViewer Members

        public event EventHandler? HScrollPositionChanged;
        public event EventHandler? VScrollPositionChanged;
        public new event EventHandler? TextChanged;

        public void ScrollToTop()
        {
            _shouldScrollToTop = true;
        }

        public void ScrollToBottom()
        {
            _shouldScrollToBottom = true;
        }

        public string GetText()
        {
            return TextEditor.Text;
        }

        public bool? ShowLineNumbers { get; set; }

        /// <summary>
        /// Set plain text in the editor.
        /// </summary>
        /// <param name="text">The text to set in the editor.</param>
        /// <param name="openWithDifftool">The command to open the difftool.</param>
        public void SetText(string text, Action? openWithDifftool)
        {
            SetText(text, openWithDifftool, viewMode: ViewMode.Text, useGitColoring: false, contentIdentification: null);
        }

        /// <summary>
        /// Set plain text in the editor.
        /// </summary>
        /// <param name="text">The text to set in the editor.</param>
        /// <param name="openWithDifftool">The command to open the difftool.</param>
        /// <param name="viewMode">the view viewMode in the file viewer, the kind of info shown</param>
        /// <returns><see langword="true"/> if a position was set.</returns>
        public bool SetText(string text, Action? openWithDifftool, ViewMode viewMode, bool useGitColoring, string? contentIdentification)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _currentViewPositionCache.Capture();

            OpenWithDifftool = openWithDifftool;

            // Get the highlight service, possibly clean escape sequences in the text and create highlight.
            _lineNumbersControl.Clear();
            int vrulerpos = -1;
            _textHighlightService = viewMode switch
            {
                ViewMode.Text => TextHighlightService.Instance,
                ViewMode.Diff or ViewMode.FixedDiff => new PatchHighlightService(ref text, useGitColoring, _lineNumbersControl),
                ViewMode.CombinedDiff => new CombinedDiffHighlightService(ref text, useGitColoring, _lineNumbersControl),
                ViewMode.Difftastic => new DifftasticHighlightService(ref text, _lineNumbersControl, out vrulerpos),
                ViewMode.RangeDiff => new RangeDiffHighlightService(ref text, _lineNumbersControl),
                ViewMode.Grep => new GrepHighlightService(ref text, _lineNumbersControl),
                _ => throw new ArgumentException($"Unexpected viewMode: {viewMode}", nameof(viewMode))
            };

            if (vrulerpos >= 0)
            {
                // Difftastic set the position (0 to hide)
                VRulerPosition = vrulerpos;
            }
            else if (VRulerPosition != AppSettings.DiffVerticalRulerPosition)
            {
                // Reset if Difftastic changed the position
                VRulerPosition = AppSettings.DiffVerticalRulerPosition;
            }

            TextEditor.Text = text;
            bool hasLineNumberControl = viewMode.IsPartialTextView();
            _lineNumbersControl.SetVisibility(hasLineNumberControl);

            if (hasLineNumberControl)
            {
                int index = TextEditor.ActiveTextAreaControl.TextArea.LeftMargins.IndexOf(_lineNumbersControl);
                if (index == -1)
                {
                    TextEditor.ActiveTextAreaControl.TextArea.InsertLeftMargin(0, _lineNumbersControl);
                }
            }

            // important to set after the text was changed
            // otherwise the may be rendering artifacts as noted in #5568
            TextEditor.ShowLineNumbers = ShowLineNumbers ?? !hasLineNumberControl;
            GutterSelectedLineChanged(-1);
            if (ShowLineNumbers.HasValue && !ShowLineNumbers.Value)
            {
                Padding = new Padding(DpiUtil.Scale(5), Padding.Top, Padding.Right, Padding.Bottom);
            }

            TextEditor.Refresh();

            // Restore position if contentIdentification matches the capture
            bool positionSet = _currentViewPositionCache.Restore(contentIdentification) && LineAtCaret > FirstLineAfterHeader;

            if (_shouldScrollToBottom || _shouldScrollToTop)
            {
                VScrollBar scrollBar = TextEditor.ActiveTextAreaControl.VScrollBar;
                if (scrollBar.Visible)
                {
                    scrollBar.Value = _shouldScrollToTop ? 0 : Math.Max(0, scrollBar.Maximum - scrollBar.Height - _bottomBlankHeight);
                    positionSet = true;
                }

                _shouldScrollToTop = false;
                _shouldScrollToBottom = false;
            }

            return positionSet;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (ShowLineNumbers.HasValue && !ShowLineNumbers.Value)
            {
                e.Graphics.FillRectangle(SystemBrushes.Window, e.ClipRectangle);
            }
            else
            {
                base.OnPaintBackground(e);
            }
        }

        public void SetHighlighting(string syntax) =>
            SetHighlightingStrategy(HighlightingStrategyFactory.CreateHighlightingStrategy(syntax));

        public void SetHighlightingForFile(string filename)
        {
            IHighlightingStrategy highlightingStrategy;
            var commentStrategy = CommentStrategyFactory.GetSelected();
            var comment = commentStrategy.GetComment(Module);
            if (filename.EndsWith("git-rebase-todo"))
            {
                highlightingStrategy = new RebaseTodoHighlightingStrategy(comment);
            }
            else if (filename.EndsWith("COMMIT_EDITMSG"))
            {
                highlightingStrategy = new CommitMessageHighlightingStrategy(comment);
            }
            else
            {
                highlightingStrategy = HighlightingManager.Manager.FindHighlighterForFile(filename);
            }

            SetHighlightingStrategy(highlightingStrategy);
        }

        private void SetHighlightingStrategy(IHighlightingStrategy highlightingStrategy)
        {
            TextEditor.Document.HighlightingStrategy =
                ThemeModule.Settings.UseSystemVisualStyle
                    ? highlightingStrategy
                    : new ThemeBasedHighlighting(highlightingStrategy);
            TextEditor.Refresh();
        }

        public string GetSelectedText()
        {
            return TextEditor.ActiveTextAreaControl.SelectionManager.SelectedText;
        }

        public int GetSelectionPosition()
        {
            if (TextEditor.ActiveTextAreaControl.SelectionManager.SelectionCollection.Count > 0)
            {
                return TextEditor.ActiveTextAreaControl.SelectionManager.SelectionCollection[0].Offset;
            }

            return TextEditor.ActiveTextAreaControl.Caret.Offset;
        }

        public int GetSelectionLength()
        {
            if (TextEditor.ActiveTextAreaControl.SelectionManager.SelectionCollection.Count > 0)
            {
                return TextEditor.ActiveTextAreaControl.SelectionManager.SelectionCollection[0].Length;
            }

            return 0;
        }

        public void EnableScrollBars(bool enable)
        {
            TextEditor.ActiveTextAreaControl.VScrollBar.Width = 0;
            TextEditor.ActiveTextAreaControl.VScrollBar.Visible = enable;
            TextEditor.ActiveTextAreaControl.TextArea.Dock = DockStyle.Fill;
        }

        /// <summary>
        /// Add text highlighting to the document.
        /// This is primarily used to highlight changed files for diffs.
        /// </summary>
        public void AddTextHighlighting()
        {
            TextEditor.Document.MarkerStrategy.RemoveAll(m => true);
            _textHighlightService.AddTextHighlighting(TextEditor.Document);
        }

        /// <summary>
        /// Get the full prefix for lines with differences.
        /// </summary>
        /// <returns>An array with the prefixes.</returns>
        /// <exception cref="ArgumentException"></exception>
        public string[] GetFullDiffPrefixes()
        {
            if (_textHighlightService is not DiffHighlightService highlightService)
            {
                throw new ArgumentException($"Unexpected highlight service {_textHighlightService.GetType()}, not a diff type.");
            }

            return highlightService.GetFullDiffPrefixes();
        }

        /// <summary>
        /// Check if the line is a search match.
        /// For normal diff, this a added deleted line.
        /// For range-diff, this is a header.
        /// </summary>
        /// <returns>An array with the prefixes.</returns>
        /// <param name="indexInText">The line to check.</param>
        private bool IsSearchMatch(int indexInText)
            => _textHighlightService.IsSearchMatch(_lineNumbersControl, indexInText);

        private int FirstLineAfterHeader
        {
            get
            {
                bool hasDiffHeader = _textHighlightService is (PatchHighlightService or CombinedDiffHighlightService);
                return hasDiffHeader ? 5 : 0;
            }
        }

        /// <summary>
        /// Go to the first change.
        /// For normal diffs, this is the first diff.
        /// For range-diff, it is the first block of commit summary header.
        /// </summary>
        public void GoToFirstChange(int contextLines)
        {
            GoToNextChange(contextLines, fromTop: true);
        }

        /// <summary>
        /// Go to the next change.
        /// For normal diffs, this is the next diff.
        /// For range-diff, it is the next block of commit summary header.
        /// </summary>
        /// <param name="contextLines">Number of context lines, to include header for new diff.</param>
        public void GoToNextChange(int contextLines)
        {
            GoToNextChange(contextLines, fromTop: false);
        }

        private void GoToNextChange(int contextLines, bool fromTop)
        {
            // Skip the file header
            int firstValidIndex = FirstLineAfterHeader;
            int startIndex = fromTop ? firstValidIndex : Math.Max(firstValidIndex, LineAtCaret);
            int totalNumberOfLines = TotalNumberOfLines;

            bool emptyLineCheck = fromTop;
            for (int line = startIndex; line < totalNumberOfLines; line++)
            {
                if (IsSearchMatch(line))
                {
                    if (emptyLineCheck)
                    {
                        if (fromTop && IsLineVisible(line))
                        {
                            // Keep FirstVisibleLine, but let it be clamped in order to avoid scrolling the text out of view
                            FirstVisibleLine = FirstVisibleLine;
                        }
                        else
                        {
                            // Include the header with the (possible) function summary line
                            FirstVisibleLine = Math.Max(line - contextLines - 1, 0);
                        }

                        LineAtCaret = line;
                        return;
                    }
                }
                else
                {
                    emptyLineCheck = true;
                }
            }

            return;

            bool IsLineVisible(int line)
            {
                int firstVisibleLine = FirstVisibleLine;
                return firstVisibleLine <= line && line < firstVisibleLine + TextEditor.ActiveTextAreaControl.TextArea.TextView.VisibleLineCount;
            }
        }

        public void GoToPreviousChange(int contextLines)
        {
            // Skip the file header
            bool hasDiffHeader = _textHighlightService is (PatchHighlightService or CombinedDiffHighlightService);
            int firstValidIndex = hasDiffHeader ? 4 : 0;
            int startIndex = LineAtCaret;
            while (startIndex > firstValidIndex && IsSearchMatch(startIndex))
            {
                // Go to line before of current diff block
                startIndex--;
            }

            // Find the start of previous diff
            bool emptyLineCheck = false;
            for (int line = startIndex; line >= firstValidIndex; line--)
            {
                if (IsSearchMatch(line) && line > firstValidIndex)
                {
                    emptyLineCheck = true;
                    continue;
                }

                if (!emptyLineCheck)
                {
                    continue;
                }

                if (!IsSearchMatch(line))
                {
                    // Restore to last diff
                    line++;
                }

                // Include the header with the (possible) function summary line
                FirstVisibleLine = Math.Max(line - contextLines - 1, 0);
                LineAtCaret = line;
                return;
            }
        }

        /// <summary>
        /// Copy the the text selected in the editor, filtering out lines starting with the given character.
        /// </summary>
        /// <param name="startChar">The start character to ignore for diffs.</param>
        public void CopyNotStartingWith(char startChar)
        {
            string text = GetSelectedText();
            bool noSelection = false;

            if (string.IsNullOrEmpty(text))
            {
                text = GetText();
                noSelection = true;
            }

            bool isDiff = _textHighlightService is DiffHighlightService;
            if (isDiff)
            {
                // add artificial space if selected text is not starting from line beginning, it will be removed later
                int pos = noSelection ? 0 : GetSelectionPosition();
                string fileText = GetText();

                if (pos > 0 && fileText[pos - 1] != '\n')
                {
                    text = " " + text;
                }

                IEnumerable<string> lines = text.LazySplit('\n')
                    .Where(s => s.Length == 0 || s[0] != startChar || (s.Length > 2 && s[1] == s[0] && s[2] == s[0]));
                int hpos = fileText.IndexOf("\n@@");

                // if header is selected then don't remove diff extra chars
                if (hpos <= pos)
                {
                    char[] specials = [' ', '-', '+'];
                    lines = lines.Select(s => s.Length > 0 && specials.Any(c => c == s[0]) ? s[1..] : s);
                }

                text = string.Join("\n", lines);
            }

            ClipboardUtil.TrySetText(text.AdjustLineEndings(Module.GetEffectiveSetting<AutoCRLFType>("core.autocrlf")));
        }

        public int HScrollPosition
        {
            get { return TextEditor.ActiveTextAreaControl.HScrollBar?.Value ?? 0; }
            set
            {
                HScrollBar scrollBar = TextEditor.ActiveTextAreaControl.HScrollBar;
                if (scrollBar is null)
                {
                    return;
                }

                int max = scrollBar.Maximum - scrollBar.LargeChange;
                max = Math.Max(max, scrollBar.Minimum);
                scrollBar.Value = max > value ? value : max;
            }
        }

        public int VScrollPosition
        {
            get { return TextEditor.ActiveTextAreaControl.VScrollBar?.Value ?? 0; }
            set
            {
                VScrollBar scrollBar = TextEditor.ActiveTextAreaControl.VScrollBar;
                if (scrollBar is null)
                {
                    return;
                }

                int max = scrollBar.Maximum - scrollBar.LargeChange;
                max = Math.Max(max, scrollBar.Minimum);
                scrollBar.Value = max > value ? value : max;
            }
        }

        public EolMarkerStyle EolMarkerStyle
        {
            get => TextEditor.EolMarkerStyle;
            set => TextEditor.EolMarkerStyle = value;
        }

        public bool ShowSpaces
        {
            get => TextEditor.ShowSpaces;
            set => TextEditor.ShowSpaces = value;
        }

        public bool ShowTabs
        {
            get => TextEditor.ShowTabs;
            set => TextEditor.ShowTabs = value;
        }

        public int VRulerPosition
        {
            get => TextEditor.VRulerRow;
            set => TextEditor.VRulerRow = value;
        }

        private int FirstVisibleLine
        {
            get => TextEditor.ActiveTextAreaControl.TextArea.TextView.FirstVisibleLine;
            set => TextEditor.ActiveTextAreaControl.TextArea.TextView.FirstVisibleLine = value;
        }

        public int GetLineFromVisualPosY(int visualPosY)
        {
            return TextEditor.ActiveTextAreaControl.TextArea.TextView.GetLogicalLine(visualPosY);
        }

        private string GetLineText(int line)
        {
            if (line >= TextEditor.Document.TotalNumberOfLines)
            {
                return string.Empty;
            }

            return TextEditor.Document.GetText(TextEditor.Document.GetLineSegment(line));
        }

        public void GoToLine(int lineNumber)
        {
            TextEditor.ActiveTextAreaControl.Caret.Position = new TextLocation(0, GetCaretOffset(lineNumber, rightFile: true));
            TextEditor.ActiveTextAreaControl.CenterViewOn(TextEditor.ActiveTextAreaControl.Caret.Position.Line, treshold: 5);
        }

        /// <summary>
        /// Convert the line number to the offset for the caret.
        /// </summary>
        /// <param name="lineNumber">The line number seen in the editor.</param>
        /// <param name="rightFile">If the line number for the right file is preferred.</param>
        /// <returns>The caret offset</returns>
        private int GetCaretOffset(int lineNumber, bool rightFile)
        {
            if (TextEditor.ShowLineNumbers)
            {
                // All contents is shown, just translate the lineNumber (that is an offset)
                return lineNumber - 1;
            }
            else
            {
                for (int offset = 0; offset < TotalNumberOfLines; ++offset)
                {
                    DiffLineInfo? diffLineNum = _lineNumbersControl.GetLineInfo(offset);
                    if (diffLineNum is not null)
                    {
                        int diffLine = rightFile ? diffLineNum.RightLineNumber : diffLineNum.LeftLineNumber;
                        if (diffLine != DiffLineInfo.NotApplicableLineNum && diffLine >= lineNumber)
                        {
                            return offset;
                        }
                    }
                }
            }

            return 0;
        }

        public int MaxLineNumber => TextEditor.ShowLineNumbers ? TotalNumberOfLines : _lineNumbersControl.MaxLineNumber;

        public int CurrentFileLine()
        {
            bool isPartial = _textHighlightService is (DiffHighlightService or GrepHighlightService or DifftasticHighlightService);
            _currentViewPositionCache.Capture();
            return _currentViewPositionCache.CurrentFileLine(isPartial);
        }

        private int LineAtCaret
        {
            get => TextEditor.ActiveTextAreaControl.Caret.Position.Line;
            set => TextEditor.ActiveTextAreaControl.Caret.Position = new TextLocation(TextEditor.ActiveTextAreaControl.Caret.Position.Column, value);
        }

        public void HighlightLines(int startLine, int endLine, Color color)
        {
            IDocument document = TextEditor.Document;
            if (startLine > endLine || endLine >= document.TotalNumberOfLines)
            {
                return;
            }

            MarkerStrategy markerStrategy = document.MarkerStrategy;
            LineSegment startLineSegment = document.GetLineSegment(startLine);
            LineSegment endLineSegment = document.GetLineSegment(endLine);
            markerStrategy.AddMarker(new TextMarker(startLineSegment.Offset,
                                                    endLineSegment.Offset - startLineSegment.Offset + endLineSegment.Length,
                                                    TextMarkerType.SolidBlock, color));
        }

        public void ClearHighlighting()
        {
            IDocument document = TextEditor.Document;
            document.MarkerStrategy.RemoveAll(t => true);
        }

        public int TotalNumberOfLines => TextEditor.Document.TotalNumberOfLines;

        public bool IsReadOnly
        {
            get => TextEditor.IsReadOnly;
            set => TextEditor.IsReadOnly = value;
        }

        public void SetFileLoader(GetNextFileFnc fileLoader)
        {
            _findAndReplaceForm.SetFileLoader(fileLoader);
        }

        private void TextArea_MouseWheel(object sender, MouseEventArgs e)
        {
            bool isScrollingTowardTop = e.Delta > 0;
            bool isScrollingTowardBottom = e.Delta < 0;
            VScrollBar scrollBar = TextEditor.ActiveTextAreaControl.VScrollBar;

            if (isScrollingTowardTop && (scrollBar.Value == 0))
            {
                _continuousScrollEventManager?.RaiseTopScrollReached(sender, e);
            }

            if (isScrollingTowardBottom && (!scrollBar.Visible || scrollBar.Value + scrollBar.Height > scrollBar.Maximum))
            {
                _continuousScrollEventManager?.RaiseBottomScrollReached(sender, e);
            }
        }

        private void OnHScrollPositionChanged(EventArgs e)
        {
            HScrollPositionChanged?.Invoke(this, e);
        }

        private void OnVScrollPositionChanged(EventArgs e)
        {
            VScrollPositionChanged?.Invoke(this, e);
        }

        #endregion

        public void SetGitBlameGutter(IEnumerable<GitBlameEntry> gitBlameEntries)
        {
            if (_showGutterAvatars)
            {
                _authorsAvatarMargin?.Initialize(gitBlameEntries);
            }
        }

        public bool ShowGutterAvatars
        {
            get => _showGutterAvatars;
            set
            {
                _showGutterAvatars = value;
                if (!_showGutterAvatars)
                {
                    _authorsAvatarMargin?.SetVisiblity(false);

                    return;
                }

                if (_authorsAvatarMargin is null)
                {
                    _authorsAvatarMargin = new BlameAuthorMargin(TextEditor.ActiveTextAreaControl.TextArea);
                    TextEditor.ActiveTextAreaControl.TextArea.InsertLeftMargin(0, _authorsAvatarMargin);
                }
                else
                {
                    _authorsAvatarMargin.SetVisiblity(true);
                }
            }
        }

        internal sealed class CurrentViewPositionCache
        {
            private readonly FileViewerInternal _viewer;
            public string? _currentIdentification;
            public string? _capturedIdentification;
            private ViewPosition _currentViewPosition;

            public CurrentViewPositionCache(FileViewerInternal viewer)
            {
                _viewer = viewer;
            }

            public void Capture()
            {
                if (_viewer.TotalNumberOfLines <= 1 || string.IsNullOrEmpty(_currentIdentification))
                {
                    return;
                }

                // store the previous view position
                _capturedIdentification = _currentIdentification;
                ViewPosition currentViewPosition = new()
                {
                    ActiveLineNum = null,
                    FirstLine = _viewer.GetLineText(0),
                    TotalNumberOfLines = _viewer.TotalNumberOfLines,
                    CaretPosition = _viewer.TextEditor.ActiveTextAreaControl.Caret.Position,
                    FirstVisibleLine = _viewer.FirstVisibleLine
                };
                currentViewPosition.CaretVisible = currentViewPosition.CaretPosition.Line >= currentViewPosition.FirstVisibleLine &&
                                                   currentViewPosition.CaretPosition.Line < currentViewPosition.FirstVisibleLine + _viewer.TextEditor.ActiveTextAreaControl.TextArea.TextView.VisibleLineCount;

                if (_viewer.TextEditor.ShowLineNumbers)
                {
                    // Full contents is shown, just set the line number
                    _currentViewPosition = currentViewPosition;
                    return;
                }

                int initialActiveLine = currentViewPosition.CaretVisible ?
                                            currentViewPosition.CaretPosition.Line :
                                            currentViewPosition.FirstVisibleLine;

                // search downwards for a text line, i.e. a line with line numbers
                int activeLine = initialActiveLine;
                while (activeLine < currentViewPosition.TotalNumberOfLines && currentViewPosition.ActiveLineNum is null)
                {
                    SetActiveLineNum(activeLine);
                    ++activeLine;
                }

                // if none found, search upwards
                activeLine = initialActiveLine - 1;
                while (activeLine >= 0 && currentViewPosition.ActiveLineNum is null)
                {
                    SetActiveLineNum(activeLine);
                    --activeLine;
                }

                _currentViewPosition = currentViewPosition;
                return;

                void SetActiveLineNum(int line)
                {
                    currentViewPosition.ActiveLineNum = _viewer._lineNumbersControl.GetLineInfo(line);
                    if (currentViewPosition.ActiveLineNum is null)
                    {
                        return;
                    }

                    if (currentViewPosition.ActiveLineNum.LeftLineNumber == DiffLineInfo.NotApplicableLineNum &&
                        currentViewPosition.ActiveLineNum.RightLineNumber == DiffLineInfo.NotApplicableLineNum)
                    {
                        currentViewPosition.ActiveLineNum = null;
                    }
                }
            }

            public bool Restore(string? contentIdentification)
            {
                _currentIdentification = contentIdentification;
                if (_viewer.TotalNumberOfLines <= 1 || string.IsNullOrEmpty(contentIdentification) || string.IsNullOrEmpty(_currentIdentification))
                {
                    return false;
                }

                bool sameIdentification = contentIdentification == _capturedIdentification;
                if (!sameIdentification)
                {
                    return false;
                }

                ViewPosition viewPosition = _currentViewPosition;
                if (viewPosition.ActiveLineNum is not null)
                {
                    // prefer the LeftLineNum because the base revision will not change
                    int line = viewPosition.ActiveLineNum.LeftLineNumber != DiffLineInfo.NotApplicableLineNum
                        ? _viewer.GetCaretOffset(viewPosition.ActiveLineNum.LeftLineNumber, rightFile: false)
                        : _viewer.GetCaretOffset(viewPosition.ActiveLineNum.RightLineNumber, rightFile: true);
                    _viewer.TextEditor.ActiveTextAreaControl.Caret.Position = new TextLocation(viewPosition.CaretPosition.Column, line);
                    if (viewPosition.CaretVisible)
                    {
                        _viewer.TextEditor.ActiveTextAreaControl.CenterViewOn(line, treshold: 5);
                    }
                    else
                    {
                        _viewer.FirstVisibleLine = line;
                    }
                }
                else
                {
                    _viewer.FirstVisibleLine = viewPosition.FirstVisibleLine;
                    _viewer.TextEditor.ActiveTextAreaControl.Caret.Position = viewPosition.CaretPosition;
                    if (!viewPosition.CaretVisible)
                    {
                        _viewer.FirstVisibleLine = viewPosition.FirstVisibleLine;
                    }
                }

                return true;
            }

            /// <summary>
            /// Get the line number at the current view position offset.
            /// </summary>
            /// <param name="isDiff">If the current contents is a file diff.</param>
            /// <returns>The current line number at the caret offset</returns>
            public int CurrentFileLine(bool isDiff)
            {
                ViewPosition viewPosition = _currentViewPosition;
                if (isDiff && _viewer.GetLineText(0) == viewPosition.FirstLine && viewPosition.ActiveLineNum is not null)
                {
                    // prefer the RightLineNum that is for the current revision
                    return viewPosition.ActiveLineNum.RightLineNumber != DiffLineInfo.NotApplicableLineNum
                        ? viewPosition.ActiveLineNum.RightLineNumber
                        : viewPosition.ActiveLineNum.LeftLineNumber;
                }

                // Convert from offset to line number
                return viewPosition.CaretPosition.Line + 1;
            }

            internal TestAccessor GetTestAccessor() => new(this);
            internal readonly struct TestAccessor
            {
                private readonly CurrentViewPositionCache _viewPositionCache;

                public TestAccessor(CurrentViewPositionCache viewPositionCache)
                {
                    _viewPositionCache = viewPositionCache;
                }

                public ViewPosition ViewPosition
                {
                    get => _viewPositionCache._currentViewPosition;
                    set => _viewPositionCache._currentViewPosition = value;
                }

                public TextEditorControl TextEditor => _viewPositionCache._viewer.TextEditor;

                public DiffViewerLineNumberControl LineNumberControl
                {
                    get => _viewPositionCache._viewer._lineNumbersControl;
                    set => _viewPositionCache._viewer._lineNumbersControl = value;
                }

                public void SetCapturedIdentification(string? contentIdentification) => _viewPositionCache._capturedIdentification = contentIdentification;
                public void SetCurrentIdentification(string? contentIdentification) => _viewPositionCache._currentIdentification = contentIdentification;
            }
        }

        internal struct ViewPosition
        {
            internal string FirstLine; // contains the file names in case of a diff
            internal int TotalNumberOfLines; // if changed, CaretPosition and FirstVisibleLine must be ignored and the line number must be searched
            internal TextLocation CaretPosition;
            internal int FirstVisibleLine;
            internal bool CaretVisible; // if not, FirstVisibleLine has priority for restoring
            internal DiffLineInfo? ActiveLineNum;
        }

        internal TestAccessor GetTestAccessor() => new(this);

        internal readonly struct TestAccessor
        {
            private readonly FileViewerInternal _control;

            public TestAccessor(FileViewerInternal control)
            {
                _control = control;
            }

            public TextEditorControl TextEditor => _control.TextEditor;
            public CurrentViewPositionCache CurrentViewPositionCache => _control._currentViewPositionCache;
        }
    }
}
