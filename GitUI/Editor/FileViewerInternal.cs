﻿using GitCommands;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI.Editor.Diff;
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
        private DiffHighlightService _diffHighlightService = DiffHighlightService.Instance;
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
                //// _diffHighlightService not disposable
                //// _lineNumbersControl not disposable
                //// _currentViewPositionCache not disposable
                _findAndReplaceForm.Dispose();
            };

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

            HighlightingManager.Manager.DefaultHighlighting.SetColorFor("LineNumbers",
                new HighlightColor(SystemColors.ControlText, SystemColors.Control, false, false));
            TextEditor.ActiveTextAreaControl.TextEditorProperties.EnableFolding = false;

            _lineNumbersControl = new DiffViewerLineNumberControl(TextEditor.ActiveTextAreaControl.TextArea);

            VRulerPosition = AppSettings.DiffVerticalRulerPosition;
        }

        public void SetContinuousScrollManager(ContinuousScrollEventManager continuousScrollEventManager)
        {
            _continuousScrollEventManager = continuousScrollEventManager;
        }

        private void SelectionManagerSelectionChanged(object sender, EventArgs e)
        {
            string text = TextEditor.ActiveTextAreaControl.TextArea.SelectionManager.SelectedText;
            TextEditor.Document.MarkerStrategy.RemoveAll(m => true);

            IList<TextMarker> selectionMarkers = GetTextMarkersMatchingWord(text);

            foreach (var selectionMarker in selectionMarkers)
            {
                TextEditor.Document.MarkerStrategy.AddMarker(selectionMarker);
            }

            _diffHighlightService.AddPatchHighlighting(TextEditor.Document);
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

            List<TextMarker> selectionMarkers = new();

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

        public void Find()
        {
            _findAndReplaceForm.ShowFor(TextEditor, false);
            OnVScrollPositionChanged(EventArgs.Empty);
        }

        public async Task FindNextAsync(bool searchForwardOrOpenWithDifftool)
        {
            if (searchForwardOrOpenWithDifftool && OpenWithDifftool is not null && string.IsNullOrEmpty(_findAndReplaceForm.LookFor))
            {
                OpenWithDifftool.Invoke();
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

        public void SetText(string text, Action? openWithDifftool, bool isDiff = false)
        {
            SetText(text, openWithDifftool, isDiff, false);
        }

        public void SetText(string text, Action? openWithDifftool, bool isDiff, bool isRangeDiff)
        {
            _currentViewPositionCache.Capture();

            OpenWithDifftool = openWithDifftool;
            _lineNumbersControl.Clear(isDiff && !isRangeDiff);
            _lineNumbersControl.SetVisibility(isDiff && !isRangeDiff);

            if (isDiff)
            {
                var index = TextEditor.ActiveTextAreaControl.TextArea.LeftMargins.IndexOf(_lineNumbersControl);
                if (index == -1)
                {
                    TextEditor.ActiveTextAreaControl.TextArea.InsertLeftMargin(0, _lineNumbersControl);
                }

                _diffHighlightService = isRangeDiff
                    ? RangeDiffHighlightService.Instance
                    : DiffHighlightService.IsCombinedDiff(text)
                        ? CombinedDiffHighlightService.Instance
                        : DiffHighlightService.Instance;

                if (!isRangeDiff)
                {
                    _lineNumbersControl.DisplayLineNumFor(text);
                }
            }

            TextEditor.Text = text;

            // important to set after the text was changed
            // otherwise the may be rendering artifacts as noted in #5568
            TextEditor.ShowLineNumbers = ShowLineNumbers ?? !isDiff;
            if (ShowLineNumbers.HasValue && !ShowLineNumbers.Value)
            {
                Padding = new Padding(DpiUtil.Scale(5), Padding.Top, Padding.Right, Padding.Bottom);
            }

            TextEditor.Refresh();

            _currentViewPositionCache.Restore(isDiff);

            if (_shouldScrollToBottom || _shouldScrollToTop)
            {
                var scrollBar = TextEditor.ActiveTextAreaControl.VScrollBar;
                if (scrollBar.Visible)
                {
                    scrollBar.Value = _shouldScrollToTop ? 0 : Math.Max(0, scrollBar.Maximum - scrollBar.Height - _bottomBlankHeight);
                }

                _shouldScrollToTop = false;
                _shouldScrollToBottom = false;
            }
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
            if (filename.EndsWith("git-rebase-todo"))
            {
                highlightingStrategy = new RebaseTodoHighlightingStrategy(Module);
            }
            else if (filename.EndsWith("COMMIT_EDITMSG"))
            {
                highlightingStrategy = new CommitMessageHighlightingStrategy(Module);
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

        public void AddPatchHighlighting()
        {
            TextEditor.Document.MarkerStrategy.RemoveAll(m => true);
            _diffHighlightService.AddPatchHighlighting(TextEditor.Document);
        }

        public int HScrollPosition
        {
            get { return TextEditor.ActiveTextAreaControl.HScrollBar?.Value ?? 0; }
            set
            {
                var scrollBar = TextEditor.ActiveTextAreaControl.HScrollBar;
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
                var scrollBar = TextEditor.ActiveTextAreaControl.VScrollBar;
                if (scrollBar is null)
                {
                    return;
                }

                int max = scrollBar.Maximum - scrollBar.LargeChange;
                max = Math.Max(max, scrollBar.Minimum);
                scrollBar.Value = max > value ? value : max;
            }
        }

        public bool ShowEOLMarkers
        {
            get => TextEditor.ShowEOLMarkers;
            set => TextEditor.ShowEOLMarkers = value;
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

        public int FirstVisibleLine
        {
            get => TextEditor.ActiveTextAreaControl.TextArea.TextView.FirstVisibleLine;
            set => TextEditor.ActiveTextAreaControl.TextArea.TextView.FirstVisibleLine = value;
        }

        public int GetLineFromVisualPosY(int visualPosY)
        {
            return TextEditor.ActiveTextAreaControl.TextArea.TextView.GetLogicalLine(visualPosY);
        }

        public string GetLineText(int line)
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
                    DiffLineInfo diffLineNum = _lineNumbersControl.GetLineInfo(offset);
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

        public int CurrentFileLine(bool isDiff)
        {
            _currentViewPositionCache.Capture();
            return _currentViewPositionCache.CurrentFileLine(isDiff);
        }

        public int LineAtCaret
        {
            get => TextEditor.ActiveTextAreaControl.Caret.Position.Line;
            set => TextEditor.ActiveTextAreaControl.Caret.Position = new TextLocation(TextEditor.ActiveTextAreaControl.Caret.Position.Column, value);
        }

        public void HighlightLine(int line, Color color)
        {
            _diffHighlightService.HighlightLine(TextEditor.Document, line, color);
        }

        public void HighlightLines(int startLine, int endLine, Color color)
        {
            _diffHighlightService.HighlightLines(TextEditor.Document, startLine, endLine, color);
        }

        public void ClearHighlighting()
        {
            var document = TextEditor.Document;
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
            var isScrollingTowardTop = e.Delta > 0;
            var isScrollingTowardBottom = e.Delta < 0;
            var scrollBar = TextEditor.ActiveTextAreaControl.VScrollBar;

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
            private ViewPosition _currentViewPosition;
            internal TestAccessor GetTestAccessor() => new(this);

            public CurrentViewPositionCache(FileViewerInternal viewer)
            {
                _viewer = viewer;
            }

            public void Capture()
            {
                if (_viewer.TotalNumberOfLines <= 1)
                {
                    return;
                }

                // store the previous view position
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
                    // a diff was displayed
                    _currentViewPosition = currentViewPosition;
                    return;
                }

                int initialActiveLine = currentViewPosition.CaretVisible ?
                                            currentViewPosition.CaretPosition.Line :
                                            currentViewPosition.FirstVisibleLine;

                // search downwards for a code line, i.e. a line with line numbers
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

            public void Restore(bool isDiff)
            {
                if (_viewer.TotalNumberOfLines <= 1)
                {
                    return;
                }

                var viewPosition = _currentViewPosition;
                if (_viewer.TotalNumberOfLines == viewPosition.TotalNumberOfLines)
                {
                    _viewer.FirstVisibleLine = viewPosition.FirstVisibleLine;
                    _viewer.TextEditor.ActiveTextAreaControl.Caret.Position = viewPosition.CaretPosition;
                    if (!viewPosition.CaretVisible)
                    {
                        _viewer.FirstVisibleLine = viewPosition.FirstVisibleLine;
                    }
                }
                else if (isDiff && _viewer.GetLineText(0) == viewPosition.FirstLine && viewPosition.ActiveLineNum is not null)
                {
                    // prefer the LeftLineNum because the base revision will not change
                    int line = viewPosition.ActiveLineNum.LeftLineNumber != DiffLineInfo.NotApplicableLineNum
                        ? _viewer.GetCaretOffset(viewPosition.ActiveLineNum.LeftLineNumber, rightFile: false)
                        : _viewer.GetCaretOffset(viewPosition.ActiveLineNum.RightLineNumber, rightFile: true);
                    if (viewPosition.CaretVisible)
                    {
                        _viewer.TextEditor.ActiveTextAreaControl.Caret.Position = new TextLocation(viewPosition.CaretPosition.Column, line);
                        _viewer.TextEditor.ActiveTextAreaControl.CenterViewOn(line, treshold: 5);
                    }
                    else
                    {
                        _viewer.FirstVisibleLine = line;
                    }
                }
            }

            /// <summary>
            /// Get the line number at the current view position offset.
            /// </summary>
            /// <param name="isDiff">If the current contents is a file diff.</param>
            /// <returns>The current line number at the caret offset</returns>
            public int CurrentFileLine(bool isDiff)
            {
                var viewPosition = _currentViewPosition;
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
        }
    }
}
