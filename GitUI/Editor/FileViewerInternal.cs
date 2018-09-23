using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitUI.Editor.Diff;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor
{
    public partial class FileViewerInternal : GitModuleControl, IFileViewer
    {
        public event EventHandler<SelectedLineEventArgs> SelectedLineChanged;
        public new event MouseEventHandler MouseMove;
        public new event EventHandler MouseEnter;
        public new event EventHandler MouseLeave;
        public new event System.Windows.Forms.KeyEventHandler KeyUp;
        public new event EventHandler DoubleClick;

        private readonly FindAndReplaceForm _findAndReplaceForm = new FindAndReplaceForm();
        private readonly DiffViewerLineNumberControl _lineNumbersControl;

        private DiffHighlightService _diffHighlightService = DiffHighlightService.Instance;

        private ViewPosition _previousViewPosition;

        public Action OpenWithDifftool { get; private set; }

        private struct ViewPosition
        {
            internal string FirstLine; // contains the file names in case of a diff
            internal int TotalNumberOfLines; // if changed, CaretPosition and FirstVisibleLine must be ignored and the line number must be searched
            internal TextLocation CaretPosition;
            internal int FirstVisibleLine;
            internal bool CaretVisible; // if not, FirstVisibleLine has priority for restoring
            internal DiffLineNum ActiveLineNum;
        }

        public FileViewerInternal(Func<GitModule> moduleProvider)
        {
            InitializeComponent();
            InitializeComplete();

            TextEditor.TextChanged += (s, e) => TextChanged?.Invoke(s, e);
            TextEditor.ActiveTextAreaControl.VScrollBar.ValueChanged += (s, e) => ScrollPosChanged?.Invoke(s, e);
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

            HighlightingManager.Manager.DefaultHighlighting.SetColorFor("LineNumbers", new HighlightColor(Color.FromArgb(80, 0, 0, 0), Color.White, false, false));
            TextEditor.ActiveTextAreaControl.TextEditorProperties.EnableFolding = false;

            _lineNumbersControl = new DiffViewerLineNumberControl(TextEditor.ActiveTextAreaControl.TextArea);

            VRulerPosition = AppSettings.DiffVerticalRulerPosition;
        }

        public new Font Font
        {
            get => TextEditor.Font;
            set => TextEditor.Font = value;
        }

        public void Find()
        {
            _findAndReplaceForm.ShowFor(TextEditor, false);
            ScrollPosChanged?.Invoke(this, null);
        }

        public async Task FindNextAsync(bool searchForwardOrOpenWithDifftool)
        {
            if (searchForwardOrOpenWithDifftool && OpenWithDifftool != null && string.IsNullOrEmpty(_findAndReplaceForm.LookFor))
            {
                OpenWithDifftool.Invoke();
                return;
            }

            await _findAndReplaceForm.FindNextAsync(viaF3: true, !searchForwardOrOpenWithDifftool, "Text not found");
            ScrollPosChanged?.Invoke(this, null);
        }

        #region IFileViewer Members

        public event EventHandler ScrollPosChanged;
        public new event EventHandler TextChanged;

        public string GetText()
        {
            return TextEditor.Text;
        }

        public bool ShowLineNumbers
        {
            get => TextEditor.ShowLineNumbers;
            set => TextEditor.ShowLineNumbers = value;
        }

        public void SetText(string text, Action openWithDifftool, bool isDiff = false)
        {
            if (TotalNumberOfLines > 1)
            {
                // store the previous view position
                _previousViewPosition.FirstLine = GetLineText(0);
                _previousViewPosition.TotalNumberOfLines = TotalNumberOfLines;
                _previousViewPosition.CaretPosition = TextEditor.ActiveTextAreaControl.Caret.Position;
                _previousViewPosition.FirstVisibleLine = FirstVisibleLine;
                _previousViewPosition.CaretVisible
                    = _previousViewPosition.CaretPosition.Line >= _previousViewPosition.FirstVisibleLine
                      && _previousViewPosition.CaretPosition.Line < _previousViewPosition.FirstVisibleLine + TextEditor.ActiveTextAreaControl.TextArea.TextView.VisibleLineCount;
                _previousViewPosition.ActiveLineNum = null;
                int initialActiveLine = _previousViewPosition.CaretVisible ? _previousViewPosition.CaretPosition.Line : _previousViewPosition.FirstVisibleLine;
                if (/*a diff was displayed*/!TextEditor.ShowLineNumbers)
                {
                    void SetActiveLineNum(int line)
                    {
                        _previousViewPosition.ActiveLineNum = _lineNumbersControl.GetLineNum(line);
                        if (_previousViewPosition.ActiveLineNum != null)
                        {
                            if (_previousViewPosition.ActiveLineNum.LeftLineNum == DiffLineNum.NotApplicableLineNum
                                && _previousViewPosition.ActiveLineNum.RightLineNum == DiffLineNum.NotApplicableLineNum)
                            {
                                _previousViewPosition.ActiveLineNum = null;
                            }
                        }
                    }

                    // search downwards for a code line, i.e. a line with line numbers
                    for (int activeLine = initialActiveLine; activeLine < _previousViewPosition.TotalNumberOfLines && _previousViewPosition.ActiveLineNum == null; ++activeLine)
                    {
                        SetActiveLineNum(activeLine);
                    }

                    // if none found, search upwards
                    for (int activeLine = initialActiveLine - 1; activeLine >= 0 && _previousViewPosition.ActiveLineNum == null; --activeLine)
                    {
                        SetActiveLineNum(activeLine);
                    }
                }
            }

            OpenWithDifftool = openWithDifftool;
            _lineNumbersControl.Clear(isDiff);

            if (isDiff)
            {
                TextEditor.ShowLineNumbers = false;
                _lineNumbersControl.SetVisibility(true);
                var index = TextEditor.ActiveTextAreaControl.TextArea.LeftMargins.IndexOf(_lineNumbersControl);
                if (index == -1)
                {
                    TextEditor.ActiveTextAreaControl.TextArea.InsertLeftMargin(0, _lineNumbersControl);
                }

                _diffHighlightService = DiffHighlightService.IsCombinedDiff(text)
                    ? CombinedDiffHighlightService.Instance
                    : DiffHighlightService.Instance;
            }
            else
            {
                TextEditor.ShowLineNumbers = true;
                _lineNumbersControl.SetVisibility(false);
            }

            TextEditor.Text = text;

            if (isDiff)
            {
                _lineNumbersControl.DisplayLineNumFor(text);
            }

            TextEditor.Refresh();

            if (TotalNumberOfLines > 1)
            {
                if (TotalNumberOfLines == _previousViewPosition.TotalNumberOfLines)
                {
                    FirstVisibleLine = _previousViewPosition.FirstVisibleLine;
                    TextEditor.ActiveTextAreaControl.Caret.Position = _previousViewPosition.CaretPosition;
                    if (!_previousViewPosition.CaretVisible)
                    {
                        FirstVisibleLine = _previousViewPosition.FirstVisibleLine;
                    }
                }
                else if (isDiff && GetLineText(0) == _previousViewPosition.FirstLine && _previousViewPosition.ActiveLineNum != null)
                {
                    // prefer the LeftLineNum because the base revision will not change
                    int line = _previousViewPosition.ActiveLineNum.LeftLineNum != DiffLineNum.NotApplicableLineNum
                               ? GetCaretLine(_previousViewPosition.ActiveLineNum.LeftLineNum, rightFile: false)
                               : GetCaretLine(_previousViewPosition.ActiveLineNum.RightLineNum, rightFile: true);
                    if (_previousViewPosition.CaretVisible)
                    {
                        TextEditor.ActiveTextAreaControl.Caret.Position = new TextLocation(_previousViewPosition.CaretPosition.Column, line);
                        TextEditor.ActiveTextAreaControl.CenterViewOn(line, treshold: 5);
                    }
                    else
                    {
                        FirstVisibleLine = line;
                    }
                }
            }
        }

        public void SetHighlighting(string syntax)
        {
            TextEditor.SetHighlighting(syntax);
            TextEditor.Refresh();
        }

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

            if (highlightingStrategy != null)
            {
                TextEditor.Document.HighlightingStrategy = highlightingStrategy;
            }
            else
            {
                TextEditor.SetHighlighting("XML");
            }

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
            _diffHighlightService.AddPatchHighlighting(TextEditor.Document);
        }

        public int ScrollPos
        {
            get { return TextEditor.ActiveTextAreaControl.VScrollBar?.Value ?? 0; }
            set
            {
                var scrollBar = TextEditor.ActiveTextAreaControl.VScrollBar;
                if (scrollBar == null)
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
            TextEditor.ActiveTextAreaControl.Caret.Position = new TextLocation(0, GetCaretLine(lineNumber, rightFile: true));
        }

        private int GetCaretLine(int lineNumber, bool rightFile)
        {
            if (TextEditor.ShowLineNumbers)
            {
                return lineNumber - 1;
            }
            else
            {
                for (int line = 0; line < TotalNumberOfLines; ++line)
                {
                    DiffLineNum diffLineNum = _lineNumbersControl.GetLineNum(line);
                    if (diffLineNum != null)
                    {
                        int diffLine = rightFile ? diffLineNum.RightLineNum : diffLineNum.LeftLineNum;
                        if (diffLine != DiffLineNum.NotApplicableLineNum && diffLine >= lineNumber)
                        {
                            return line;
                        }
                    }
                }
            }

            return 0;
        }

        public int MaxLineNumber => TextEditor.ShowLineNumbers ? TotalNumberOfLines : _lineNumbersControl.MaxValueOfLineNum;

        public int LineAtCaret => TextEditor.ActiveTextAreaControl.Caret.Position.Line;

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

        #endregion
    }
}