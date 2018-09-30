using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitUI.Editor.Diff;
using GitUIPluginInterfaces;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using ResourceManager;

namespace GitUI.Editor
{
    public partial class FileViewerInternal : GitExtensionsControl, IFileViewer
    {
        public event EventHandler<SelectedLineEventArgs> SelectedLineChanged;
        public new event MouseEventHandler MouseMove;
        public new event EventHandler MouseEnter;
        public new event EventHandler MouseLeave;
        public new event System.Windows.Forms.KeyEventHandler KeyUp;
        public new event EventHandler DoubleClick;

        private readonly FindAndReplaceForm _findAndReplaceForm = new FindAndReplaceForm();
        private readonly DiffViewerLineNumberControl _lineNumbersControl;
        private readonly Func<IGitModule> _moduleProvider;
        private DiffHighlightService _diffHighlightService = DiffHighlightService.Instance;

        public Action OpenWithDifftool { get; private set; }

        private struct ViewPosition
        {
            internal string FirstLine; // contains the file names in case of a diff
            internal int TotalNumberOfLines; // if changed, CaretPosition and FirstVisibleLine must be ignored and the line number must be searched
            internal TextLocation CaretPosition;
            internal int FirstVisibleLine;
            internal bool CaretVisible; // if not, FirstVisibleLine has priority for restoring
            internal DiffLineInfo ActiveLineNum;
        }

        public FileViewerInternal(Func<IGitModule> moduleProvider)
        {
            _moduleProvider = moduleProvider;

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

        public void SetText(string text, Action openWithDifftool, bool isDiff)
        {
            var viewPosition = GetCurrentViewPosition();

            OpenWithDifftool = openWithDifftool;
            _lineNumbersControl.Clear(isDiff);
            _lineNumbersControl.SetVisibility(isDiff);

            if (isDiff)
            {
                var index = TextEditor.ActiveTextAreaControl.TextArea.LeftMargins.IndexOf(_lineNumbersControl);
                if (index == -1)
                {
                    TextEditor.ActiveTextAreaControl.TextArea.InsertLeftMargin(0, _lineNumbersControl);
                }

                _diffHighlightService = DiffHighlightService.IsCombinedDiff(text)
                    ? CombinedDiffHighlightService.Instance
                    : DiffHighlightService.Instance;

                _lineNumbersControl.DisplayLineNumFor(text);
            }

            TextEditor.ShowLineNumbers = !isDiff;
            TextEditor.Text = text;
            TextEditor.Refresh();

            SetCurrentViewPosition(isDiff, viewPosition);
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
                highlightingStrategy = new RebaseTodoHighlightingStrategy(_moduleProvider());
            }
            else if (filename.EndsWith("COMMIT_EDITMSG"))
            {
                highlightingStrategy = new CommitMessageHighlightingStrategy(_moduleProvider());
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
                    DiffLineInfo diffLineNum = _lineNumbersControl.GetLineInfo(line);
                    if (diffLineNum != null)
                    {
                        int diffLine = rightFile ? diffLineNum.RightLineNumber : diffLineNum.LeftLineNumber;
                        if (diffLine != DiffLineInfo.NotApplicableLineNum && diffLine >= lineNumber)
                        {
                            return line;
                        }
                    }
                }
            }

            return 0;
        }

        public int MaxLineNumber => TextEditor.ShowLineNumbers ? TotalNumberOfLines : _lineNumbersControl.MaxLineNumber;

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

        private ViewPosition GetCurrentViewPosition()
        {
            if (TotalNumberOfLines <= 1)
            {
                return new ViewPosition();
            }

            // store the previous view position
            var previousViewPosition = new ViewPosition
            {
                ActiveLineNum = null,
                FirstLine = GetLineText(0),
                TotalNumberOfLines = TotalNumberOfLines,
                CaretPosition = TextEditor.ActiveTextAreaControl.Caret.Position,
                FirstVisibleLine = FirstVisibleLine
            };
            previousViewPosition.CaretVisible = previousViewPosition.CaretPosition.Line >= previousViewPosition.FirstVisibleLine &&
                                                previousViewPosition.CaretPosition.Line < previousViewPosition.FirstVisibleLine + TextEditor.ActiveTextAreaControl.TextArea.TextView.VisibleLineCount;

            if (TextEditor.ShowLineNumbers)
            {
                // a diff was displayed
                return previousViewPosition;
            }

            int initialActiveLine = previousViewPosition.CaretVisible ? previousViewPosition.CaretPosition.Line : previousViewPosition.FirstVisibleLine;

            // search downwards for a code line, i.e. a line with line numbers
            int activeLine = initialActiveLine;
            while (activeLine < previousViewPosition.TotalNumberOfLines && previousViewPosition.ActiveLineNum == null)
            {
                SetActiveLineNum(activeLine);
                ++activeLine;
            }

            // if none found, search upwards
            activeLine = initialActiveLine - 1;
            while (activeLine >= 0 && previousViewPosition.ActiveLineNum == null)
            {
                SetActiveLineNum(activeLine);
                --activeLine;
            }

            return previousViewPosition;

            void SetActiveLineNum(int line)
            {
                previousViewPosition.ActiveLineNum = _lineNumbersControl.GetLineInfo(line);
                if (previousViewPosition.ActiveLineNum == null)
                {
                    return;
                }

                if (previousViewPosition.ActiveLineNum.LeftLineNumber == DiffLineInfo.NotApplicableLineNum &&
                    previousViewPosition.ActiveLineNum.RightLineNumber == DiffLineInfo.NotApplicableLineNum)
                {
                    previousViewPosition.ActiveLineNum = null;
                }
            }
        }

        private void SetCurrentViewPosition(bool isDiff, ViewPosition viewPosition)
        {
            if (TotalNumberOfLines <= 1)
            {
                return;
            }

            if (TotalNumberOfLines == viewPosition.TotalNumberOfLines)
            {
                FirstVisibleLine = viewPosition.FirstVisibleLine;
                TextEditor.ActiveTextAreaControl.Caret.Position = viewPosition.CaretPosition;
                if (!viewPosition.CaretVisible)
                {
                    FirstVisibleLine = viewPosition.FirstVisibleLine;
                }
            }
            else if (isDiff && GetLineText(0) == viewPosition.FirstLine && viewPosition.ActiveLineNum != null)
            {
                // prefer the LeftLineNum because the base revision will not change
                int line = viewPosition.ActiveLineNum.LeftLineNumber != DiffLineInfo.NotApplicableLineNum
                    ? GetCaretLine(viewPosition.ActiveLineNum.LeftLineNumber, rightFile: false)
                    : GetCaretLine(viewPosition.ActiveLineNum.RightLineNumber, rightFile: true);
                if (viewPosition.CaretVisible)
                {
                    TextEditor.ActiveTextAreaControl.Caret.Position = new TextLocation(viewPosition.CaretPosition.Column, line);
                    TextEditor.ActiveTextAreaControl.CenterViewOn(line, treshold: 5);
                }
                else
                {
                    FirstVisibleLine = line;
                }
            }
        }
    }
}